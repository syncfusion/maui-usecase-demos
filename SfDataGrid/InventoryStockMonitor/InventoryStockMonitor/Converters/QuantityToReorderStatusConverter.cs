using Microsoft.Maui.Controls;

namespace StockMonitor.Converters
{
    public class QuantityToReorderStatusConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity < 10)
                    return "CRITICAL";
                else if (quantity <= 50)
                    return "LOW";
                else
                    return "OK";
            }
            return "UNKNOWN";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
