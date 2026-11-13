using System.Globalization;

namespace FluentMail.Converters;

public class DateGroupTitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return string.Empty;
        return date == DateTime.Today ? "TODAY" : date == DateTime.Today.AddDays(-1) ? "YESTERDAY" : date.ToString("MMM d", culture).ToUpperInvariant();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
