using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using DigitalQueueManagement.Model;

namespace DigitalQueueManagement.ViewModel;

public partial class QueueSessionViewModel : ObservableObject
{
    private readonly QueueStore _store;
    private readonly Dictionary<string, string> _completedTokenByCounter = new(StringComparer.Ordinal);

    public ObservableCollection<QueueAnalyticsPoint> ServiceAnalytics { get; } = [];

    public ObservableCollection<QueueAnalyticsPoint> AverageWaitAnalytics { get; } = [];

    public QueueSessionViewModel(QueueStore store)
    {
        _store = store;
        _store.Changed += OnStoreChanged;
        RefreshCollections();
    }

    public ObservableCollection<QueueToken> Tokens { get; } = [];

    public IReadOnlyList<QueueToken> VisibleTokens { get; private set; } = Array.Empty<QueueToken>();

    public ObservableCollection<QueueTokenListItem> VisibleTokenRows { get; } = [];

    public ObservableCollection<QueueToken> UpcomingTokens { get; } = [];

    public ObservableCollection<QueueTokenListItem> CompletedStatusTokens { get; } = [];

    public ObservableCollection<QueueTokenListItem> WaitingStatusTokens { get; } = [];

    public ObservableCollection<QueueCounter> Counters { get; } = [];

    public ObservableCollection<QueueCounterSummary> CounterSummaries { get; } = [];

    public QueueStore Store => _store;

    public int TotalTokenCount => Tokens.Count;

    public int WaitingTokenCount => Tokens.Count(token => token.Status == TokenStatus.Waiting);

    public int SelectedCounterWaitingCount =>
        SelectedCounter is { } counter ? WaitingTokenCountFor(counter.Name) : 0;

    public int SelectedCounterCompletedTokenCount => CountTokensForSelectedCounter(TokenStatus.Completed);

    public int SelectedCounterSkippedTokenCount => CountTokensForSelectedCounter(TokenStatus.Skipped);

    public int ServingTokenCount => Tokens.Count(token => token.Status == TokenStatus.Serving);

    public int CompletedTokenCount => Tokens.Count(token => token.Status == TokenStatus.Completed);

    public string CompletedTokenPercentageText => GetTokenPercentageText(CompletedTokenCount);

    public string CompletedTokensPerCounterText =>
        $"{(Counters.Count == 0 ? 0 : CompletedTokenCount / (double)Counters.Count):0.#} tokens/counter";

    public string CounterActivitySummary =>
        $"{CompletedTokenCount} completed · {ServingTokenCount} serving · {WaitingTokenCount} waiting across {Counters.Count} counters";

    public int SkippedTokenCount => Tokens.Count(token => token.Status == TokenStatus.Skipped);

    public int PriorityWaitingCount => Tokens.Count(token =>
        token.Status == TokenStatus.Waiting && token.Priority == TokenPriority.Priority);

    public int NormalWaitingCount => Tokens.Count(token =>
        token.Status == TokenStatus.Waiting && token.Priority == TokenPriority.Normal);

    public int PriorityRoutingRate => WaitingTokenCount == 0
        ? 0
        : (int)Math.Round(PriorityWaitingCount * 100d / WaitingTokenCount);

    public double PriorityRoutingProgress => PriorityRoutingRate / 100d;

    public int QueueLoad => Math.Min(100, (int)Math.Round(
        WaitingTokenCount / (double)Math.Max(1, Counters.Count * 3) * 100));

    public int OnlineCounterCount => Counters.Count(IsCounterActive);

    public string OnlineCounterSummary => $"{OnlineCounterCount} of {Counters.Count} active";

    public string EstimatedWaitText => $"~{AverageWaitMinutes:0.#}m";

    public string AverageWaitText => $"{AverageWaitMinutes:0.#}m";

    public double AverageWaitMinutes
    {
        get
        {
            var waitingTokens = Tokens.Where(token => token.Status == TokenStatus.Waiting).ToArray();
            return waitingTokens.Length == 0 ? 0 : waitingTokens.Average(GetWaitMinutes);
        }
    }

    public string VisibleTokenCountText => $"{VisibleTokens.Count} of {TotalTokenCount} showing";

    public bool HasVisibleTokens => VisibleTokens.Count > 0;

    public bool HasNoVisibleTokens => !HasVisibleTokens;

    public bool HasUpcomingTokens => UpcomingTokens.Count > 0;

    public bool HasNoUpcomingTokens => !HasUpcomingTokens;

    public int CompletedStatusTokenCount => CompletedStatusTokens.Count;

    public int WaitingStatusTokenCount => WaitingStatusTokens.Count;

    public bool HasCompletedStatusTokens => CompletedStatusTokens.Count > 0;

    public bool HasNoCompletedStatusTokens => !HasCompletedStatusTokens;

    public bool HasWaitingStatusTokens => WaitingStatusTokens.Count > 0;

    public bool HasNoWaitingStatusTokens => !HasWaitingStatusTokens;

    public bool HasCounters => Counters.Count > 0;

    public bool HasNoCounters => !HasCounters;

    public double CounterPerformanceCardHeight => Math.Max(420, 196 + CounterSummaries.Count * 94);

    public double CounterStatusCardHeight => Math.Max(430, 110 + CounterSummaries.Count * 108);

    public string SelectedCounterDisplayName => SelectedCounter?.Name ?? "No counter selected";

    public string SelectedCounterDisplayService => SelectedCounter?.Service ?? "Create a counter to begin";

    public string SelectedCounterAvailabilityText =>
        SelectedCounter is { } counter ? GetCounterAvailabilityText(counter) : "Offline";

    public string SelectedCounterAvailabilityColor =>
        SelectedCounterAvailabilityText switch
        {
            "Online" => "#159B85",
            "Waiting" => "#B45309",
            _ => "#64748B"
        };

    public string SelectedCounterAvailabilityBackgroundColor =>
        SelectedCounterAvailabilityText switch
        {
            "Online" => "#E9FBF4",
            "Waiting" => "#FFF6E8",
            _ => "#F1F5F9"
        };

    public string SelectedCounterTokenId => SelectedCounterToken?.Id ?? "—";

    public string SelectedCounterTokenService => SelectedCounterToken?.Service ?? SelectedCounterDisplayService;

    public string SelectedCounterTokenStatus => SelectedCounterToken?.Status.ToString() ?? "Ready";

    public string SelectedCounterTokenWait => SelectedCounterToken?.Wait ?? "—";

    public string SelectedCounterServingDurationText
    {
        get
        {
            if (SelectedCounterToken is not { Status: TokenStatus.Serving } token
                || _store.ServingStartedAt(token.Id) is not { } startedAt)
            {
                return "—";
            }

            var elapsed = DateTime.Now - startedAt;
            return $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}";
        }
    }

    public void RefreshServingDuration() =>
        OnPropertyChanged(nameof(SelectedCounterServingDurationText));

    public bool CanCallNext =>
        SelectedCounter is { } counter && CanCallNextFor(counter.Name);

    public int WaitingTokenCountFor(string counterName) =>
        Tokens.Count(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Waiting);

    public bool CanCallNextFor(string counterName) =>
        Counters.Any(counter => counter.Name == counterName && counter.IsOnline)
        && !Tokens.Any(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving)
        && WaitingTokenCountFor(counterName) > 0;

    public bool CanCompleteCurrent =>
        SelectedCounter is { IsOnline: true } && SelectedCounterToken?.Status == TokenStatus.Serving;

    public bool CanSkipCurrent => CanCompleteCurrent;

    public bool CanRecallCurrent =>
        SelectedCounter is { IsOnline: true }
        && Tokens.Any(token =>
            token.CounterName == SelectedCounter.Name && token.Status == TokenStatus.Skipped);

    public bool CanTransferCurrent =>
        CanCompleteCurrent && Counters.Any(counter =>
            counter.IsOnline && counter.Name != SelectedCounter!.Name);

    public bool CanOpenSelectedToken => SelectedCounterToken is not null;

    public string NowServingTokenId =>
        Tokens.FirstOrDefault(token => token.Status == TokenStatus.Serving)?.Id ?? "—";

    public string NowServingCounterName =>
        Tokens.FirstOrDefault(token => token.Status == TokenStatus.Serving)?.CounterName ?? "No active counter";

    public string ActiveHeaderTitle => ActiveHeaderTab switch
    {
        "Tokens" => "Token Directory",
        "Counters" => "Counter Management",
        _ => "Queue Overview"
    };

    public string CurrentDateText => DateTime.Now.ToString("dddd, MMM d");

    public int HeaderTabIndex
    {
        get => ActiveHeaderTab switch
        {
            "Tokens" => 1,
            "Counters" => 2,
            _ => 0
        };
        set => SetHeader(value switch
        {
            1 => "Tokens",
            2 => "Counters",
            _ => "Overview"
        });
    }

    public int UpcomingFilterIndex
    {
        get => UpcomingFilter switch
        {
            "Normal" => 1,
            "Priority" => 2,
            _ => 0
        };
        set => SetUpcomingFilter(value switch
        {
            1 => "Normal",
            2 => "Priority",
            _ => "All"
        });
    }

    public int TokenListPriorityFilterIndex
    {
        get => TokenListPriorityFilter switch
        {
            "Normal" => 1,
            "Priority" => 2,
            _ => 0
        };
        set => SetTokenListPriorityFilter(value switch
        {
            1 => "Normal",
            2 => "Priority",
            _ => "All"
        });
    }

    public int StatusFilterIndex
    {
        get => StatusFilter switch
        {
            "Waiting" => 1,
            "Serving" => 2,
            "Skipped" => 3,
            "Completed" => 4,
            _ => 0
        };
        set => SetStatusFilter(value switch
        {
            1 => "Waiting",
            2 => "Serving",
            3 => "Skipped",
            4 => "Completed",
            _ => "All Tokens"
        });
    }

    public QueueCounter? SelectedCounter =>
        Counters.FirstOrDefault(counter => counter.Name == ActiveCounterName) ?? Counters.FirstOrDefault();

    public QueueToken? SelectedCounterToken =>
        SelectedCounter is null ? null : CurrentTokenFor(SelectedCounter.Name);

    public bool IsSelectedCounterServiceCompleted =>
        SelectedCounter is { } counter && _completedTokenByCounter.ContainsKey(counter.Name);

    [ObservableProperty]
    public partial string ActiveHeaderTab { get; set; } = "Overview";

    [ObservableProperty]
    public partial string ActiveTab { get; set; } = "Tokens";

    [ObservableProperty]
    public partial string ActiveCounterName { get; set; } = "Counter 1";

    [ObservableProperty]
    public partial string StatusFilter { get; set; } = "Serving";

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpcomingFilter { get; set; } = "All";

    [ObservableProperty]
    public partial string TokenListPriorityFilter { get; set; } = "All";

    public void SetHeader(string tab)
    {
        if (tab is not ("Overview" or "Tokens" or "Counters"))
        {
            return;
        }

        if (tab is "Tokens" or "Counters")
        {
            ActiveTab = tab;
        }

        ActiveHeaderTab = tab;
        OnPropertyChanged(nameof(HeaderTabIndex));
        OnPropertyChanged(nameof(ActiveHeaderTitle));
    }

    public void SetStatusFilter(string filter)
    {
        if (filter is not ("All Tokens" or "Waiting" or "Serving" or "Skipped" or "Completed"))
        {
            return;
        }

        StatusFilter = filter;
        OnPropertyChanged(nameof(StatusFilterIndex));
    }

    public void SetUpcomingFilter(string filter)
    {
        if (filter is not ("All" or "Normal" or "Priority"))
        {
            return;
        }

        UpcomingFilter = filter;
        OnPropertyChanged(nameof(UpcomingFilterIndex));
        RefreshUpcomingTokens();
    }

    public void SetTokenListPriorityFilter(string filter)
    {
        if (filter is not ("All" or "Normal" or "Priority"))
        {
            return;
        }

        TokenListPriorityFilter = filter;
        OnPropertyChanged(nameof(TokenListPriorityFilterIndex));
    }

    public void SelectCounter(string counterName)
    {
        if (ActiveCounterName != counterName
            && Counters.Any(counter => counter.Name == counterName))
        {
            ActiveCounterName = counterName;
            NotifySelectedCounterChanged();
        }
    }

    public QueueToken? CurrentTokenFor(string counterName)
    {
        if (_completedTokenByCounter.TryGetValue(counterName, out var completedTokenId))
        {
            var completedToken = Tokens.FirstOrDefault(token =>
                token.Id == completedTokenId
                && token.CounterName == counterName
                && token.Status == TokenStatus.Completed);
            if (completedToken is not null)
            {
                return completedToken;
            }
        }

        return Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving)
        ?? Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Waiting);
    }

    public bool IsCounterActive(QueueCounter counter) => counter.IsOnline;

    public string GetCounterAvailabilityText(QueueCounter counter)
    {
        if (!counter.IsOnline)
        {
            return "Offline";
        }

        if (Tokens.Any(token =>
                token.CounterName == counter.Name && token.Status == TokenStatus.Serving))
        {
            return "Online";
        }

        return Tokens.Any(token =>
            token.CounterName == counter.Name && token.Status == TokenStatus.Waiting)
            ? "Waiting"
            : "Online";
    }

    public QueueToken UpdateToken(string tokenId, string service, TokenPriority priority, string counterName) =>
        _store.UpdateToken(tokenId, service, priority, counterName);

    public bool DeleteToken(string tokenId) => _store.DeleteToken(tokenId);

    public QueueCounter AddCounter(string service) => _store.AddCounter(service);

    public QueueCounter UpdateCounter(string counterName, string service, bool isOnline) =>
        UpdateCounterState(counterName, service, isOnline);

    public void DeleteCounter(string counterName, string destinationName)
    {
        _store.DeleteCounter(counterName, destinationName);
        _completedTokenByCounter.Remove(counterName);
        OnPropertyChanged(nameof(CanCallNext));
    }

    public QueueToken? CallNext(string counterName)
    {
        if (!CanCallNextFor(counterName))
        {
            return null;
        }

        var token = _store.CallNext(counterName);
        if (token is not null)
        {
            _completedTokenByCounter.Remove(counterName);
            NotifySelectedCounterChanged(refreshStatusTokens: false);
        }

        return token;
    }

    public bool CompleteCurrent(string counterName)
    {
        var servingToken = Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving);
        var completed = _store.CompleteCurrent(counterName);
        if (completed)
        {
            if (servingToken is null)
            {
                throw new InvalidOperationException("The completed token could not be found.");
            }

            _completedTokenByCounter[counterName] = servingToken.Id;
            NotifySelectedCounterChanged(refreshStatusTokens: false);
        }

        return completed;
    }

    public bool SkipCurrent(string counterName)
    {
        var skipped = _store.SkipCurrent(counterName);
        if (skipped)
        {
            _completedTokenByCounter.Remove(counterName);
            NotifySelectedCounterChanged(refreshStatusTokens: false);
        }

        return skipped;
    }

    public bool RecallCurrent(string counterName)
    {
        var recalled = _store.RecallCurrent(counterName);
        if (recalled)
        {
            _completedTokenByCounter.Remove(counterName);
            NotifySelectedCounterChanged(refreshStatusTokens: false);
        }

        return recalled;
    }

    public bool RecallToken(string tokenId)
    {
        return _store.RecallToken(tokenId);
    }

    public QueueToken? TransferToken(string tokenId, string destinationName)
    {
        return _store.TransferToken(tokenId, destinationName);
    }

    public void ResetQueue()
    {
        _completedTokenByCounter.Clear();
        ActiveCounterName = "Counter 1";
        SearchText = string.Empty;
        SetHeader("Tokens");
        SetStatusFilter("All Tokens");
        SetUpcomingFilter("All");
        SetTokenListPriorityFilter("All");
        _store.Reset();
    }

    private void OnStoreChanged(object? sender, EventArgs e)
    {
        RefreshCollections();
        var refreshCounterSummaries = false;
        var refreshStatusTokens = false;
        foreach (var (counterName, tokenId) in _completedTokenByCounter.ToArray())
        {
            if (!Tokens.Any(token =>
                    token.Id == tokenId
                    && token.CounterName == counterName
                    && token.Status == TokenStatus.Completed))
            {
                _completedTokenByCounter.Remove(counterName);
                refreshCounterSummaries = true;
            }
        }

        if (!Counters.Any(counter => counter.Name == ActiveCounterName))
        {
            ActiveCounterName = Counters.FirstOrDefault()?.Name ?? string.Empty;
            refreshCounterSummaries = true;
            refreshStatusTokens = true;
        }

        NotifySelectedCounterChanged(refreshCounterSummaries, refreshStatusTokens);
        OnPropertyChanged(nameof(ActiveHeaderTitle));
        OnPropertyChanged(nameof(NowServingTokenId));
        OnPropertyChanged(nameof(NowServingCounterName));
        OnPropertyChanged(nameof(TotalTokenCount));
        OnPropertyChanged(nameof(WaitingTokenCount));
        OnPropertyChanged(nameof(ServingTokenCount));
        OnPropertyChanged(nameof(CompletedTokenCount));
        OnPropertyChanged(nameof(CompletedTokenPercentageText));
        OnPropertyChanged(nameof(CompletedTokensPerCounterText));
        OnPropertyChanged(nameof(CounterActivitySummary));
        OnPropertyChanged(nameof(SkippedTokenCount));
        OnPropertyChanged(nameof(PriorityWaitingCount));
        OnPropertyChanged(nameof(NormalWaitingCount));
        OnPropertyChanged(nameof(PriorityRoutingRate));
        OnPropertyChanged(nameof(PriorityRoutingProgress));
        OnPropertyChanged(nameof(QueueLoad));
        OnPropertyChanged(nameof(OnlineCounterCount));
        OnPropertyChanged(nameof(OnlineCounterSummary));
        OnPropertyChanged(nameof(EstimatedWaitText));
        OnPropertyChanged(nameof(AverageWaitText));
        OnPropertyChanged(nameof(AverageWaitMinutes));
        OnPropertyChanged(nameof(HasCounters));
        OnPropertyChanged(nameof(HasNoCounters));
    }

    private void NotifySelectedCounterChanged(
        bool refreshCounterSummaries = true,
        bool refreshStatusTokens = true)
    {
        if (refreshCounterSummaries)
        {
            RefreshCounterSummaries();
        }

        if (refreshStatusTokens)
        {
            RefreshStatusTokens();
        }

        OnPropertyChanged(nameof(SelectedCounter));
        OnPropertyChanged(nameof(SelectedCounterToken));
        OnPropertyChanged(nameof(SelectedCounterDisplayName));
        OnPropertyChanged(nameof(SelectedCounterCompletedTokenCount));
        OnPropertyChanged(nameof(SelectedCounterSkippedTokenCount));
        OnPropertyChanged(nameof(SelectedCounterDisplayService));
        OnPropertyChanged(nameof(SelectedCounterAvailabilityText));
        OnPropertyChanged(nameof(SelectedCounterAvailabilityColor));
        OnPropertyChanged(nameof(SelectedCounterAvailabilityBackgroundColor));
        OnPropertyChanged(nameof(SelectedCounterTokenId));
        OnPropertyChanged(nameof(SelectedCounterTokenService));
        OnPropertyChanged(nameof(SelectedCounterTokenStatus));
        OnPropertyChanged(nameof(SelectedCounterTokenWait));
        OnPropertyChanged(nameof(SelectedCounterServingDurationText));
        OnPropertyChanged(nameof(IsSelectedCounterServiceCompleted));
        OnPropertyChanged(nameof(CanCallNext));
        OnPropertyChanged(nameof(CanCompleteCurrent));
        OnPropertyChanged(nameof(CanSkipCurrent));
        OnPropertyChanged(nameof(CanRecallCurrent));
        OnPropertyChanged(nameof(CanTransferCurrent));
        OnPropertyChanged(nameof(CanOpenSelectedToken));
    }

    private QueueCounter UpdateCounterState(string counterName, string service, bool isOnline)
    {
        var counter = _store.UpdateCounter(counterName, service, isOnline);
        if (!isOnline && _completedTokenByCounter.Remove(counterName))
        {
            NotifySelectedCounterChanged();
        }

        return counter;
    }

    private void RefreshCollections()
    {
        Replace(Tokens, _store.Tokens);
        Replace(Counters, _store.Counters);
        RefreshAnalytics();
        RefreshCounterSummaries();
        RefreshStatusTokens();
        RefreshVisibleTokens();
        RefreshUpcomingTokens();
    }

    private void RefreshAnalytics()
    {
        var serviceColors = new[] { "#183E91", "#159B85", "#7589D4" };
        Replace(ServiceAnalytics, QueueServices.All.Select((service, index) =>
        {
            var count = Tokens.Count(token => token.Service == service);
            return new QueueAnalyticsPoint(
                service,
                count,
                serviceColors[index % serviceColors.Length],
                GetTokenPercentageText(count));
        }));

        var waitColors = new[] { "#183E91", "#159B85", "#7589D4" };
        Replace(AverageWaitAnalytics, QueueServices.All.Select((service, index) =>
        {
            var waitingTokens = Tokens
                .Where(token => token.Service == service && token.Status == TokenStatus.Waiting)
                .ToArray();
            var averageWait = waitingTokens.Length == 0
                ? 0
                : waitingTokens.Average(GetWaitMinutes);
            return new QueueAnalyticsPoint(
                service,
                averageWait,
                waitColors[index % waitColors.Length],
                $"{waitingTokens.Length} waiting");
        }));

    }

    private string GetTokenPercentageText(int count) =>
        TotalTokenCount == 0
            ? "0%"
            : $"{(int)Math.Round(count * 100d / TotalTokenCount)}%";

    private static double GetWaitMinutes(QueueToken token)
    {
        var separator = token.Wait.IndexOf(' ');
        return separator > 0
            && double.TryParse(
                token.Wait.AsSpan(0, separator),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var minutes)
                ? minutes
                : 0;
    }

    private void RefreshStatusTokens()
    {
        var counterName = SelectedCounter?.Name;
        var counterTokens = Tokens
            .Where(token => token.CounterName == counterName)
            .ToArray();
        var waitingPositions = counterTokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Select((token, index) => (token.Id, Position: index + 1))
            .ToDictionary(item => item.Id, item => item.Position);
        Replace(CompletedStatusTokens, counterTokens
            .Where(token => token.Status == TokenStatus.Completed)
            .Select(token => new QueueTokenListItem(token, GetQueuePositionText(token, waitingPositions))));
        Replace(WaitingStatusTokens, counterTokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Select(token => new QueueTokenListItem(token, GetQueuePositionText(token, waitingPositions))));
        OnPropertyChanged(nameof(CompletedStatusTokenCount));
        OnPropertyChanged(nameof(WaitingStatusTokenCount));
        OnPropertyChanged(nameof(HasCompletedStatusTokens));
        OnPropertyChanged(nameof(HasNoCompletedStatusTokens));
        OnPropertyChanged(nameof(HasWaitingStatusTokens));
        OnPropertyChanged(nameof(HasNoWaitingStatusTokens));
        OnPropertyChanged(nameof(SelectedCounterWaitingCount));
    }

    private int CountTokensForSelectedCounter(TokenStatus status) =>
        SelectedCounter is { } counter
            ? Tokens.Count(token => token.CounterName == counter.Name && token.Status == status)
            : 0;

    private void RefreshCounterSummaries()
    {
        Replace(CounterSummaries, Counters.Select(counter =>
        {
            var token = Tokens.FirstOrDefault(item =>
                item.CounterName == counter.Name && item.Status == TokenStatus.Serving)
                ?? CurrentTokenFor(counter.Name);
            return new QueueCounterSummary(
                counter.Name,
                counter.Service,
                token?.Id ?? "Available",
                token?.Status.ToString() ?? "Ready",
                IsCounterActive(counter),
                counter.Name == ActiveCounterName)
            {
                CompletedTokenCount = Tokens.Count(item =>
                    item.CounterName == counter.Name && item.Status == TokenStatus.Completed),
                ServingTokenCount = Tokens.Count(item =>
                    item.CounterName == counter.Name && item.Status == TokenStatus.Serving),
                WaitingTokenCount = WaitingTokenCountFor(counter.Name)
            };
        }));
        OnPropertyChanged(nameof(CounterPerformanceCardHeight));
        OnPropertyChanged(nameof(CounterStatusCardHeight));
    }

    private void RefreshVisibleTokens()
    {
        var statusFilter = StatusFilter;
        var priorityFilter = TokenListPriorityFilter;
        var filteredStatus = statusFilter switch
        {
            "Waiting" => TokenStatus.Waiting,
            "Serving" => TokenStatus.Serving,
            "Skipped" => TokenStatus.Skipped,
            "Completed" => TokenStatus.Completed,
            _ => (TokenStatus?)null
        };
        var showAllStatuses = statusFilter == "All Tokens";
        var searchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
        var visibleTokens = new List<QueueToken>();
        var visibleRows = new List<QueueTokenListItem>();
        var waitingPosition = 0;

        foreach (var token in Tokens)
        {
            if (token.Status == TokenStatus.Waiting)
            {
                waitingPosition++;
            }

            var statusMatches = showAllStatuses || token.Status == filteredStatus;
            var priorityMatches = priorityFilter == "All" || token.Priority.ToString() == priorityFilter;
            var searchMatches = searchText is null
                || $"{token.Id} {token.Service} {token.CounterName}"
                    .Contains(searchText, StringComparison.OrdinalIgnoreCase);
            if (!statusMatches || !priorityMatches || !searchMatches)
            {
                continue;
            }

            visibleTokens.Add(token);
            visibleRows.Add(new QueueTokenListItem(token, GetQueuePositionText(token, waitingPosition)));
        }

        VisibleTokens = visibleTokens;
        Replace(VisibleTokenRows, visibleRows);
        OnPropertyChanged(nameof(VisibleTokens));
        OnPropertyChanged(nameof(VisibleTokenCountText));
        OnPropertyChanged(nameof(HasVisibleTokens));
        OnPropertyChanged(nameof(HasNoVisibleTokens));
    }

    private static string GetQueuePositionText(QueueToken token, int waitingPosition)
    {
        if (token.Status == TokenStatus.Serving)
        {
            return "At Desk";
        }

        if (token.Status != TokenStatus.Waiting)
        {
            return token.Status.ToString();
        }

        return waitingPosition switch
        {
            1 => "Next in line",
            2 => "2nd in line",
            3 => "3rd in line",
            _ => $"{waitingPosition}th in line"
        };
    }

    private static string GetQueuePositionText(
        QueueToken token,
        IReadOnlyDictionary<string, int> waitingPositions)
    {
        if (token.Status == TokenStatus.Serving)
        {
            return "At Desk";
        }

        if (token.Status != TokenStatus.Waiting
            || !waitingPositions.TryGetValue(token.Id, out var position))
        {
            return token.Status.ToString();
        }

        return position switch
        {
            1 => "Next in line",
            2 => "2nd in line",
            3 => "3rd in line",
            _ => $"{position}th in line"
        };
    }

    private void RefreshUpcomingTokens()
    {
        var upcoming = Counters
            .SelectMany((counter, counterIndex) => Tokens
                .Where(token =>
                    token.CounterName == counter.Name
                    && token.Status == TokenStatus.Waiting
                    && (UpcomingFilter == "All" || token.Priority.ToString() == UpcomingFilter))
                .Select((token, position) => (Token: token, Position: position, CounterIndex: counterIndex)))
            .OrderBy(item => item.Position)
            .ThenBy(item => item.CounterIndex)
            .Select(item => item.Token)
            .Take(5);
        Replace(UpcomingTokens, upcoming);
        OnPropertyChanged(nameof(HasUpcomingTokens));
        OnPropertyChanged(nameof(HasNoUpcomingTokens));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        var updatedItems = items as IReadOnlyList<T> ?? items.ToArray();
        var sharedCount = Math.Min(target.Count, updatedItems.Count);
        for (var index = 0; index < sharedCount; index++)
        {
            if (!EqualityComparer<T>.Default.Equals(target[index], updatedItems[index]))
            {
                target[index] = updatedItems[index];
            }
        }

        for (var index = target.Count - 1; index >= updatedItems.Count; index--)
        {
            target.RemoveAt(index);
        }

        for (var index = sharedCount; index < updatedItems.Count; index++)
        {
            target.Add(updatedItems[index]);
        }
    }

    partial void OnSearchTextChanged(string value) => RefreshVisibleTokens();

    partial void OnStatusFilterChanged(string value) => RefreshVisibleTokens();

    partial void OnTokenListPriorityFilterChanged(string value) => RefreshVisibleTokens();
}
