
namespace SmartExpense;

/// <summary>
/// Represents an application-level expense category with visual metadata
/// and optional subcategories.
/// </summary>
public class AppCategory
{
    /// <summary>
    /// Display name of the category.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Icon font glyph associated with the category.
    /// </summary>
    public string IconGlyph { get; init; } = string.Empty;

    /// <summary>
    /// Hex color code used for category visuals.
    /// </summary>
    public string Color { get; init; } = "#000000";

    /// <summary>
    /// Read-only list of supported subcategories.
    /// </summary>
    public IReadOnlyList<string> SubCategories { get; init; } = Array.Empty<string>();
}
