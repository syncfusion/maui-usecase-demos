using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HotelManagerSample;
using Syncfusion.Maui.Calendar;

namespace HotelManagerSample;

/// <summary>
/// Form-state + validation for the "Add booking" modal. Public mutable
/// properties bind directly to the popup's text entries, calendar and
/// combo box so XAML never has to update VM fields by hand. Bound at
/// popup create time to the same singleton <see cref="IHotelStore"/>
/// the rest of the app reads — so the available-room dropdown snaps to
/// the live inventory (any room flipped to Reserved by another page in
/// the meantime disappears), and a booking committed through the popup
/// immediately falls out of the list for the next visitor.
/// </summary>
public sealed class AddBookingViewModel : INotifyPropertyChanged
{
    /// <summary>Shared inventory injected by DI. Never null after
    /// construction.</summary>
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
    /// <summary>Raises <see cref="PropertyChanged"/> for several
    /// property names in one call. Saves a few keystrokes when a
    /// single setter invalidates multiple derived bindings.</summary>
    /// <param name="names">Names of the properties that changed.</param>
    private void OnMany(params string[] names)
    {
        foreach (var n in names) OnPropertyChanged(n);
    }

    /// <summary>Constructs the VM, projects the floor options from the
    /// master inventory, and seeds sensible default dates so the popup
    /// isn't blank on first open.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public AddBookingViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        // Build floor options from the actual inventory's distinct
        // floors so adding/removing floors never desynchronises with the
        // shared store. First floor is auto-selected.
        var floorNumbers = _store.Rooms
            .Select(r => r.Floor)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Distinct()
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        if (floorNumbers.Count == 0)
            floorNumbers.Add("1"); // degenerate single-floor fallback

        FloorOptions = new ObservableCollection<FloorOption>(
            floorNumbers.Select((f, idx) => new FloorOption
            {
                FloorNumber = f,
                IsSelected = idx == 0
            }));

        SelectedFloor = FloorOptions.FirstOrDefault()?.FloorNumber ?? string.Empty;

        // Sensible defaults so the popup isn't blank on first open.
        _checkIn = DateTime.Today;
        _checkOut = DateTime.Today.AddDays(4);
        _selectedDateRange = new CalendarDateRange(_checkIn, _checkOut);
    }

    #region Floor selection + available-rooms projection

    /// <summary>All floors in the property as interactive chip options.</summary>
    public ObservableCollection<FloorOption> FloorOptions { get; }

    /// <summary>Backing field for <see cref="SelectedFloor"/>.</summary>
    private string _selectedFloor = string.Empty;
    /// <summary>Persistent identity (FloorNumber) of the currently selected
    /// floor chip. Used by <see cref="RefreshAvailableRoomsOnSelectedFloor"/>
    /// to project <see cref="RoomsOnSelectedFloor"/> from the shared store.</summary>
    public string SelectedFloor
    {
        get => _selectedFloor;
        set => SetSelectedFloor(value);
    }

    /// <summary>Single entry point for floor changes. Used by the popup's
    /// SfChipGroup <c>SelectionChanged</c> event handler in the
    /// code-behind and by the popup's constructor (which projects rooms
    /// for the auto-selected first floor before SfChipGroup has had a
    /// chance to raise its own SelectionChanged event). Idempotent — when
    /// the user re-taps the already-selected chip this is a no-op apart
    /// from re-running the warning-strip reconciliation.</summary>
    /// <param name="floorNumber">Floor identifier to select. Empty /
    /// whitespace clears the selection.</param>
    public void SetSelectedFloor(string? floorNumber)
    {
        var normalized = string.IsNullOrWhiteSpace(floorNumber) ? string.Empty : floorNumber!;

        // Always rebuild the projections + the warning strip so the UI
        // stays in sync even if SfChipGroup fires the event with the
        // same value the user has already picked (the group can do that
        // on its initial render pass).
        if (!string.Equals(_selectedFloor, normalized, StringComparison.Ordinal))
        {
            _selectedFloor = normalized;
            OnPropertyChanged(nameof(SelectedFloor));
        }

        RefreshAvailableRoomsOnSelectedFloor();
    }

    /// <summary>All rooms that exist on the selected floor (regardless of
    /// status). The projection lives in the VM so the popup can keep an
    /// "Any room available?" flag in sync without doing the LINQ in XAML.</summary>
    public ObservableCollection<Room> RoomsOnSelectedFloor { get; } = new();

    /// <summary>Only rooms on the selected floor with status "Available".
    /// This is the binding source for the Availability dropdown.</summary>
    public ObservableCollection<Room> AvailableRooms { get; } = new();

    /// <summary>True when the current floor has 0 available rooms.
    /// Used to show the "No rooms available on this floor…" hint and to
    /// block submission until the user picks a different floor.</summary>
    public bool HasNoAvailableRooms => AvailableRooms.Count == 0;

    /// <summary>Constant copy shown in the pink warning strip when the
    /// current floor has zero available rooms. Matches the Figma
    /// language: "Rooms not available in the floor you have selected.
    /// Select another floor." Set once so a stray test can assert the
    /// exact wording.</summary>
    public const string NoRoomsWarningMessage = "Rooms not available in the floor you have selected. Select another floor.";

    /// <summary>True while the warning belongs to a no-rooms-on-floor
    /// selection. We only clear <see cref="ErrorMessage"/> while our
    /// own warning is present so user-typed validation errors never
    /// get wiped by a chip click — the flag is the contract.</summary>
    private bool _errorMessageIsFloorWarning;

    /// <summary>Re-projects <see cref="RoomsOnSelectedFloor"/> and
    /// <see cref="AvailableRooms"/> from the master store whenever the
    /// selected floor changes. Always clears the previous contents
    /// first so stale rooms disappear.</summary>
    private void RefreshAvailableRoomsOnSelectedFloor()
    {
        RoomsOnSelectedFloor.Clear();
        AvailableRooms.Clear();

        if (string.IsNullOrEmpty(_selectedFloor))
        {
            OnMany(nameof(HasNoAvailableRooms));
            ReconcileNoRoomsWarning(emptyFloor: true);
            return;
        }

        var onFloor = _store.Rooms
            .Where(r => string.Equals(r.Floor, _selectedFloor, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Number)
            .ToList();

        foreach (var r in onFloor) RoomsOnSelectedFloor.Add(r);

        var available = onFloor
            .Where(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var r in available) AvailableRooms.Add(r);

        // Drop a stale SelectedRoom if it doesn't belong to the new floor.
        if (SelectedRoom is not null && !available.Any(r => r.Number == SelectedRoom.Number))
            SelectedRoom = null;

        OnMany(nameof(HasNoAvailableRooms));
        ReconcileNoRoomsWarning(emptyFloor: false);
    }

    /// <summary>Synchronises <see cref="ErrorMessage"/> with the
    /// no-rooms-on-floor condition. Pushes the warning copy onto the
    /// pink strip when the picked floor has zero rooms and clears it
    /// (only if OUR warning was the last thing set) the moment any
    /// room becomes available. This single reconciliation point keeps
    /// "user validation error vs. floor warning" completely separate so
    /// a NoRoomsOnFloor chip click never overwrites a real
    /// TryValidate failure.</summary>
    /// <param name="emptyFloor">When <c>true</c> the user has cleared
    /// the floor selection entirely (vs. picking a floor with no
    /// available rooms).</param>
    private void ReconcileNoRoomsWarning(bool emptyFloor)
    {
        var shouldWarn = !emptyFloor && AvailableRooms.Count == 0;

        if (shouldWarn)
        {
            // Only overwrite the message if it either isn't displaying
            // anything yet or is currently displaying OUR warning —
            // never trample a real validation error.
            if (string.IsNullOrEmpty(_errorMessage) || _errorMessageIsFloorWarning)
            {
                ErrorMessage = NoRoomsWarningMessage;
                _errorMessageIsFloorWarning = true;
            }
        }
        else if (_errorMessageIsFloorWarning)
        {
            // Only clear if our warning is what's on screen.
            ErrorMessage = string.Empty;
            _errorMessageIsFloorWarning = false;
        }
    }

    #endregion

    #region Form fields

    /// <summary>Backing field for <see cref="GuestName"/>.</summary>
    private string _guestName = string.Empty;
    /// <summary>Guest full name as entered into the popup. Empty input
    /// is normalised to a blank string so the validation message reads
    /// cleanly.</summary>
    public string GuestName
    {
        get => _guestName;
        set { if (_guestName != value) { _guestName = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    /// <summary>Backing field for <see cref="GuestEmail"/>.</summary>
    private string _guestEmail = string.Empty;
    /// <summary>Guest email address. Setter triggers the
    /// <see cref="CanSubmit"/> recomputation.</summary>
    public string GuestEmail
    {
        get => _guestEmail;
        set { if (_guestEmail != value) { _guestEmail = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    /// <summary>Backing field for <see cref="Phone"/>.</summary>
    private string _phone = string.Empty;
    /// <summary>Phone digits only — the code-behind sanitises pasted
    /// values down to 10 digits before they reach this setter.</summary>
    public string Phone
    {
        get => _phone;
        set { if (_phone != value) { _phone = value ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(CanSubmit)); } }
    }

    /// <summary>Backing field for <see cref="SelectedRoom"/>.</summary>
    private Room? _selectedRoom;
    /// <summary>Room the user picked from the inventory dropdown. Drives
    /// every "selected room"-derived property and gates
    /// <see cref="CanSubmit"/>.</summary>
    public Room? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (_selectedRoom == value) return;
            _selectedRoom = value;
            OnMany(nameof(CanSubmit), nameof(ComputedTotal), nameof(TotalFootnote));
        }
    }

    /// <summary>Backing field for <see cref="CheckIn"/>.</summary>
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
            // Keep CheckOut strictly after CheckIn.
            if (_checkOut <= _checkIn)
                CheckOut = _checkIn.AddDays(1);
            else
                OnMany(nameof(ComputedTotal), nameof(TotalFootnote));
            OnPropertyChanged();
        }
    }

    /// <summary>Backing field for <see cref="CheckOut"/>.</summary>
    private DateTime _checkOut;
    /// <summary>Check-out date. Setter refreshes every derived
    /// property so the wizard's total + stay-summary labels stay in
    /// sync.</summary>
    public DateTime CheckOut
    {
        get => _checkOut;
        set
        {
            if (_checkOut != value)
            {
                _checkOut = value;
                OnPropertyChanged();
                OnMany(nameof(ComputedTotal), nameof(TotalFootnote));
            }
        }
    }

    /// <summary>Convenience setter for the inline calendar — keeps
    /// <see cref="CheckIn"/> and <see cref="CheckOut"/> in sync whenever
    /// the user drags a range. Driven by
    /// <c>SelectedDateRange="{Binding SelectedDateRange, Mode=TwoWay}"</c>.</summary>
    private CalendarDateRange? _selectedDateRange;
    /// <summary>Range bound to the inline calendar. Setting this forwards
    /// the start / end dates onto <see cref="CheckIn"/> / <see cref="CheckOut"/>.</summary>
    public CalendarDateRange? SelectedDateRange
    {
        get => _selectedDateRange;
        set
        {
            _selectedDateRange = value;
            if (value?.StartDate is { } start)
                CheckIn = start.Date;
            if (value?.EndDate is { } end)
                CheckOut = end.Date;
        }
    }

    /// <summary>Backing field for <see cref="Adults"/>.</summary>
    private int _adults = 2;
    /// <summary>Adult count, clamped to a minimum of one. Negative
    /// inputs are silently coerced to 1.</summary>
    public int Adults
    {
        get => _adults;
        set
        {
            var clamped = Math.Max(1, value);
            if (_adults != clamped) { _adults = clamped; OnPropertyChanged(); }
        }
    }

    /// <summary>Backing field for <see cref="Children"/>.</summary>
    private int _children = 0;
    /// <summary>Children count, clamped to a minimum of zero.</summary>
    public int Children
    {
        get => _children;
        set
        {
            var clamped = Math.Max(0, value);
            if (_children != clamped) { _children = clamped; OnPropertyChanged(); }
        }
    }

    #endregion

    #region Calendar toggle

    /// <summary>Backing field for <see cref="IsCalendarOpen"/>.</summary>
    private bool _isCalendarOpen;
    /// <summary>True while the inline calendar dropdown is open.</summary>
    public bool IsCalendarOpen
    {
        get => _isCalendarOpen;
        set { if (_isCalendarOpen != value) { _isCalendarOpen = value; OnPropertyChanged(); } }
    }

    /// <summary>Toggles the inline calendar; called from the chevron
    /// button in the Stay Dates row.</summary>
    public void ToggleCalendar()
    {
        IsCalendarOpen = !IsCalendarOpen;
        // Once at least one range has been picked we no longer need the
        // "Tap the calendar icon…" reminder strip — but we keep it
        // dependent only on this toggle to remain a pure VM signal.
    }

    #endregion

    #region Messaging

    /// <summary>Backing field for <see cref="ErrorMessage"/>.</summary>
    private string _errorMessage = string.Empty;
    /// <summary>Inline error strip text. Always set through the
    /// setter so the <see cref="HasErrorMessage"/> companion property
    /// stays in lockstep with the UI.</summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value) return;
            _errorMessage = value ?? string.Empty;

            // Reflect the new text in the "owner" flag so subsequent
            // floor changes don't accidentally treat a user validation
            // message ("Phone number must contain exactly 10 digits.")
            // as our own floor warning. We treat any incoming text that
            // matches our warning copy as the floor warning; anything
            // else is considered a user-facing validation error and
            // drops back into the floor-warning reconciliation flow.
            _errorMessageIsFloorWarning =
                string.Equals(_errorMessage, NoRoomsWarningMessage, StringComparison.Ordinal);

            OnMany(nameof(ErrorMessage), nameof(HasErrorMessage));
        }
    }

    /// <summary>True when <see cref="ErrorMessage"/> holds any text.
    /// Drives the inline strip's visibility binding.</summary>
    public bool HasErrorMessage => !string.IsNullOrEmpty(_errorMessage);

    /// <summary>Backing field for <see cref="HasInfoMessage"/>.</summary>
    private bool _hasInfoMessage = true;
    /// <summary>True while the "Tap the calendar icon…" hint strip is
    /// visible. Flipped off the first time the user interacts with a
    /// floor chip.</summary>
    public bool HasInfoMessage
    {
        get => _hasInfoMessage;
        set { if (_hasInfoMessage != value) { _hasInfoMessage = value; OnPropertyChanged(); } }
    }

    /// <summary>Clears <see cref="ErrorMessage"/> and resets the
    /// "owner" flag so future floor-warning reconciliation does not
    /// accidentally clear a user-typed validation error.</summary>
    public void ClearError()
    {
        ErrorMessage = string.Empty;
        // Force the "owner" flag back to false — ClearError() is a
        // 'unset everything' escape hatch used by external tests and
        // reset flows, not just by the floor-warning reconciliation.
        _errorMessageIsFloorWarning = false;
    }

    #endregion

    #region Derived

    /// <summary>Nights between CheckIn / CheckOut — at least 1.</summary>
    public int Nights
    {
        get
        {
            var n = (int)Math.Round((_checkOut.Date - _checkIn.Date).TotalDays);
            return Math.Max(1, n);
        }
    }

    /// <summary>Total = rate × nights. Tied to the selected room only when the
    /// user picks one — otherwise 0.</summary>
    public decimal ComputedTotal =>
        SelectedRoom is null ? 0m : Math.Round(SelectedRoom.Rate * Nights, 0);

    /// <summary>Small print under the TOTAL headline in the footer
    /// (e.g. "($320 × 4 nights)" so the user can see how the total was
    /// computed without needing to expand a breakdown).</summary>
    public string TotalFootnote
    {
        get
        {
            if (SelectedRoom is null) return string.Empty;
            return $"({SelectedRoom.DisplayLabel} · {Nights} {(Nights == 1 ? "night" : "nights")})";
        }
    }

    #endregion

    #region Final commit — invoked by the popup's "Add Booking" button.
    /// <summary>
    /// Atomically appends a new <see cref="Booking"/> to the shared store
    /// AND flips the assigned <see cref="Room"/> to "Reserved" through
    /// <see cref="IHotelStore.AddBooking"/>. Returns the form-result
    /// payload the popup hands to its host page on success, or null when
    /// validation fails (after writing the message into
    /// <see cref="ErrorMessage"/>).
    ///
    /// Kept inside the VM (rather than assembled in code-behind) so the
    /// popup stays thin and so any caller — code-behind, an automated
    /// test, a future "Duplicate reservation" button — gets the same
    /// canonical booking shape.
    /// </summary>
    /// <returns>The booking payload on success; <c>null</c> when
    /// validation failed.</returns>
    public AddBookingFormResult? TryCreateBooking()
    {
        ClearError();
        if (!TryValidate(out var error))
        {
            ErrorMessage = error;
            return null;
        }

        if (SelectedRoom is null)
        {
            ErrorMessage = "Please choose an available room.";
            return null;
        }

        // Deterministic sequential id (mirrors the Bookings page logic).
        var highestId = 0;
        foreach (var b in _store.Bookings)
        {
            var digits = new string((b.Reference ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length == 0) continue;
            if (int.TryParse(digits, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var n)
                && n > highestId)
                highestId = n;
        }
        var reference = $"#BK-{highestId + 1:D5}";

        var nights = Math.Max(1, (int)Math.Round((CheckOut.Date - CheckIn.Date).TotalDays));
        var room = SelectedRoom;
        var wing = SelectedRoom.Type;
        var phone = $"+1 {Phone.Trim()}";

        var booking = new Booking
        {
            Reference       = reference,
            Guest           = GuestName.Trim(),
            GuestEmail      = GuestEmail.Trim(),
            Phone           = phone,
            Room            = $"Room {room.Number}",
            RoomType        = room.Type,
            RoomSegment     = $"Floor {room.Floor} · {wing}",
            Dates           = $"{CheckIn:MMM dd} – {CheckOut:MMM dd, yyyy}",
            CheckIn         = CheckIn.Date,
            CheckOut        = CheckOut.Date,
            Adults          = Adults,
            Children        = Children,
            Source          = "Direct",
            SourceSegment   = "DIRECT • OTA",
            SourceTagColor  = "#6E7A8F",
            Status          = "Confirmed",
            FollowUp        = "Pre-Auth Needed",
            Tier            = string.Empty,
            TierColor       = string.Empty,
            Total           = Math.Round(room.Rate * nights, 0),
            Notes           = $"Booked via in-app add-booking popup ({nights}-night stay)",
        };

        _store.AddBooking(booking, room);

        return new AddBookingFormResult
        {
            GuestName     = booking.Guest,
            GuestEmail    = booking.GuestEmail,
            Phone         = booking.Phone,
            Room          = booking.Room,
            RoomType      = booking.RoomType,
            CheckIn       = booking.CheckIn,
            CheckOut      = booking.CheckOut,
            Adults        = booking.Adults,
            Children      = booking.Children,
            RatePerNight  = room.Rate,
        };
    }
    #endregion

    #region Validation

    /// <summary>True when every required field has a sensible value.</summary>
    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(_guestName) &&
        !string.IsNullOrWhiteSpace(_guestEmail) &&
        _guestEmail.Contains('@') &&
        !string.IsNullOrWhiteSpace(_phone) &&
        _phone.Length == 10 &&
        SelectedRoom is not null &&
        _checkOut.Date > _checkIn.Date;

    /// <summary>Returns true when the form is valid; fills
    /// <paramref name="error"/> otherwise so the caller can show it.</summary>
    /// <param name="error">Output: the validation message describing
    /// the first failure encountered.</param>
    /// <returns><c>true</c> when every required field has a sensible
    /// value; otherwise <c>false</c>.</returns>
    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(_guestName))            { error = "Guest name is required.";                                                    return false; }
        if (string.IsNullOrWhiteSpace(_guestEmail) ||
            !_guestEmail.Contains('@') || !_guestEmail.Contains('.'))
                                                            { error = "A valid email address is required (e.g. name@example.com).";                  return false; }
        if (string.IsNullOrWhiteSpace(_phone))                { error = "Phone number is required.";                                                  return false; }
        if (_phone.Length != 10)                              { error = "Phone number must contain exactly 10 digits.";                              return false; }
        if (SelectedRoom is null)                              { error = HasNoAvailableRooms
                                                                       ? "No available rooms on this floor — pick another floor or first release a room."
                                                                       : "Please choose an available room.";                                                return false; }
        if (_checkOut.Date <= _checkIn.Date)                   { error = "Check-out date must be after check-in date.";                                return false; }
        error = string.Empty;
        return true;
    }

    #endregion

    #region Stepper handlers (bound directly to − / + buttons)

    /// <summary>Increments the adult count by one. Bound to the plus
    /// glyph on the Adults stepper.</summary>
    public void IncrementAdults()   => Adults++;
    /// <summary>Decrements the adult count by one. Bound to the minus
    /// glyph on the Adults stepper.</summary>
    public void DecrementAdults()   => Adults--;
    /// <summary>Increments the children count by one. Bound to the
    /// plus glyph on the Children stepper.</summary>
    public void IncrementChildren() => Children++;
    /// <summary>Decrements the children count by one. Bound to the
    /// minus glyph on the Children stepper.</summary>
    public void DecrementChildren() => Children--;

    /// <summary>Compatibility shim — the popup's SfChipGroup
    /// <c>SelectionChanged</c> handler now goes through
    /// <see cref="SetSelectedFloor"/> directly with the
    /// <see cref="FloorOption.FloorNumber"/> string. This method remains
    /// as a single point of entry for any external caller (tests, custom
    /// FloorOption hosts) that already holds a <see cref="FloorOption"/>
    /// instance instead of a floor id.</summary>
    /// <param name="floor">Floor option to select. <c>null</c> clears
    /// the selection.</param>
    public void SelectFloor(FloorOption? floor)
    {
        if (floor is null)
        {
            SetSelectedFloor(string.Empty);
            return;
        }

        // Mirror the chip's <c>IsSelected</c> flag so the legacy XAML
        // pattern (Border driver with FloorChipBg/Fg converters) still
        // shows the right tint. SfChipGroup with ChipType=Choice manages
        // its own visual selection, so this is purely a fallback for
        // any caller rendering FloorOptions outside of SfChipGroup.
        foreach (var f in FloorOptions) f.IsSelected = false;
        floor.IsSelected = true;

        SetSelectedFloor(floor.FloorNumber);

        // Now that the user has interacted, the "Tap the calendar icon"
        // hint strip in the body is no longer relevant.
        HasInfoMessage = false;
    }

    #endregion
}

/// <summary>Pure data payload passed from the popup content to the host
/// page when the user commits. The page translates this into a real
/// <see cref="Booking"/> + Room-reservation. Mirror of the same type the
/// popup's code-behind used to build by hand — kept here so the VM owns
/// the booking-assembly logic end-to-end.</summary>
public sealed class AddBookingFormResult
{
    /// <summary>Trimmed guest full name.</summary>
    public required string GuestName     { get; init; }
    /// <summary>Trimmed guest email address.</summary>
    public required string GuestEmail    { get; init; }
    /// <summary>Formatted phone number (e.g. "+1 5551234567").</summary>
    public required string Phone         { get; init; }
    /// <summary>Human-readable room label (e.g. "Room 101").</summary>
    public required string Room          { get; init; }
    /// <summary>Room type label from the inventory (e.g. "King Deluxe").</summary>
    public required string RoomType      { get; init; }
    /// <summary>Check-in date.</summary>
    public required DateTime CheckIn     { get; init; }
    /// <summary>Check-out date.</summary>
    public required DateTime CheckOut    { get; init; }
    /// <summary>Adult count.</summary>
    public required int Adults          { get; init; }
    /// <summary>Children count.</summary>
    public required int Children        { get; init; }
    /// <summary>Per-night rate for the chosen room.</summary>
    public required decimal RatePerNight { get; init; }
}
