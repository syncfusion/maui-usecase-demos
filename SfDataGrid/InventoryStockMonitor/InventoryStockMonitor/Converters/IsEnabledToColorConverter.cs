using System.Globalization;

namespace StockMonitor.Converters
{
    /// <summary>
    /// Converts IsEnabled boolean state to appropriate button background brushes
    /// </summary>
    public class IsEnabledToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                if (isEnabled)
                {
                    // Return enabled color based on button type (passed as parameter)
                    if (parameter is string buttonType)
                    {
                        return buttonType switch
                        {
                            "Primary" => new SolidColorBrush(Color.FromArgb("#6750A4")),      // Light Primary
                            "Delete" => new SolidColorBrush(Color.FromArgb("#B3261E")),       // Error Red
                            "Cancel" => new SolidColorBrush(Color.FromArgb("#79747E")),       // Outline/Secondary Gray
                            _ => new SolidColorBrush(Color.FromArgb("#6750A4"))
                        };
                    }
                    return new SolidColorBrush(Color.FromArgb("#6750A4"));
                }
                else
                {
                    // Disabled state - grayed out
                    return new SolidColorBrush(Color.FromArgb("#CAC7D0"));
                }
            }
            return new SolidColorBrush(Color.FromArgb("#6750A4"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts IsEnabled state to text color for buttons
    /// </summary>
    public class IsEnabledToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                if (isEnabled)
                {
                    // Enabled text color
                    return Colors.White;
                }
                else
                {
                    // Disabled text color - lighter gray
                    return Color.FromArgb("#9E9B9E");
                }
            }
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
