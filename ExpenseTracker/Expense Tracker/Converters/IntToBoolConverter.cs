namespace SmartExpense;

/// <summary>
/// Converts an integer (typically a collection Count) to bool (true when > 0).
/// If ConverterParameter equals "Invert", the result is inverted.
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            if (value is int i)
            {
                var result = i > 0;
                if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                    return !result;
                return result;
            }

            // If value is not int, treat null/empty as false
            if (value == null) return false;
        }
        catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
