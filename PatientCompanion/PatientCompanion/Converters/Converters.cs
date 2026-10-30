using System.Globalization;

namespace PatientCompanion.Converters;

public sealed class IntToDoubleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int i => (double)i,
            double d => d,
            _ => 0d
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
            return (int)Math.Round(d);

        return 0;
    }
}
