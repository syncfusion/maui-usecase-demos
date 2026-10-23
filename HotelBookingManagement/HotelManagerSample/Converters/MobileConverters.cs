using System.Globalization;
using HotelManagerSample;

namespace HotelManagerSample.Converters;

/// <summary>
/// Returns a tinted background <see cref="Color"/> for a status pill on the
/// mobile booking card. Mirrors the desktop
/// <c>ReservationStatusToBrushConverter</c> but tuned to the mobile palette
/// tokens already declared in MobileColors.xaml.
/// </summary>
public sealed class MobileStatusPillBackgroundConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "Confirmed"  => Color.FromArgb("#D7F7E8"),
            "Checked in" => Color.FromArgb("#D7F7E8"),
            "Pending"    => Color.FromArgb("#FFF1D6"),
            "Payment Due"=> Color.FromArgb("#FFF1D6"),
            "Checked out"=> Color.FromArgb("#E8EFFD"),
            "Cancelled"  => Color.FromArgb("#FFE2E7"),
            _            => Color.FromArgb("#EAF1FF")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Returns the matching text color for a status pill (taken from
/// MobileStatusPillBackgroundConverter). Use together.
/// </summary>
public sealed class MobileStatusPillTextConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "Confirmed"  => Color.FromArgb("#15803D"),
            "Checked In" => Color.FromArgb("#15803D"),
            "Pending"    => Color.FromArgb("#B45309"),
            "Payment Due"=> Color.FromArgb("#B45309"),
            "Checked Out"=> Color.FromArgb("#1D4ED8"),
            "Cancelled"  => Color.FromArgb("#B91C1C"),
            _            => Color.FromArgb("#284DA0")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the left dot color drawn just before the status pill text.
/// Mirrors ReservationStatusToDotColorConverter but uses the mobile palette.
/// </summary>
public sealed class MobileStatusDotConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "Confirmed"  => Color.FromArgb("#22C55E"),
            "Checked In" => Color.FromArgb("#22C55E"),
            "Pending"    => Color.FromArgb("#F59E0B"),
            "Payment Due"=> Color.FromArgb("#F59E0B"),
            "Checked Out"=> Color.FromArgb("#38BDF8"),
            "Cancelled"  => Color.FromArgb("#EF4444"),
            _            => Color.FromArgb("#4F46E5")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Background color used for the room availability dot in the mobile Rooms
/// page list. Falls back to informational blue so an unknown enum value
/// still renders something.
/// </summary>
public sealed class MobileRoomStatusBackgroundConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };

        return status switch
        {
            RoomCleaningStatus.Available   => Color.FromArgb("#DCFCE7"),
            RoomCleaningStatus.Occupied    => Color.FromArgb("#EEF0FE"),
            RoomCleaningStatus.Reserved    => Color.FromArgb("#E0F2FE"),
            RoomCleaningStatus.Maintenance => Color.FromArgb("#FEE2E2"),
            _                              => Color.FromArgb("#E0F2FE")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Foreground (icon/text) color for the room availability dot tile on the
/// mobile Rooms page.
/// </summary>
public sealed class MobileRoomStatusForegroundConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };

        return status switch
        {
            RoomCleaningStatus.Available   => Color.FromArgb("#15803D"),
            RoomCleaningStatus.Occupied    => Color.FromArgb("#4338CA"),
            RoomCleaningStatus.Reserved    => Color.FromArgb("#0369A1"),
            RoomCleaningStatus.Maintenance => Color.FromArgb("#B91C1C"),
            _                              => Color.FromArgb("#0369A1")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the friendly room-status icon glyph (Material asset character)
/// for a status value. Used by the mobile Rooms tile to swap the leading
/// dot for an emoji-style icon at the same position.
/// </summary>
public sealed class MobileRoomStatusIconConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };

        // MAUI Material asset glyphs (already loaded via .ConfigureFonts in
        // MauiProgram). Encoded inline as escape sequences so they survive
        // XAML inflation.
        return status switch
        {
            RoomCleaningStatus.Available   => "\ue876",   // check_circle
            RoomCleaningStatus.Occupied    => "\ue53a",   // bed
            RoomCleaningStatus.Reserved    => "\ue616",   // calendar
            RoomCleaningStatus.Maintenance => "\ue5d3",   // build/wrench
            _                              => "\ue88f"    // help
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Picks a booking-CTA label based on the booking status. Used by the mobile
/// booking card to keep the action button wording aligned with the
/// guest's position in the lifecycle (Check In / Prepare Key / Send Link /
/// Re-Book / Folio).
/// </summary>
public sealed class MobileBookingCtaConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "Confirmed"   => "Check In",
            "Checked In"  => "Folio",
            "Pending"     => "Send Link",
            "Checked Out" => "Folio",
            "Cancelled"   => "Re-Book",
            _             => "View"
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Picks a CTA icon glyph for the booking card (Material asset character).
/// </summary>
public sealed class MobileBookingCtaIconConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "Confirmed"   => "\ue85d",  // login
            "Checked in"  => "\ue53e",  // receipt_long
            "Pending"     => "\ue89e",  // forward_in_mail
            "Checked out" => "\ue53e",  // receipt_long
            "Cancelled"   => "\ue5d5",  // replay
            _             => "\ue88e"   // visibility
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the friendly date format used by the mobile booking card
/// ("Sep 12 - Sep 15"). Bound to Booking.Dates when the model exposes it,
/// otherwise falls back to a short month-day format.
/// </summary>
public sealed class MobileStayRangeConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Booking booking)
        {
            // Keep stable sept en-dash copy straight from the model.
            return $"{booking.CheckIn:MMM dd} - {booking.CheckOut:MMM dd}";
        }
        return value as string ?? string.Empty;
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a room status enum to its singular label (used by the mobile Rooms
/// tile). Keeps the surface consistent with the desktop StatusFilters list
/// but exposed as a tiny short label for compact displays (e.g. "Clean",
/// "Maint").
/// </summary>
public sealed class MobileRoomStatusShortConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };

        return status switch
        {
            RoomCleaningStatus.Available   => "Ready",
            RoomCleaningStatus.Occupied    => "In-House",
            RoomCleaningStatus.Reserved    => "Reserved",
            RoomCleaningStatus.Maintenance => "Maint.",
            _                              => status.ToString()
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Mobile helper: returns true if the bound string is non-null and
/// non-empty. Used to control the visibility of the inline clear-search
/// glyph on the bookings page.
/// </summary>
public sealed class MobileStringToBoolConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}