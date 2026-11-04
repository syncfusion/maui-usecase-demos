using System.Globalization;
using VisitorPass.Models;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a <see cref="VisitorStatus"/> into the
/// <strong>pale background</strong> <see cref="Color"/> used inside
/// status pills and chips (e.g. <c>#DBEAFE</c> for Expected).
///
/// Pair with <see cref="VisitorStatusToColorConverter"/> which
/// returns the corresponding foreground.
/// </summary>
public sealed class VisitorStatusToBackgroundConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not VisitorStatus status)
            return Colors.Transparent;

        var brushKey = status.BackgroundBrushKey();
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
