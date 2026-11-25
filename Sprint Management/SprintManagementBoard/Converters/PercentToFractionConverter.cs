using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace SprintManagementBoard
{
    /// <summary>
    /// PercentToFractionConverter is a value converter that converts a percentage value (0-100) to a fraction (0.0-1.0) for use in UI elements such as progress bars.
    /// </summary>
    public class PercentToFractionConverter : IValueConverter
    {
        /// <summary>
        /// Method to convert a percentage value (0-100) to a fraction (0.0-1.0).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null) return 0.0;
            if (value is double d) return Math.Clamp(d, 0.0, 1.0);
            if (value is float f) return Math.Clamp(f, 0f, 1f);
            if (value is int i) return Math.Clamp(i / 100.0, 0.0, 1.0);
            if (double.TryParse(value.ToString(), out var parsed))
            {
                if (parsed > 1) return Math.Clamp(parsed / 100.0, 0.0, 1.0);
                return Math.Clamp(parsed, 0.0, 1.0);
            }
            return 0.0;
        }

        /// <summary>
        /// Method to convert a fraction (0.0-1.0) back to a percentage value (0-100).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d) return (int)Math.Round(d * 100);
            return 0;
        }
    }
}
