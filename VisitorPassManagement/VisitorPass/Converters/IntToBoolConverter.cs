using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// Returns <c>true</c> when an integer is greater than zero.
/// Used by the registration surface to show its inline error banner
/// only when <c>ValidationErrors.Count &gt; 0</c>.
/// </summary>
public sealed class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
