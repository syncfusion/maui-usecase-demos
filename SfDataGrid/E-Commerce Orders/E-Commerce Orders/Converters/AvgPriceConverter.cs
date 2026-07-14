using System.Globalization;

namespace E_Commerce_Orders.Converters
{
    public class AvgPriceConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal totalRevenue)
            {
                // This will be used with TotalRevenue, we'll pass Orders.Count through Parameter
                // For now, return formatted revenue - in code-behind we'll calculate properly
                return totalRevenue > 0 ? $"${totalRevenue:N0}" : "$0";
            }
            return "$0";
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
