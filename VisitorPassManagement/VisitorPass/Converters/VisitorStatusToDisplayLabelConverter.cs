using System.Globalization;
using VisitorPass.Models;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a <see cref="VisitorStatus"/> into the full
/// display label (e.g. <c>Checked In</c>). Pair with
/// <see cref="VisitorStatusToShortLabelConverter"/> when binding the
/// same <c>Status</c> property to two labels and switching their
/// visibility via <c>OnIdiom</c>.
/// </summary>
public sealed class VisitorStatusToDisplayLabelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is VisitorStatus status ? status.Display() : string.Empty;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
