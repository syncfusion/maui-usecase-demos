namespace HotelManagerSample;

/// <summary>
/// Housekeeping status used by the rooms page / mobile dashboard. Kept
/// as a strongly-typed enum so XAML bindings can compare cleanly.
/// </summary>
public enum RoomCleaningStatus
{
    /// <summary>Room is ready for the next guest.</summary>
    Available,
    /// <summary>Guest is currently in residence.</summary>
    Occupied,
    /// <summary>Future arrival — room is held.</summary>
    Reserved,
    /// <summary>Engineering ticket open — room is out of service.</summary>
    Maintenance
}

/// <summary>
/// Rich projection of a master <see cref="Room"/>. Holds every field
/// the rooms card / table / maintenance work-order surfaces so the XAML
/// never has to re-derive anything from a plain <see cref="Room"/>.
/// </summary>
public sealed class RoomDetailsModel
{
    /// <summary>Room number (e.g. "101").</summary>
    public string Number { get; init; } = string.Empty;
    /// <summary>Room type label (e.g. "King Deluxe").</summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>Floor identifier (e.g. "1").</summary>
    public string Floor { get; init; } = string.Empty;
    /// <summary>Strongly-typed housekeeping status.</summary>
    public RoomCleaningStatus Status { get; init; }
    /// <summary>Per-night rate.</summary>
    public decimal Rate { get; init; }
    /// <summary>Rate unit (only per-night is supported today).</summary>
    public RateUnitKind RateUnit { get; init; } = RateUnitKind.PerNight;

    /// <summary>Maximum capacity of the room (adults + children).</summary>
    public int Capacity { get; init; }
    /// <summary>Number of adults currently in residence.</summary>
    public int Adults { get; init; }
    /// <summary>Number of children currently in residence.</summary>
    public int Children { get; init; }

    /// <summary>True when a guest is currently assigned to the room.</summary>
    public bool IsGuestAssigned { get; init; }
    /// <summary>Guest full name (empty when no guest is in residence).</summary>
    public string GuestName { get; init; } = string.Empty;
    /// <summary>Two-letter guest monogram for the avatar tile.</summary>
    public string GuestInitials { get; init; } = string.Empty;

    /// <summary>Date-range label rendered in the card body.</summary>
    public string CheckInRange { get; init; } = string.Empty;
    /// <summary>Full check-in date label (e.g. "Sep 10, 2026").</summary>
    public string CheckInDayName { get; init; } = string.Empty;
    /// <summary>Check-in hour-of-day used by the timeline band.</summary>
    public int CheckInTime { get; init; } = 0;

    /// <summary>
    /// Image filename (with extension) shown on the card hero.
    /// Override per instance via this property; otherwise
    /// <see cref="PhotoSource"/> resolves to <c>hotel.png</c> for every room.
    /// </summary>
    public string Photo { get; init; } = string.Empty;

    /// <summary>
    /// Resolved photo asset. Returns the explicit <see cref="Photo"/> when set,
    /// otherwise always resolves to <c>hotel.png</c> so every room shares the
    /// same image regardless of category.
    /// </summary>
    public string PhotoSource =>
        string.IsNullOrWhiteSpace(Photo) ? "hotel.png" : Photo;

    /// <summary>Optional amenity list (e.g. "King Bed - Balcony - City View").</summary>
    public string AmenitiesLine { get; init; } = string.Empty;

    // Optional note fields for UI
    /// <summary>Housekeeping note rendered when status is "Available".</summary>
    public string HousekeepingNote { get; init; } = string.Empty;
    /// <summary>Maintenance work-order description.</summary>
    public string MaintenanceNote { get; init; } = string.Empty;
    /// <summary>Active maintenance ticket identifier.</summary>
    public string MaintenanceTicketId { get; init; } = string.Empty;
    /// <summary>Name of the person who reported the maintenance issue.</summary>
    public string ReportedBy { get; init; } = string.Empty;
    /// <summary>Technician assigned to the active ticket.</summary>
    public string Technician { get; init; } = string.Empty;
    /// <summary>Maintenance work-order reference.</summary>
    public string WorkOrderReference { get; init; } = string.Empty;
    /// <summary>ETA label for ticket completion.</summary>
    public string EstimatedCompletion { get; init; } = string.Empty;
    /// <summary>Remaining-downtime label.</summary>
    public string Downtime { get; init; } = string.Empty;
    /// <summary>Concierge note attached to the room.</summary>
    public string ConciergeNote { get; init; } = string.Empty;
    /// <summary>Free-form special request attached to the room.</summary>
    public string SpecialRequest { get; init; } = string.Empty;

    /// <summary>True while housekeeping turnover is in progress.</summary>
    public bool IsTurnoverInProgress { get; init; }
    /// <summary>True when <see cref="ConciergeNote"/> is non-empty.</summary>
    public bool HasConciergeNote { get; init; }
    /// <summary>True when <see cref="SpecialRequest"/> is non-empty.</summary>
    public bool HasSpecialRequest { get; init; }
    /// <summary>True when <see cref="AmenitiesLine"/> is non-empty.</summary>
    public bool HasAmenities => !string.IsNullOrWhiteSpace(AmenitiesLine);
    /// <summary>True when <see cref="PhotoSource"/> resolves to a
    /// renderable image.</summary>
    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoSource);

    /// <summary>Party label rendered in the card ("2 Adults, 1 Kids").</summary>
    public string PartyLabel => Children > 0 ? $"{Adults} Adults, {Children} Kids" : $"{Adults} Adults";
    /// <summary>"$XXX/night" label rendered in the card price column.</summary>
    public string RateLabel
    {
        get
        {
            var unit = RateUnit switch
            {
                RateUnitKind.PerNight => "/night",
                _ => string.Empty
            };
            return $"${Rate:0}{unit}";
        }
    }

    /// <summary>True when a maintenance ticket is attached to the room.</summary>
    public bool HasMaintenanceInfo => !string.IsNullOrWhiteSpace(MaintenanceTicketId);
    /// <summary>True when the room's rate is set to a non-zero value.</summary>
    public bool HasRate => Rate > 0;
}

/// <summary>
/// Supported rate units. Only per-night is wired today.
/// </summary>
public enum RateUnitKind
{
    /// <summary>Rate is per night.</summary>
    PerNight
}

/// <summary>
/// Aggregate metric shown in the KPI strip on the Rooms page.
/// </summary>
public sealed class RoomInventoryStat
{
    /// <summary>Label rendered on the tile.</summary>
    public string Label { get; init; } = string.Empty;
    /// <summary>Numeric value (e.g. "78").</summary>
    public string Value { get; init; } = string.Empty;
    /// <summary>Percentage of the inventory this stat represents.</summary>
    public double Percent { get; init; }
    /// <summary>"<see cref="Percent"/>%" caption rendered next to the
    /// value.</summary>
    public string PercentLabel { get; init; } = string.Empty;
    /// <summary>Hex accent colour used for borders / dividers.</summary>
    public string AccentColor { get; init; } = "#4A43E6";
    /// <summary>Material icon kind.</summary>
    public string IconKind { get; init; } = "Bed";
    /// <summary>Hex background colour for the icon tile.</summary>
    public string IconBackgroundColor { get; init; } = "#F1F4FA";
    /// <summary>Hex foreground colour for the icon glyph.</summary>
    public string IconForegroundColor { get; init; } = "#4A43E6";
    /// <summary>Image-asset file name rendered on the tile.</summary>
    public string ImageIcon { get; init; } = string.Empty;
}

/// <summary>
/// A floor filter chip (e.g. "All Floors 128", "Floor 1 32").
/// </summary>
public sealed class FloorFilterItem
{
    /// <summary>Chip text rendered to the user.</summary>
    public string Label { get; init; } = string.Empty;
    /// <summary>Number of rooms in the matched floor.</summary>
    public int Count { get; init; }
    /// <summary>True when this is the "All Floors" chip.</summary>
    public bool IsAll { get; init; }
    /// <summary>Display label rendered on the chip including the
    /// count badge.</summary>
    public string Display => Count > 0 ? $"{Label}  {Count}" : Label;
}
