using System.Globalization;

namespace SmartExpense
{
    public class TransactionAmountColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type)
                return Colors.Black;

            return type == "Income"
                ? Color.FromArgb("#3F6F4E") // ✅ green (income)
                : Color.FromArgb("#9F3A38"); // 🔴 red (expense – matches image)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
