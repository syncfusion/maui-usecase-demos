using System.Globalization;
using HotelManagerSample;

namespace HotelManagerSample.Converters;

/// <summary>
/// Returns a boolean indicating whether the bound ActiveTab equals the
/// parameter-supplied NavigationTab name. Used to drive DataTrigger rows
/// (active state, badge visibility) where multiple setters must apply together.
/// </summary>
public sealed class TabMatchToBoolConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // First positional: active-tab enum
        // Second positional: optional string ("Dashboard"/"Bookings"/"Rooms") via ConverterParameter.
        var current = value is NavigationTab nav ? nav : NavigationTab.Dashboard;
        var paramString = parameter as string ?? string.Empty;
        return Enum.TryParse<NavigationTab>(paramString, out var expected) && current == expected;
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Resolves the active/inactive sidebar row to the right visual brush (or text color).
///
/// Parameter contract (string tokens):
///   "<TabName>"        → Active-row background brush: dark fill when bound tab matches
///                          the parameter, transparent otherwise.
///   "<TabName>Text"    → Active-row label text color: white when active, #526075 otherwise.
///   "<TabName>Badge"   → Bool indicating the badge should show (currently only
///                          "BookingsBadge" is used, and it's hidden when the Bookings
///                          tab is active — same as the reference design).
///
/// Bound value can be either the <see cref="NavigationTab"/> enum (for tab matching)
/// or a string (for badge-visibility check) — the converter inspects the runtime type.
/// </summary>
public sealed class ActiveRowToBrushConverter : IValueConverter
{
    // Active dark fill (#17243A) — matches the reference sidebar pill.
    private static readonly Color ActiveFill = Color.FromArgb("#17243A");

    // Inactive overlay colors.
    private static readonly Color InactiveFill = Colors.Transparent;
    private static readonly Color ActiveText = Colors.White;
    private static readonly Color InactiveText = Color.FromArgb("#526075");

    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var token = (parameter as string ?? string.Empty).Trim();

        // Deal with the badge-visibility tokens first.
        if (token.EndsWith("Badge", StringComparison.OrdinalIgnoreCase))
        {
            var tabToken = token[..^"Badge".Length];
            // Compute active state: in the reference UI the badge is hidden when
            // its own tab is currently active (replaced by "LIVE" / dot pill).
            var current = ParseTab(value);
            var target = ParseTabName(tabToken);
            return current != target;
        }

        if (token.EndsWith("Text", StringComparison.OrdinalIgnoreCase))
        {
            var tabToken = token[..^"Text".Length];
            var current = ParseTab(value);
            var target = ParseTabName(tabToken);
            return current == target ? ActiveText : InactiveText;
        }

        // Default: a tab background fill.
        var tab = ParseTab(value);
        var expected = ParseTabName(token);
        return tab == expected ? ActiveFill : InactiveFill;
    }

    /// <summary>Best-effort <see cref="NavigationTab"/> extraction from
    /// the bound value. Falls back to <see cref="NavigationTab.Dashboard"/>
    /// when the bound value is neither the enum nor a parseable string.</summary>
    /// <param name="value">Bound value supplied by the XAML binding.</param>
    /// <returns>The resolved <see cref="NavigationTab"/>.</returns>
    private static NavigationTab ParseTab(object? value)
    {
        return value switch
        {
            NavigationTab nav => nav,
            string s when Enum.TryParse<NavigationTab>(s, true, out var parsed) => parsed,
            _ => NavigationTab.Dashboard
        };
    }

    /// <summary>Best-effort <see cref="NavigationTab"/> extraction from
    /// the converter-parameter string. Returns <c>null</c> when the
    /// string does not match a known tab name (so callers can fall back
    /// to transparent / unstyled).</summary>
    /// <param name="token">Converter-parameter token (e.g. "Dashboard").</param>
    /// <returns>The parsed tab or <c>null</c> on no match.</returns>
    private static NavigationTab? ParseTabName(string token)
    {
        if (Enum.TryParse<NavigationTab>(token.Replace(" ", string.Empty), true, out var parsed))
            return parsed;
        return null;
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Resolves the active/inactive sidebar icon to the right ImageSource asset.
/// PNG icons cannot be recolored dynamically in MAUI, so this converter swaps
/// the Image.Source between an "active" variant (a high-contrast icon used
/// when the row is filled with dark #17243A) and the default colored PNG
/// (used when the row is sitting on the light sidebar background).
///
/// Parameter contract:
///   "&lt;TabName&gt;"        → ImageSource keyed off the bound <see cref="NavigationTab"/>.
///
/// Asset names per tab (the active asset is the WHITE variant used on the
/// dark #17243A active fill; the inactive asset is the DARK variant on the
/// light sidebar background):
///   Dashboard → active: dashboard.png (white!) / inactive: dashboardblack.png (dark!)
///   Bookings  → active: bookingwhite.png    / inactive: booking.png
///   Rooms     → active: roomswhite.png      / inactive: rooms.png
/// </summary>
public sealed class TabIconSourceConverter : IValueConverter
{
    /// <summary>Converts the bound value to the converter's target type.</summary>
    /// <param name="value">Source value supplied by the binding engine.</param>
    /// <param name="targetType">Target type requested by the binding engine.</param>
    /// <param name="parameter">Converter parameter from the XAML binding.</param>
    /// <param name="culture">Culture passed by the binding engine.</param>
    /// <returns>The converted value, or a converter-specific fallback.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var token = (parameter as string ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(token))
            return null;

        var current = ParseTab(value);
        var target = ParseTabName(token);
        if (target is null)
            return null;

        var (baseName, activeAsset) = ResolveAssets(target.Value);
        var fileName = current == target.Value ? activeAsset : $"{baseName}.png";
        return ImageSource.FromFile(fileName);
    }

    /// <summary>Not supported — this converter is one-way.</summary>
    /// <returns>Always throws <see cref="NotSupportedException"/>.</returns>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    // Per-tab icon filenames. The "active" asset is the high-contrast variant
    // rendered on top of the dark row fill (#17243A); the inactive asset is the
    // default colored PNG rendered on the light sidebar background.
    private static (string baseName, string activeAsset) ResolveAssets(NavigationTab tab)
    {
        return tab switch
        {
            // Dashboard's naming is inverted vs the other tabs:
            // dashboard.png is WHITE (for the dark active fill) and
            // dashboardblack.png is DARK (for the light inactive row).
            NavigationTab.Dashboard => ("dashboardblack", "dashboard.png"),

            // Bookings / Rooms follow the "{name}.png" = dark default,
            // "{name}white.png" = white active variant convention.
            NavigationTab.Bookings  => ("booking",  "bookingwhite.png"),
            NavigationTab.Rooms     => ("rooms",     "roomswhite.png"),

            // The remaining tabs only have one variant each; render with
            // BorderTint so they stay legible on dark background.
            // For these, the inactive asset (e.g. "person.png") is re-used
            // and the converter returns null to fall back to a neutral glyph.
            NavigationTab.Guests    => ("person",    "person.png"),
            NavigationTab.Calendar  => ("date",      "date.png"),
            NavigationTab.Reports   => ("container", "container.png"),
            NavigationTab.Settings  => ("key",       "key.png"),

            _ => (tab.ToString().ToLowerInvariant(), $"{tab.ToString().ToLowerInvariant()}white.png")
        };
    }

    // Local duplicates of the helpers above so this converter is self-contained.
    // (Keeps the file decoupled from ActiveRowToBrushConverter internals.)
    private static NavigationTab ParseTab(object? value)
    {
        return value switch
        {
            NavigationTab nav => nav,
            string s when Enum.TryParse<NavigationTab>(s, true, out var parsed) => parsed,
            _ => NavigationTab.Dashboard
        };
    }

    private static NavigationTab? ParseTabName(string token)
    {
        if (Enum.TryParse<NavigationTab>(token.Replace(" ", string.Empty), true, out var parsed))
            return parsed;
        return null;
    }
}