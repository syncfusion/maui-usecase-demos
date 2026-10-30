using System.Globalization;
using VisitorPass.Models;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a <see cref="VisitorStatus"/> into the compact
/// phone label (e.g. <c>Checked In</c> -> <c>In</c>). Used by
/// <c>VisitorStatusBadge</c> on phone and by status filter chips in
/// tight rows.
/// </summary>
public sealed class VisitorStatusToShortLabelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is VisitorStatus status ? status.Short() : string.Empty;

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
