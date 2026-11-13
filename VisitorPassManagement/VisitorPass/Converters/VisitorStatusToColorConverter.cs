using System.Globalization;
using VisitorPass.Models;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a <see cref="VisitorStatus"/> into a
/// <see cref="Color"/>.
///
/// The value resolved is the <strong>foreground</strong> brush for the
/// status (e.g. <c>#3B82F6</c> for Expected). For the pale background
/// shade used inside the status pill, use
/// <see cref="VisitorStatusToBackgroundConverter"/> instead.
///
/// Source palette lives in <c>Resources/Styles/Colors.xaml</c>; this
/// converter reads its values from <c>App.Current.Resources</c> so
/// it stays in lockstep with the theme. Theme overrides via
/// <c>AppThemeBinding</c> are honoured automatically.
/// </summary>
public sealed class VisitorStatusToColorConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VisitorStatus status)
            return Colors.Transparent;

        // Resolve the brush key from the static helper, then look up
        // the Color in the live resource dictionary so an
        // AppThemeBinding or runtime override wins.
        var brushKey = status.BrushKey();
        if (Application.Current?.Resources.TryGetValue(brushKey, out var brushObj) == true
            && brushObj is SolidColorBrush brush)
        {
            return brush.Color;
        }

        return Colors.Transparent;
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
