using System.Globalization;
using VisitorPass.Models;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a <see cref="VisitorStatus"/> into the
/// <see cref="SolidColorBrush"/> registered under the status's
/// foreground key. Useful when the bound target wants a Brush
/// rather than a Color (e.g. <c>SfBadgeView.BadgeBackground</c>).
/// </summary>
public sealed class VisitorStatusToBrushConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VisitorStatus status)
            return new SolidColorBrush(Colors.Transparent);

        var brushKey = status.BrushKey();
        if (Application.Current?.Resources.TryGetValue(brushKey, out var brushObj) == true
            && brushObj is SolidColorBrush brush)
        {
            return brush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
