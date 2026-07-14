using System.Globalization;

namespace E_Commerce_Orders.Converters
{
    /// <summary>
    /// Converts order status to modern text colors with dark theme support
    /// </summary>
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.White;

            string status = value!.ToString()?.Trim() ?? "";

            // Get current app theme
            var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;

            return status.ToLower() switch
            {
                "delivered" => isDarkMode
                    ? Color.FromArgb("#10B981")      // Bright green for dark mode visibility
                    : Color.FromArgb("#10B981"),     // Modern green for light mode
                "shipped" => isDarkMode
                    ? Color.FromArgb("#3B82F6")      // Bright blue for dark mode
                    : Color.FromArgb("#3B82F6"),     // Modern blue for light mode
                "pending" => isDarkMode
                    ? Color.FromArgb("#FBBF24")      // Lighter orange for dark mode
                    : Color.FromArgb("#F59E0B"),     // Modern orange for light mode
                _ => isDarkMode
                    ? Color.FromArgb("#94A3B8")      // Lighter gray for dark mode
                    : Color.FromArgb("#6B7280")      // Gray fallback for light mode
            };
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            return value ?? Colors.White;
        }
    }

    /// <summary>
    /// Converts order status to modern background colors with dark theme support
    /// </summary>
    public class StatusBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.Transparent;

            string status = value?.ToString()?.Trim() ?? "";

            // Get current app theme
            var isDarkMode = Application.Current?.RequestedTheme == AppTheme.Dark;

            return status.ToLower() switch
            {
                "delivered" => isDarkMode
                    ? Color.FromArgb("#064E3B")      // Dark green background for dark mode
                    : Color.FromArgb("#D1FAE5"),     // Light green background for light mode
                "shipped" => isDarkMode
                    ? Color.FromArgb("#082F4F")      // Dark blue background for dark mode
                    : Color.FromArgb("#DBEAFE"),     // Light blue background for light mode
                "pending" => isDarkMode
                    ? Color.FromArgb("#5A3A1F")      // Dark orange background for dark mode
                    : Color.FromArgb("#FEF3C7"),     // Light orange background for light mode
                _ => isDarkMode
                    ? Color.FromArgb("#334155")      // Dark gray background for dark mode
                    : Color.FromArgb("#F3F4F6")      // Gray fallback for light mode
            };
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo culture)
        {
            return value ?? Colors.Transparent;
        }
    }
}
