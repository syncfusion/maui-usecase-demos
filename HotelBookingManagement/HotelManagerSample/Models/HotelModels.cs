using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HotelManagerSample;

/// <summary>
/// One room in the 15-key inventory. Holds the immutable master
/// fields and exposes the derived <see cref="DisplayLabel"/> used by
/// the Add-Booking popup.
/// </summary>
public sealed class Room
{
    /// <summary>Room number (e.g. "101"). Initialised to empty so
    /// accidental nulls never leak into the XAML bindings.</summary>
    public string Number { get; init; } = string.Empty;
    /// <summary>Room type label (e.g. "King Deluxe").</summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>Floor identifier (e.g. "1").</summary>
    public string Floor { get; init; } = string.Empty;
    /// <summary>Housekeeping status — plain string so any external
    /// writer (Excel import, JSON loader, etc.) can mutate without
    /// casting through the <see cref="RoomCleaningStatus"/> enum.</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>Per-night rate in dollars.</summary>
    public decimal Rate { get; init; }

    /// <summary>Composite label shown in the Add-Booking popup's SfComboBox
    /// dropdown — composed of room number, type and rate so the user can
    /// tell suites apart at a glance. Matches the Figma "Room 101 – Deluxe"
    /// format rendered in the dropdown row.</summary>
    public string DisplayLabel => $"Room {Number} – {Type}";
}

/// <summary>
/// One reservation in the property. Holds the immutable master fields
/// the dashboard / bookings / add-booking popup share, plus a couple of
/// convenience properties for XAML formatters.
/// </summary>
public sealed class Booking
{
    /// <summary>Booking reference (e.g. "#BK-10482").</summary>
    public string Reference { get; init; } = string.Empty;
    /// <summary>Guest full name.</summary>
    public string Guest { get; init; } = string.Empty;
    /// <summary>Guest email address.</summary>
    public string GuestEmail { get; init; } = string.Empty;
    /// <summary>Guest phone number.</summary>
    public string Phone { get; init; } = string.Empty;
    /// <summary>Human-readable room label (e.g. "Room 101").</summary>
    public string Room { get; init; } = string.Empty;
    /// <summary>Room type label (e.g. "King Deluxe").</summary>
    public string RoomType { get; init; } = string.Empty;
    /// <summary>Room segment (e.g. "Floor 1 + Garden Wing").</summary>
    public string RoomSegment { get; init; } = string.Empty;
    /// <summary>Date-range label rendered in the bookings grid.</summary>
    public string Dates { get; init; } = string.Empty;
    /// <summary>Check-in date.</summary>
    public DateTime CheckIn { get; init; }
    /// <summary>Check-out date.</summary>
    public DateTime CheckOut { get; init; }
    /// <summary>Adult count.</summary>
    public int Adults { get; init; }
    /// <summary>Children count.</summary>
    public int Children { get; init; }
    /// <summary>Source label (e.g. "Direct").</summary>
    public string Source { get; init; } = string.Empty;
    /// <summary>Source segment (e.g. "DIRECT • OTA").</summary>
    public string SourceSegment { get; init; } = string.Empty;
    /// <summary>Hex colour used for the source tag background.</summary>
    public string SourceTagColor { get; init; } = "#4A43E6";
    /// <summary>Lifecycle status (e.g. "Confirmed", "Checked in", "Cancelled").</summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>Follow-up note (e.g. "Pre-Auth Needed").</summary>
    public string FollowUp { get; init; } = string.Empty;
    /// <summary>Loyalty tier label (e.g. "DIAMOND").</summary>
    public string Tier { get; init; } = string.Empty;
    /// <summary>Hex colour used for the loyalty tier tag.</summary>
    public string TierColor { get; init; } = "#8C6B1F";
    /// <summary>Free-form notes attached to the booking.</summary>
    public string Notes { get; init; } = string.Empty;
    /// <summary>Total folio value in dollars.</summary>
    public decimal Total { get; init; }

    /// <summary>Night count, floored at zero.</summary>
    public int Nights => Math.Max(0, (int)Math.Round((CheckOut.Date - CheckIn.Date).TotalDays));
    /// <summary>Party label rendered in the guest column ("2 Adults, 1 Kids").</summary>
    public string PartyLabel => Children > 0 ? $"{Adults} Adults, {Children} Kids" : $"{Adults} Adults";
    /// <summary>Three-letter day name for the check-in date.</summary>
    public string CheckInDayLabel => CheckIn.ToString("ddd").TrimEnd('.');
    /// <summary>Three-letter day name for the check-out date.</summary>
    public string CheckOutDayLabel => CheckOut.ToString("ddd").TrimEnd('.');

    /// <summary>Two-letter monogram rendered in the guest avatar tile.
    /// Falls back to "?" when the guest name has no letters.</summary>
    public string Initials
    {
        get
        {
            // Pre-built default for known names.
            var joined = string.Join(string.Empty, Guest.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => part.Length > 0 ? part[0].ToString() : string.Empty));
            if (!string.IsNullOrEmpty(joined))
                return joined.Length > 2 ? joined[..2].ToUpperInvariant() : joined.ToUpperInvariant();
            return "?";
        }
    }
}

/// <summary>
/// One floor pill in the Add-Booking popup's "SELECT FLOOR" row.
/// <see cref="IsSelected"/> drives both the chip background and the
/// foreground icon/text color via the <c>FloorChipBg</c> / <c>FloorChipFg</c>
/// converters the popup declares in its resource dictionary. The control
/// emits a Tapped gesture carrying the whole option as CommandParameter so
/// the code-behind can flip selection without a two-way binding to an
/// intermediate bool field.
/// </summary>
public sealed class FloorOption : INotifyPropertyChanged
{
    /// <summary>Floor identifier — also the segment key passed back to
    /// the view-model when the chip is tapped.</summary>
    public string FloorNumber { get; init; } = string.Empty;
    /// <summary>Display label rendered on the chip ("Floor 1" or
    /// "Floor" when <see cref="FloorNumber"/> is empty).</summary>
    public string Display => string.IsNullOrEmpty(FloorNumber) ? "Floor" : $"Floor {FloorNumber}";

    /// <summary>Backing field for <see cref="IsSelected"/>.</summary>
    private bool _isSelected;
    /// <summary>True when the chip is currently highlighted.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    /// <summary>Standard <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event used by the chip's converters to observe <see cref="IsSelected"/>
    /// toggles.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// One data point on the revenue chart. The dashboard projects a
/// different set of these for the 7D / 1M / 3M views.
/// </summary>
public sealed class RevenuePoint
{
    /// <summary>X-axis label (e.g. "Fri 04").</summary>
    public string Day { get; init; } = string.Empty;
    /// <summary>Short day name (e.g. "Fri").</summary>
    public string DayName { get; init; } = string.Empty;
    /// <summary>Direct-channel revenue for the day, expressed in
    /// thousands of dollars.</summary>
    public double DirectRevenue { get; init; }
    /// <summary>OTA-channel revenue for the day, expressed in thousands
    /// of dollars.</summary>
    public double OtaRevenue { get; init; }
    /// <summary>Total revenue for the day (Direct + OTA).</summary>
    public double Total => DirectRevenue + OtaRevenue;
}

/// <summary>
/// One slice of the inventory donut. The dashboard paints four of these
/// per render and re-fetches them every time the master rooms collection
/// changes.
/// </summary>
public sealed class InventoryBreakdownPoint
{
    /// <summary>Slice label (e.g. "Occupied").</summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>Numeric value (number of keys).</summary>
    public double Value { get; init; }
    /// <summary>Hex colour used by the chart slice.</summary>
    public string Color { get; init; } = string.Empty;
}

/// <summary>
/// One row of the inventory-breakdown legend (color dot + name + value +
/// percentage). All fields are derived in
/// <c>DashboardViewModel.BuildInventoryLegend</c> from the master Room
/// collection so the XAML never has to repeat "78 keys / 65%".
/// </summary>
public sealed class InventoryLegendRow
{
    /// <summary>Legend row label (e.g. "Occupied").</summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>Hex colour drawn in the dot.</summary>
    public string Color { get; init; } = string.Empty;
    /// <summary>Numeric value (number of keys).</summary>
    public int Value { get; init; }
    /// <summary>Percentage of the inventory this row represents.</summary>
    public double Percent { get; init; }
    /// <summary>Total room count snapshot used to compute the
    /// percentage label.</summary>
    public int TotalKeys { get; init; }
    /// <summary>"<see cref="Value"/>" caption rendered under the
    /// category label.</summary>
    public string ValueLabel => $"{Value}";
    /// <summary>"<see cref="Percent"/>%" caption rendered next to the
    /// value label.</summary>
    public string PercentLabel => $"{Percent:0.#}%";
}

/// <summary>
/// Single source of truth for hotel-wide configuration. Anything that should
/// drive KPI tiles / inventory headlines flows from values declared here and
/// is then projected in <see cref="HotelData"/> and the relevant view-models.
/// </summary>
public static class HotelConfig
{
    /// <summary>Property-wide inventory (3 floors × 5 rooms). Every KPI key count,
    /// pager total, sidebar badge and dashboard summary that references
    /// "rooms" must be derived from this constant — never hard-coded.</summary>
    public const int TotalFloors = 3;
    /// <summary>Rooms per floor. Combined with <see cref="TotalFloors"/> yields
    /// <see cref="TotalRooms"/>.</summary>
    public const int RoomsPerFloor = 5;
    /// <summary>Total inventory size (15 keys for the demo property).</summary>
    public const int TotalRooms = TotalFloors * RoomsPerFloor; // 15

    /// <summary>Reference "today" — kept deterministic so the dashboard renders
    /// the same data on every cold start.</summary>
    public static readonly DateTime ReferenceToday = new(2026, 9, 10);
}

/// <summary>
/// Factory methods that build the deterministic demo data the app boots with.
/// The mutable master collections live on the singleton <see cref="IHotelStore"/>
/// registered in <see cref="MauiProgram"/> — pages must read
/// <c>hotelStore.Rooms</c> / <c>hotelStore.Bookings</c> through DI, not through
/// these static helpers. <see cref="BuildRooms"/> and <see cref="BuildBookings"/>
/// remain here so <c>HotelStore</c> can call them once at construction time and
/// unit tests can rebuild isolated snapshots.
/// </summary>
public static class HotelData
{
    /// <summary>
    /// 15-key inventory: 5 rooms per floor across 3 floors (numbered 101–105,
    /// 201–205, 301–305). Room type / status distribution approximates a
    /// realistic boutique property mix: ~40% Available, ~33% Occupied, ~13%
    /// ~7% Reserved, ~7% Maintenance.
    /// </summary>
    /// <returns>A freshly-seeded <see cref="ObservableCollection{Room}"/>
    /// containing 15 rooms.</returns>
    public static ObservableCollection<Room> BuildRooms()
    {
        var rooms = new ObservableCollection<Room>();
        var typePool = new[]
        {
            ("King Deluxe",        245m),
            ("Twin Garden",        215m),
            ("Suite Harbor",       390m),
            ("Executive Suite",    320m),
            ("Standard Queen",     195m),
            ("Family Suite",       365m),
            ("Penthouse",          620m),
            ("Presidential Suite", 880m)
        };

        // 15-key boutique mix (weights tuned so roughly 40/33/13/7/7 fall around the 15 rooms).
        var statusPool = new (string Status, double Weight)[]
        {
            ("Available",   0.40),
            ("Occupied",    0.33),
            ("Reserved",    0.07),
            ("Maintenance", 0.07)
        };

        var rng = new Random(20260910);          // deterministic seed for stable demo data
        var perFloor = HotelConfig.RoomsPerFloor; // 5 rooms per floor × 3 floors = 15

        // Force-pin two keys to "Reserved" so the demo board always shows
        // upcoming arrivals even though the weighted draw would otherwise
        // produce 0–1 reserved rooms. 0201 and 0205 are drawn as "Occupied"
        // by the seeded RNG above; pinning them here promotes them to
        // "Reserved" so the Rooms and Bookings boards have a stable pair
        // of upcoming arrivals. Numbers are stored as 4-digit strings to
        // match the "/-format" produced by floorCode + i.ToString("00")
        // above (which yields "101"…"305", not the visual 3-digit codes
        // the same numbers appear as in the Figma copy).
        var pinnedReserved = new HashSet<string> { "201", "205" };

        // Force-pin one key to "Available" so the Rooms page always shows
        // exactly 5 ready rooms. The seeded RNG draws 0102 as "Occupied";
        // without this override the available count would land at 4 — one
        // short of what the Figma-based KPI strip expects.
        var pinnedAvailable = new HashSet<string> { "102" };

        var pinnedMaintain = new HashSet<string> { "104" };

        for (var floor = 1; floor <= HotelConfig.TotalFloors; floor++)
        {
            var floorCode = floor.ToString("0");
            for (var i = 1; i <= perFloor; i++)
            {
                // Room numbers: 101..105, 201..205, 301..305
                var number = floorCode + i.ToString("00");
                var (type, rate) = typePool[rng.Next(typePool.Length)];

                // Pick status. Pin sets win first (Reserved pins keep the
                // future-arrivals pair stable, Available pins bump the
                // ready-room count up to 5). For any room that is pinned
                // we still consume one rng roll so the deterministic seed
                // keeps producing the same sequence for the un-pinned
                // rooms below us — otherwise pinning one room would shift
                // every subsequent room's status.
                string status;
                if (pinnedReserved.Contains(number))
                {
                    var _reservedRoll = rng.NextDouble();
                    status = "Reserved";
                }
                else if (pinnedAvailable.Contains(number))
                {
                    var _availableRoll = rng.NextDouble();
                    status = "Available";
                }
                else if (pinnedMaintain.Contains(number))
                {
                    var _availableRoll = rng.NextDouble();
                    status = "Maintenance";
                }
                else
                {
                    var roll = rng.NextDouble();
                    var cumulative = 0.0;
                    status = statusPool[0].Status;
                    foreach (var entry in statusPool)
                    {
                        cumulative += entry.Weight;
                        if (roll <= cumulative)
                        {
                            status = entry.Status;
                            break;
                        }
                    }
                }

                rooms.Add(new Room
                {
                    Number = number,
                    Type = type,
                    Floor = floorCode,
                    Status = status,
                    Rate = rate
                });
            }
        }

        return rooms;
    }

    /// <summary>Builds the master 24-ish bookings list used by the
    /// dashboard and bookings pages. Each named booking is pinned to a
    /// real <see cref="Room"/> from <see cref="BuildRooms"/> so the
    /// rooms / bookings boards line up perfectly.</summary>
    /// <returns>A freshly-seeded <see cref="ObservableCollection{Booking}"/>.</returns>
    public static ObservableCollection<Booking> BuildBookings()
    {
        // First the named demo bookings — each pinned to a real Room from the
        // 15-key inventory so every booking points at a room number that
        // exists on the Rooms page.
        var realRooms = BuildRooms().ToList();
        Room PickRoom(Random rng) => realRooms[rng.Next(realRooms.Count)];

        var rng0 = new Random(20260910);
        var list = new List<Booking>
        {
            RealBooking("#BK-10482", "Olivia Bennett",   PickRoom(rng0), HotelConfig.ReferenceToday.AddDays(2), 3, new(2026, 9, 12), "#6E7A8F", "Direct",      "DIRECT • OTA", "Confirmed",   "Paid in full",      594, "DIAMOND", "#4A43E6"),
            RealBooking("#BK-10481", "Mateo Rodriguez",  PickRoom(rng0), HotelConfig.ReferenceToday,         2, new(2026, 9, 12), "#0E6E80", "Phone Desk",  "PHONE DESK",   "Confirmed",   "On Arrival",        310, null,     null),
            RealBooking("#BK-10480", "Hon. Marcus Vance",PickRoom(rng0),HotelConfig.ReferenceToday,         4, new(2026, 9, 14), "#7A4ECC", "Phone Desk",   "PHONE DESK",    "Checked in", "Corporate Billing", 3450,"PLATINUM","#5C36C7"),
            RealBooking("#BK-10479", "Dr. Elena Rostova",PickRoom(rng0), HotelConfig.ReferenceToday,         3, new(2026, 9, 13), "#188A61", "Direct",   "DIRECT • OTA",    "Checked in", "Authorized",        870, "GOLD",    "#B86A11"),
            RealBooking("#BK-10478", "Liam Davies",      PickRoom(rng0), HotelConfig.ReferenceToday.AddDays(4),4, new(2026, 9, 18), "#003580", "Phone Desk", "PHONE DESK",  "Confirmed",   "Pre-Auth held",     790, null,     null),
            RealBooking("#BK-10475", "Dr. David & Sarah Kim",PickRoom(rng0), HotelConfig.ReferenceToday.AddDays(1),5, new(2026, 9, 16), "#7A879B", "Phone Desk",     "PHONE DESK",      "Confirmed",   "Pre-Auth needed",  1250, null,     null),
            RealBooking("#BK-10472", "Sophia Chen",      PickRoom(rng0), HotelConfig.ReferenceToday,         1, new(2026, 9, 11), "#B86A11", "Direct",  "DIRECT • OTA",   "Confirmed",   "Paid in full",      195, "SILVER",  "#526075"),
            RealBooking("#BK-10469", "Arthur Pendelton", PickRoom(rng0), HotelConfig.ReferenceToday.AddDays(-2),2,new(2026, 9, 10), "#B0233E", "Direct",      "CANCELLATION", "Cancelled",   "Refund Proceeded",  280, null,     null, "Cancelled (Guest Notice)"),
            RealBooking("#BK-10468", "Julian Thorne",    PickRoom(rng0), HotelConfig.ReferenceToday.AddDays(-1),4,new(2026, 9, 13), "#6E7A8F", "Direct",      "DIRECT • OTA", "Checked out", "Settled",             320, null,     null),
        };

        // A few extra generated bookings centered on today's date so the
        // dashboard's "Today's check-ins" filter / "Pending payment" pill /
        // bookings pager every page have something meaningful. The size of
        // this pool scales with the inventory (so the pager never feels
        // empty for the 15-key mix) but every generated row still uses a
        // real Room reference × a real RoomType label.
        var rng = new Random(20260910);
        var firstNames = new[] { "Avery", "Mia", "Noah", "Liam", "Zara", "Idris", "Hana", "Jules", "Kai", "Mira", "Anya", "Theo", "Ruby", "Otto" };
        var lastNames  = new[] { "Ashford", "Bennett", "Carver", "Devereux", "Eastwood", "Fairchild", "Gallagher", "Hawthorne", "Ibarra", "Jenkinson", "Karim", "Lockhart", "Mercier", "Nakamura" };
        var statuses   = new[] { "Confirmed", "Pending", "Checked in", "Checked out", "Cancelled" };
        var sources    = new[] { ("Direct",      "DIRECT • OTA",   "#6E7A8F"),
                                 ("Phone Desk",  "PHONE DESK",     "#0E6E80"),
                                 ("Concierge",   "CONCIERGE",      "#7A4ECC"),
                                 ("Booking.com", "BOOKING.COM",    "#003580") };

        // ≈ HotelConfig.TotalRooms generated bookings layered on top of the
        // 9 named ones above (so total ≈ 24 bookings — a comfortable two
        // pages of 10 with ~4 on page 3).
        var generatedCount = Math.Max(HotelConfig.TotalRooms, 15);
        for (var i = 0; i < generatedCount; i++)
        {
            var firstName = firstNames[rng.Next(firstNames.Length)];
            var lastName  = lastNames[rng.Next(lastNames.Length)];
            var room      = PickRoom(rng);
            var nights    = rng.Next(1, 5);
            var checkIn   = HotelConfig.ReferenceToday.AddDays(rng.Next(-5, 14));
            var checkOut  = checkIn.AddDays(nights);
            var status    = statuses[rng.Next(statuses.Length)];
            var src       = sources[rng.Next(sources.Length)];
            var total     = (decimal)(180 + (nights * (60 + rng.Next(0, 220))) + rng.Next(0, 100));

            list.Add(RealBooking(
                $"#BK-{(10500 + i):D5}",
                $"{firstName} {lastName}",
                room,
                checkIn,
                nights,
                checkOut,
                src.Item3,
                src.Item1,
                src.Item2,
                status,
                status == "Pending" ? "Pre-Auth Needed" : "Paid In Full",
                total));
        }

        return new ObservableCollection<Booking>(list);
    }

    /// <summary>
    /// Helper that builds a <see cref="Booking"/> whose numbers + room
    /// segment are derived entirely from the supplied <see cref="Room"/>.
    /// Centralised here so every booking on the dashboard / bookings page
    /// reflects the actual 15-key inventory — no strings stay orphan.
    /// </summary>
    /// <param name="reference">Booking reference number.</param>
    /// <param name="guest">Guest full name.</param>
    /// <param name="room">Master <see cref="Room"/> the booking
    /// references.</param>
    /// <param name="checkIn">Check-in date.</param>
    /// <param name="nights">Number of nights. Combined with
    /// <paramref name="checkIn"/> to compute
    /// <paramref name="checkOut"/>.</param>
    /// <param name="checkOut">Explicit check-out date.</param>
    /// <param name="sourceColor">Hex colour for the source tag.</param>
    /// <param name="sourceLabel">Short source label (e.g. "Direct").</param>
    /// <param name="sourceSegment">Long source segment (e.g. "DIRECT • OTA").</param>
    /// <param name="status">Lifecycle status string.</param>
    /// <param name="followUp">Follow-up note.</param>
    /// <param name="total">Total folio value.</param>
    /// <param name="tier">Optional loyalty tier.</param>
    /// <param name="tierColor">Optional tier hex colour.</param>
    /// <param name="notes">Optional notes.</param>
    /// <returns>A freshly-built <see cref="Booking"/>.</returns>
    private static Booking RealBooking(
        string reference,
        string guest,
        Room room,
        DateTime checkIn,
        int nights,
        DateTime checkOut,
        string sourceColor,
        string sourceLabel,
        string sourceSegment,
        string status,
        string followUp,
        decimal total,
        string? tier = null,
        string? tierColor = null,
        string? notes = null)
    {
        var wing = (int.Parse(room.Number) % 2 == 0) ? "Garden Wing" : "Pool Terrace";
        return new Booking
        {
            Reference     = reference,
            Guest         = guest,
            GuestEmail    = $"{guest.Split(' ')[0].ToLowerInvariant()}@zenstays.com",
            Phone         = $"+1 (555) {100 + reference.GetHashCode() % 900:000}-{1000 + reference.GetHashCode() % 8999:0000}",
            Room          = $"Room No: {room.Number}",
            RoomType      = room.Type,
            RoomSegment   = $"Floor {room.Floor} + {wing}",
            Dates         = $"{checkIn:MMM dd} – {checkOut:MMM dd, yyyy}",
            CheckIn       = checkIn,
            CheckOut      = checkOut,
            Adults        = 2,
            Children      = 0,
            Source        = sourceLabel,
            SourceSegment = sourceSegment,
            SourceTagColor = sourceColor,
            Status        = status,
            FollowUp      = followUp,
            Tier          = tier ?? string.Empty,
            TierColor     = tierColor ?? string.Empty,
            Total         = total,
            Notes         = notes ?? string.Empty,
        };
    }

    // 7D: daily points (Fri..Today) — values in $k
    /// <summary>7-day revenue series (Fri..Today).</summary>
    public static IReadOnlyList<RevenuePoint> Revenue7D { get; } = new[]
    {
        new RevenuePoint { Day = "Fri 04", DayName = "Fri",   DirectRevenue = 13.4, OtaRevenue = 9.8 },
        new RevenuePoint { Day = "Sat 05", DayName = "Sat",   DirectRevenue = 15.8, OtaRevenue = 10.4 },
        new RevenuePoint { Day = "Sun 06", DayName = "Sun",   DirectRevenue = 17.2, OtaRevenue = 11.6 },
        new RevenuePoint { Day = "Mon 07", DayName = "Mon",   DirectRevenue = 11.9, OtaRevenue = 7.9 },
        new RevenuePoint { Day = "Tue 08", DayName = "Tue",   DirectRevenue = 13.4, OtaRevenue = 8.4 },
        new RevenuePoint { Day = "Wed 09", DayName = "Wed",   DirectRevenue = 18.2, OtaRevenue = 11.5 },
        new RevenuePoint { Day = "Today 10", DayName = "Thu", DirectRevenue = 19.8, OtaRevenue = 12.1 }
    };

    // 1M: weekly points for a month view (W1..W4)
    /// <summary>1-month revenue series (W1..W4).</summary>
    public static IReadOnlyList<RevenuePoint> Revenue1M { get; } = new[]
    {
        new RevenuePoint { Day = "W1", DayName = "W1", DirectRevenue = 14.4, OtaRevenue = 10.6 },
        new RevenuePoint { Day = "W2", DayName = "W2", DirectRevenue = 12.8, OtaRevenue = 8.1 },
        new RevenuePoint { Day = "W3", DayName = "W3", DirectRevenue = 16.9, OtaRevenue = 11.2 },
        new RevenuePoint { Day = "W4", DayName = "W4", DirectRevenue = 18.4, OtaRevenue = 12.0 }
    };

    // 3M: monthly points for three months view
    /// <summary>3-month revenue series (Jul..Sep).</summary>
    public static IReadOnlyList<RevenuePoint> Revenue3M { get; } = new[]
    {
        new RevenuePoint { Day = "Jul", DayName = "Jul", DirectRevenue = 17.1, OtaRevenue = 11.2 },
        new RevenuePoint { Day = "Aug", DayName = "Aug", DirectRevenue = 15.6, OtaRevenue = 10.8 },
        new RevenuePoint { Day = "Sep", DayName = "Sep", DirectRevenue = 19.2, OtaRevenue = 12.4 }
    };

    // Inventory breakdown is computed in DashboardViewModel.BuildInventoryBreakdown
    // from the rooms collection — no static magic numbers here.
}
