using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HotelManagerSample;

namespace HotelManagerSample;

/// <summary>
/// View-model for the Rooms page. Owns one rich <see cref="RoomDetailsModel"/>
/// per master 15-key inventory entry so the cards / table view / KPIs are
/// always a projection of the live <see cref="IHotelStore.Rooms"/> collection.
/// Subscribes to <see cref="IHotelStore.BookingsChanged"/> and
/// <see cref="IHotelStore.RoomsChanged"/> so every page shares the same
/// source of truth: when the Add-Booking popup commits a fresh reservation
/// the dashboard KPI tiles, the rooms board inventory stats and the
/// BookingsPage grid all refresh from a single event delivered to a single
/// store.
/// </summary>
public sealed class RoomsViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Total floor count. Backed by <see cref="HotelConfig.TotalFloors"/>.</summary>
    public int FloorCount => HotelConfig.TotalFloors;
    /// <summary>Rooms per floor. Backed by <see cref="HotelConfig.RoomsPerFloor"/>.</summary>
    public int KeysPerFloor => HotelConfig.RoomsPerFloor;
    /// <summary>Total room count. Backed by <see cref="HotelConfig.TotalRooms"/>.</summary>
    public int TotalKeys => HotelConfig.TotalRooms;

    /// <summary>Backing field for <see cref="SelectedFloorFilter"/>.</summary>
    private FloorFilterItem? _selectedFloorFilter;
    /// <summary>Active floor-filter chip. Setter re-runs the visible-room
    /// projection.</summary>
    public FloorFilterItem? SelectedFloorFilter
    {
        get => _selectedFloorFilter;
        set
        {
            if (_selectedFloorFilter == value) return;
            _selectedFloorFilter = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    /// <summary>Backing field for <see cref="SelectedStatusFilter"/>.</summary>
    private StatusFilterItem? _selectedStatusFilter;
    /// <summary>Active status-filter chip. Setter re-runs the visible-room
    /// projection.</summary>
    public StatusFilterItem? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (_selectedStatusFilter == value) return;
            _selectedStatusFilter = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    /// <summary>Backing field for <see cref="SearchText"/>.</summary>
    private string _searchText = string.Empty;
    /// <summary>Current search-entry text. Setter re-runs the
    /// visible-room projection immediately.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value) return;
            _searchText = value ?? string.Empty;
            OnPropertyChanged();
            ApplyFilter();
        }
    }

    /// <summary>Backing field for <see cref="ActiveView"/>.</summary>
    private string _activeView = "Grid";
    /// <summary>"Grid" or "Table".</summary>
    public string ActiveView
    {
        get => _activeView;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "Grid" : value!;
            if (_activeView == normalized) return;
            _activeView = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsGridView));
            OnPropertyChanged(nameof(IsTableView));
        }
    }

    /// <summary>True when the rooms page should render the grid card view.</summary>
    public bool IsGridView => string.Equals(_activeView, "Grid", StringComparison.OrdinalIgnoreCase);
    /// <summary>True when the rooms page should render the table view.</summary>
    public bool IsTableView => string.Equals(_activeView, "Table", StringComparison.OrdinalIgnoreCase);

    /// <summary>Floor-filter chips bound to the SfChipGroup.</summary>
    public ObservableCollection<FloorFilterItem> FloorFilters { get; } = new();
    /// <summary>Status-filter chips bound to the SfChipGroup.</summary>
    public ObservableCollection<StatusFilterItem> StatusFilters { get; } = new();
    /// <summary>KPI strip rows derived from the rooms collection.</summary>
    public ObservableCollection<RoomInventoryStat> InventoryStats { get; } = new();
    /// <summary>Rich room models (one per master inventory entry).</summary>
    public ObservableCollection<RoomDetailsModel> Rooms { get; } = new();
    /// <summary>Filtered subset backing the grid card view.</summary>
    public ObservableCollection<RoomDetailsModel> FilteredRooms { get; } = new();
    /// <summary>Filtered subset backing the table view.</summary>
    public ObservableCollection<RoomDetailsModel> TableRooms { get; } = new();

    /// <summary>Constants that mirror the XAML on <c>RoomsTable</c>:
    /// <c>HeaderRowHeight="32"</c>, <c>RowHeight="56"</c>, and a 1-DIP
    /// bottom grid-line stroke. Living on the VM keeps the height math
    /// in the same file as the row-count projection so the two cannot
    /// drift apart.</summary>
    public const double RoomsGridHeaderHeight = 32;
    /// <summary>Row height in DIPs for the rooms SfDataGrid.</summary>
    public const double RoomsGridRowHeight = 56;
    /// <summary>Bottom grid-line stroke thickness in DIPs.</summary>
    public const double RoomsGridBottomBorder = 1;

    /// <summary>Height (in DIPs) that the rooms SfDataGrid should request
    /// based on the visible row count. Always renders at least one row
    /// so the column header stays visible on an empty result.</summary>
    public double GridHeight
    {
        get
        {
            var shown = Math.Max(1, TableRooms.Count);
            return RoomsGridHeaderHeight + shown * RoomsGridRowHeight + RoomsGridBottomBorder;
        }
    }

    /// <summary>Command bound to the Grid / Table view toggle.</summary>
    public ICommand SetActiveViewCommand { get; }

    /// <summary>Constructs the VM, seeds every bound collection from the
    /// master store and subscribes to the store's mutation events.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public RoomsViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        // Initial seed: derive a rich model for every master room. After
        // this, the master collection is the only source of truth — every
        // change ripples through the store's events into this VM.
        foreach (var r in _store.Rooms)
            Rooms.Add(BuildRichRoomFromHotelData(r));

        BuildStats();
        BuildFloorFilters();
        BuildStatusFilters();
        SelectedFloorFilter = FloorFilters.FirstOrDefault();
        SelectedStatusFilter = StatusFilters.FirstOrDefault();
        ApplyFilter();

        // Subscribe to the SHARED store. Both events mean "the master
        // state changed in a way that this VM cares about" — a booking was
        // added (which flipped a room to Reserved), or a room's status
        // was mutated directly (housekeeping turnovers, etc). One handler
        // rebuilds the affected row + refreshes derived collections.
        _store.BookingsChanged += OnStoreDataChanged;
        _store.RoomsChanged   += OnRoomChanged;

        SetActiveViewCommand = new Command<string>(view =>
        {
            if (string.IsNullOrWhiteSpace(view)) return;
            ActiveView = view!;
        });
    }

    /// <summary>
    /// <see cref="IHotelStore.BookingsChanged"/> handler — rebuilds the
    /// matching rich model so the visible tile (and KPI counts) reflects
    /// the change without resetting the user's scroll/filter.
    /// </summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The booking that changed.</param>
    private void OnStoreDataChanged(object? sender, Booking e)
    {
        var roomNumber = e.Room.Replace("Room ", string.Empty).Trim();
        var masterRoom = _store.Rooms.FirstOrDefault(r =>
            string.Equals(r.Number, roomNumber, StringComparison.OrdinalIgnoreCase));
        if (masterRoom is null) return;

        ReplaceRichRoomForMaster(masterRoom);
        BuildStats();
        BuildFloorFilters();
        BuildStatusFilters();
        ApplyFilter();
        OnPropertyChanged(nameof(TotalRooms));
        OnPropertyChanged(nameof(VisibleCountLabel));
        NotifyGridHeight();
    }

    /// <summary>
    /// RoomsChanged handler — fires whenever any room's status is mutated.
    /// Rebuilds the matching rich model so the visible tile (and KPI counts)
    /// reflects the change without resetting the user's scroll/filter.
    /// </summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The room that changed.</param>
    private void OnRoomChanged(object? sender, Room e)
    {
        ReplaceRichRoomForMaster(e);
        BuildStats();
        BuildFloorFilters();
        BuildStatusFilters();
        ApplyFilter();
        OnPropertyChanged(nameof(TotalRooms));
        OnPropertyChanged(nameof(VisibleCountLabel));
        NotifyGridHeight();
    }

    /// <summary>Replaces (in-place) the rich model backing the supplied
    /// master <see cref="Room"/> without disturbing other rows' references
    /// — important so any visible cards bound to a specific room do not
    /// lose their scroll position or attached event handlers.</summary>
    /// <param name="masterRoom">Master <see cref="Room"/> whose rich
    /// model needs to be rebuilt.</param>
    private void ReplaceRichRoomForMaster(Room masterRoom)
    {
        var rebuilt = BuildRichRoomFromHotelData(masterRoom);
        var existing = Rooms.FirstOrDefault(r =>
            string.Equals(r.Number, masterRoom.Number, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            Rooms.Add(rebuilt);
            return;
        }

        var idx = Rooms.IndexOf(existing);
        if (idx >= 0) Rooms[idx] = rebuilt;
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the
    /// <see cref="GridHeight"/> derived property so the XAML
    /// <c>HeightRequest</c> binding re-evaluates whenever the
    /// visible row count changes.</summary>
    private void NotifyGridHeight() => OnPropertyChanged(nameof(GridHeight));

    /// <summary>Count of rooms currently held in the rich collection.</summary>
    public int TotalRooms => Rooms.Count;
    /// <summary>"Showing X of Y rooms" caption rendered above the rooms table.</summary>
    public string VisibleCountLabel => $"Showing {FilteredRooms.Count} of {Rooms.Count} rooms";

    /// <summary>Re-seeds <see cref="InventoryStats"/> with the KPI-strip
    /// rows. Called on every store change.</summary>
    private void BuildStats()
    {
        InventoryStats.Clear();
        var total = Math.Max(1, Rooms.Count);

        InventoryStats.Add(new RoomInventoryStat
        {
            Label = "Total Inventory",
            Value = Rooms.Count.ToString(),
            Percent = 100,
            AccentColor = "#17243A",
            IconBackgroundColor = "#E5EBF6",
            IconForegroundColor = "#17243A",
            ImageIcon = "inventory.png",
            PercentLabel = "100% cap"
        });

        AddStat(RoomCleaningStatus.Available,   "Available",   "#16A1E0", "Check",     "#DDF1F8", "#0F77A8", "available.png",   "{0:0.0}%");
        AddStat(RoomCleaningStatus.Occupied,    "Occupied",    "#5146E5", "Bed",       "#E8E5FE", "#3B30B5", "occupied.png",    "{0:0.0}%");
        AddStat(RoomCleaningStatus.Reserved,    "Reserved",    "#1F8FA8", "Calendar",  "#DDEFF3", "#166475", "reserved.png",    "{0:0.0}%");
        AddStat(RoomCleaningStatus.Maintenance, "Maintenance", "#E9446B", "Wrench",    "#FBD9E4", "#A6204B", "maintainance.png", "{0:0.0}%");
    }

    /// <summary>Adds one KPI-strip row to <see cref="InventoryStats"/>
    /// for the supplied <paramref name="status"/>.</summary>
    /// <param name="status">Room status the row is counting.</param>
    /// <param name="label">Label rendered on the row.</param>
    /// <param name="accent">Hex accent colour.</param>
    /// <param name="icon">Material icon kind.</param>
    /// <param name="iconBg">Hex background colour for the icon tile.</param>
    /// <param name="iconFg">Hex foreground colour for the icon glyph.</param>
    /// <param name="imageIcon">Image-asset file name rendered on the row.</param>
    /// <param name="percentFormat">Composite format string used to
    /// render the percentage label.</param>
    private void AddStat(RoomCleaningStatus status, string label, string accent, string icon, string iconBg, string iconFg, string imageIcon, string percentFormat)
    {
        var count = Rooms.Count(r => r.Status == status);
        var pct = Math.Round(count / (double)Math.Max(1, Rooms.Count) * 100.0, 1);
        InventoryStats.Add(new RoomInventoryStat
        {
            Label = label,
            Value = count.ToString(),
            Percent = pct,
            PercentLabel = string.Format(CultureInfo.InvariantCulture, percentFormat, pct),
            AccentColor = accent,
            IconKind = icon,
            IconBackgroundColor = iconBg,
            IconForegroundColor = iconFg,
            ImageIcon = imageIcon
        });
    }

    /// <summary>Re-seeds <see cref="FloorFilters"/> with the
    /// "All Floors" chip followed by one chip per distinct floor in the
    /// master inventory.</summary>
    private void BuildFloorFilters()
    {
        FloorFilters.Clear();

        FloorFilters.Add(new FloorFilterItem
        {
            Label = "All Floors",
            IsAll = true
        });

        foreach (var floor in Rooms
            .Select(r => r.Floor)
            .Distinct()
            .OrderBy(f => f))
        {
            FloorFilters.Add(new FloorFilterItem
            {
                Label = $"Floor {floor}",
                IsAll = false
            });
        }
    }

    /// <summary>Re-seeds <see cref="StatusFilters"/> with the
    /// "All" chip followed by one chip per <see cref="RoomCleaningStatus"/>
    /// value.</summary>
    private void BuildStatusFilters()
    {
        StatusFilters.Clear();
        StatusFilters.Add(new StatusFilterItem
        {
            Label = "All",
            Count = Rooms.Count,
            Status = RoomCleaningStatus.Available,
            IsAll = true,
            StatusColor = "#9AA5BB"
        });
        // Cleaning is intentionally excluded from the filter chips because it
        // groups under Maintenance on the housekeeping board.
        foreach (RoomCleaningStatus s in Enum.GetValues(typeof(RoomCleaningStatus)))
        {
            var count = Rooms.Count(r => r.Status == s);
            StatusFilters.Add(new StatusFilterItem
            {
                Label = s.ToString(),
                Count = count,
                Status = s,
                IsAll = false,
                StatusColor = s switch
                {
                    RoomCleaningStatus.Available => "#16A1E0",
                    RoomCleaningStatus.Occupied => "#5146E5",
                    RoomCleaningStatus.Reserved => "#1F8FA8",
                    RoomCleaningStatus.Maintenance => "#E9446B",
                    _ => "#9AA5BB"
                }
            });
        }
    }

    /// <summary>Recomputes <see cref="FilteredRooms"/> and
    /// <see cref="TableRooms"/> from the master rich collection, applying
    /// the active floor / status / search filters.</summary>
    private void ApplyFilter()
    {
        var ordered = ComputeFiltered();

        FilteredRooms.Clear();
        TableRooms.Clear();
        foreach (var item in ordered)
        {
            FilteredRooms.Add(item);
            TableRooms.Add(item);
        }

        OnPropertyChanged(nameof(FilteredRooms));
        OnPropertyChanged(nameof(TableRooms));
        OnPropertyChanged(nameof(VisibleCountLabel));
        NotifyGridHeight();
    }

    /// <summary>Projects <see cref="Rooms"/> through the active
    /// floor / status / search filters and returns the ordered result.</summary>
    /// <returns>The filtered room sequence, sorted by room number.</returns>
    private IEnumerable<RoomDetailsModel> ComputeFiltered()
    {
        var source = Rooms.AsEnumerable();

        if (SelectedFloorFilter is not null && !SelectedFloorFilter.IsAll)
        {
            var floorDigit = SelectedFloorFilter.Label.Replace("Floor ", string.Empty).Trim();
            source = source.Where(r => string.Equals(r.Floor, floorDigit, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedStatusFilter is not null && !SelectedStatusFilter.IsAll)
        {
            var target = SelectedStatusFilter.Status;
            source = source.Where(r => r.Status == target);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var needle = SearchText.Trim();
            source = source.Where(r =>
                r.Number.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                r.Type.Contains(needle, StringComparison.OrdinalIgnoreCase) ||
                r.GuestName.Contains(needle, StringComparison.OrdinalIgnoreCase));
        }

        return source.OrderBy(r => r.Number);
    }

    /// <summary>Forwards the chip's selection into
    /// <see cref="SelectedFloorFilter"/>. Called from the floor-chip
    /// <c>SelectionChanged</c> handler.</summary>
    /// <param name="addedItem">Chip selected by the user. May be
    /// <c>null</c> when the user deselected everything.</param>
    public void OnFloorChipSelectionChanged(object? addedItem)
    {
        if (addedItem is FloorFilterItem item)
            SelectedFloorFilter = item;
    }

    /// <summary>Forwards the chip's selection into
    /// <see cref="SelectedStatusFilter"/>. Called from the status-chip
    /// <c>SelectionChanged</c> handler.</summary>
    /// <param name="addedItem">Chip selected by the user. May be
    /// <c>null</c> when the user deselected everything.</param>
    public void OnStatusChipSelectionChanged(object? addedItem)
    {
        if (addedItem is StatusFilterItem item)
            SelectedStatusFilter = item;
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Amenity catalog (line shown in the AMENITIES column / card).
    private static readonly (string TypeMatch, string Amenities)[] AmenityCatalog =
    {
        ("King Deluxe",        "King Bed - Free Wi-Fi - Air Conditioning"),
        ("Twin Garden",        "Twin Beds - Garden View - Free Wi-Fi"),
        ("Suite Harbor",       "King Bed - Harbor View - Lounge Access - Free Wi-Fi"),
        ("Executive Suite",    "King Bed - Work Desk - Lounge Access - Espresso Bar"),
        ("Standard Queen",     "Queen Bed - Free Wi-Fi - Smart TV"),
        ("Family Suite",       "Two Bedrooms - Balcony - Free Wi-Fi - Mini Fridge"),
        ("Penthouse",          "King Bed - Private Terrace - Butler Service"),
        ("Presidential Suite", "King Bed - Private Terrace - Butler Service - Lounge")
    };

    // Name pools for Occupied / Reserved rows.
    private static readonly string[] FirstNames =
    {
        "Olivia", "Marcus", "Elena", "David", "Sarah", "James", "Aisha", "Hiroshi",
        "Priya", "Liam", "Sofia", "Anders", "Yuki", "Rafael", "Chloe", "Tariq",
        "Nadia", "Felix", "Ingrid", "Mateo", "Zara", "Owen", "Maya", "Henrik",
        "Lucia", "Kenji", "Amir", "Beatrice", "Diego", "Sienna"
    };

    private static readonly string[] LastNames =
    {
        "Bennett", "Vance", "Rostova", "Kim", "Holt", "Carter", "Patel", "Tanaka",
        "Singh", "O'Sullivan", "Marquez", "Lindqvist", "Sato", "Vargas", "Dubois",
        "Hassan", "Petrov", "Nilsen", "Reyes", "Khan", "Whitaker", "Ramirez",
        "Schmidt", "Ferrari", "Watanabe", "Mahmoud", "Fairchild", "Ortega"
    };

    /// <summary>
    /// Resolves an amenity line for a given room type. Same type always returns
    /// the same label so the table view is stable.
    /// </summary>
    /// <param name="type">Room type label to resolve amenities for.</param>
    /// <returns>A pipe-separated amenity line.</returns>
    private static string ResolveAmenities(string type)
    {
        if (string.IsNullOrWhiteSpace(type)) return "Free Wi-Fi - Air Conditioning";
        foreach (var (match, amenities) in AmenityCatalog)
        {
            if (type.Contains(match, StringComparison.OrdinalIgnoreCase)) return amenities;
        }
        return "Free Wi-Fi - Air Conditioning - Smart TV";
    }

    /// <summary>
    /// Builds a deterministic guest name + initials from a room number so each
    /// row in the table view has a stable guest identity.
    /// </summary>
    /// <param name="number">Room number — used as the RNG seed so the
    /// name stays stable across rebuilds.</param>
    /// <param name="rng">Pre-seeded <see cref="Random"/> instance.</param>
    /// <returns>The generated full name and two-letter initials.</returns>
    private static (string Name, string Initials) ResolveGuest(string number, Random rng)
    {
        var first = FirstNames[rng.Next(FirstNames.Length)];
        var last = LastNames[rng.Next(LastNames.Length)];
        var initials = $"{first[0]}{last[0]}";
        return ($"{first} {last}", initials);
    }

    /// <summary>
    /// Promote a plain <see cref="Room"/> (from the master 15-key hotel data)
    /// into a richer <see cref="RoomDetailsModel"/>. Every card on the Rooms
    /// page derives from this single helper so there is never a second
    /// source of truth for inventory.
    /// </summary>
    /// <param name="room">Master <see cref="Room"/> to promote.</param>
    /// <returns>The richer model backing the room card / row.</returns>
    private RoomDetailsModel BuildRichRoomFromHotelData(Room room)
    {
        var status = Enum.TryParse<RoomCleaningStatus>(room.Status, out var parsed) ? parsed : RoomCleaningStatus.Available;
        var seedRng = new Random(room.Number.GetHashCode() ^ 20260910);
        var amenities = ResolveAmenities(room.Type);
        var isGuest = status is RoomCleaningStatus.Occupied or RoomCleaningStatus.Reserved;
        var (guestName, guestInitials) = ResolveGuest(room.Number, seedRng);

        var today = HotelConfig.ReferenceToday;
        var checkInDay = today.AddDays(status switch
        {
            RoomCleaningStatus.Occupied => 0,
            RoomCleaningStatus.Reserved => 1,
            RoomCleaningStatus.Maintenance => 2,
            _ => 0
        });
        var rangeText = status switch
        {
            RoomCleaningStatus.Occupied => "In residence",
            RoomCleaningStatus.Reserved => $"Arrives {checkInDay:MMM dd}",
            RoomCleaningStatus.Maintenance => $"ETA {checkInDay:MMM dd}",
            RoomCleaningStatus.Available => "Ready",
            _ => "Ready"
        };

        return new RoomDetailsModel
        {
            Number = room.Number,
            Type = room.Type,
            Floor = room.Floor,
            Status = status,
            Rate = room.Rate,
            AmenitiesLine = amenities,
            CheckInRange = rangeText,
            CheckInDayName = checkInDay.ToString("MMM dd, yyyy"),
            CheckInTime = seedRng.Next(7, 22),
            HousekeepingNote = status == RoomCleaningStatus.Available ? "Inspected & Sanitized" : string.Empty,
            IsGuestAssigned = !isGuest,
            GuestName = isGuest ? guestName : string.Empty,
            GuestInitials = isGuest ? guestInitials : string.Empty,
            Adults = isGuest ? seedRng.Next(2) + 1 : 0,
            Children = isGuest && seedRng.Next(4) == 0 ? 1 : 0,
            SpecialRequest = status == RoomCleaningStatus.Occupied && seedRng.Next(2) == 0 ? "GUEST FLAG: Do Not Disturb" :
                             status == RoomCleaningStatus.Reserved && seedRng.Next(2) == 0 ? "Special Request: Crib & High Chair" :
                             string.Empty,
            MaintenanceTicketId = status == RoomCleaningStatus.Maintenance ? "ACTIVE ENGINEERING TICKET" : string.Empty,
            ReportedBy = status == RoomCleaningStatus.Maintenance ? "HVAC Thermostat recalibration" : string.Empty,
            Technician = status == RoomCleaningStatus.Maintenance ? "Technician: James T." : string.Empty,
            WorkOrderReference = status == RoomCleaningStatus.Maintenance ? "MWO-482" : string.Empty,
            EstimatedCompletion = status == RoomCleaningStatus.Maintenance ? "1 hr 45 min" : string.Empty,
            Downtime = status == RoomCleaningStatus.Maintenance ? "1 hr left" : string.Empty,
        };
    }
}

/// <summary>
/// One chip the SfChipGroup binds against for the rooms page's
/// status-filter row. Carries a label, a per-status count, an active
/// flag and a dot colour.
/// </summary>
public sealed class StatusFilterItem
{
    /// <summary>Chip text rendered to the user.</summary>
    public string Label { get; init; } = string.Empty;
    /// <summary>Per-status count displayed inside the chip.</summary>
    public int Count { get; init; }
    /// <summary>Room status this chip filters by.</summary>
    public RoomCleaningStatus Status { get; init; }
    /// <summary>True when this is the "All" chip and the chip carries
    /// no per-status filter.</summary>
    public bool IsAll { get; init; }

    /// <summary>Display label rendered to the user including the
    /// count badge.</summary>
    public string Display
    {
        get
        {
            if (IsAll) return $"All ({Count})";
            return Count > 0 ? $"{Label} ({Count})" : Label;
        }
    }

    /// <summary>Dot color drawn to the left of the chip text.</summary>
    public string StatusColor { get; init; } = "#9AA5BB";
}
