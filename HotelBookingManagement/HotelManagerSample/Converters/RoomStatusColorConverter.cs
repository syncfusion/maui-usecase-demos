using System.Globalization;
using HotelManagerSample;

namespace HotelManagerSample.Converters;

/// <summary>
/// Maps a <see cref="RoomCleaningStatus"/> (or its string form) to the
/// brand-status palette colour used by the rooms page. Available / Occupied
/// pull from the page's resource dictionary so the design system stays
/// consistent across the app.
/// </summary>
public sealed class RoomStatusColorConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };

        return status switch
        {
            RoomCleaningStatus.Available => (Color)App.Current!.Resources["Success"],
            RoomCleaningStatus.Occupied => (Color)App.Current!.Resources["Primary"],
            RoomCleaningStatus.Reserved => Color.FromArgb("#4A43E6"),
            RoomCleaningStatus.Maintenance => Color.FromArgb("#EE4770"),
            _ => Color.FromArgb("#4A43E6")
        };
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
