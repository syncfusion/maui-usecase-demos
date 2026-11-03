namespace DigitalQueueManagement.Model;

public sealed class QueueStore
{
    private readonly List<QueueToken> _tokens;
    private readonly List<QueueCounter> _counters;
    private readonly IReadOnlyList<QueueToken> _initialTokens;
    private readonly IReadOnlyList<QueueCounter> _initialCounters;

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
    }

    public event EventHandler? Changed;

    public IReadOnlyList<QueueToken> Tokens => _tokens.AsReadOnly();

    public IReadOnlyList<QueueCounter> Counters => _counters.AsReadOnly();

    public QueueToken CreateToken(string service, TokenPriority priority, string counterName)
    {
        ValidateService(service);
        EnsureOnlineCounter(counterName);

        var token = new QueueToken(
            NextTokenId(QueueServices.PrefixFor(service)),
            service,
            priority,
            TokenStatus.Waiting,
            counterName,
            WaitFor(counterName));

        _tokens.Add(token);
        OnChanged();
        return token;
    }

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
        OnChanged();
        return updated;
    }

    public bool DeleteToken(string tokenId)
    {
        var index = _tokens.FindIndex(token => token.Id == tokenId);
        if (index < 0)
        {
            return false;
        }

        _tokens.RemoveAt(index);
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
        var next = _tokens.FirstOrDefault(token =>
            token.Status == TokenStatus.Waiting && token.CounterName == counterName)
            ?? _tokens.FirstOrDefault(token => token.Status == TokenStatus.Waiting);
        if (next is null)
        {
            return null;
        }

        var servingIndex = _tokens.FindIndex(token =>
            token.Status == TokenStatus.Serving && token.CounterName == counterName);
        if (servingIndex >= 0)
        {
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
        OnChanged();
        return serving;
    }

    public bool CompleteCurrent(string counterName) =>
        ChangeCurrentStatus(counterName, TokenStatus.Serving, TokenStatus.Completed, "Done");

    public bool SkipCurrent(string counterName) =>
        ChangeCurrentStatus(counterName, TokenStatus.Serving, TokenStatus.Skipped, "Skipped");

    public bool RecallCurrent(string counterName)
    {
        EnsureOnlineCounter(counterName);
        var skipped = _tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Skipped);
        return skipped is not null && RecallToken(skipped.Id);
    }

    public bool RecallToken(string tokenId)
    {
        var index = _tokens.FindIndex(token =>
            token.Id == tokenId && token.Status == TokenStatus.Skipped);
        if (index < 0)
        {
            return false;
        }

        _tokens[index] = _tokens[index] with { Status = TokenStatus.Waiting, Wait = "Now" };
        OnChanged();
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
        var moved = source with
        {
            Id = NextTokenId(destination.Name, destination.Service),
            CounterName = destination.Name,
            Status = TokenStatus.Waiting,
            Wait = WaitFor(destination.Name)
        };
        _tokens.Add(moved);
        OnChanged();
        return moved;
    }

    public void Reset()
    {
        _tokens.Clear();
        _tokens.AddRange(_initialTokens);
        _counters.Clear();
        _counters.AddRange(_initialCounters);
        OnChanged();
    }

    private bool ChangeCurrentStatus(
        string counterName,
        TokenStatus requiredStatus,
        TokenStatus newStatus,
        string wait)
    {
        EnsureOnlineCounter(counterName);
        var index = _tokens.FindIndex(token =>
            token.CounterName == counterName && token.Status == requiredStatus);
        if (index < 0)
        {
            return false;
        }

        _tokens[index] = _tokens[index] with { Status = newStatus, Wait = wait };
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

    private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);

    private static IReadOnlyList<QueueToken> CreateInitialTokens() =>
    [
        new("A-104", "Payments & Billing", TokenPriority.Priority, TokenStatus.Serving, "Counter 1", "Now"),
        new("A-105", "Payments & Billing", TokenPriority.Priority, TokenStatus.Waiting, "Counter 1", "4 mins"),
        new("A-106", "Payments & Billing", TokenPriority.Normal, TokenStatus.Waiting, "Counter 1", "8 mins"),
        new("A-107", "Payments & Billing", TokenPriority.Normal, TokenStatus.Completed, "Counter 1", "Done"),
        new("A-108", "Payments & Billing", TokenPriority.Priority, TokenStatus.Skipped, "Counter 1", "Skipped"),
        new("A-109", "Payments & Billing", TokenPriority.Normal, TokenStatus.Waiting, "Counter 1", "12 mins"),
        new("A-110", "Payments & Billing", TokenPriority.Priority, TokenStatus.Completed, "Counter 1", "Done"),
        new("B-202", "General Inquiry", TokenPriority.Normal, TokenStatus.Serving, "Counter 2", "Now"),
        new("B-203", "General Inquiry", TokenPriority.Priority, TokenStatus.Waiting, "Counter 2", "4 mins"),
        new("B-204", "General Inquiry", TokenPriority.Normal, TokenStatus.Waiting, "Counter 2", "8 mins"),
        new("B-205", "General Inquiry", TokenPriority.Normal, TokenStatus.Completed, "Counter 2", "Done"),
        new("B-206", "General Inquiry", TokenPriority.Priority, TokenStatus.Waiting, "Counter 2", "12 mins"),
        new("B-207", "General Inquiry", TokenPriority.Normal, TokenStatus.Skipped, "Counter 2", "Skipped"),
        new("B-208", "General Inquiry", TokenPriority.Normal, TokenStatus.Completed, "Counter 2", "Done"),
        new("C-301", "Customer Support", TokenPriority.Priority, TokenStatus.Serving, "Counter 3", "Now"),
        new("C-302", "Customer Support", TokenPriority.Normal, TokenStatus.Waiting, "Counter 3", "4 mins"),
        new("C-303", "Customer Support", TokenPriority.Priority, TokenStatus.Waiting, "Counter 3", "8 mins"),
        new("C-304", "Customer Support", TokenPriority.Normal, TokenStatus.Completed, "Counter 3", "Done"),
        new("C-305", "Customer Support", TokenPriority.Normal, TokenStatus.Waiting, "Counter 3", "12 mins"),
        new("C-306", "Customer Support", TokenPriority.Priority, TokenStatus.Waiting, "Counter 3", "16 mins"),
        new("C-307", "Customer Support", TokenPriority.Normal, TokenStatus.Skipped, "Counter 3", "Skipped"),
        new("C-308", "Customer Support", TokenPriority.Normal, TokenStatus.Completed, "Counter 3", "Done")
    ];
}
