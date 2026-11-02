using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// Returns <c>true</c> when the bound string is non-null and
/// non-whitespace. Used to drive
/// <c>IsVisible</c> on captions that may be unset.
/// </summary>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
