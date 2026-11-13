using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HotelManagerSample;

namespace HotelManagerSample.Bookings;

/// <summary>
/// View-model for the mobile bookings page. Wraps the master
/// <see cref="IHotelStore.Bookings"/> collection into a list of cards
/// that drive the page's SfListView. Exposes:
///   - VisibleBookings: filtered projection backing the SfListView
///   - SearchText / ActiveFilter: input bindings to drive filtering
///   - FilterChips: bound to SfChipGroup (All, Arriving Today, In House,
///     Departing, Checked Out)
///   - StatusFilterTitle / StatusFilterCounts: bound to the chip legend
/// </summary>
public sealed class MobileBookingsViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with this object's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Chip label that disables every filter (default landing chip).</summary>
    public const string FilterAll          = "All";
    /// <summary>Chip label for bookings whose check-in date is today.</summary>
    public const string FilterArrivingToday = "Arriving Today";
    /// <summary>Chip label for bookings currently in-house.</summary>
    public const string FilterInHouse       = "In House";
    /// <summary>Chip label for bookings whose check-out date is today.</summary>
    public const string FilterDeparting     = "Departing";
    /// <summary>Chip label for completed reservations.</summary>
    public const string FilterCheckedOut    = "Checked Out";

    /// <summary>Constructs the VM, seeds <see cref="VisibleBookings"/>
    /// from the master store and subscribes to
    /// <see cref="IHotelStore.BookingsChanged"/> so the new-booking
    /// wizard results show up here too.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public MobileBookingsViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        _activeFilter = FilterAll;

        // Seed VisibleBookings from the master list once.
        foreach (var b in _store.Bookings)
            VisibleBookings.Add(b);

        RebuildFilterChips();
        UpdateCounts();

        // Subscribe to the SHARED store. New bookings inserted via the
        // NewBooking wizard show up here, and any status flip that the
        // dashboard records (e.g. evening turnovers) updates the list.
        _store.BookingsChanged += OnBookingsChanged;
        _store.RoomsChanged    += OnRoomsChanged;
    }

    // ==================================================================
    // ============================ Inputs ==============================
    // ==================================================================

    private string _searchText = string.Empty;
    /// <summary>Text currently typed in the bookings search entry.
    /// Setter re-runs <see cref="ApplyFilters"/> so the visible list
    /// updates immediately.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            var normalized = value ?? string.Empty;
            if (_searchText == normalized) return;
            _searchText = normalized;
            OnPropertyChanged(nameof(SearchText));
            ApplyFilters();
        }
    }

    private string _activeFilter = FilterAll;
    /// <summary>Active chip label, drives the segment filter applied
    /// in <see cref="ApplyFilters"/>. Empty input is normalised to
    /// <see cref="FilterAll"/>.</summary>
    public string ActiveFilter
    {
        get => _activeFilter;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? FilterAll : value!;
            if (_activeFilter == normalized) return;
            _activeFilter = normalized;
            OnPropertyChanged(nameof(ActiveFilter));
            ApplyFilters();
        }
    }

    /// <summary>Filtered projection backing the SfListView.
    /// Rebuilt by <see cref="ApplyFilters"/> on every input change.</summary>
    public ObservableCollection<Booking> VisibleBookings { get; } = new();

    /// <summary>Chips bound to SfChipGroup. The list always starts with
    /// "All" and is rebuilt from the store whenever the booking set
    /// changes.</summary>
    public ObservableCollection<BookingFilterChip> FilterChips { get; } = new();

    /// <summary>Re-seeds <see cref="FilterChips"/> with the current
    /// per-filter counts. Called once from the constructor and any
    /// time the master store raises <see cref="IHotelStore.BookingsChanged"/>.</summary>
    private void RebuildFilterChips()
    {
        FilterChips.Clear();
        FilterChips.Add(new BookingFilterChip(this, FilterAll,         AllCount));
        FilterChips.Add(new BookingFilterChip(this, FilterArrivingToday, ArrivingTodayCount));
        FilterChips.Add(new BookingFilterChip(this, FilterInHouse,     InHouseCount));
        FilterChips.Add(new BookingFilterChip(this, FilterDeparting,   DepartingCount));
        FilterChips.Add(new BookingFilterChip(this, FilterCheckedOut,  CheckedOutCount));
    }

    // ==================================================================
    // =========================== Counts ==============================
    // ==================================================================

    /// <summary>Re-evaluates each chip's badge against the master
    /// store and notifies the count properties so XAML re-reads them.</summary>
    private void UpdateCounts()
    {
        // Recompute each chip on the fly so the small badge inside each
        // chip stays in sync with the master store.
        foreach (var chip in FilterChips)
            chip.RefreshCount();

        OnPropertyChanged(nameof(AllCount));
        OnPropertyChanged(nameof(ArrivingTodayCount));
        OnPropertyChanged(nameof(InHouseCount));
        OnPropertyChanged(nameof(DepartingCount));
        OnPropertyChanged(nameof(CheckedOutCount));
    }

    /// <summary>Total bookings in the master store (no filter).</summary>
    public int AllCount => _store.Bookings.Count;
    /// <summary>Bookings arriving today and not cancelled.</summary>
    public int ArrivingTodayCount => _store.Bookings.Count(b =>
        b.CheckIn.Date == HotelConfig.ReferenceToday.Date
        && !IsCancelled(b));
    /// <summary>Bookings currently in-house on the reference date.</summary>
    public int InHouseCount => _store.Bookings.Count(b =>
        b.CheckIn.Date <= HotelConfig.ReferenceToday.Date
        && b.CheckOut.Date > HotelConfig.ReferenceToday.Date
        && string.Equals(b.Status, "CHECKED IN", StringComparison.OrdinalIgnoreCase));
    /// <summary>Bookings departing today and not cancelled.</summary>
    public int DepartingCount => _store.Bookings.Count(b =>
        b.CheckOut.Date == HotelConfig.ReferenceToday.Date
        && !IsCancelled(b));
    /// <summary>Bookings whose lifecycle status is "Checked Out".</summary>
    public int CheckedOutCount => _store.Bookings.Count(b =>
        string.Equals(b.Status, "CHECKED OUT", StringComparison.OrdinalIgnoreCase));

    /// <summary>True when <paramref name="filter"/> matches the
    /// currently-active chip label.</summary>
    /// <param name="filter">Filter label to test. Compared case-insensitively.</param>
    /// <returns><c>true</c> when <paramref name="filter"/> is the
    /// active filter; otherwise <c>false</c>.</returns>
    public bool IsActive(string filter) => string.Equals(_activeFilter, filter, StringComparison.OrdinalIgnoreCase);

    /// <summary>True when the supplied booking's status equals
    /// "Cancelled" (case-insensitive). Used by every chip-count
    /// projection so cancelled rows stay hidden.</summary>
    /// <param name="b">Booking to test. Must not be null.</param>
    /// <returns><c>true</c> when <paramref name="b"/> is cancelled;
    /// otherwise <c>false</c>.</returns>
    private static bool IsCancelled(Booking b)
        => string.Equals(b.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase);

    // ==================================================================
    // ========================== Filtering =============================
    // ==================================================================

    /// <summary>Recomputes <see cref="VisibleBookings"/> from the
    /// master store, applying the active segment + search-text
    /// filters and sorting the result by check-in descending.</summary>
    private void ApplyFilters()
    {
        IEnumerable<Booking> q = _store.Bookings;

        // Search filter
        var needle = _searchText.Trim();
        if (needle.Length > 0)
        {
            q = q.Where(b =>
                ContainsIgnoreCase(b.Guest, needle) ||
                ContainsIgnoreCase(b.Room, needle) ||
                ContainsIgnoreCase(b.Reference, needle) ||
                ContainsIgnoreCase(b.RoomType, needle) ||
                ContainsIgnoreCase(b.Source, needle));
        }

        // Segment filter
        if (string.Equals(_activeFilter, FilterArrivingToday, StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(b => b.CheckIn.Date == HotelConfig.ReferenceToday.Date && !IsCancelled(b));
        }
        else if (string.Equals(_activeFilter, FilterInHouse, StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(b => b.CheckIn.Date <= HotelConfig.ReferenceToday.Date
                            && b.CheckOut.Date > HotelConfig.ReferenceToday.Date
                            && string.Equals(b.Status, "CHECKED IN", StringComparison.OrdinalIgnoreCase));
        }
        else if (string.Equals(_activeFilter, FilterDeparting, StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(b => b.CheckOut.Date == HotelConfig.ReferenceToday.Date && !IsCancelled(b));
        }
        else if (string.Equals(_activeFilter, FilterCheckedOut, StringComparison.OrdinalIgnoreCase))
        {
            q = q.Where(b => string.Equals(b.Status, "CHECKED OUT", StringComparison.OrdinalIgnoreCase));
        }

        VisibleBookings.Clear();
        foreach (var b in q.OrderByDescending(b => b.CheckIn))
            VisibleBookings.Add(b);
    }

    /// <summary>Case-insensitive substring search helper.</summary>
    /// <param name="haystack">String to search. May be null or empty.</param>
    /// <param name="needle">Substring to look for. Must be non-null.</param>
    /// <returns><c>true</c> when <paramref name="haystack"/> contains
    /// <paramref name="needle"/>; otherwise <c>false</c>.</returns>
    private static bool ContainsIgnoreCase(string? haystack, string needle)
        => !string.IsNullOrEmpty(haystack) && haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;

    /// <summary>Master-store callback. Refreshes every chip count
    /// and re-applies the active filters so the new row from the
    /// New-Booking wizard appears in <see cref="VisibleBookings"/>.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The booking that changed.</param>
    private void OnBookingsChanged(object? sender, Booking e)
    {
        UpdateCounts();
        ApplyFilters();
    }

    /// <summary>Master-store callback for room changes. The bookings
    /// page does not directly render rooms but the floor /
    /// arriving-today chip counts may shift when housekeeping flips
    /// a status, so we refresh them defensively.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The room that changed.</param>
    private void OnRoomsChanged(object? sender, Room e)
    {
        // No direct dependency on a room here, but the floor/arriving
        // counts may shift if housekeeping flips a room back from
        // maintenance — refresh them.
        UpdateCounts();
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed. Defaults
    /// to the caller member name.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// One filter chip the Bookings page binds against. BookingsFilterChip is
/// a property-populating wrapper that refreshes its own badge from the
/// relevant VM property whenever the parent notifies a count change.
/// </summary>
public sealed class BookingFilterChip : INotifyPropertyChanged
{
    private readonly MobileBookingsViewModel _owner;

    /// <summary>Constructs a chip owned by <paramref name="owner"/>
    /// with the given label and badge count.</summary>
    /// <param name="owner">Parent VM the chip queries for counts.</param>
    /// <param name="label">Chip text — also the segment key passed to
    /// <see cref="MobileBookingsViewModel.ActiveFilter"/>.</param>
    /// <param name="initialCount">Initial badge count.</param>
    public BookingFilterChip(MobileBookingsViewModel owner, string label, int initialCount)
    {
        _owner = owner;
        Label = label;
        _count = initialCount;
    }

    /// <summary>Segment key for this chip. Matches one of the
    /// <see cref="MobileBookingsViewModel.Filter*"/> constants.</summary>
    public string Label { get; init; }

    private int _count;
    /// <summary>Badge count displayed inside the chip.</summary>
    public int Count
    {
        get => _count;
        private set
        {
            if (_count == value) return;
            _count = value;
            OnPropertyChanged();
        }
    }

    /// <summary>True when this chip is the currently active filter.</summary>
    public bool IsActive => _owner.IsActive(Label);

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with this object's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Refreshes <see cref="Count"/> from the parent VM based
    /// on which chip we represent. Called by the parent whenever a
    /// booking change is reported.</summary>
    public void RefreshCount()
    {
        Count = Label switch
        {
            MobileBookingsViewModel.FilterAll           => _owner.AllCount,
            MobileBookingsViewModel.FilterArrivingToday => _owner.ArrivingTodayCount,
            MobileBookingsViewModel.FilterInHouse       => _owner.InHouseCount,
            MobileBookingsViewModel.FilterDeparting     => _owner.DepartingCount,
            MobileBookingsViewModel.FilterCheckedOut    => _owner.CheckedOutCount,
            _ => 0
        };
        // IsActive may have flipped since last call. Notify so bindings
        // update the active styling.
        OnPropertyChanged(nameof(IsActive));
    }
}
