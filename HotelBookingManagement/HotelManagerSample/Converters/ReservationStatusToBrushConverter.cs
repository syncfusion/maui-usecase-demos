using System.Globalization;

namespace HotelManagerSample.Converters;

/// <summary>
/// Maps a reservation status (CONFIRMED, CHECKED IN, PENDING, CHECKED OUT, etc.)
/// to a paired background/text brush set used for the Status pill in the
/// Recent Reservations DataGrid.
/// </summary>
public sealed class ReservationStatusToBrushConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;

        // Default pair (neutral)
        var pair = (background: Color.FromArgb("#EAF1FF"), text: Color.FromArgb("#284DA0"));

        switch (status)
        {
            case "CONFIRMED":
                pair = (Color.FromArgb("#D7F7E8"), Color.FromArgb("#188A61"));
                break;
            case "CHECKED IN":
                pair = (Color.FromArgb("#D7F7E8"), Color.FromArgb("#188A61"));
                break;
            case "PENDING":
            case "PAYMENT DUE":
                pair = (Color.FromArgb("#FFF1D6"), Color.FromArgb("#B86A11"));
                break;
            case "CHECKED OUT":
                pair = (Color.FromArgb("#E8EFFD"), Color.FromArgb("#33538A"));
                break;
            case "CANCELLED":
                pair = (Color.FromArgb("#FFE2E7"), Color.FromArgb("#B0233E"));
                break;
        }

        // Return the right member of the pair based on the converter parameter.
        // "Bg" -> background, anything else (default) -> text.
        if (string.Equals(parameter as string, "Bg", StringComparison.OrdinalIgnoreCase))
            return pair.background;

        return pair.text;
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a status to a leading dot color used inside the status pill.
/// Mirrors [ReservationStatusToBrushConverter] colors but as a solid color.
/// </summary>
public sealed class ReservationStatusToDotColorConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = (value as string)?.Trim().ToUpperInvariant() ?? string.Empty;
        return status switch
        {
            "CONFIRMED"     => Color.FromArgb("#188A61"),
            "CHECKED IN"    => Color.FromArgb("#188A61"),
            "PENDING"       => Color.FromArgb("#B86A11"),
            "PAYMENT DUE"   => Color.FromArgb("#B86A11"),
            "CHECKED OUT"   => Color.FromArgb("#33538A"),
            "CANCELLED"     => Color.FromArgb("#B0233E"),
            _               => Color.FromArgb("#284DA0")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the few-letter monogram (e.g. "O", "DK") used for the avatar tile
/// in the GUEST column. Reads the existing [Booking.Initials] if the model
/// exposes it, otherwise synthesises initials from the guest name.
/// </summary>
public sealed class GuestInitialsConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return string.Empty;

        if (value is Booking booking)
            return string.IsNullOrEmpty(booking.Initials) ? "?" : booking.Initials;

        if (value is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        return string.Empty;
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns a stable background color for an avatar tile based on the guest
/// name, so the GUEST column visually varies row-to-row.
/// </summary>
public sealed class AvatarBackgroundConverter : IValueConverter
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb("#E8DCFF"),
        Color.FromArgb("#DCE8FF"),
        Color.FromArgb("#D7F7E8"),
        Color.FromArgb("#FFE2E7"),
        Color.FromArgb("#FFF1D6"),
        Color.FromArgb("#DCF5F7"),
    };

    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var seed = (value as string) ?? string.Empty;
        var idx = Math.Abs(seed.GetHashCode()) % Palette.Length;
        return Palette[idx];
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns a stable monospace-style text color matching the avatar background.
/// Keeps contrast legible across the palette.
/// </summary>
public sealed class AvatarForegroundConverter : IValueConverter
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb("#5C36C7"),
        Color.FromArgb("#284DA0"),
        Color.FromArgb("#188A61"),
        Color.FromArgb("#B0233E"),
        Color.FromArgb("#B86A11"),
        Color.FromArgb("#0E6E80"),
    };

    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var seed = (value as string) ?? string.Empty;
        var idx = Math.Abs(seed.GetHashCode()) % Palette.Length;
        return Palette[idx];
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}