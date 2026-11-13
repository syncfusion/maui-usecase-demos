using System.Globalization;
using HotelManagerSample;

namespace HotelManagerSample.Converters;

/// <summary>
/// True → Visible. False/null → Collapsed. Optional "invert" parameter
/// flips the result so the converter can be reused for the inverse case.
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>Converts the bound boolean to <c>true</c> (visible) or
    /// <c>false</c> (collapsed). Pass <c>"invert"</c> in the
    /// <paramref name="parameter"/> to flip the result.</summary>
    /// <param name="value">Bound boolean. Any non-bool value resolves
    /// to <c>false</c>.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Optional flag. <c>"invert"</c> flips
    /// the result.</param>
    /// <param name="culture">Culture passed by the binding engine.
    /// Unused.</param>
    /// <returns><c>true</c> when visible, <c>false</c> when collapsed.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        if (parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag;
    }

    /// <summary>Not supported — visibility conversion is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Inverts a boolean (True → False, False → True). Used as a MultiBinding
/// helper when several inputs have to agree before a UI element enables.
/// </summary>
public sealed class InverseBoolConverter : IValueConverter
{
    /// <summary>Inverts the bound boolean.</summary>
    /// <param name="value">Bound boolean.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The logical NOT of the bound boolean.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !(value is bool b && b);

    /// <summary>Symmetric to <see cref="Convert"/> — also inverts.</summary>
    /// <returns>The logical NOT of the bound boolean.</returns>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !(value is bool b && b);
}

/// <summary>
/// Resolves a hex string (e.g. "#4A43E6") to a MAUI <see cref="Color"/>.
/// Falls back to <see cref="Colors.Transparent"/> on parse failure.
/// </summary>
public sealed class StringToColorConverter : IValueConverter
{
    /// <summary>Converts a hex string to a MAUI <see cref="Color"/>.</summary>
    /// <param name="value">Bound value. Accepts <see cref="Color"/> (passthrough)
    /// or <see cref="string"/>.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The resolved <see cref="Color"/>; <see cref="Colors.Transparent"/>
    /// on parse failure or empty input.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c) return c;
        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            try { return Color.FromArgb(s); } catch { return Colors.Transparent; }
        }
        return Colors.Transparent;
    }

    /// <summary>Not supported — color stringification is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns one of two bound brushes (or colors) based on a boolean value.
/// ConverterParameter format: "trueValue|falseValue" with fallback to
/// StaticResource keys.
/// </summary>
public sealed class BoolToBrushConverter : IValueConverter
{
    /// <summary>Resolves the brush / colour that corresponds to the
    /// bound boolean. The converter parameter is the
    /// <c>"trueValue|falseValue"</c> pair; each token can be a hex
    /// string (<c>"#RRGGBB"</c>) or a <c>StaticResource</c> key.</summary>
    /// <param name="value">Bound boolean.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Pipe-separated tokens (see class doc).</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The resolved colour / brush, or <see cref="Colors.Transparent"/>
    /// when the parameter is empty or unresolvable.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        var parts = (parameter as string ?? string.Empty).Split('|');
        var token = flag ? parts.ElementAtOrDefault(0) : parts.ElementAtOrDefault(1);
        if (string.IsNullOrWhiteSpace(token)) return Colors.Transparent;

        if (token.StartsWith('#'))
        {
            try { return Color.FromArgb(token); } catch { return Colors.Transparent; }
        }

        if (Application.Current?.Resources.TryGetValue(token, out var res) == true)
            return res;

        return Colors.Transparent;
    }

    /// <summary>Not supported — brush selection is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns the tile background color for an inventory stat block based on
/// the label string ("Total Inventory", "Available", "Occupied", etc.).
/// </summary>
public sealed class StatTileBackgroundConverter : IValueConverter
{
    /// <summary>Maps a label string to a tinted background <see cref="Color"/>.</summary>
    /// <param name="value">Bound label string.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The tile background <see cref="Color"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var label = value as string ?? string.Empty;
        return label switch
        {
            "Total Inventory" => Color.FromArgb("#F4F6FB"),
            "Available" => Color.FromArgb("#F1FAFD"),
            "Occupied" => Color.FromArgb("#F5F4FE"),
            "Reserved" => Color.FromArgb("#F0F7F9"),
            "Cleaning" => Color.FromArgb("#FEF6E8"),
            "Maintenance" => Color.FromArgb("#FDEEF3"),
            _ => Color.FromArgb("#F4F6FB")
        };
    }

    /// <summary>Not supported — label → colour mapping is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Returns a status palette color for the inventory icons. Accepts a
/// <see cref="RoomCleaningStatus"/> (or its string form) and maps it to
/// the brand-status colour scheme.
/// </summary>
public sealed class StatusColorConverter : IValueConverter
{
    /// <summary>Resolves the icon colour for the bound room status.</summary>
    /// <param name="value">Bound status (enum or string).</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The icon <see cref="Color"/>.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => RoomStatusColor(value);

    /// <summary>Resolves the room-status colour used by the inventory
    /// icons. Accepts either a strongly-typed <see cref="RoomCleaningStatus"/>
    /// or its string form.</summary>
    /// <param name="value">Bound value (status enum or string).</param>
    /// <returns>The hex <see cref="Color"/> for the status.</returns>
    private static Color RoomStatusColor(object? value)
    {
        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };
        return status switch
        {
            RoomCleaningStatus.Available => Color.FromArgb("#16A1E0"),
            RoomCleaningStatus.Occupied => Color.FromArgb("#5146E5"),
            RoomCleaningStatus.Reserved => Color.FromArgb("#1F8FA8"),
            RoomCleaningStatus.Maintenance => Color.FromArgb("#E9446B"),
            _ => Color.FromArgb("#5146E5")
        };
    }

    /// <summary>Not supported — status → colour mapping is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Soft tile background color rendered behind a status pill. The converter
/// parameter is the comparison status; the result is <c>true</c> when the
/// bound status matches the parameter.
/// </summary>
public sealed class StatusToStatusGradientConverter : IValueConverter
{
    /// <summary>Returns <c>true</c> when the bound status matches the
    /// parameter string (case-insensitive).</summary>
    /// <param name="value">Bound status (enum or string).</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Comparison status string.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns><c>true</c> when the statuses match.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is string comparisonStatus && !string.IsNullOrWhiteSpace(comparisonStatus))
        {
            if (value is RoomCleaningStatus s)
                return s.ToString().Equals(comparisonStatus, StringComparison.OrdinalIgnoreCase);
            if (value is string str)
                return str.Equals(comparisonStatus, StringComparison.OrdinalIgnoreCase);
        }

        var status = value switch
        {
            RoomCleaningStatus s => s,
            string str when Enum.TryParse<RoomCleaningStatus>(str, true, out var parsed) => parsed,
            _ => RoomCleaningStatus.Available
        };
        return status switch
        {
            RoomCleaningStatus.Available => Color.FromArgb("#F1FAFD"),
            RoomCleaningStatus.Occupied => Color.FromArgb("#F5F4FE"),
            RoomCleaningStatus.Reserved => Color.FromArgb("#F0F7F9"),
            RoomCleaningStatus.Maintenance => Color.FromArgb("#FDEEF3"),
            _ => Color.FromArgb("#F4F6FB")
        };
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a percentage (0-100) to a width string (e.g. "65%"). Used by
/// horizontal progress bars that want to bind directly to a KPI percent.
/// </summary>
public sealed class PercentToWidthConverter : IValueConverter
{
    /// <summary>Formats the bound number as a percentage width string.</summary>
    /// <param name="value">Numeric percent (0-100).</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The formatted percentage (e.g. <c>"65%"</c>).</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return $"{d:0.#}%";
        if (value is int i) return $"{i}%";
        if (value is decimal m) return $"{m:0.#}%";
        if (value is string s && double.TryParse(s, out var parsed)) return $"{parsed:0.#}%";
        return "0%";
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a percentage (0-1 or 0-100) to the corresponding fill ratio
/// for a Syncfusion SfLinearProgressBar. Returns a value in [0, 1].
/// </summary>
public sealed class PercentToBarConverter : IValueConverter
{
    /// <summary>Normalises the bound percent to a fill ratio in [0, 1].</summary>
    /// <param name="value">Numeric percent (0-100 or 0-1).</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The fill ratio as a <see cref="double"/> in [0, 1].</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double raw;
        if (value is double d) raw = d;
        else if (value is int i) raw = i;
        else if (value is decimal m) raw = (double)m;
        else if (value is string s && double.TryParse(s, out var parsed)) raw = parsed;
        else return 0d;

        // Heuristic: values in [0, 1] are treated as a ratio; otherwise
        // they are treated as a percentage and divided by 100.
        var ratio = raw > 1d ? raw / 100d : raw;
        return Math.Clamp(ratio, 0d, 1d);
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps a boolean to one of two font family names supplied via the
/// converter parameter ("activeFont|inactiveFont"). Used to render the
/// selected-vs-unselected typography of the rooms status chips.
/// </summary>
public sealed class BoolToFontFamilyConverter : IValueConverter
{
    /// <summary>Resolves the font family string the chip should use.</summary>
    /// <param name="value">Bound boolean. <c>true</c> picks the first
    /// token; <c>false</c> picks the second.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Pipe-separated font family names.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The resolved font family name, or <see cref="string.Empty"/>
    /// when the parameter is empty.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? string.Empty).Split('|');
        var flag = value is bool b && b;
        return flag ? parts.ElementAtOrDefault(0) ?? string.Empty
                    : parts.ElementAtOrDefault(1) ?? string.Empty;
    }

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Converts a string to a boolean. Empty / null strings resolve to
/// <c>false</c>; any other value resolves to <c>true</c>.
/// </summary>
public sealed class StringToBoolConverter : IValueConverter
{
    /// <summary>Resolves a string to a boolean (non-empty = true).</summary>
    /// <param name="value">Bound string.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns><c>true</c> for non-empty strings, <c>false</c> otherwise.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => !string.IsNullOrWhiteSpace(value as string);

    /// <summary>Not supported.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// True → the supplied <c>ConverterParameter</c> hex colour. False →
/// <c>#3B4860</c>. Binds the revenue-range toggle labels to a
/// view-model flag instead of an imperative code-behind handler.
/// </summary>
public sealed class ActiveBoolToColorConverter : IValueConverter
{
    /// <summary>Resolves the colour for the bound boolean.</summary>
    /// <param name="value">Bound boolean. Any non-bool resolves to false.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Active-state hex colour (e.g. "#4A43E6").</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns>The active colour when <paramref name="value"/> is true;
    /// otherwise the inactive muted-slate colour.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isActive = value is bool b && b;
        var colorParam = parameter as string;

        // Text color usage
        if (colorParam == "White")
        {
            return isActive ? Colors.White : Colors.Black;
        }

        // Background color usage
        if (isActive && !string.IsNullOrEmpty(colorParam))
        {
            return Color.FromArgb(colorParam);
        }

        return Color.FromArgb("#F6F8FD");
    }

    /// <summary>Not supported — colour resolution is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// True → <see cref="FontAttributes.Bold"/>. False →
/// <see cref="FontAttributes.None"/>. Binds the revenue-range toggle
/// labels' bold state to a view-model flag.
/// </summary>
public sealed class ActiveBoolToFontAttributesConverter : IValueConverter
{
    /// <summary>Resolves the font-attribute state for the bound boolean.</summary>
    /// <param name="value">Bound boolean. Any non-bool resolves to false.</param>
    /// <param name="targetType">Target type. Unused.</param>
    /// <param name="parameter">Converter parameter. Unused.</param>
    /// <param name="culture">Culture. Unused.</param>
    /// <returns><see cref="FontAttributes.Bold"/> when true;
    /// <see cref="FontAttributes.None"/> otherwise.</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? FontAttributes.Bold : FontAttributes.None;

    /// <summary>Not supported — font-attribute resolution is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
