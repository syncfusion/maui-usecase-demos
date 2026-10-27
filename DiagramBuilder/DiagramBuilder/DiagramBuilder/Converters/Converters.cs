using System.Globalization;

namespace DiagramBuilder.Converters;

/// <summary>
/// True → one value, False → another (parameters supply both). Inverts when
/// ConverterParameter contains "invert".
/// </summary>
public class BoolToObjectConverter : IValueConverter
{
    public object? TrueValue { get; set; }

    public object? FalseValue { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is true;
        if (parameter is string p && p.Contains("invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag ? TrueValue : FalseValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Zoom factor (1.0 = 100 %) ↔ display percent (double or formatted string).
/// </summary>
public class ZoomToPercentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double zoom
            ? zoom * 100
            : 100d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is IConvertible convertible
            ? convertible.ToDouble(CultureInfo.InvariantCulture) / 100
            : 1d;
}

/// <summary>
/// Color key string (ResourceKeys) → resolved Color, so XAML can bind swatches
/// to the same key-based colors the diagram styles use.
/// </summary>
public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var color = Services.ResourceLookup.Color(value as string, Colors.White);
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Enum member (or its ToString) → display string via the parameter dictionary
/// ("Png=PNG;Jpeg=JPEG" syntax). Returns the raw name when unmapped.
/// </summary>
public class EnumToDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var name = value.ToString() ?? string.Empty;
        if (parameter is not string map)
        {
            return name;
        }

        foreach (var pair in map.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0] == name)
            {
                return kv[1];
            }
        }

        return name;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// null → false, anything else → true; "invert" flips the result.
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is not null;
        if (parameter is string p && p.Contains("invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Collection count → bool (count &gt; threshold, default 0); parameter = threshold;
/// "invert" flips.
/// </summary>
public class CountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var threshold = 0;
        if (parameter is string p && !p.Contains("invert", StringComparison.OrdinalIgnoreCase))
        {
            int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out threshold);
        }

        var count = value is IConvertible convertible
            ? System.Convert.ToInt32(convertible.ToDouble(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            : 0;
        var flag = count > threshold;
        if (parameter is string pi && pi.Contains("invert", StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Selection kind → Visibility: parameter "node" shows when exactly one node is
/// selected; "any" shows when any selection exists; "none" shows when empty.
/// </summary>
public class SelectionKindToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var mode = parameter as string ?? "node";
        var count = value is IConvertible convertible
            ? System.Convert.ToInt32(convertible.ToDouble(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            : 0;
        var visible = mode switch
        {
            "any" => count > 0,
            "none" => count == 0,
            _ => count == 1, // "node": exactly one node selected
        };
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
