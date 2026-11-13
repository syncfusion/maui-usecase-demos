using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HotelManagerSample;

namespace HotelManagerSample.Bookings;

/// <summary>
/// View-model for the mobile New-Booking wizard. Mirrors the desktop
/// <see cref="HotelManagerSample.AddBookingViewModel"/>
/// surface but is fully self-contained (no popup sharing) so the
/// wizard can be pushed as a standalone modal page from the
/// SfTabView host. The wizard:
///   - Validates every field via <see cref="TryValidate"/> to gate <see cref="CanSubmit"/>
///   - Subscribes to the shared <see cref="IHotelStore"/> so floor/room
///     options refresh the moment housekeeping flips a status elsewhere
///   - Maps the floor selection onto <see cref="RoomsOnSelectedFloor"/> and
///     <see cref="AvailableRooms"/> so the inventory dropdown only ever
///     surfaces bookable keys
/// At commit time it calls <see cref="IHotelStore.AddBooking"/> so the
/// full app gets a single BookingsChanged notification.
/// </summary>
public sealed class MobileNewBookingViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    // Pre-compiled regex used by email validation. Cached so the validator
    // path doesn't allocate per call.
    private static readonly Regex EmailRegex = new(
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Mobile phone is exactly 10 digits — no country code, dashes, spaces.
    /// Hardened from the earlier 7-20-char tolerance so a stray "+1" or
    /// "(555)" prefix can't sneak through the wizard on its way to create.</summary>
    public const string StandardPhonePattern = @"^\d{10}$";

    /// <summary>Constructs the wizard VM, wires <see cref="SubmitCommand"/>
    /// to <see cref="OnSubmitTapped"/>, seeds the floor options, and
    /// subscribes to <see cref="IHotelStore.RoomsChanged"/> so the
    /// dropdown stays in lockstep with housekeeping flips.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public MobileNewBookingViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        // The wizard's submit command is backed by CanSubmit — when the
        // user fixes the last failing field, the button enables itself.
        SubmitCommand = new Command(() => OnSubmitTapped(), () => CanSubmit);
        SubmitCommand.CanExecuteChanged += (_, _) => OnPropertyChanged(nameof(CanSubmit));

        RebuildFloorOptions();

        _checkIn  = HotelConfig.ReferenceToday;
        _checkOut = HotelConfig.ReferenceToday.AddDays(3);

        // --- Subscribe + open the first floor's rooms list ---
        _store.RoomsChanged += OnStoreRoomsChanged;

        if (FloorOptions.Count > 0)
        {
            SelectedFloor = FloorOptions[0].FloorNumber;
        }
        else
        {
            SelectedFloor = string.Empty;
        }
    }

    // ==================================================================
    // ========================== Floor + Rooms =========================
    // ==================================================================

    /// <summary>Floor chips the wizard binds against. Populated from
    /// the master store; rebuilt whenever the store raises
    /// <see cref="IHotelStore.RoomsChanged"/>.</summary>
    public ObservableCollection<MobileFloorOption> FloorOptions { get; } = new();

    private string _selectedFloor = string.Empty;
    /// <summary>Persistent identity of the currently selected floor
    /// chip. Drives the projection of <see cref="RoomsOnSelectedFloor"/>
    /// and <see cref="AvailableRooms"/>.</summary>
    public string SelectedFloor
    {
        get => _selectedFloor;
        set => SetSelectedFloor(value);
    }

    /// <summary>All rooms on the selected floor regardless of status.</summary>
    public ObservableCollection<Room> RoomsOnSelectedFloor { get; } = new();
    /// <summary>Rooms on the selected floor with status "Available".</summary>
    public ObservableCollection<Room> AvailableRooms { get; } = new();

    /// <summary>True when <see cref="AvailableRooms"/> is empty so
    /// the wizard can show a "no rooms" hint and block submission.</summary>
    public bool HasNoAvailableRooms => AvailableRooms.Count == 0;

    /// <summary>Floors are derived from the live store. Rebuilding any
    /// time the rooms collection changes keeps the wizard aligned with
    /// any future "merge two floors" operation that the operations team
    /// might run from the Rooms page.</summary>
    private void RebuildFloorOptions()
    {
        var floors = _store.Rooms
            .Select(r => r.Floor)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        if (floors.Count == 0)
            floors.Add("1");

        FloorOptions.Clear();
        foreach (var f in floors)
            FloorOptions.Add(new MobileFloorOption(f));
    }

    /// <summary>Single entry point for floor changes. Called from the
    /// setter and from external callers (the SfChipGroup
    /// <c>SelectionChanged</c> handler).</summary>
    /// <param name="floorNumber">Floor number to select. Empty / whitespace
    /// clears the selection.</param>
    private void SetSelectedFloor(string? floorNumber)
    {
        var normalized = string.IsNullOrWhiteSpace(floorNumber) ? string.Empty : floorNumber!;
        if (!string.Equals(_selectedFloor, normalized, StringComparison.Ordinal))
        {
            _selectedFloor = normalized;
            OnPropertyChanged(nameof(SelectedFloor));
        }

        RefreshAvailableRooms();
    }

    /// <summary>Re-projects <see cref="RoomsOnSelectedFloor"/> and
    /// <see cref="AvailableRooms"/> from the master store. Always
    /// clears the previous contents first so stale rooms disappear
    /// when the user changes floors.</summary>
    private void RefreshAvailableRooms()
    {
        RoomsOnSelectedFloor.Clear();
        AvailableRooms.Clear();

        if (string.IsNullOrEmpty(_selectedFloor))
        {
            OnPropertyChanged(nameof(HasNoAvailableRooms));
            return;
        }

        var onFloor = _store.Rooms
            .Where(r => string.Equals(r.Floor, _selectedFloor, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Number)
            .ToList();

        foreach (var r in onFloor) RoomsOnSelectedFloor.Add(r);

        var aval = onFloor
            .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var r in aval) AvailableRooms.Add(r);

        // Drop a stale SelectedRoom if its floor just disappeared
        if (SelectedRoom is not null && !aval.Any(r => r.Number == SelectedRoom.Number))
            SelectedRoom = null;

        OnPropertyChanged(nameof(HasNoAvailableRooms));
    }

    /// <summary>Master-store callback for room changes. Rebuilds the
    /// floor chips, refreshes the visible-room projection when the
    /// current floor is still on screen, and clears the selection
    /// when the chosen room has flipped away from "Available".</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The room that changed.</param>
    private void OnStoreRoomsChanged(object? sender, Room e)
    {
        RebuildFloorOptions();

        // Refresh the projections if the user is currently viewing the
        // affected floor.
        if (!string.IsNullOrEmpty(_selectedFloor))
            RefreshAvailableRooms();

        // If a status flip on a room we currently have selected demoted
        // it from Available, drop our reference so the user has to
        // re-pick a fresh key.
        if (SelectedRoom is not null && !string.Equals(SelectedRoom.Status, "Available", StringComparison.OrdinalIgnoreCase))
            SelectedRoom = null;
    }

    // ==================================================================
    // ============================ Fields ==============================
    // ==================================================================

    private string _guestName = string.Empty;
    /// <summary>Guest full name. Empty input is normalised to a blank
    /// string so the validation message reads cleanly.</summary>
    public string GuestName
    {
        get => _guestName;
        set { if (_guestName != value) { _guestName = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    private string _guestEmail = string.Empty;
    /// <summary>Guest email address. Setter triggers the
    /// <see cref="CanSubmit"/> recomputation.</summary>
    public string GuestEmail
    {
        get => _guestEmail;
        set { if (_guestEmail != value) { _guestEmail = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    private string _phone = string.Empty;
    /// <summary>Phone digits only — the code-behind sanitises pasted
    /// values down to 10 digits before they reach this setter.</summary>
    public string Phone
    {
        get => _phone;
        set { if (_phone != value) { _phone = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    private Room? _selectedRoom;
    /// <summary>Room the user picked from the inventory dropdown. Drives
    /// every "selected room"-derived property (amenity chips, total,
    /// total footnote) and gates <see cref="CanSubmit"/>.</summary>
    public Room? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (_selectedRoom == value) return;
            _selectedRoom = value;
            OnPropertyChanged(nameof(SelectedRoom));
            OnPropertyChanged(nameof(SelectedRoomAmenityChips));
            OnPropertyChanged(nameof(HasSelectedRoom));
            OnPropertyChanged(nameof(ComputedTotal));
            OnPropertyChanged(nameof(TotalFootnote));
            OnPropertyChanged(nameof(CanSubmit));
        }
    }

    /// <summary>True when <see cref="SelectedRoom"/> is non-null.</summary>
    public bool HasSelectedRoom => SelectedRoom is not null;

    /// <summary>Amenity chips rendered under the inventory card. They
    /// expand only after the user has picked a room so the wizard
    /// doesn't waste space when no room has been selected.</summary>
    public IReadOnlyList<string> SelectedRoomAmenityChips
        => SelectedRoom is null ? Array.Empty<string>() : ResolveAmenities(SelectedRoom.Type);

    private DateTime _checkIn;
    /// <summary>Check-in date. When the new value falls on or after
    /// <see cref="CheckOut"/>, the latter is bumped by one day so the
    /// pair stays valid.</summary>
    public DateTime CheckIn
    {
        get => _checkIn;
        set
        {
            if (_checkIn == value) return;
            _checkIn = value;
            if (_checkOut <= _checkIn)
                _checkOut = _checkIn.AddDays(1);
            OnPropertyChanged(nameof(CheckIn));
            OnPropertyChanged(nameof(CheckOut));
            OnPropertyChanged(nameof(ComputedTotal));
            OnPropertyChanged(nameof(TotalFootnote));
            OnPropertyChanged(nameof(StaySummary));
            OnPropertyChanged(nameof(Nights));
            OnPropertyChanged(nameof(CanSubmit));
        }
    }

    private DateTime _checkOut;
    /// <summary>Check-out date. Setter refreshes every derived
    /// property so the wizard's total + stay-summary labels stay in
    /// sync.</summary>
    public DateTime CheckOut
    {
        get => _checkOut;
        set
        {
            if (_checkOut == value) return;
            _checkOut = value;
            OnPropertyChanged(nameof(CheckOut));
            OnPropertyChanged(nameof(ComputedTotal));
            OnPropertyChanged(nameof(TotalFootnote));
            OnPropertyChanged(nameof(StaySummary));
            OnPropertyChanged(nameof(Nights));
            OnPropertyChanged(nameof(CanSubmit));
        }
    }

    private int _adults = 2;
    /// <summary>Adult count, clamped to a minimum of one. Negative
    /// inputs are silently coerced to 1.</summary>
    public int Adults
    {
        get => _adults;
        set
        {
            var clamped = Math.Max(1, value);
            if (_adults == clamped) return;
            _adults = clamped;
            OnPropertyChanged();
        }
    }

    private int _children = 0;
    /// <summary>Children count, clamped to a minimum of zero.</summary>
    public int Children
    {
        get => _children;
        set
        {
            var clamped = Math.Max(0, value);
            if (_children == clamped) return;
            _children = clamped;
            OnPropertyChanged();
        }
    }

    // ==================================================================
    // ========================== Stay Summary ==========================
    // ==================================================================

    /// <summary>Number of nights computed from
    /// <see cref="CheckIn"/> / <see cref="CheckOut"/>, floored at 1.</summary>
    public int Nights => Math.Max(1, (int)Math.Round((_checkOut.Date - _checkIn.Date).TotalDays));

    /// <summary>Compact stay-range label rendered in the summary card
    /// (e.g. "Sep 10 - Sep 13").</summary>
    public string StaySummary =>
        $"{_checkIn:MMM dd} - {_checkOut:MMM dd}";

    /// <summary>Total = rate × nights, rounded to whole dollars. Zero
    /// when no room has been picked.</summary>
    public decimal ComputedTotal =>
        SelectedRoom is null ? 0m : Math.Round(SelectedRoom.Rate * Nights, 0);

    /// <summary>Footnote under the TOTAL headline in the summary card,
    /// e.g. "Room 101 – Deluxe King · 4 nights".</summary>
    public string TotalFootnote =>
        SelectedRoom is null
            ? string.Empty
            : $"{SelectedRoom.DisplayLabel} · {Nights} {(Nights == 1 ? "night" : "nights")}";

    // ==================================================================
    // =========================== Messaging ============================
    // ==================================================================

    private string _errorMessage = string.Empty;
    /// <summary>Inline error strip text. Always set through the
    /// setter so the <see cref="HasErrorMessage"/> companion
    /// property stays in lockstep with the UI.</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value ?? string.Empty;
            OnPropertyChanged(nameof(ErrorMessage));
            OnPropertyChanged(nameof(HasErrorMessage));
        }
    }

    /// <summary>True when <see cref="ErrorMessage"/> holds any text.
    /// Drives the inline strip's visibility binding.</summary>
    public bool HasErrorMessage => !string.IsNullOrEmpty(_errorMessage);

    // ==================================================================
    // ========================== Validation ============================
    // ==================================================================

    /// <summary>True when every required field has a sensible value.</summary>
    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(_guestName)
        && !string.IsNullOrWhiteSpace(_guestEmail)
        && EmailRegex.IsMatch(_guestEmail)
        && !string.IsNullOrWhiteSpace(_phone)
        && Regex.IsMatch(_phone, StandardPhonePattern)
        && SelectedRoom is not null
        && _checkOut.Date > _checkIn.Date
        && !HasNoAvailableRooms;

    /// <summary>Runs the same set of validations <see cref="CanSubmit"/>
    /// uses, but stores the offending message into
    /// <see cref="ErrorMessage"/> if anything fails. Returns true if the
    /// form is ready to submit.</summary>
    /// <returns><c>true</c> when the form is valid; otherwise
    /// <c>false</c>.</returns>
    public bool TryValidate()
    {
        if (string.IsNullOrWhiteSpace(_guestName))
        {
            ErrorMessage = "Full name is required."; return false;
        }
        if (string.IsNullOrWhiteSpace(_guestEmail) || !EmailRegex.IsMatch(_guestEmail))
        {
            ErrorMessage = "Please enter a valid email address."; return false;
        }
        if (string.IsNullOrWhiteSpace(_phone))
        {
            ErrorMessage = "Phone number is required."; return false;
        }
        if (!Regex.IsMatch(_phone, StandardPhonePattern))
        {
            ErrorMessage = "Phone number must contain exactly 10 digits."; return false;
        }
        if (HasNoAvailableRooms || SelectedRoom is null)
        {
            ErrorMessage = HasNoAvailableRooms
                ? "No available rooms on this floor — try a different floor."
                : "Please select an available room.";
            return false;
        }
        if (_checkOut.Date <= _checkIn.Date)
        {
            ErrorMessage = "Check-out date must be after check-in date."; return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    // ==================================================================
    // ============================= Commit =============================
    // ==================================================================

    /// <summary>True when the Create Booking button should accept taps.
    /// Backed by <see cref="TryValidate"/> so the inline error message is
    /// also kept in sync.</summary>
    public ICommand SubmitCommand { get; private set; } = null!;

    /// <summary>Invoked when the wizard's Create Booking button is
    /// enabled and tapped. The page catches the CommandParameter="OK"
    /// path and pops the modal; failures leave
    /// <see cref="ErrorMessage"/> set for the inline ribbon.</summary>
    private void OnSubmitTapped() => CreateBooking();

    /// <summary>Hand off the new reservation to the IHotelStore and
    /// return the result so the page can pop the modal. Returns
    /// <c>false</c> if validation failed and writes the message into
    /// <see cref="ErrorMessage"/>.</summary>
    /// <returns><c>true</c> on successful commit; <c>false</c> when
    /// validation failed.</returns>
    public bool CreateBooking()
    {
        if (!TryValidate())
            return false;

        if (SelectedRoom is null)
        {
            ErrorMessage = "Please choose an available room.";
            return false;
        }

        var highestId = 0;
        foreach (var b in _store.Bookings)
        {
            var digits = new string((b.Reference ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length == 0) continue;
            if (int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
                && n > highestId)
                highestId = n;
        }
        var reference = $"#BK-{highestId + 1:D5}";

        var nights = Nights;
        var room   = SelectedRoom;
        var wing   = (int.TryParse(room.Number, out var rn) && rn % 2 == 0) ? "Garden Wing" : "Pool Terrace";

        var booking = new Booking
        {
            Reference     = reference,
            Guest         = _guestName.Trim(),
            GuestEmail    = _guestEmail.Trim(),
            Phone         = _phone.Trim(),
            Room          = $"Room {room.Number}",
            RoomType      = room.Type,
            RoomSegment   = $"Floor {room.Floor} · {wing}",
            Dates         = $"{CheckIn:MMM dd} - {CheckOut:MMM dd, yyyy}",
            CheckIn       = CheckIn.Date,
            CheckOut      = CheckOut.Date,
            Adults        = _adults,
            Children      = _children,
            Source        = "Direct",
            SourceSegment = "DIRECT • OTA",
            SourceTagColor = "#6E7A8F",
            Status        = "Confirmed",
            FollowUp      = "Pre-Auth Needed",
            Total         = Math.Round(room.Rate * nights, 0),
            Notes         = $"Booked via the mobile New Booking wizard ({nights}-night stay).",
        };

        // Single mutation point -> raises IHotelStore.BookingsChanged + RoomsChanged.
        _store.AddBooking(booking, room);
        return true;
    }

    /// <summary>Static amenity catalog used by <see cref="SelectedRoomAmenityChips"/>.
    /// Each room type resolves consistently so the same key on the same
    /// floor maps to the same chips.</summary>
    private static readonly (string TypeMatch, string[] Amenities)[] AmenityCatalog =
    {
        ("King Deluxe",        new[] { "King Pillowtop", "Ocean Frontage", "Private Balcony" }),
        ("Twin Garden",        new[] { "Twin Beds", "Garden View", "Free Wi-Fi" }),
        ("Suite Harbor",       new[] { "Harbor View", "Lounge Access", "Espresso Bar" }),
        ("Executive Suite",    new[] { "Work Desk", "Lounge Access", "Espresso Bar" }),
        ("Standard Queen",     new[] { "Queen Bed", "Smart TV", "Free Wi-Fi" }),
        ("Family Suite",       new[] { "Two Bedrooms", "Balcony", "Mini Fridge", "Free Wi-Fi" }),
        ("Penthouse",          new[] { "Private Terrace", "Butler Service", "Jacuzzi" }),
        ("Presidential Suite", new[] { "Private Terrace", "Butler Service", "Jacuzzi", "Lounge" })
    };

    /// <summary>Resolves the amenity chip list for a given room type.
    /// Returns a sensible default for unrecognised types so the wizard
    /// never has to render an empty chip strip.</summary>
    /// <param name="type">Room type label to look up.</param>
    /// <returns>Chip strings in the order they should appear under the
    /// inventory card.</returns>
    private static string[] ResolveAmenities(string type)
    {
        if (string.IsNullOrWhiteSpace(type)) return new[] { "Free Wi-Fi" };
        foreach (var (match, amenities) in AmenityCatalog)
        {
            if (type.Contains(match, StringComparison.OrdinalIgnoreCase))
                return amenities;
        }
        return new[] { "Free Wi-Fi", "Air Conditioning", "Smart TV" };
    }

    // ==================================================================
    // ============================ INPC =================================
    // ==================================================================

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Pure data class representing a floor option on the wizard. Identical
/// in shape to <see cref="FloorOption"/> — we declare a separate one here
/// so the wizard doesn't depend on the inner desktop type.
/// </summary>
public sealed class MobileFloorOption
{
    /// <summary>Constructs a wizard floor option with the supplied
    /// floor-number string.</summary>
    /// <param name="floorNumber">Floor number to display (e.g. "1").</param>
    public MobileFloorOption(string floorNumber) => FloorNumber = floorNumber;
    /// <summary>Floor-number identifier — used as the segment key.</summary>
    public string FloorNumber { get; init; }
    /// <summary>Display label formatted as "Floor {n}".</summary>
    public string Display => $"Floor {FloorNumber}";
}
