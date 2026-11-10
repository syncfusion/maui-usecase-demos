using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — formats a <see cref="DateTime"/> for use in a DataGrid cell.
/// Default format is <c>d MMM, HH:mm</c> (e.g. <c>21 Jan, 14:30</c>) but
/// the bound target can pass a custom .NET date format string via
/// <c>ConverterParameter</c> when a more compact form is needed (the
/// phone directory pass uses <c>HH:mm</c>).
/// </summary>
public sealed class DateToShortLabelConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt)
            return string.Empty;

        var format = parameter as string ?? "d MMM, HH:mm";
        return dt.ToString(format, CultureInfo.CurrentCulture);
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
