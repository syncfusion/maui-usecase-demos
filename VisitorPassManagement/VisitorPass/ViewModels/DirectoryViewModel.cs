using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisitorPass.Models;
using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// VP-050 — ViewModel for the Directory screen (Screen 2).
///
/// Subscribes to <see cref="IVisitorStore.VisitorsChanged"/> and
/// rebuilds <see cref="FilteredVisitors"/> whenever the store mutates
/// OR the search text / status filter changes.
///
/// Filter logic:
///
///   1. <see cref="SearchText"/> — case-insensitive substring match
///      across VisitorName, Host, and PassId. Empty / whitespace means
///      "no search filter applied".
///   2. <see cref="SelectedStatus"/> — exact-match against
///      Visitor.Status. <c>null</c> means "All" (no status filter).
///
/// The chip row in VP-051 binds to <see cref="StatusFilters"/> — a
/// mutable ObservableCollection whose entries carry their own
/// <c>IsSelected</c> flag. <see cref="SelectStatusCommand"/> is raised
/// when the user taps a chip; it updates <see cref="SelectedStatus"/>
/// and walks the collection flipping each item's flag.
///
/// Also exposes <see cref="HasNoResults"/> so the page can show the
/// <c>EmptyState</c> control when the filters exclude every visitor,
/// and <see cref="ResetFiltersCommand"/> which clears every filter at
/// once.
/// </summary>
public partial class DirectoryViewModel : BaseViewModel
{
    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "directory";

    private readonly IVisitorStore _store;

    /// <summary>
    /// Mutable, observable collection of status filter chips. The
    /// page binds <c>CollectionView.ItemsSource</c> to this property;
    /// each entry's <see cref="StatusFilter.IsSelected"/> flag drives
    /// its visual selected state.
    /// </summary>
    public ObservableCollection<StatusFilter> StatusFilters { get; } = new();

    /// <summary>Filtered visitor list bound to the SfDataGrid.</summary>
    public ObservableCollection<Visitor> FilteredVisitors { get; } = new();


    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Optional status filter (null = All).</summary>
    [ObservableProperty]
    private StatusFilter? _selectedStatus = StatusFilter.All;

    [ObservableProperty]
    private bool _hasNoResults;

    public int TotalVisitors => _store.Visitors.Count;
    public int ExpectedVisitors => _store.Visitors.Count(visitor => visitor.Status == VisitorStatus.Expected);
    public int CheckedInVisitors => _store.Visitors.Count(visitor => visitor.Status == VisitorStatus.CheckedIn);
    public int OverdueVisitors => _store.Visitors.Count(visitor => visitor.Status == VisitorStatus.Overdue);

    public int CheckedOutVisitors => _store.Visitors.Count(visitor => visitor.Status == VisitorStatus.CheckedOut);

    public int CancelledVisitors => _store.Visitors.Count(visitor => visitor.Status == VisitorStatus.Cancelled);


    /// <summary>
    /// Currently focused row in the directory grid. The Active Dossier
    /// preview card at the bottom of the page binds to this property
    /// so the user sees a summary (PassId + status badge + QR barcode)
    /// for the row they tapped without leaving the Directory page.
    /// Null until the user taps a row (or until OnCellTapped is
    /// called by the page).
    /// </summary>
    [ObservableProperty]
    private Visitor? _selectedVisitor;

    /// <summary>True when SelectedVisitor has been resolved.</summary>
    public bool HasSelectedVisitor => SelectedVisitor is not null;

    public DirectoryViewModel(IVisitorStore store)
    {
        _store = store;
        _store.VisitorsChanged += OnVisitorsChanged;

        // Seed the chip collection. Done once in the constructor so
        // the page binds to a stable list; per-chip selection state
        // is mutated via UpdateSelectionFlags below.
        StatusFilters.Add(StatusFilter.All);
        StatusFilters.Add(StatusFilter.Of(VisitorStatus.Expected));
        StatusFilters.Add(StatusFilter.Of(VisitorStatus.CheckedIn));
        StatusFilters.Add(StatusFilter.Of(VisitorStatus.CheckedOut));
        StatusFilters.Add(StatusFilter.Of(VisitorStatus.Cancelled));
        StatusFilters.Add(StatusFilter.Of(VisitorStatus.Overdue));
        UpdateSelectionFlags();

        Rebuild();
    }

    public double GridHeight =>
                    42 + (FilteredVisitors?.Count ?? 0) *
                    (DeviceInfo.Idiom == DeviceIdiom.Phone ? 120 : 56);

    /// <summary>
    /// Raised when the user taps a status chip. Sets
    /// <see cref="SelectedStatus"/> to the tapped entry, which in turn
    /// triggers the OnSelectedStatusChanged partial method (and
    /// therefore the filter rebuild).
    /// </summary>
    [RelayCommand]
    public void SelectStatus(StatusFilter? filter)
    {
        if (filter is null) return;
        SelectedStatus = filter;
    }

    /// <summary>Re-evaluate the filtered list. Public so the page can
    /// call it from the search-entry's TextChanged handler without
    /// waiting for a store mutation.</summary>
    [RelayCommand]
    public void Rebuild()
    {
        var visitors = _store.Visitors;
        var query = (SearchText ?? string.Empty).Trim();
        var status = SelectedStatus;

        FilteredVisitors.Clear();
        foreach (var v in ApplyFilters(visitors, query, status))
            FilteredVisitors.Add(v);

        HasNoResults = FilteredVisitors.Count == 0;
        OnPropertyChanged(nameof(TotalVisitors));
        OnPropertyChanged(nameof(ExpectedVisitors));
        OnPropertyChanged(nameof(CheckedInVisitors));
        OnPropertyChanged(nameof(OverdueVisitors));
    }

    /// <summary>Reset every filter and re-run the query. Bound to
    /// the EmptyState's "Clear filters" affordance.</summary>
    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = StatusFilter.All;
        Rebuild();
    }

    partial void OnSearchTextChanged(string value) => Rebuild();

    partial void OnSelectedStatusChanged(StatusFilter? value)
    {
        UpdateSelectionFlags();
        Rebuild();
    }

    private void OnVisitorsChanged(object? sender, Visitor? e) => Rebuild();

    /// <summary>
    /// Flip <see cref="StatusFilter.IsSelected"/> on every chip entry
    /// so the visual state matches <see cref="SelectedStatus"/>.
    /// </summary>
    private void UpdateSelectionFlags()
    {
        foreach (var filter in StatusFilters)
            filter.IsSelected = ReferenceEquals(filter, SelectedStatus);
    }

    private static IEnumerable<Visitor> ApplyFilters(
        IEnumerable<Visitor> source, string query, StatusFilter? status)
    {
        var hasQuery = !string.IsNullOrEmpty(query);
        var hasStatus = status is { IsAll: false };

        foreach (var v in source)
        {
            if (hasStatus && v.Status != status!.Status)
                continue;

            if (hasQuery)
            {
                var name = v.VisitorName ?? string.Empty;
                var host = v.Host ?? string.Empty;
                var pass = v.PassId ?? string.Empty;
                var match = name.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || host.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || pass.Contains(query, StringComparison.OrdinalIgnoreCase);
                if (!match)
                    continue;
            }

            yield return v;
        }
    }

    // ---------------------------------------------------------------
    // StatusFilter — bound model for each chip in the status row.
    // Not a record (records are immutable); class so IsSelected can
    // be flipped from the VM and the chip Border's DataTrigger can
    // bind to it without converter plumbing.
    // ---------------------------------------------------------------

    /// <summary>
    /// Bound model for a status filter chip. <see cref="Status"/> is
    /// <c>null</c> for the "All" pseudo-entry. <see cref="IsSelected"/>
    /// is updated by <see cref="DirectoryViewModel.UpdateSelectionFlags"/>.
    /// </summary>
    public sealed class StatusFilter : ObservableObject
    {
        /// <summary>The underlying status, or <c>null</c> for "All".</summary>
        public VisitorStatus? Status { get; }

        /// <summary>True when this entry represents "All statuses" (no filter).</summary>
        public bool IsAll => Status is null;

        /// <summary>Caption rendered on the chip.</summary>
        public string Label { get; }

        private bool _isSelected;
        /// <summary>True when this chip is the currently selected filter.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private StatusFilter(VisitorStatus? status, string label)
        {
            Status = status;
            Label = label;
        }

        /// <summary>Special "All" entry — <see cref="Status"/> is null.</summary>
        public static StatusFilter All { get; } = new(status: null, label: "All");

        /// <summary>Specific-status entry.</summary>
        public static StatusFilter Of(VisitorStatus status)
            => new(status: status, label: status.Display());
    }
}
