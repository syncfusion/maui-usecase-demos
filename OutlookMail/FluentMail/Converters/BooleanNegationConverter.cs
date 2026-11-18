using System.Globalization;
using Microsoft.Maui.Controls;

namespace FluentMail.Converters;

public class BooleanNegationConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool flag && !flag;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
