using System.Globalization;

namespace SmartExpense;

/// <summary>
/// Returns true if the string is non-null and non-empty.
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
