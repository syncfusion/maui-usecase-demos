using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts the current device idiom to a boolean. The
/// <c>ConverterParameter</c> selects which idiom returns true:
///
///   <c>Phone</c>       -> true on phones, false elsewhere
///   <c>Tablet</c>      -> true on tablets, false elsewhere
///   <c>Desktop</c>     -> true on desktops, false elsewhere
///
/// Rarely used (XAML's <c>OnIdiom</c> markup covers the same cases in
/// less code) but available for cases where a code-behind value
/// converter needs the idiom — per AGENTS.md the only legitimate
/// places to read <c>DeviceInfo.Idiom</c> are these value converters.
/// </summary>
public sealed class IdiomToBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var target = parameter as string ?? "Phone";
        var idiom = DeviceInfo.Current.Idiom;

        return target.Equals("Phone", StringComparison.OrdinalIgnoreCase) ? idiom == DeviceIdiom.Phone
            : target.Equals("Tablet", StringComparison.OrdinalIgnoreCase) ? idiom == DeviceIdiom.Tablet
            : target.Equals("Desktop", StringComparison.OrdinalIgnoreCase) ? idiom == DeviceIdiom.Desktop
            : false;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
