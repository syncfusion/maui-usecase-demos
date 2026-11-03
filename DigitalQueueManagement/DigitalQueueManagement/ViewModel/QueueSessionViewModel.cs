using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DigitalQueueManagement.Model;

namespace DigitalQueueManagement.ViewModel;

public partial class QueueSessionViewModel : ObservableObject
{
    private readonly QueueStore _store;

    public IReadOnlyList<QueueAnalyticsPoint> ServiceAnalytics { get; } =
    [
        new("Enquiry", 22, "#183E91", "44%"),
        new("Payment", 18, "#159B85", "36%"),
        new("Support", 10, "#7589D4", "20%")
    ];

    public IReadOnlyList<QueueAnalyticsPoint> FulfillmentAnalytics { get; } =
    [
        new("Completed", 42, "#159B85"),
        new("Pending", 8, "#93B4E2")
    ];

    public IReadOnlyList<QueueAnalyticsPoint> CounterAnalytics { get; } =
    [
        new("Counter 1", 18, "#183E91"),
        new("Counter 2", 14, "#159B85"),
        new("Counter 3", 10, "#7589D4")
    ];

    public IReadOnlyList<QueueAnalyticsPoint> AverageWaitAnalytics { get; } =
    [
        new("Enquiry", 7.5, "#183E91"),
        new("Payment", 4.2, "#159B85"),
        new("Support", 11, "#D18A16")
    ];

    public QueueSessionViewModel(QueueStore store)
    {
        _store = store;
        _store.Changed += OnStoreChanged;
        RefreshCollections();
    }

    public ObservableCollection<QueueToken> Tokens { get; } = [];

    public ObservableCollection<QueueToken> VisibleTokens { get; } = [];

    public ObservableCollection<QueueTokenListItem> VisibleTokenRows { get; } = [];

    public ObservableCollection<QueueToken> UpcomingTokens { get; } = [];

    public ObservableCollection<QueueTokenListItem> CounterWaitingPreview { get; } = [];

    public ObservableCollection<QueueCounter> Counters { get; } = [];

    public ObservableCollection<QueueCounterSummary> CounterSummaries { get; } = [];

    public QueueStore Store => _store;

    public int TotalTokenCount => Tokens.Count;

    public int WaitingTokenCount => Tokens.Count(token => token.Status == TokenStatus.Waiting);

    public int ServingTokenCount => Tokens.Count(token => token.Status == TokenStatus.Serving);

    public int CompletedTokenCount => Tokens.Count(token => token.Status == TokenStatus.Completed);

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

    public int OnlineCounterCount => Counters.Count(counter => counter.IsOnline);

    public string OnlineCounterSummary => $"{OnlineCounterCount} of {Counters.Count} active";

    public string EstimatedWaitText => $"~{WaitingTokenCount * 4}m";

    public string AverageWaitText => $"{WaitingTokenCount * 4}m";

    public string VisibleTokenCountText => $"{VisibleTokens.Count} of {TotalTokenCount} showing";

    public bool HasVisibleTokens => VisibleTokens.Count > 0;

    public bool HasNoVisibleTokens => !HasVisibleTokens;

    public bool HasUpcomingTokens => UpcomingTokens.Count > 0;

    public bool HasNoUpcomingTokens => !HasUpcomingTokens;

    public bool HasCounterWaitingTokens => CounterWaitingPreview.Count > 0;

    public bool HasNoCounterWaitingTokens => !HasCounterWaitingTokens;

    public bool HasCounters => Counters.Count > 0;

    public bool HasNoCounters => !HasCounters;

    public string SelectedCounterDisplayName => SelectedCounter?.Name ?? "No counter selected";

    public string SelectedCounterDisplayService => SelectedCounter?.Service ?? "Create a counter to begin";

    public string SelectedCounterAvailabilityText =>
        SelectedCounter?.IsOnline == true ? "Online" : "Offline";

    public string SelectedCounterTokenId => SelectedCounterToken?.Id ?? "—";

    public string SelectedCounterTokenService => SelectedCounterToken?.Service ?? SelectedCounterDisplayService;

    public string SelectedCounterTokenStatus => SelectedCounterToken?.Status.ToString() ?? "Ready";

    public string SelectedCounterTokenWait => SelectedCounterToken?.Wait ?? "—";

    public bool CanCallNext => SelectedCounter is { IsOnline: true } && WaitingTokenCount > 0;

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

    public string ActiveHeaderTitle =>
        ActiveHeaderTab == "Overview" ? "Queue Overview" : "Token Directory";

    public string CurrentDateText => DateTime.Now.ToString("dddd, MMM d");

    public int HeaderTabIndex
    {
        get => ActiveHeaderTab == "Overview" ? 0 : 1;
        set => SetHeader(value == 0 ? "Overview" : "Manage");
    }

    public int ManageTabIndex
    {
        get => ActiveTab switch
        {
            "Counters" => 1,
            "Insights" => 2,
            _ => 0
        };
        set => SetManageTab(value switch
        {
            1 => "Counters",
            2 => "Insights",
            _ => "Tokens"
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

    [ObservableProperty]
    public partial string ActiveHeaderTab { get; set; } = "Overview";

    [ObservableProperty]
    public partial string ActiveTab { get; set; } = "Tokens";

    [ObservableProperty]
    public partial string ActiveCounterName { get; set; } = "Counter 1";

    [ObservableProperty]
    public partial string StatusFilter { get; set; } = "All Tokens";

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpcomingFilter { get; set; } = "All";

    public void SetHeader(string tab)
    {
        if (tab is not ("Overview" or "Manage"))
        {
            return;
        }

        ActiveHeaderTab = tab;
        OnPropertyChanged(nameof(HeaderTabIndex));
        OnPropertyChanged(nameof(ActiveHeaderTitle));
    }

    public void SetManageTab(string tab)
    {
        if (tab is not ("Tokens" or "Counters" or "Insights"))
        {
            return;
        }

        ActiveTab = tab;
        OnPropertyChanged(nameof(ManageTabIndex));
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

    public void SelectCounter(string counterName)
    {
        if (Counters.Any(counter => counter.Name == counterName))
        {
            ActiveCounterName = counterName;
            NotifySelectedCounterChanged();
        }
    }

    public QueueToken? CurrentTokenFor(string counterName) =>
        Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Serving)
        ?? Tokens.FirstOrDefault(token =>
            token.CounterName == counterName && token.Status == TokenStatus.Waiting);

    public QueueToken CreateToken(string service, TokenPriority priority, string counterName) =>
        _store.CreateToken(service, priority, counterName);

    public QueueToken UpdateToken(string tokenId, string service, TokenPriority priority, string counterName) =>
        _store.UpdateToken(tokenId, service, priority, counterName);

    public bool DeleteToken(string tokenId) => _store.DeleteToken(tokenId);

    public QueueCounter AddCounter(string service) => _store.AddCounter(service);

    public QueueCounter UpdateCounter(string counterName, string service, bool isOnline) =>
        _store.UpdateCounter(counterName, service, isOnline);

    public void DeleteCounter(string counterName, string destinationName) =>
        _store.DeleteCounter(counterName, destinationName);

    public QueueToken? CallNext(string counterName) => _store.CallNext(counterName);

    public bool CompleteCurrent(string counterName) => _store.CompleteCurrent(counterName);

    public bool SkipCurrent(string counterName) => _store.SkipCurrent(counterName);

    public bool RecallCurrent(string counterName) => _store.RecallCurrent(counterName);

    public bool RecallToken(string tokenId) => _store.RecallToken(tokenId);

    public QueueToken? TransferToken(string tokenId, string destinationName) =>
        _store.TransferToken(tokenId, destinationName);

    public void ResetQueue()
    {
        ActiveCounterName = "Counter 1";
        SearchText = string.Empty;
        SetHeader("Manage");
        SetManageTab("Tokens");
        SetStatusFilter("All Tokens");
        SetUpcomingFilter("All");
        _store.Reset();
    }

    private void OnStoreChanged(object? sender, EventArgs e)
    {
        RefreshCollections();
        if (!Counters.Any(counter => counter.Name == ActiveCounterName))
        {
            ActiveCounterName = Counters.FirstOrDefault()?.Name ?? string.Empty;
        }

        NotifySelectedCounterChanged();
        OnPropertyChanged(nameof(ActiveHeaderTitle));
        OnPropertyChanged(nameof(NowServingTokenId));
        OnPropertyChanged(nameof(NowServingCounterName));
        OnPropertyChanged(nameof(TotalTokenCount));
        OnPropertyChanged(nameof(WaitingTokenCount));
        OnPropertyChanged(nameof(ServingTokenCount));
        OnPropertyChanged(nameof(CompletedTokenCount));
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
        OnPropertyChanged(nameof(HasCounters));
        OnPropertyChanged(nameof(HasNoCounters));
    }

    private void NotifySelectedCounterChanged()
    {
        RefreshCounterSummaries();
        OnPropertyChanged(nameof(SelectedCounter));
        OnPropertyChanged(nameof(SelectedCounterToken));
        OnPropertyChanged(nameof(SelectedCounterDisplayName));
        OnPropertyChanged(nameof(SelectedCounterDisplayService));
        OnPropertyChanged(nameof(SelectedCounterAvailabilityText));
        OnPropertyChanged(nameof(SelectedCounterTokenId));
        OnPropertyChanged(nameof(SelectedCounterTokenService));
        OnPropertyChanged(nameof(SelectedCounterTokenStatus));
        OnPropertyChanged(nameof(SelectedCounterTokenWait));
        OnPropertyChanged(nameof(CanCallNext));
        OnPropertyChanged(nameof(CanCompleteCurrent));
        OnPropertyChanged(nameof(CanSkipCurrent));
        OnPropertyChanged(nameof(CanRecallCurrent));
        OnPropertyChanged(nameof(CanTransferCurrent));
        OnPropertyChanged(nameof(CanOpenSelectedToken));
    }

    private void RefreshCollections()
    {
        Replace(Tokens, _store.Tokens);
        Replace(Counters, _store.Counters);
        RefreshCounterSummaries();
        var waitingPositions = Tokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Select((token, index) => (token.Id, Position: index + 1))
            .ToDictionary(item => item.Id, item => item.Position);
        Replace(CounterWaitingPreview, Tokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Take(3)
            .Select(token => new QueueTokenListItem(token, GetQueuePositionText(token, waitingPositions))));
        RefreshVisibleTokens();
        RefreshUpcomingTokens();
        OnPropertyChanged(nameof(HasCounterWaitingTokens));
        OnPropertyChanged(nameof(HasNoCounterWaitingTokens));
    }

    private void RefreshCounterSummaries() =>
        Replace(CounterSummaries, Counters.Select(counter =>
        {
            var token = CurrentTokenFor(counter.Name);
            return new QueueCounterSummary(
                counter.Name,
                counter.Service,
                token?.Id ?? "Available",
                token?.Status.ToString() ?? "Ready",
                counter.IsOnline,
                counter.Name == ActiveCounterName);
        }));

    private void RefreshVisibleTokens()
    {
        var filtered = Tokens.Where(token =>
        {
            var statusMatches = StatusFilter == "All Tokens" || token.Status.ToString() == StatusFilter;
            var searchMatches = string.IsNullOrWhiteSpace(SearchText)
                || $"{token.Id} {token.Service} {token.CounterName}"
                    .Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase);
            return statusMatches && searchMatches;
        });
        Replace(VisibleTokens, filtered);
        var waitingPositions = Tokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Select((token, index) => (token.Id, Position: index + 1))
            .ToDictionary(item => item.Id, item => item.Position);
        Replace(VisibleTokenRows, VisibleTokens.Select(token =>
            new QueueTokenListItem(token, GetQueuePositionText(token, waitingPositions))));
        OnPropertyChanged(nameof(VisibleTokenCountText));
        OnPropertyChanged(nameof(HasVisibleTokens));
        OnPropertyChanged(nameof(HasNoVisibleTokens));
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
        var upcoming = Tokens
            .Where(token => token.Status == TokenStatus.Waiting)
            .Where(token => UpcomingFilter == "All" || token.Priority.ToString() == UpcomingFilter)
            .Take(5);
        Replace(UpcomingTokens, upcoming);
        OnPropertyChanged(nameof(HasUpcomingTokens));
        OnPropertyChanged(nameof(HasNoUpcomingTokens));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnSearchTextChanged(string value) => RefreshVisibleTokens();

    partial void OnStatusFilterChanged(string value) => RefreshVisibleTokens();
}
