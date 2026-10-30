using System.Globalization;

namespace HotelManagerSample.Converters;

/// <summary>
/// Background color for the floor pill chips in the Add-Booking popup.
/// Selected → "#5146E5" (the strong brand purple). Unselected → "#FFFFFF"
/// (white surface, no stroke). Returned as <see cref="Color"/> so Border
/// and Label can both bind without an extra conversion to Brush.
/// </summary>
public sealed class FloorChipBgConverter : IValueConverter
{
    /// <summary>Resolves the background colour for a floor chip.</summary>
    /// <param name="value">Bound boolean indicating selection state.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The chip background <see cref="Color"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selected = value is bool b && b;
        return selected ? Color.FromArgb("#5146E5") : Color.FromArgb("#FFFFFF");
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Foreground (icon + label text) color for the floor pill chips.
/// Selected → white. Unselected → "#3A4658" (body text).
/// </summary>
public sealed class FloorChipFgConverter : IValueConverter
{
    /// <summary>Resolves the foreground colour for a floor chip.</summary>
    /// <param name="value">Bound boolean indicating selection state.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The chip foreground <see cref="Color"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var selected = value is bool b && b;
        return selected ? Colors.White : Color.FromArgb("#3A4658");
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
