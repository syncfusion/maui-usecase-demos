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
        if (value is null || parameter is not string map || map.Length == 0)
        {
            return value?.ToString() ?? string.Empty;
        }

        foreach (var pair in map.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx > 0 && pair[..idx].Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return pair[(idx + 1)..];
            }
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Null → one value, non-null → another. Inverts when parameter contains "invert".
/// </summary>
public class NullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = value is not null && !(value is string s && s.Length == 0);
        if (parameter is string p && p.Contains("invert", StringComparison.OrdinalIgnoreCase))
        {
            hasValue = !hasValue;
        }

        return hasValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// True when the count exceeds an optional threshold parameter ("2" → > 2).
/// Inverts on "invert".
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
/// Hides/show the toolbar group-6 / property panel based on the current selection
/// kind and count. Parameter modes:
///   "node"   — visible when exactly one node is selected
///   "any"    — visible when anything is selected
///   "none"   — visible when nothing is selected
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
            "node" => count == 1,
            "any" => count >= 1,
            "none" => count == 0,
            _ => count == 1,
        };
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Boolean inversion (used to flip a "show when empty" condition into "show when not empty").
/// </summary>
public class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b ? !b : false;
}