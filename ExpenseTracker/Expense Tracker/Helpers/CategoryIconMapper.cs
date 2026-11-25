
namespace SmartExpense;

/// <summary>
/// Maps expense categories to their corresponding icon glyphs and background colors.
/// Centralizes UI-related category visuals.
/// </summary>
public static class CategoryIconMapper
{
    /// <summary>
    /// Returns the icon glyph name for a given category.
    /// </summary>
    public static string GetIconGlyph(string categoryName)
    {
        return categoryName switch
        {
            "Food & Dining" => "restaurant",
            "Transportation" => "directions_car",
            "Utilities" => "bolt",
            "Housing" => "home",
            "Health & Fitness" => "fitness_center",
            "Travel" => "flight",
            "Social Events" => "event",
            "Education" => "school",
            "Others" => "more_horiz",
            "Freelance" => "business_center",
            "Bills" => "receipt_long",
            "Groceries" => "local_grocery_store",
            "Entertainment" => "movie",
            _ => "home"
        };
    }

    /// <summary>
    /// Returns the background color associated with a category icon.
    /// </summary>
    public static Color GetIconBackground(string category)
    {
        return category switch
        {
            "Food & Dining" => Color.FromArgb("#E64A19"),
            "Transportation" => Color.FromArgb("#455A64"),
            "Utilities" => Color.FromArgb("#455A64"),
            "Housing" => Color.FromArgb("#1565C0"),
            "Health & Fitness" => Color.FromArgb("#AD1457"),
            "Travel" => Color.FromArgb("#1565C0"),
            "Social Events" => Color.FromArgb("#F9A825"),
            "Education" => Color.FromArgb("#3949AB"),
            "Others" => Color.FromArgb("#5D4037"),
            "Freelance" => Color.FromArgb("#00695C"),
            "Bills" => Color.FromArgb("#8D6E63"),
            "Groceries" => Color.FromArgb("#388E3C"),
            "Entertainment" => Color.FromArgb("#6A1B9A"),
            _ => Color.FromArgb("#90A4AE")
        };
    }
}