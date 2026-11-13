using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HotelManagerSample;

namespace HotelManagerSample.Rooms;

/// <summary>
/// View-model for the mobile Rooms page. Reuses the live
/// <see cref="IHotelStore"/> so any housekeeping flips or new bookings
/// from elsewhere ripple here. The page binds <see cref="VisibleRooms"/>
/// directly to SfListView.
/// </summary>
public sealed class MobileRoomsViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with this object's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Chip label that disables the floor filter.</summary>
    public const string FilterAll        = "All";
    /// <summary>Chip label for rooms flagged "Available".</summary>
    public const string FilterAvailable  = "Available";
    /// <summary>Chip label for rooms flagged "Occupied".</summary>
    public const string FilterOccupied   = "Occupied";
    /// <summary>Chip label for rooms flagged "Reserved".</summary>
    public const string FilterReserved   = "Reserved";
    /// <summary>Chip label for rooms flagged "Maintenance".</summary>
    public const string FilterMaintenance= "Maintenance";

    /// <summary>Constructs the VM, seeds the chip collections from the
    /// master store and subscribes to <see cref="IHotelStore.RoomsChanged"/>
    /// / <see cref="IHotelStore.BookingsChanged"/>.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public MobileRoomsViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        ActiveStatusFilter = FilterAll;
        ActiveFloorFilter  = FilterAll;

        RebuildFloorChips();
        RebuildStatusChips();
        RebuildVisibleRooms();

        // Re-emit all derived values when bookings or rooms move.
        _store.RoomsChanged += OnStoreChanged;
        _store.BookingsChanged += OnStoreChanged;
    }

    // ==================================================================
    // ============================ Inputs ==============================
    // ==================================================================

    private string _searchText = string.Empty;
    /// <summary>Search-entry text. Setter re-runs
    /// <see cref="RebuildVisibleRooms"/> so the SfListView updates
    /// immediately.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value ?? string.Empty;
            OnPropertyChanged(nameof(SearchText));
            RebuildVisibleRooms();
        }
    }

    private string _activeFloorFilter = FilterAll;
    /// <summary>Active floor chip label, drives the projection in
    /// <see cref="RebuildVisibleRooms"/>.</summary>
    public string ActiveFloorFilter
    {
        get => _activeFloorFilter;
        set
        {
            if (_activeFloorFilter == value) return;
            _activeFloorFilter = value ?? FilterAll;
            OnPropertyChanged(nameof(ActiveFloorFilter));
            RebuildVisibleRooms();
        }
    }

    private string _activeStatusFilter = FilterAll;
    /// <summary>Active status chip label, drives the projection in
    /// <see cref="RebuildVisibleRooms"/>.</summary>
    public string ActiveStatusFilter
    {
        get => _activeStatusFilter;
        set
        {
            if (_activeStatusFilter == value) return;
            _activeStatusFilter = value ?? FilterAll;
            OnPropertyChanged(nameof(ActiveStatusFilter));
            RebuildVisibleRooms();
        }
    }

    // ==================================================================
    // =========================== Counts ===============================
    // ==================================================================

    /// <summary>Total room count from the master store.</summary>
    public int TotalRooms => _store.Rooms.Count;
    /// <summary>Count surfaced on the "All" chip — same as
    /// <see cref="TotalRooms"/>.</summary>
    public int AllCount => _store.Rooms.Count;
    /// <summary>Count of rooms flagged "Available".</summary>
    public int AvailableCount => _store.Rooms.Count(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase));
    /// <summary>Count of rooms flagged "Occupied".</summary>
    public int OccupiedCount => _store.Rooms.Count(r => string.Equals(r.Status, "Occupied", StringComparison.OrdinalIgnoreCase));
    /// <summary>Count of rooms flagged "Reserved".</summary>
    public int ReservedCount => _store.Rooms.Count(r => string.Equals(r.Status, "Reserved", StringComparison.OrdinalIgnoreCase));
    /// <summary>Count of rooms flagged "Maintenance".</summary>
    public int MaintenanceCount => _store.Rooms.Count(r => string.Equals(r.Status, "Maintenance", StringComparison.OrdinalIgnoreCase));

    // ==================================================================
    // =========================== Floor list ===========================
    // ==================================================================

    /// <summary>Floor chips the SfChipGroup binds against. Rebuilt on
    /// every <see cref="IHotelStore.RoomsChanged"/> delivery so the
    /// chips mirror the live inventory.</summary>
    public ObservableCollection<RoomFilterChip> FloorChips { get; } = new();

    /// <summary>Re-seeds <see cref="FloorChips"/> from the master
    /// store. Always starts with the "All" chip followed by one chip
    /// per distinct floor.</summary>
    private void RebuildFloorChips()
    {
        FloorChips.Clear();

        var floors = _store.Rooms
            .Select(r => r.Floor)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        FloorChips.Add(new RoomFilterChip(this, FilterAll, AllCount, isFloorChip: true));
        foreach (var floor in floors)
            FloorChips.Add(new RoomFilterChip(this, $"Floor {floor}", _store.Rooms.Count(r => r.Floor == floor), isFloorChip: true));
    }

    // ==================================================================
    // =========================== Status list ==========================
    // ==================================================================

    /// <summary>Status chips the SfChipGroup binds against. Rebuilt on
    /// every store change.</summary>
    public ObservableCollection<RoomFilterChip> StatusChips { get; } = new();

    /// <summary>Re-seeds <see cref="StatusChips"/> from the master
    /// store. Always starts with the "All" chip followed by one chip
    /// per housekeeping status.</summary>
    private void RebuildStatusChips()
    {
        StatusChips.Clear();
        StatusChips.Add(new RoomFilterChip(this, FilterAll,       AllCount));
        StatusChips.Add(new RoomFilterChip(this, FilterAvailable, AvailableCount));
        StatusChips.Add(new RoomFilterChip(this, FilterOccupied,  OccupiedCount));
        StatusChips.Add(new RoomFilterChip(this, FilterReserved,  ReservedCount));
        StatusChips.Add(new RoomFilterChip(this, FilterMaintenance, MaintenanceCount));
    }

    // ==================================================================
    // =========================== Rooms list ===========================
    // ==================================================================

    /// <summary>Filtered room projection that backs the SfListView.</summary>
    public ObservableCollection<Room> VisibleRooms { get; } = new();

    /// <summary>Recomputes <see cref="VisibleRooms"/> from the master
    /// store applying the floor / status / search filters and sorting
    /// by room number.</summary>
    private void RebuildVisibleRooms()
    {
        IEnumerable<Room> src = _store.Rooms;

        // Floor filter — "Floor N" maps to matching floor digit, "All" passes through.
        if (!string.Equals(_activeFloorFilter, FilterAll, StringComparison.OrdinalIgnoreCase))
        {
            var digit = _activeFloorFilter.Replace("Floor ", string.Empty).Trim();
            src = src.Where(r => string.Equals(r.Floor, digit, StringComparison.OrdinalIgnoreCase));
        }

        // Status filter.
        if (!string.Equals(_activeStatusFilter, FilterAll, StringComparison.OrdinalIgnoreCase))
            src = src.Where(r => string.Equals(r.Status, _activeStatusFilter, StringComparison.OrdinalIgnoreCase));

        // Search filter.
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var needle = _searchText.Trim();
            src = src.Where(r =>
                r.Number.Contains(needle, StringComparison.OrdinalIgnoreCase)
                || r.Type.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        VisibleRooms.Clear();
        foreach (var r in src.OrderBy(r => r.Number))
            VisibleRooms.Add(r);

        OnPropertyChanged(nameof(EmptyVisibleFilter));
    }

    /// <summary>True when the active filter combo produces zero rooms —
    /// drives the "no rooms match your filter" placeholder.</summary>
    public bool EmptyVisibleFilter => VisibleRooms.Count == 0;

    // ==================================================================
    // ======================= Store subscription =======================
    // ==================================================================

    /// <summary>Master-store callback. Rebuilds every chip and the
    /// visible room list so the page reflects the latest state.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="_">Either a <see cref="Room"/> or
    /// <see cref="Booking"/> payload — the rooms page reacts to both.</param>
    private void OnStoreChanged(object? sender, object? _) // either Room or Booking
    {
        // Reset the chips with fresh counts.
        RebuildFloorChips();
        RebuildStatusChips();
        RebuildVisibleRooms();
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Single room-filter chip exposed by <see cref="MobileRoomsViewModel"/>.
/// Same protocol as <see cref="BookingFilterChip"/>: each chip carries
/// its own count so the badge updates inline even before the page
/// refreshes. <see cref="IsFloorChip"/> lets the page decide which setter
/// (floor vs status) to drive.
/// </summary>
public sealed class RoomFilterChip : INotifyPropertyChanged
{
    private readonly MobileRoomsViewModel _owner;

    /// <summary>Constructs a chip owned by <paramref name="owner"/>
    /// with the given label, initial count, and chip-type flag.</summary>
    /// <param name="owner">Parent VM the chip queries for counts.</param>
    /// <param name="label">Chip text — also the segment key passed
    /// back to the owner.</param>
    /// <param name="initialCount">Initial badge count.</param>
    /// <param name="isFloorChip">When <c>true</c> the chip belongs to
    /// the floor row; otherwise to the status row.</param>
    public RoomFilterChip(MobileRoomsViewModel owner, string label, int initialCount, bool isFloorChip = false)
    {
        _owner = owner;
        Label = label;
        _count = initialCount;
        IsFloorChip = isFloorChip;
    }

    /// <summary>Segment key for this chip.</summary>
    public string Label { get; init; }
    /// <summary>True when the chip belongs to the floor row.</summary>
    public bool IsFloorChip { get; init; }
    /// <summary>Legacy alias kept for backwards compatibility with
    /// any XAML bindings that referenced the original name.</summary>
    public bool IsFloorFilter { get; init; }

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

    /// <summary>True when this chip is the currently active filter on
    /// the row it belongs to.</summary>
    public bool IsActive => IsFloorChip
        ? string.Equals(_owner.ActiveFloorFilter, Label, StringComparison.OrdinalIgnoreCase)
        : string.Equals(_owner.ActiveStatusFilter, Label, StringComparison.OrdinalIgnoreCase);

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

    /// <summary>Refreshes <see cref="Count"/> from parent VM based on
    /// this chip's label/type. Called on every store change.</summary>
    public void RefreshCount()
    {
        if (IsFloorChip)
        {
            if (Label == MobileRoomsViewModel.FilterAll)
                Count = _owner.AllCount;
            else
                Count = _owner.AllCount; // Floor chips display total room count, not the breakdown of that floor — kept simple for mobile.
        }
        else
        {
            Count = Label switch
            {
                MobileRoomsViewModel.FilterAll         => _owner.AllCount,
                MobileRoomsViewModel.FilterAvailable   => _owner.AvailableCount,
                MobileRoomsViewModel.FilterOccupied    => _owner.OccupiedCount,
                MobileRoomsViewModel.FilterReserved    => _owner.ReservedCount,
                MobileRoomsViewModel.FilterMaintenance => _owner.MaintenanceCount,
                _                                       => 0
            };
        }
        OnPropertyChanged(nameof(IsActive));
    }
}
