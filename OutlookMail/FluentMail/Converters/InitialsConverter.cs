using System.Globalization;
using Microsoft.Maui.Controls;

namespace FluentMail.Converters;

public class InitialsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value as string;
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        var first = parts[0][0];
        var second = parts.Length > 1 ? parts[1][0] : parts[0].Length > 1 ? parts[0][1] : parts[0][0];
        return $"{char.ToUpperInvariant(first)}{char.ToUpperInvariant(second)}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
