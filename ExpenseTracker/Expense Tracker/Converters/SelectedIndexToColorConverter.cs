namespace SmartExpense;

/// <summary>
/// Returns a Color depending on whether the integer value equals the ConverterParameter.
/// Parameter should be an int string. If equal, returns the SelectedColor; otherwise returns DefaultColor.
/// Use ConverterParameter to pass the expected index. Optionally pass colors as string in parameter as 'index|selectedHex|defaultHex'.
/// </summary>
public class SelectedIndexToColorConverter : IValueConverter
{
    private static Color ParseColor(string hex, Color? fallback = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return fallback ?? Colors.Transparent;
            var s = hex.Trim();
            if (!s.StartsWith("#", StringComparison.Ordinal)) s = "#" + s;
            // Normalize #RRGGBB to #AARRGGBB with full alpha
            if (s.Length == 7) s = "#FF" + s.Substring(1);
            // Accept already #AARRGGBB as-is
            return Color.FromArgb(s);
        }
        catch
        {
            return fallback ?? Colors.Transparent;
        }
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            if (value is not int i) return Colors.Transparent;

            if (parameter is string p)
            {
                // Supported formats:
                //  - "index"
                //  - "index|selectedHex|defaultHex" (hex may be #RRGGBB or #AARRGGBB)
                var parts = p.Split('|');
                if (parts.Length == 1 && int.TryParse(parts[0], out var idx1))
                {
                    return i == idx1 ? ParseColor("#E8F5FF") : Colors.Transparent;
                }
                if (parts.Length >= 3 && int.TryParse(parts[0], out var idx))
                {
                    var sel = ParseColor(parts[1]);
                    var def = ParseColor(parts[2], Colors.Transparent);
                    return i == idx ? sel : def;
                }
            }
        }
        catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
