using System.Globalization;

namespace SmartExpense
{
    public class ColorWithOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                var opacity = parameter != null
                    ? double.Parse(parameter.ToString()!)
                    : 0.2;

                return color.WithAlpha((float)opacity);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
