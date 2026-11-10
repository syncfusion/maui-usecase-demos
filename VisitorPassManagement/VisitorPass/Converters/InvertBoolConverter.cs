using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// Flips a boolean value. Registered in <c>App.xaml</c> as
/// <c>InvertBoolConverter</c>.
///
/// Used by the Digital Pass page to flip <c>HasVisitor</c> into the
/// "show EmptyState when no visitor is loaded" visibility.
/// </summary>
public sealed class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : false;
}
