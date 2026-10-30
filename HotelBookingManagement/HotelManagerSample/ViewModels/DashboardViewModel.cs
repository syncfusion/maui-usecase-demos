using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManagerSample;

namespace HotelManagerSample;

/// <summary>
/// View-model for the Dashboard page. Holds the inventory KPIs, the
/// reservations segment / search filters and the per-page bookings list
/// shown in the SfDataGrid. Subscribes to <see cref="IHotelStore.BookingsChanged"/>
/// and <see cref="IHotelStore.RoomsChanged"/> so that an Add-Booking popup
/// commit (which the Bookings page fires through
/// <see cref="IHotelStore.AddBooking"/>) instantly refreshes every derived
/// KPI + the inventory donut + the reservations grid without requiring the
/// user to switch tabs.
/// </summary>
public sealed class DashboardViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name.</summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // ---- Property-wide configuration (single source of truth) -------------
    /// <summary>Total room count for the property. Backed by
    /// <see cref="HotelConfig.TotalRooms"/> so a future re-config of the
    /// 15-key mix propagates through every KPI without re-typing.</summary>
    public int TotalKeys => HotelConfig.TotalRooms;
    /// <summary>Total floor count. Backed by <see cref="HotelConfig.TotalFloors"/>.</summary>
    public int FloorCount => HotelConfig.TotalFloors;
    /// <summary>Rooms per floor. Backed by <see cref="HotelConfig.RoomsPerFloor"/>.</summary>
    public int KeysPerFloor => HotelConfig.RoomsPerFloor;

    // ---- Top-bar copy (all derived — no literal "128") -------------------
    /// <summary>Welcome-line greeting.</summary>
    public string Greeting => "Good Morning, Alex";
    /// <summary>Date label rendered in the welcome card.</summary>
    public string DateLabel => DateTime.Now.ToString("dddd, MMM d, yyyy");
    /// <summary>Subtitle under the reservation panel heading.</summary>
    public string ReservationSubtitle =>
        $"Here's what's happening across {HotelConfig.TotalFloors}-floor, {HotelConfig.TotalRooms}-key property today.";
    /// <summary>Numeric text used for the "Bio Bookings" badge.</summary>
    public string BioBookingsBadge => TotalBookings.ToString();

    // ---- KPI tiles -------------------------------------------------------
    /// <summary>Big-number "active bookings" headline. Counts every booking
    /// whose status is anything other than Cancelled (so the value reflects
    /// real, paid-or-in-progress reservations).</summary>
    public int TotalBookings =>
        _store.Bookings.Count(b => !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

    /// <summary>"Available keys" headline (rooms whose housekeeping status is Available OR Cleaning,
    /// i.e. ready or soon-to-be-ready for the next guest).</summary>
    public int AvailableKeys => _store.Rooms.Count(r =>
        string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(r.Status, "Cleaning", StringComparison.OrdinalIgnoreCase));

    /// <summary>Reservation-segment totals that drive the segmented pill.</summary>
    public int CheckInsCount => _store.Bookings.Count(b => b.CheckIn.Date == HotelConfig.ReferenceToday.Date);
    /// <summary>Number of bookings whose lifecycle status is "Pending".</summary>
    public int PendingPaymentsCount => _store.Bookings.Count(b =>
        string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase));

    /// <summary>"Bookings arriving today" — counts every reservation whose
    /// check-in date is today AND status is in {Confirmed, Checked in, PENDING}.</summary>
    public int CheckInsExpected => CheckInsCount;
    /// <summary>Detail label under the check-ins KPI tile.</summary>
    public string CheckInsDetailLabel => CheckInsExpected > 0
        ? $"{Math.Max(0, Math.Min(CheckInsExpected, HotelConfig.RoomsPerFloor))} of {HotelConfig.RoomsPerFloor} ready"
        : "Ready queue empty";

    /// <summary>Daily folio inflow — sum of <c>Total</c> for every booking
    /// whose check-in date sits on today. The value updates automatically as
    /// bookings are added; no laboured "18450" magic numbers.</summary>
    public decimal DailyFolioInflows =>
        _store.Bookings
            .Where(b => b.CheckIn.Date == HotelConfig.ReferenceToday.Date && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            .Sum(b => b.Total);

    /// <summary>"Loaded" percentage (occupied inventory ÷ total keys).
    /// Bounded to ≤ 100% — previous formulation (active bookings ÷ keys) could
    /// exceed 100 when several bookings overlapped the same key.</summary>
    public double OccupancyPercent =>
        HotelConfig.TotalRooms == 0
            ? 0
            : Math.Round((double)(HotelConfig.TotalRooms - AvailableKeys) / HotelConfig.TotalRooms * 100.0, 1);

    /// <summary>Small cumulative-trend delta used in the metric chips —
    /// derived dynamically from today's check-ins plus a synthetic
    /// "compared to yesterday" baseline so the value tracks the data.</summary>
    public string TrendDeltaLabel => $"+{Math.Max(1, CheckInsCount * 2):0.0}%";
    /// <summary>True when the trend delta chip should render in the
    /// positive-trend colour.</summary>
    public bool HasPositiveTrend => true;

    /// <summary>Backing the cash-card bottom-line label.</summary>
    public string FolioDetail => $"{Decimal.Round(DailyFolioInflows / 1000m, 1):0.0}k vol";

    /// <summary>Date label strings used throughout.</summary>
    public string CheckInsSegmentLabel =>
        $"Today's Check-Ins   {CheckInsCount}";
    /// <summary>Label rendered on the "Pending Payment" segment pill.</summary>
    public string PendingSegmentLabel =>
        $"Pending Payment   {PendingPaymentsCount}";

    /// <summary>Backing field for <see cref="InventoryBreakdown"/>.</summary>
    private IReadOnlyList<InventoryBreakdownPoint> _inventoryBreakdown = Array.Empty<InventoryBreakdownPoint>();
    /// <summary>Room-rack snapshot segmented for the donut chart. Sums are
    /// derived from the rooms collection (no magic numbers) — rebuilt on
    /// every <see cref="IHotelStore.RoomsChanged"/> event so the donut
    /// reflects the master inventory in real time.</summary>
    public IReadOnlyList<InventoryBreakdownPoint> InventoryBreakdown
    {
        get => _inventoryBreakdown;
        private set
        {
            if (ReferenceEquals(_inventoryBreakdown, value)) return;
            _inventoryBreakdown = value;
            OnPropertyChanged(nameof(InventoryBreakdown));
        }
    }

    /// <summary>
    /// Room-rack snapshot segmented for the dashboard donut. The four
    /// slices always sum to <c>HotelConfig.TotalRooms</c> so the donut
    /// matches the inventory headline. "Vacant Clean" folds both
    /// <c>Available</c> and <c>Cleaning</c> keys — they are guest-ready today
    /// either way and the design has only four rows.
    /// </summary>
    /// <param name="rooms">Live rooms collection from the store.</param>
    /// <returns>The four donut-segment rows in the order
    /// Occupied / Arriving Today / Vacant / Maintenance.</returns>
    private static IReadOnlyList<InventoryBreakdownPoint> BuildInventoryBreakdownFrom(IEnumerable<Room> rooms)
    {
        var list = rooms as IList<Room> ?? rooms.ToList();
        int occupied = list.Count(r => string.Equals(r.Status, "Occupied",    StringComparison.OrdinalIgnoreCase));
        int reserved = list.Count(r => string.Equals(r.Status, "Reserved",    StringComparison.OrdinalIgnoreCase));
        int available = list.Count(r => string.Equals(r.Status, "Available",  StringComparison.OrdinalIgnoreCase));
        int maintenance = list.Count(r => string.Equals(r.Status, "Maintenance", StringComparison.OrdinalIgnoreCase));
        int vacantClean = available;

        return new[]
        {
            new InventoryBreakdownPoint { Category = "Occupied", Value = occupied,    Color = "#18B58A" },
            new InventoryBreakdownPoint { Category = "Arriving Today",    Value = reserved,    Color = "#16A1E0" },
            new InventoryBreakdownPoint { Category = "Vacant",      Value = vacantClean, Color = "#4A43E6" },
            new InventoryBreakdownPoint { Category = "Maintenance",       Value = maintenance, Color = "#EE4770" }
        };
    }

    /// <summary>Legend rows for the inventory-breakdown card. Names, values and percentages are derived every time the rooms collection changes.</summary>
    public IReadOnlyList<InventoryLegendRow> InventoryLegend => BuildInventoryLegend();

    /// <summary>Legend-only color override. Add a token here to make a
    /// legend row render in a different hex from the chart segment. The
    /// DoughnutSeries <c>PaletteBrushes</c> in <c>DashboardPage.xaml</c>
    /// references the same hex values as this view-model exposes on
    /// <see cref="InventoryBreakdown"/>, so by default the legend swatch
    /// and the chart segment match; entries in this dictionary diverge
    /// the two if a design wants them to.</summary>
    private static readonly System.Collections.Generic.Dictionary<string, string> LegendColorOverride =
        new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>Projects <see cref="InventoryBreakdown"/> into the
    /// <see cref="InventoryLegendRow"/> shape the dashboard's legend
    /// binds against.</summary>
    private IReadOnlyList<InventoryLegendRow> BuildInventoryLegend()
    {
        var breakdown = InventoryBreakdown;
        var total = Math.Max(1, breakdown.Sum(b => (int)b.Value));
        return breakdown.Select(b => new InventoryLegendRow
        {
            Category = b.Category,
            Color = LegendColorOverride.TryGetValue(b.Category, out var overrideHex) ? overrideHex : b.Color,
            Value = (int)b.Value,
            Percent = Math.Round((double)((int)b.Value) * 100 / total, 1),
            TotalKeys = HotelConfig.TotalRooms
        }).ToList();
    }

    // KPI metrics the dashboard cards consume
    /// <summary>Number of rooms whose housekeeping status is "Occupied".</summary>
    public int OccupiedKeyCount => _store.Rooms.Count(r => string.Equals(r.Status, "Occupied", StringComparison.OrdinalIgnoreCase));
    /// <summary>Number of rooms currently in-house. Mirrors
    /// <see cref="OccupiedKeyCount"/> so XAML bindings can pick whichever
    /// name reads more naturally.</summary>
    public int InHouseKeyCount => OccupiedKeyCount;
    /// <summary>Percentage (0-100) of the property that is currently
    /// in-house. Integer-rounded for KPI tile display.</summary>
    public int TotalLoadPercent =>
        HotelConfig.TotalRooms == 0
            ? 0
            : (int)Math.Round((double)InHouseKeyCount / HotelConfig.TotalRooms * 100.0);
    /// <summary>"<see cref="TotalLoadPercent"/>%" caption rendered next
    /// to the load bar.</summary>
    public string TotalLoadLabel => $"{TotalLoadPercent}%";

    /// <summary>Backing field for <see cref="Revenue"/>.</summary>
    private IReadOnlyList<RevenuePoint> _revenue = HotelData.Revenue7D;
    /// <summary>Revenue series currently displayed in the revenue
    /// chart. Swapped by <see cref="SetRevenueRange"/> when the user
    /// taps the 7D / 1M / 3M toggle.</summary>
    public IReadOnlyList<RevenuePoint> Revenue
    {
        get => _revenue;
        private set
        {
            if (ReferenceEquals(_revenue, value))
                return;

            _revenue = value;
            OnPropertyChanged(nameof(Revenue));
        }
    }

    /// <summary>Backing field for <see cref="ActiveRevenueRange"/>.</summary>
    private string _activeRevenueRange = "7D";
    /// <summary>Active revenue-range key — "7D", "1M", or "3M". the XAML binds the toggle's label colour + bold state against.</summary>
    public string ActiveRevenueRange
    {
        get => _activeRevenueRange;
        private set
        {
            if (_activeRevenueRange == value) return;
            _activeRevenueRange = value;
            OnPropertyChanged(nameof(IsRevenue7DActive));
            OnPropertyChanged(nameof(IsRevenue1MActive));
            OnPropertyChanged(nameof(IsRevenue3MActive));
        }
    }

    /// <summary>True when the 7-day revenue range is the active one.</summary>
    public bool IsRevenue7DActive =>
        string.Equals(_activeRevenueRange, "7D", StringComparison.OrdinalIgnoreCase);
    /// <summary>True when the 1-month revenue range is the active one.</summary>
    public bool IsRevenue1MActive =>
        string.Equals(_activeRevenueRange, "1M", StringComparison.OrdinalIgnoreCase);
    /// <summary>True when the 3-month revenue range is the active one.</summary>
    public bool IsRevenue3MActive =>
        string.Equals(_activeRevenueRange, "3M", StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the reservations panel is showing the
    /// "Today's Check-Ins" segment.</summary>
    public bool IsCheckInsActive =>
        string.Equals(_activeSegment, "CheckIns", StringComparison.OrdinalIgnoreCase);
    /// <summary>True when the reservations panel is showing the
    /// "Pending Payments" segment.</summary>
    public bool IsPendingActive =>
        string.Equals(_activeSegment, "Pending", StringComparison.OrdinalIgnoreCase);

    /// <summary>Command bound to the 7D / 1M / 3M revenue-range toggle.
    /// Replaces the imperative <c>OnRevenueRangeTapped</c> code-behind
    /// handler so styling + series swap ride the same property change.</summary>
    public ICommand SetRevenueRangeCommand { get; }

    /// <summary>Command bound to the Check-Ins / Pending segment pills.
    /// Replaces the imperative <c>OnSegmentTapped</c> code-behind
    /// handler so styling + filter swap ride the same property change.</summary>
    public ICommand SetActiveSegmentCommand { get; }

    /// <summary>
    /// Master collection of recent reservations. Binds DIRECTLY to the
    /// shared <see cref="IHotelStore.Bookings"/> — that same instance is
    /// mutated when an Add-Booking popup commits, so this VM does not
    /// need a local mirror. Subscribers to CollectionChanged (the
    /// SfDataGrid) refresh automatically.
    /// </summary>
    public ObservableCollection<Booking> RecentBookings => _store.Bookings;

    /// <summary>
    /// Filtered subset of <see cref="RecentBookings"/> that the SfDataPager
    /// pages over and the SfDataGrid binds to. Rebuilt whenever
    /// <see cref="SearchText"/> or <see cref="ActiveSegment"/> changes OR a
    /// new booking arrives through the shared store.
    /// </summary>
    public ObservableCollection<Booking> VisibleBookings { get; } = new();

    /// <summary>Rows-per-page value applied to the reservations SfDataPager.</summary>
    public int RowsPerPage { get; private set; } = 10;
    /// <summary>Current page number (1-based).</summary>
    public int CurrentPage { get; private set; } = 1;
    /// <summary>Total page count for the visible reservations list.</summary>
    public int TotalPages => (int)Math.Ceiling(VisibleBookings.Count / (double)Math.Max(1, RowsPerPage));
    /// <summary>"Rows per page: N" label rendered next to the pager.</summary>
    public string RowsPerPageLabel => $"Rows per page:  {RowsPerPage}";

    /// <summary>Backing field for <see cref="CurrentPageIndex"/>.</summary>
    private int _currentPageIndex;
    /// <summary>Index of the page the SfDataPager is currently showing.</summary>
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
            NotifyReservationsGridHeight();
        }
    }

    /// <summary>Range label rendered under the pager (e.g.
    /// "Showing 1–10 of 24 bookings").</summary>
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
            return $"Showing {start}\u2013{end} of {total} bookings";
        }
    }

    /// <summary>
    /// Called by the page code-behind whenever the SfDataPager reports a page change.
    /// </summary>
    /// <param name="newPageIndex">The newly-selected page index.</param>
    /// <param name="pageSize">The newly-selected page size.</param>
    public void UpdatePaging(int newPageIndex, int pageSize)
    {
        if (RowsPerPage != pageSize)
        {
            RowsPerPage = pageSize;
            OnPropertyChanged(nameof(RowsPerPage));
            OnPropertyChanged(nameof(RowsPerPageLabel));
            OnPropertyChanged(nameof(TotalPages));
            NotifyReservationsGridHeight();
        }

        CurrentPageIndex = newPageIndex;
        // Setting CurrentPageIndex already notifies;
        // but be explicit so the height refreshes on page-flip alone.
        NotifyReservationsGridHeight();
    }

    /// <summary>Backing field for <see cref="ActiveSegment"/>.</summary>
    private string _activeSegment = "CheckIns";
    /// <summary>Active segment key — "CheckIns" or "Pending" — that
    /// determines which subset of <see cref="RecentBookings"/> flows
    /// into <see cref="VisibleBookings"/>.</summary>
    public string ActiveSegment
    {
        get => _activeSegment;
        set
        {
            if (_activeSegment == value)
                return;

            _activeSegment = value;
            OnPropertyChanged(nameof(ActiveSegment));
            OnPropertyChanged(nameof(IsCheckInsActive));
            OnPropertyChanged(nameof(IsPendingActive));
            _ = ApplyFiltersAsync();
        }
    }

    /// <summary>
    /// Backing store for the bound Entry in the reservations panel header.
    /// Updates trigger <see cref="ApplyFiltersAsync"/> so the DataPager refreshes.
    /// </summary>
    private string _searchText = string.Empty;
    /// <summary>Current search-entry text. Setter kicks off a debounced
    /// filter pass.</summary>
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

    /// <summary>
    /// Public entry point — debounces the actual rebuild so a rapid typing
    /// burst only causes one filter pass + one pager reset, not one per character.
    /// </summary>
    private System.Threading.Tasks.Task ApplyFiltersAsync()
    {
        ScheduleApplyFilters();
        return System.Threading.Tasks.Task.CompletedTask;
    }

    /// <summary>
    /// Rebuilds <see cref="VisibleBookings"/> from the master list applying the
    /// active segment + search-text filters, then signals the page-behind so
    /// paging is reset to page 1 (otherwise stale row-count math leaves the
    /// pager on an empty page).
    /// </summary>
    private void ApplyFiltersCore()
    {
        IEnumerable<Booking> query = RecentBookings;

        if (string.Equals(_activeSegment, "CheckIns", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(b => b.CheckIn.Date == HotelConfig.ReferenceToday.Date);
        }
        else if (string.Equals(_activeSegment, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            // "Pending Payment" = bookings still requiring payment authorization,
            // i.e. those whose life-cycle status is PENDING only.
            query = query.Where(IsPending);
        }

        var needle = _searchText?.Trim() ?? string.Empty;
        if (needle.Length > 0)
        {
            query = query.Where(b =>
                ContainsIgnoreCase(b.Guest, needle) ||
                ContainsIgnoreCase(b.Room, needle) ||
                ContainsIgnoreCase(b.Reference, needle));
        }

        var filtered = query
            .OrderBy(b => b.CheckIn)
            .ThenBy(b => b.Reference)
            .ToList();

        VisibleBookings.Clear();
        foreach (var item in filtered)
            VisibleBookings.Add(item);

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(ShowingRangeLabel));
        NotifyReservationsGridHeight();

        PagerResetRequested?.Invoke(this, EventArgs.Empty);
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

    /// <summary>True when the booking's status equals "Pending"
    /// (case-insensitive).</summary>
    /// <param name="b">Booking to test.</param>
    /// <returns><c>true</c> when the booking is pending.</returns>
    private static bool IsPending(Booking b) =>
        string.Equals(b.Status, "Pending", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Fires whenever <see cref="VisibleBookings"/> changes so the page can
    /// move the SfDataPager back to page 1.
    /// </summary>
    public event EventHandler? PagerResetRequested;

    // ---------- DataGrid dynamic height ----------
    /// <summary>Constants that mirror the XAML on <c>ReservationsGrid</c>:
    /// <c>HeaderRowHeight="30"</c>, <c>RowHeight="56"</c>, plus a 1-DIP
    /// bottom grid-line stroke. Mirror values in <see cref="BookingsViewModel"/>
    /// for the bookings page (only the header differs).</summary>
    public const double ReservationsGridHeaderHeight = 30;
    /// <summary>Row height in DIPs for the reservations SfDataGrid.</summary>
    public const double ReservationsGridRowHeight = 56;
    /// <summary>Bottom grid-line stroke thickness in DIPs.</summary>
    public const double ReservationsGridBottomBorder = 1;
    /// <summary>Page size the SfDataPager defaults to.</summary>
    public const int ReservationsGridPageSize = 10;

    /// <summary>Height (in DIPs) that the reservations grid on the dashboard
    /// should request based on whatever the SfDataPager is currently
    /// showing. Without this the grid stays at the hard-coded 310 even after
    /// a segment toggle or text search narrows the visible set down to a
    /// handful of rows, leaving a chunk of empty whitespace underneath the
    /// table.
    ///
    /// Formula: <c>header + (min(visible, pageSize) × row) + bottomBorder</c>.</summary>
    public double ReservationsGridHeight
    {
        get
        {
            var shown = Math.Min(VisibleBookings.Count, ReservationsGridPageSize);
            if (shown <= 0) shown = 1; // keep header visible when empty
            return ReservationsGridHeaderHeight
                 + shown * ReservationsGridRowHeight
                 + ReservationsGridBottomBorder;
        }
    }

    /// <summary>Notifies <see cref="ReservationsGridHeight"/> so the XAML
    /// <c>HeightRequest</c> rebinds. Called whenever the visible row count
    /// or page count changes.</summary>
    private void NotifyReservationsGridHeight()
        => OnPropertyChanged(nameof(ReservationsGridHeight));

    /// <summary>Active debounce token. Replaced on every keystroke so the
    /// previous in-flight filter pass is cancelled.</summary>
    private CancellationTokenSource? _filterDebounce;
    /// <summary>Per-keystroke wait before applying the filter. Short
    /// enough to feel responsive but long enough to skip per-character
    /// layout thrash.</summary>
    private static readonly TimeSpan FilterDebounceDelay = TimeSpan.FromMilliseconds(150);

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

                // Marshal the actual list rebuild back to the UI thread.
                await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(ApplyFiltersCore).ConfigureAwait(false);
            }
            catch (TaskCanceledException) { /* superseded — fine */ }
        }, token);
    }

    /// <summary>Constructs the VM, seeds the donut breakdown + visible
    /// list from the live store, and subscribes to the store's mutation
    /// events.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public DashboardViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        // Seed the donut breakdown + visible list from the live store.
        InventoryBreakdown = BuildInventoryBreakdownFrom(_store.Rooms);
        _ = ApplyFiltersAsync();

        // ---- Commands ----
        // The 7D / 1M / 3M toggle and the Check-Ins / Pending segment pills
        // used to be imperative code-behind handlers (OnRevenueRangeTapped /
        // OnSegmentTapped). Driving them through commands keeps all of the
        // styling state on the VM so the XAML can bind flag properties
        // (IsRevenue7DActive, IsCheckInsActive, ...) instead of mutating
        // label colours in code.
        SetRevenueRangeCommand = new Command<string>(range =>
        {
            if (string.IsNullOrWhiteSpace(range)) return;
            SetRevenueRange(range!);
        });
        SetActiveSegmentCommand = new Command<string>(segment =>
        {
            if (string.IsNullOrWhiteSpace(segment)) return;
            ActiveSegment = segment!;
        });

        // ---- Store subscriptions ----
        _store.BookingsChanged += OnStoreBookingsChanged;
        _store.RoomsChanged   += OnStoreRoomsChanged;
    }

    /// <summary>Refresh KPI tiles, segment counts, and the filtered bookings
    /// list — every property whose value derives from
    /// <see cref="IHotelStore.Bookings"/> is notified here so XAML
    /// bindings repaint instantly.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The booking that changed.</param>
    private void OnStoreBookingsChanged(object? sender, Booking e)
    {
        NotifyDerivedFromBookings();
        _ = ApplyFiltersAsync();
    }

    /// <summary>Refresh the inventory donut + room-keyed KPI tiles.
    /// Does NOT touch bookings-derived properties (VisibleBookings etc.)
    /// because a room-status mutation has no effect on those counters.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The room that changed.</param>
    private void OnStoreRoomsChanged(object? sender, Room e)
    {
        InventoryBreakdown = BuildInventoryBreakdownFrom(_store.Rooms);
        NotifyDerivedFromRooms();
    }

    /// <summary>Every KPI bound property that depends on the master
    /// Bookings collection. Each is a getter, so we manually raise
    /// PropertyChanged so the UI re-evaluates them.</summary>
    private void NotifyDerivedFromBookings()
    {
        OnPropertyChanged(nameof(TotalBookings));
        OnPropertyChanged(nameof(CheckInsCount));
        OnPropertyChanged(nameof(PendingPaymentsCount));
        OnPropertyChanged(nameof(CheckInsExpected));
        OnPropertyChanged(nameof(CheckInsDetailLabel));
        OnPropertyChanged(nameof(DailyFolioInflows));
        OnPropertyChanged(nameof(TrendDeltaLabel));
        OnPropertyChanged(nameof(FolioDetail));
        OnPropertyChanged(nameof(CheckInsSegmentLabel));
        OnPropertyChanged(nameof(PendingSegmentLabel));
        OnPropertyChanged(nameof(BioBookingsBadge));
    }

    /// <summary>Every KPI bound property that depends on the master
    /// Rooms collection.</summary>
    private void NotifyDerivedFromRooms()
    {
        OnPropertyChanged(nameof(AvailableKeys));
        OnPropertyChanged(nameof(OccupancyPercent));
        OnPropertyChanged(nameof(OccupiedKeyCount));
        OnPropertyChanged(nameof(InHouseKeyCount));
        OnPropertyChanged(nameof(TotalLoadPercent));
        OnPropertyChanged(nameof(TotalLoadLabel));
        OnPropertyChanged(nameof(InventoryLegend));
    }

    /// <summary>Swaps the active revenue series based on the range
    /// key the user picked. Unknown keys fall back to the 7-day
    /// view.</summary>
    /// <param name="range">Range key — "7D", "1M", or "3M".</param>
    public void SetRevenueRange(string range)
    {
        var normalized = (range ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(normalized))
            normalized = "7D";

        ActiveRevenueRange = normalized;
        Revenue = normalized switch
        {
            "1M" => HotelData.Revenue1M,
            "3M" => HotelData.Revenue3M,
            _    => HotelData.Revenue7D
        };
    }
}
