using System.Globalization;

namespace StockMonitor.Converters
{
    public class BoolToChipColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // true  → M3 ErrorContainer    #F9DEDC (Reorder)
                // false → M3 TertiaryContainer #FFD8E4 (OK)
                return boolValue
                    ? Color.FromArgb("#F9DEDC")
                    : Color.FromArgb("#FFD8E4");
            }
            return Color.FromArgb("#FFD8E4");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
