using DiagramBuilder.Constants;

namespace DiagramBuilder.Services;

/// <summary>
/// Resolves theme resource keys to <see cref="Color"/> values for Syncfusion diagram styles.
/// Falls back to <see cref="Colors.White"/>/transparent when a key is unknown, so a bad key
/// never crashes the designer.
/// </summary>
public static class ResourceLookup
{
    private static readonly Dictionary<string, Color> Palette = new()
    {
        // Mirrors Resources/Styles/Colors.xaml (single source of truth for the theme).
        [ResourceKeys.PrimaryColor] = Microsoft.Maui.Graphics.Color.FromArgb("#0D6EFD"),
        [ResourceKeys.WindowBackground] = Microsoft.Maui.Graphics.Color.FromArgb("#F5F6F8"),
        [ResourceKeys.CanvasBackground] = Microsoft.Maui.Graphics.Color.FromArgb("#FFFFFF"),
        [ResourceKeys.PanelBackground] = Microsoft.Maui.Graphics.Color.FromArgb("#FAFAFA"),
        [ResourceKeys.PanelBorderColor] = Microsoft.Maui.Graphics.Color.FromArgb("#DADADA"),
        [ResourceKeys.SelectionBlue] = Microsoft.Maui.Graphics.Color.FromArgb("#2196F3"),
        [ResourceKeys.TextPrimary] = Microsoft.Maui.Graphics.Color.FromArgb("#1F1F1F"),
        [ResourceKeys.TextSecondary] = Microsoft.Maui.Graphics.Color.FromArgb("#5F6368"),
        [ResourceKeys.HoverBackground] = Microsoft.Maui.Graphics.Color.FromArgb("#EFEFEF"),
        [ResourceKeys.AccentFlow] = Microsoft.Maui.Graphics.Color.FromArgb("#0D6EFD"),
        [ResourceKeys.AccentMindMap] = Microsoft.Maui.Graphics.Color.FromArgb("#7C3AED"),
        [ResourceKeys.AccentOrgChart] = Microsoft.Maui.Graphics.Color.FromArgb("#0F9D58"),
        [ResourceKeys.ErrorColor] = Microsoft.Maui.Graphics.Color.FromArgb("#D93025"),
    };

    /// <summary>
    /// Returns the <see cref="Color"/> for a theme key, or <paramref name="fallback"/>
    /// when the key is empty or unknown.
    /// </summary>
    public static Color Color(string? key, Color? fallback = null)
    {
        fallback ??= Colors.Transparent;
        return string.IsNullOrEmpty(key) || !Palette.TryGetValue(key, out var color)
            ? fallback
            : color;
    }

    /// <summary>Read-only snapshot of every (key, color) the lookup knows about.</summary>
    public static IReadOnlyDictionary<string, Color> PaletteSnapshot() => Palette;

    /// <summary>
    /// Resolves an application resource to a <see cref="Color"/>. The theme exposes
    /// both <c>Color</c> keys (e.g. "TextPrimary") and <c>Brush</c> keys (e.g.
    /// "TextPrimaryBrush"), so a resource may hold either. Casting blindly to
    /// <see cref="Color"/> throws <see cref="InvalidCastException"/> at runtime when a
    /// brush key is used — this helper accepts both and returns the underlying color.
    /// </summary>
    public static Color ResourceColor(string key, Color? fallback = null)
    {
        fallback ??= Colors.Transparent;
        return TryGetResource(key, out var value) ? value switch
        {
            Color color => color,
            SolidColorBrush solid => solid.Color,
            _ => fallback,
        } : fallback;
    }

    /// <summary>
    /// Resolves an application resource to a <see cref="Brush"/>. Accepts a
    /// <c>Color</c> resource (wrapped in a <see cref="SolidColorBrush"/>) or a brush
    /// resource (returned as-is).
    /// </summary>
    public static Brush ResourceBrush(string key, Brush? fallback = null)
    {
        fallback ??= new SolidColorBrush(Colors.Transparent);
        return TryGetResource(key, out var value) ? value switch
        {
            Brush brush => brush,
            Color color => new SolidColorBrush(color),
            _ => fallback,
        } : fallback;
    }

    private static bool TryGetResource(string key, out object? value)
    {
        value = null;
        var resources = Application.Current?.Resources;
        if (resources is null)
        {
            return false;
        }

        // ResourceDictionary.TryGetValue searches merged dictionaries too.
        if (resources.TryGetValue(key, out var found))
        {
            value = found;
            return true;
        }

        return false;
    }
}
