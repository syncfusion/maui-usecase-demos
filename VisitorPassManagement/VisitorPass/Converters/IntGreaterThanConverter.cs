using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// Returns <c>true</c> when the bound integer is strictly greater
/// than the integer in <c>ConverterParameter</c>. Used by the
/// registration surface to show the "Back" button only on Step 2+.
///
///   IsVisible="{Binding CurrentStep,
///        Converter={StaticResource IntGreaterThanConverter},
///        ConverterParameter=1}"
/// </summary>
public sealed class IntGreaterThanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i
            && parameter is not null
            && int.TryParse(parameter.ToString(), out var target))
        {
            return i > target;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
