namespace DigitalQueueManagement.Model;

public sealed class QueueStore
{
    private readonly List<QueueToken> _tokens;
    private readonly List<QueueCounter> _counters;
    private readonly IReadOnlyList<QueueToken> _initialTokens;
    private readonly IReadOnlyList<QueueCounter> _initialCounters;
    private readonly Dictionary<string, DateTime> _servingStartedAt = new(StringComparer.Ordinal);

    public QueueStore()
    {
        _initialTokens = CreateInitialTokens();
        _initialCounters =
        [
            new("Counter 1", "Payments & Billing"),
            new("Counter 2", "General Inquiry"),
            new("Counter 3", "Customer Support")
        ];
        _tokens = [.. _initialTokens];
        _counters = [.. _initialCounters];
        StartServingTimers();
    }

    public event EventHandler? Changed;

    public IReadOnlyList<QueueToken> Tokens => _tokens.AsReadOnly();

    public IReadOnlyList<QueueCounter> Counters => _counters.AsReadOnly();

    public DateTime? ServingStartedAt(string tokenId) =>
        _servingStartedAt.TryGetValue(tokenId, out var startedAt) ? startedAt : null;

    public QueueToken UpdateToken(
        string tokenId,
        string service,
        TokenPriority priority,
        string counterName)
    {
        ValidateService(service);
        EnsureOnlineCounter(counterName);
        var index = FindTokenIndex(tokenId);
        var existing = _tokens[index];
        if (existing.Status == TokenStatus.Serving
            && existing.CounterName != counterName
            && _tokens.Any(token =>
                token.Id != tokenId
                && token.CounterName == counterName
                && token.Status == TokenStatus.Serving))
        {
            throw new InvalidOperationException("The selected counter is already serving a token.");
        }

        var updated = existing with
        {
            Service = service,
            Priority = priority,
            CounterName = counterName
        };

        _tokens[index] = updated;
        if (existing.Status == TokenStatus.Waiting && existing.CounterName != counterName)
        {
            PromoteNextWaitingTokenIfIdle(counterName);
        }

        OnChanged();
        return _tokens[index];
    }

    public bool DeleteToken(string tokenId)
    {
        var index = _tokens.FindIndex(token => token.Id == tokenId);
        if (index < 0)
        {
            return false;
        }

        _tokens.RemoveAt(index);
        _servingStartedAt.Remove(tokenId);
        OnChanged();
        return true;
    }

    public QueueCounter AddCounter(string service)
    {
        ValidateService(service);
        var number = _counters
            .Select(counter => int.TryParse(counter.Name.AsSpan("Counter ".Length), out var value) ? value : 0)
            .DefaultIfEmpty()
            .Max() + 1;
        var counter = new QueueCounter($"Counter {number}", service);
        _counters.Add(counter);
        OnChanged();
        return counter;
    }

    public QueueCounter UpdateCounter(string counterName, string service, bool isOnline)
    {
        ValidateService(service);
        var index = FindCounterIndex(counterName);
        var updated = _counters[index] with { Service = service, IsOnline = isOnline };
        _counters[index] = updated;
        if (!isOnline)
        {
            var servingIndex = _tokens.FindIndex(token =>
                token.CounterName == counterName && token.Status == TokenStatus.Serving);
            if (servingIndex >= 0)
            {
                _servingStartedAt.Remove(_tokens[servingIndex].Id);
                _tokens[servingIndex] = _tokens[servingIndex] with
                {
                    Status = TokenStatus.Waiting,
                    Wait = WaitFor(counterName)
                };
            }
        }

        OnChanged();
        return updated;
    }

    public void DeleteCounter(string counterName, string destinationName)
    {
        if (_counters.Count <= 1)
        {
            throw new InvalidOperationException("At least one counter must remain.");
        }

        var sourceIndex = FindCounterIndex(counterName);
        EnsureOnlineCounter(destinationName);
        if (counterName == destinationName)
        {
            throw new InvalidOperationException("Choose a different destination counter.");
        }

        var destination = _counters.Single(counter => counter.Name == destinationName);
        var moving = _tokens.Where(token => token.CounterName == counterName).ToArray();
        _tokens.RemoveAll(token => token.CounterName == counterName);

        foreach (var token in moving)
        {
            _servingStartedAt.Remove(token.Id);
            var status = token.Status == TokenStatus.Serving ? TokenStatus.Waiting : token.Status;
            _tokens.Add(token with
            {
                Id = NextTokenId(destination.Name, destination.Service),
                CounterName = destination.Name,
                Status = status,
                Wait = status == TokenStatus.Waiting ? WaitFor(destination.Name) : token.Wait
            });
        }

        _counters.RemoveAt(sourceIndex);
        OnChanged();
    }

    public QueueToken? CallNext(string counterName)
    {
        EnsureOnlineCounter(counterName);
        if (_tokens.Any(token =>
                token.CounterName == counterName && token.Status == TokenStatus.Serving))
        {
            return null;
        }

        var serving = PromoteNextWaitingToken(counterName);
        if (serving is not null)
        {
            OnChanged();
        }

        return serving;
    }

    private QueueToken? PromoteNextWaitingToken(string counterName)
    {
        var next = _tokens.FirstOrDefault(token =>
            token.Status == TokenStatus.Waiting && token.CounterName == counterName);
        if (next is null)
        {
            return null;
        }

        var servingIndex = _tokens.FindIndex(token =>
            token.Status == TokenStatus.Serving && token.CounterName == counterName);
        if (servingIndex >= 0)
        {
            _servingStartedAt.Remove(_tokens[servingIndex].Id);
            _tokens[servingIndex] = _tokens[servingIndex] with
            {
                Status = TokenStatus.Completed,
                Wait = "Done"
            };
        }

        var nextIndex = FindTokenIndex(next.Id);
        var serving = _tokens[nextIndex] with
        {
            Status = TokenStatus.Serving,
            CounterName = counterName,
            Wait = "Now"
        };
        _tokens[nextIndex] = serving;
        _servingStartedAt[next.Id] = DateTime.Now;
        return serving;
    }

    private QueueToken? PromoteNextWaitingTokenIfIdle(string counterName) =>
        _tokens.Any(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving)
            ? null
            : PromoteNextWaitingToken(counterName);

    public bool CompleteCurrent(string counterName) =>
        ChangeCurrentStatus(counterName, TokenStatus.Serving, TokenStatus.Completed, "Done");

    public bool SkipCurrent(string counterName) =>
        ChangeCurrentStatus(counterName, TokenStatus.Serving, TokenStatus.Skipped, "Skipped", advanceQueue: true);

    public bool RecallCurrent(string counterName)
    {
        EnsureOnlineCounter(counterName);
        var skipped = _tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Skipped);
        if (skipped is null || !RestoreSkippedToken(skipped.Id))
        {
            return false;
        }

        if (!_tokens.Any(token =>
                token.CounterName == counterName && token.Status == TokenStatus.Serving))
        {
            PromoteNextWaitingToken(counterName);
        }

        OnChanged();
        return true;
    }

    public bool RecallToken(string tokenId)
    {
        if (!RestoreSkippedToken(tokenId))
        {
            return false;
        }

        OnChanged();
        return true;
    }

    private bool RestoreSkippedToken(string tokenId)
    {
        var index = _tokens.FindIndex(token =>
            token.Id == tokenId && token.Status == TokenStatus.Skipped);
        if (index < 0)
        {
            return false;
        }

        var skipped = _tokens[index];
        _tokens.RemoveAt(index);
        var firstWaitingIndex = _tokens.FindIndex(token =>
            token.CounterName == skipped.CounterName && token.Status == TokenStatus.Waiting);
        if (firstWaitingIndex < 0)
        {
            var servingIndex = _tokens.FindIndex(token =>
                token.CounterName == skipped.CounterName && token.Status == TokenStatus.Serving);
            firstWaitingIndex = servingIndex < 0 ? _tokens.Count : servingIndex + 1;
        }

        _tokens.Insert(firstWaitingIndex, skipped with { Status = TokenStatus.Waiting, Wait = "4 mins" });
        return true;
    }

    public QueueToken? TransferToken(string tokenId, string destinationName)
    {
        var destination = _counters.FirstOrDefault(counter => counter.Name == destinationName);
        if (destination is null || !destination.IsOnline)
        {
            return null;
        }

        var index = _tokens.FindIndex(token => token.Id == tokenId && token.Status == TokenStatus.Serving);
        if (index < 0 || _tokens[index].CounterName == destinationName)
        {
            return null;
        }

        var source = _tokens[index];
        _tokens.RemoveAt(index);
        _servingStartedAt.Remove(source.Id);
        var moved = source with
        {
            CounterName = destination.Name,
            Status = TokenStatus.Waiting,
            Wait = WaitFor(destination.Name)
        };
        _tokens.Add(moved);
        PromoteNextWaitingTokenIfIdle(destination.Name);
        PromoteNextWaitingToken(source.CounterName);
        OnChanged();
        return _tokens.First(token => token.Id == moved.Id);
    }

    public void Reset()
    {
        _tokens.Clear();
        _tokens.AddRange(_initialTokens);
        _counters.Clear();
        _counters.AddRange(_initialCounters);
        StartServingTimers();
        OnChanged();
    }

    private bool ChangeCurrentStatus(
        string counterName,
        TokenStatus requiredStatus,
        TokenStatus newStatus,
        string wait,
        bool advanceQueue = false)
    {
        EnsureOnlineCounter(counterName);
        var index = _tokens.FindIndex(token =>
            token.CounterName == counterName && token.Status == requiredStatus);
        if (index < 0)
        {
            return false;
        }

        var current = _tokens[index];
        _servingStartedAt.Remove(current.Id);
        _tokens[index] = current with { Status = newStatus, Wait = wait };
        if (advanceQueue)
        {
            PromoteNextWaitingToken(counterName);
            OnChanged();
            return true;
        }

        OnChanged();
        return true;
    }

    private string NextTokenId(string counterName, string fallbackService)
    {
        var prefix = _tokens
            .FirstOrDefault(token => token.CounterName == counterName)?
            .Id.Split('-', 2)[0] ?? QueueServices.PrefixFor(fallbackService);
        return NextTokenId(prefix);
    }

    private string NextTokenId(string prefix)
    {
        var largest = _tokens
            .Where(token => token.Id.StartsWith($"{prefix}-", StringComparison.Ordinal))
            .Select(token => int.TryParse(token.Id.AsSpan(prefix.Length + 1), out var value) ? value : 0)
            .DefaultIfEmpty(prefix switch { "A" => 103, "B" => 201, "C" => 300, _ => 400 })
            .Max();
        return $"{prefix}-{largest + 1}";
    }

    private string WaitFor(string counterName)
    {
        var position = _tokens.Count(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Waiting) + 1;
        return $"{position * 4} mins";
    }

    private int FindTokenIndex(string tokenId)
    {
        var index = _tokens.FindIndex(token => token.Id == tokenId);
        return index >= 0 ? index : throw new KeyNotFoundException($"Token '{tokenId}' was not found.");
    }

    private int FindCounterIndex(string counterName)
    {
        var index = _counters.FindIndex(counter => counter.Name == counterName);
        return index >= 0 ? index : throw new KeyNotFoundException($"Counter '{counterName}' was not found.");
    }

    private void EnsureOnlineCounter(string counterName)
    {
        var index = FindCounterIndex(counterName);
        if (!_counters[index].IsOnline)
        {
            throw new InvalidOperationException("Choose an online counter.");
        }
    }

    private static void ValidateService(string service)
    {
        if (!QueueServices.All.Contains(service, StringComparer.Ordinal))
        {
            throw new ArgumentException("Choose a supported service.", nameof(service));
        }
    }

    private void OnChanged()
    {
        var waitingPositions = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var index = 0; index < _tokens.Count; index++)
        {
            var token = _tokens[index];
            if (token.Status != TokenStatus.Waiting)
            {
                continue;
            }

            var position = waitingPositions.GetValueOrDefault(token.CounterName) + 1;
            waitingPositions[token.CounterName] = position;
            var wait = $"{position * 4} mins";
            if (token.Wait != wait)
            {
                _tokens[index] = token with { Wait = wait };
            }
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void StartServingTimers()
    {
        _servingStartedAt.Clear();
        foreach (var token in _tokens.Where(token => token.Status == TokenStatus.Serving))
        {
            _servingStartedAt[token.Id] = DateTime.Now;
        }
    }

    private static IReadOnlyList<QueueToken> CreateInitialTokens()
    {
        var tokens = new List<QueueToken>(60);
        AddCounterTokens(tokens, "A", "Counter 1", "Payments & Billing", 101);
        AddCounterTokens(tokens, "B", "Counter 2", "General Inquiry", 201);
        AddCounterTokens(tokens, "C", "Counter 3", "Customer Support", 301);
        return tokens;
    }

    private static void AddCounterTokens(
        ICollection<QueueToken> tokens,
        string prefix,
        string counterName,
        string service,
        int firstNumber)
    {
        var waitingPosition = 0;
        for (var index = 0; index < 20; index++)
        {
            var status = index switch
            {
                0 => TokenStatus.Serving,
                <= 13 => TokenStatus.Waiting,
                <= 17 => TokenStatus.Completed,
                _ => TokenStatus.Skipped
            };
            var wait = status switch
            {
                TokenStatus.Serving => "Now",
                TokenStatus.Waiting => $"{++waitingPosition * 4} mins",
                TokenStatus.Completed => "Done",
                _ => "Skipped"
            };
            var priority = index % 4 == 0 ? TokenPriority.Priority : TokenPriority.Normal;
            tokens.Add(new QueueToken(
                $"{prefix}-{firstNumber + index}",
                service,
                priority,
                status,
                counterName,
                wait));
        }
    }
}
