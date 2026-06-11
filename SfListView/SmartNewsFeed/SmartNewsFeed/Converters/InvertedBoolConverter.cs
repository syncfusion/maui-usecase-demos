using System.Globalization;

namespace SmartNewsFeed
{
    /// <summary>
    /// Converts a boolean value to its inverted value.
    /// True becomes False, False becomes True.
    /// Useful for hiding UI elements when something is loading/refreshing.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }
    }
}
