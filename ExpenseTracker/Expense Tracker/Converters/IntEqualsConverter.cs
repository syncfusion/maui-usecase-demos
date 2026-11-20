namespace SmartExpense;

/// <summary>
/// Converts an integer to a bool by comparing it with the ConverterParameter (int).
/// Returns true when equal. If ConverterParameter is missing or unparsable, returns false.
/// </summary>
public class IntEqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            if (value is int i && parameter is string p && int.TryParse(p, out var expected))
                return i == expected;
        }
        catch { }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
