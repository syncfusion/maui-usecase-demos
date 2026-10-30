using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — returns <c>true</c> when the bound string is non-null and
/// non-empty. Used by registration surface's "Next" button to enable
/// itself only when required fields are filled in. The <c>parameter</c>
/// argument (if any) is treated as the desired <c>false</c> value when
/// the bound string is null / whitespace — leave null/empty for the
/// default <c>false</c>.
/// </summary>
public sealed class StringNotEmptyToBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
