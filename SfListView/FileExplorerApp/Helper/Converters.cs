using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FileExplorerApp
{
    using System;
    using System.Globalization;
    using Microsoft.Maui.Controls;

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;

            return false;
        }
    }

}
