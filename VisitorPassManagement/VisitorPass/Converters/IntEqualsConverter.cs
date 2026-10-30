using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// Returns <c>true</c> when the bound integer equals the
/// <c>ConverterParameter</c> integer. Used by the registration
/// surface to switch between Step 1 / Step 2 / Step 3 layouts based
/// on the VM's <c>CurrentStep</c>.
///
///   IsVisible="{Binding CurrentStep,
///        Converter={StaticResource IntEqualsConverter},
///        ConverterParameter=2}"
///
/// Per HARNESS §7 this is an acceptable code-behind-free idiom
/// switch: the converter is generic enough to be reused.
/// </summary>
public sealed class IntEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i
            && parameter is not null
            && int.TryParse(parameter.ToString(), out var target))
        {
            return i == target;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
