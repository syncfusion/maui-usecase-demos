using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace StockMonitor.Converters
{
    public class QuantityToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity < 10)
                    return Color.FromArgb("#D32F2F"); // CRITICAL - Red
                else if (quantity <= 50)
                    return Color.FromArgb("#FFB300"); // LOW - Amber
                else
                    return Color.FromArgb("#4CAF50"); // OK - Green
            }
            return Color.FromArgb("#757575"); // Gray - Unknown
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
