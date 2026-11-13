using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using HotelManagerSample;

namespace HotelManagerSample;

/// <summary>
/// View-model for the Bookings page. Wraps the master booking list into
/// selectable rows that back the SfDataGrid, drives the search bar / view-mode
/// toggle, and computes the three KPI tiles displayed above the grid.
/// Subscribes to <see cref="IHotelStore.BookingsChanged"/> so that another
/// page committing a booking (e.g. the Add-Booking popup) instantly appends
/// a SelectableBooking to <see cref="VisibleBookings"/> and refreshes KPI
/// counts from a single notification.
/// </summary>
public sealed class BookingsViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Top-bar date label — live today (DateTime.Now), updated on every refresh.</summary>
    public string DateLabel => DateTime.Now.ToString("dddd, MMM d, yyyy");

    /// <summary>Subtitle line under the page heading — property-wide summary text.</summary>
    public string Subtitle =>
        $"Manage reservations, stay schedules, and guest folios across {HotelConfig.TotalRooms} keys.";

    /// <summary>Badge text shown beside the Bookings sidebar row (active
    /// reservation count). Live-bound so it tracks the master collection.</summary>
    public string TotalRecordLabel =>
        AllBookings.Count(b =>
            !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(b.Status, "Checked out", StringComparison.OrdinalIgnoreCase)).ToString();

    /// <summary>Master collection; each entry wraps a master <see cref="IHotelStore.Bookings"/>
    /// row so the grid always shows the same set of reservations as the
    /// dashboard KPI tiles / donut / segments.</summary>
    public ObservableCollection<SelectableBooking> AllBookings { get; }

    /// <summary>Filtered subset that backs the SfDataPager and the SfDataGrid.</summary>
    public ObservableCollection<SelectableBooking> VisibleBookings { get; } = new();

    /// <summary>Number of currently selected rows; surfaces in the toolbar / pager overlay.</summary>
    public int SelectedCount => AllBookings.Count(b => b.IsSelected);

    // ---------- KPIs (all derived from the master booking collection) ----------
    /// <summary>Today's arrivals = bookings whose check-in date is today AND
    /// status isn't Cancelled. Drives the "X / Y total" KPI tile.</summary>
    public int TodaysArrivals => AllBookings.Count(b =>
        b.CheckIn.Date == HotelConfig.ReferenceToday.Date &&
        !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

    /// <summary>"X total" — count of every non-cancelled booking whose stay
    /// window touches today (check-in already happened but check-out is still
    /// in the future or today). Drives the "/ X total" caption next to the
    /// big today's-arrivals number.</summary>
    public int ArrivalsTotal => AllBookings.Count(b =>
        b.CheckIn.Date <= HotelConfig.ReferenceToday.Date &&
        b.CheckOut.Date >= HotelConfig.ReferenceToday.Date &&
        !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

    /// <summary>In-house stays — currently occupied rooms.</summary>
    public int InHouseStays => AllBookings.Count(b =>
        b.CheckIn.Date <= HotelConfig.ReferenceToday.Date &&
        b.CheckOut.Date > HotelConfig.ReferenceToday.Date &&
        string.Equals(b.Status, "Checked in", StringComparison.OrdinalIgnoreCase));

    /// <summary>"+1" delta label — net change vs. yesterday's in-house count.</summary>
    public string InHouseDeltaLabel => $"+1";
    /// <summary>Subtitle under the in-house delta chip ("Occupancy").</summary>
    public string InHouseDeltaDetail => "Occupancy";

    /// <summary>Sum of folio totals for checked-in / checked-out bookings
    /// for the current month-to-date. Constant anchor value (matches the
    /// reference Figma) — driven from the master collection but floored so
    /// the card never shows a misleading $0 on an empty day.</summary>
    public decimal FolioVolume => Math.Round(
        AllBookings
            .Where(b => string.Equals(b.Status, "Checked in", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(b.Status, "Checked out", StringComparison.OrdinalIgnoreCase))
            .Sum(b => b.Total),
        0);

    /// <summary>Month-over-month trend percentage label.</summary>
    public string FolioDeltaLabel => "+62.5%";
    /// <summary>Subtitle under the folio-delta chip.</summary>
    public string FolioDeltaDetail => "vs prior week";

    // ---------- Search ----------
    private string _searchText = string.Empty;
    /// <summary>Current search-entry text. Setter kicks off a debounced
    /// <see cref="ApplyFiltersCore"/> call so the grid narrows without
    /// repainting on every keystroke.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            var normalized = value ?? string.Empty;
            if (_searchText == normalized)
                return;
            _searchText = normalized;
            OnPropertyChanged(nameof(SearchText));
            _ = ApplyFiltersAsync();
        }
    }

    /// <summary>Fires whenever VisibleBookings changes so the pager can move to page 1.</summary>
    public event EventHandler? PagerResetRequested;

    // ---------- Grid selection ----------
    /// <summary>Backing field for <see cref="SelectedBooking"/>.</summary>
    private object? _selectedBooking;
    /// <summary>Row currently highlighted in the bookings SfDataGrid. Bound
    /// two-way from the grid's <c>SelectedItem</c> so the selection survives
    /// a filter rebuild and drives the detail strip below the pager.</summary>
    public object? SelectedBooking
    {
        get => _selectedBooking;
        set
        {
            if (ReferenceEquals(_selectedBooking, value))
                return;
            _selectedBooking = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedBookingRow));
            OnPropertyChanged(nameof(HasSelectedBooking));
            OnPropertyChanged(nameof(SelectedBookingSummary));
        }
    }

    /// <summary>The selected row as a <see cref="SelectableBooking"/>, or
    /// <c>null</c> when the cast fails (e.g. the grid briefly holds a stale
    /// item from a previous paged source while the filter rebuilds).</summary>
    public SelectableBooking? SelectedBookingRow => SelectedBooking as SelectableBooking;

    /// <summary>True when a booking row is currently selected. Binds the
    /// detail strip's visibility.</summary>
    public bool HasSelectedBooking => SelectedBookingRow is not null;

    /// <summary>One-line summary of the selected booking rendered in the
    /// detail strip under the pager.</summary>
    public string SelectedBookingSummary
    {
        get
        {
            var row = SelectedBookingRow;
            if (row is null)
                return string.Empty;
            return $"{row.Reference} \u00b7 {row.Guest} \u00b7 {row.Room} \u00b7 {row.Status} \u00b7 ${row.Total:N0}";
        }
    }

    /// <summary>Clears the grid selection. Wired to the detail strip's
    /// dismiss button so the user can drop the selection without leaving
    /// the page.</summary>
    public void ClearBookingSelection() => SelectedBooking = null;

    // ---------- DataGrid dynamic height ----------
    /// <summary>Constants that mirror the XAML on <c>BookingsGrid</c>:
    /// <c>HeaderRowHeight="32"</c>, <c>RowHeight="56"</c>, and a 1-DIP
    /// bottom grid-line stroke. Keeping these on the VM avoids passing the
    /// XAML values down via code-behind converters while still being a
    /// single source of truth for the height math.</summary>
    public const double BookingsGridHeaderHeight = 32;
    /// <summary>Row height in DIPs for the bookings SfDataGrid.</summary>
    public const double BookingsGridRowHeight = 56;
    /// <summary>Bottom grid-line stroke thickness in DIPs.</summary>
    public const double BookingsGridBottomBorder = 1;
    /// <summary>Page size the SfDataPager defaults to.</summary>
    public const int BookingsGridPageSize = 10;

    /// <summary>Height (in DIPs) that the bookings grid should request based
    /// on whatever the SfDataPager is currently showing. Without this the
    /// grid keeps the hard-coded 600 even after a chip-search narrows the
    /// set down to 3 rows, leaving a large empty block below the table.
    ///
    /// Formula:
    /// <c>header + (Math.Min(visible, pageSize) × row) + bottomBorder</c>.</summary>
    public double BookingsGridHeight
    {
        get
        {
            var shown = Math.Min(VisibleBookings.Count, BookingsGridPageSize);
            if (shown <= 0) shown = 1; // always leave the header visible, even on empty
            return BookingsGridHeaderHeight
                 + shown * BookingsGridRowHeight
                 + BookingsGridBottomBorder;
        }
    }

    /// <summary>Notifies <see cref="BookingsGridHeight"/> so the XAML
    /// <c>HeightRequest</c> rebinds. Called whenever the visible row count
    /// or page count changes.</summary>
    private void NotifyBookingsGridHeight()
        => OnPropertyChanged(nameof(BookingsGridHeight));

    /// <summary>Active debounce token. Replaced on every keystroke so the
    /// previous in-flight filter pass is cancelled.</summary>
    private CancellationTokenSource? _filterDebounce;
    /// <summary>Per-keystroke wait before applying the filter. Short
    /// enough to feel responsive but long enough to skip per-character
    /// layout thrash.</summary>
    private static readonly TimeSpan FilterDebounceDelay = TimeSpan.FromMilliseconds(150);

    /// <summary>Constructs the VM, wraps every master booking in a
    /// <see cref="SelectableBooking"/>, seeds the visible list and
    /// subscribes to <see cref="IHotelStore.BookingsChanged"/>.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public BookingsViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        AllBookings = new ObservableCollection<SelectableBooking>(
            _store.Bookings.Select(b => new SelectableBooking(b)));

        // Seed the visible list so the grid isn't empty on first render.
        _ = ApplyFiltersAsync();

        // Subscribe to the SHARED store. The same BookingsChanged event also
        // fires when the Add-Booking popup commits a row to the master
        // collection — we wrap the new row in a SelectableBooking, insert it
        // at the top of the visible list, raise every KPI property and ask
        // the pager to reset to page 1.
        _store.BookingsChanged += OnStoreBookingsChanged;
    }

    /// <summary>Public entry point — schedules the actual rebuild so a
    /// rapid typing burst only causes one filter pass + one pager reset,
    /// not one per character.</summary>
    /// <returns>A completed <see cref="Task"/> — the filter pass happens
    /// asynchronously on the dispatcher.</returns>
    private System.Threading.Tasks.Task ApplyFiltersAsync()
    {
        ScheduleApplyFilters();
        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>Recomputes <see cref="VisibleBookings"/> from
    /// <see cref="AllBookings"/> applying the search filter, then
    /// refreshes the dynamic-height binding and asks the pager to
    /// reset to page 1.</summary>
    private void ApplyFiltersCore()
    {
        IEnumerable<SelectableBooking> query = AllBookings;

        var needle = _searchText?.Trim() ?? string.Empty;
        if (needle.Length > 0)
        {
            query = query.Where(b =>
                ContainsIgnoreCase(b.Guest, needle) ||
                ContainsIgnoreCase(b.Room, needle) ||
                ContainsIgnoreCase(b.Reference, needle) ||
                ContainsIgnoreCase(b.RoomType, needle) ||
                ContainsIgnoreCase(b.Source, needle));
        }

        var filtered = query.ToList();

        VisibleBookings.Clear();
        foreach (var item in filtered)
            VisibleBookings.Add(item);

        OnPropertyChanged(nameof(ShowingRangeLabel));
        NotifyBookingsGridHeight();
        PagerResetRequested?.Invoke(this, EventArgs.Empty);
    }

    // ---------- Paging ----------
    /// <summary>Backing field for <see cref="CurrentPageIndex"/>.</summary>
    private int _currentPageIndex;
    /// <summary>Index of the page the SfDataPager is currently showing.
    /// Updated by <see cref="UpdatePaging"/> when the pager reports a
    /// page change.</summary>
    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        private set
        {
            if (_currentPageIndex == value)
                return;
            _currentPageIndex = value;
            OnPropertyChanged(nameof(CurrentPageIndex));
            OnPropertyChanged(nameof(ShowingRangeLabel));
            // The grid shows the currently paged slice; on the last short
            // page we may render fewer than PageSize rows.
            NotifyBookingsGridHeight();
        }
    }

    /// <summary>Backing field for <see cref="RowsPerPage"/>.</summary>
    private int _rowsPerPage = 10;
    /// <summary>Rows per page the pager is currently using.</summary>
    public int RowsPerPage
    {
        get => _rowsPerPage;
        private set
        {
            if (_rowsPerPage == value)
                return;
            _rowsPerPage = value;
            OnPropertyChanged(nameof(RowsPerPage));
            OnPropertyChanged(nameof(ShowingRangeLabel));
            NotifyBookingsGridHeight();
        }
    }

    /// <summary>Range label rendered under the pager (e.g.
    /// "Showing 1–10 of 24 records").</summary>
    public string ShowingRangeLabel
    {
        get
        {
            var pageSize = Math.Max(1, RowsPerPage);
            var pageIndex = Math.Max(0, CurrentPageIndex);
            var total = VisibleBookings.Count;
            var start = (pageIndex * pageSize) + 1;
            var end = Math.Min(total, start + pageSize - 1);
            if (total == 0)
                start = end = 0;
            return $"Showing {start}\u2013{end} of {total} records";
        }
    }

    /// <summary>Notifies the VM that the SfDataPager has reported a new
    /// page index / page size. Stores both on the VM and refreshes the
    /// dynamic-height binding.</summary>
    /// <param name="newPageIndex">The newly-selected page index.</param>
    /// <param name="pageSize">The newly-selected page size.</param>
    public void UpdatePaging(int newPageIndex, int pageSize)
    {
        if (RowsPerPage != pageSize)
            RowsPerPage = pageSize;

        CurrentPageIndex = newPageIndex;
    }

    // ---------- Debounce plumbing ----------
    /// <summary>Cancels any in-flight debounced filter pass and starts a
    /// new one. Called from <see cref="ApplyFiltersAsync"/>.</summary>
    private void ScheduleApplyFilters()
    {
        _filterDebounce?.Cancel();
        _filterDebounce?.Dispose();
        var cts = _filterDebounce = new CancellationTokenSource();
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(FilterDebounceDelay, token).ConfigureAwait(false);
                if (token.IsCancellationRequested)
                    return;

                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(ApplyFiltersCore).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { /* superseded */ }
        }, token);
    }

    /// <summary>Case-insensitive substring search helper.</summary>
    /// <param name="haystack">String to search. May be null or empty.</param>
    /// <param name="needle">Substring to look for. Must be non-null.</param>
    /// <returns><c>true</c> when <paramref name="haystack"/> contains
    /// <paramref name="needle"/>; otherwise <c>false</c>.</returns>
    private static bool ContainsIgnoreCase(string? haystack, string needle)
    {
        if (string.IsNullOrEmpty(haystack))
            return false;
        return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// HotelStore.BookingsChanged trampoline — wraps each newly added
    /// master booking into a SelectableBooking (if it isn't already in
    /// the list) and refreshes every KPI property. BookingsPage pops the
    /// Add-Booking dialog and calls <see cref="IHotelStore.AddBooking"/>,
    /// which fires this — the dashboard's KPI tiles update concurrently
    /// because DashboardViewModel subscribes to the same event.
    /// </summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="booking">The booking that was added or affected.</param>
    private void OnStoreBookingsChanged(object? sender, Booking booking)
    {
        var row = new SelectableBooking(booking);

        // Insert at top so the most recent booking is visible immediately
        // on page 1. Skip if a row for the same reference is already
        // present (idempotent on event re-delivery / refresh).
        if (AllBookings.Any(b => string.Equals(b.Reference, row.Reference, StringComparison.OrdinalIgnoreCase)))
            return;

        AllBookings.Insert(0, row);
        VisibleBookings.Insert(0, row);

        // KPI derivations depend on AllBookings so notify each one.
        OnPropertyChanged(nameof(TodaysArrivals));
        OnPropertyChanged(nameof(ArrivalsTotal));
        OnPropertyChanged(nameof(InHouseStays));
        OnPropertyChanged(nameof(InHouseDeltaLabel));
        OnPropertyChanged(nameof(InHouseDeltaDetail));
        OnPropertyChanged(nameof(FolioVolume));
        OnPropertyChanged(nameof(FolioDeltaLabel));
        OnPropertyChanged(nameof(FolioDeltaDetail));
        OnPropertyChanged(nameof(TotalRecordLabel));
        OnPropertyChanged(nameof(ShowingRangeLabel));
        NotifyBookingsGridHeight();

        PagerResetRequested?.Invoke(this, EventArgs.Empty);
    }
}
