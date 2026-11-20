namespace SmartExpense;

/// <summary>
/// Provides predefined category data used to seed the application on startup.
/// Centralizes default category definitions for consistency and reuse.
/// </summary>
public static class CategorySeedData
{
    /// <summary>
    /// Returns the default set of expense categories with name, icon, and color.
    /// </summary>
    public static IReadOnlyList<Category> GetDefaultCategories() =>
    [
        new Category { Name = "Food & Dining",    Icon = "🍽️", Color = "#E64A19" },
        new Category { Name = "Transportation",  Icon = "🚗", Color = "#455A64" },
        new Category { Name = "Utilities",        Icon = "💡", Color = "#455A64" },
        new Category { Name = "Housing",          Icon = "🏠", Color = "#1565C0" },
        new Category { Name = "Health & Fitness", Icon = "💪", Color = "#AD1457" },
        new Category { Name = "Travel",           Icon = "✈️", Color = "#1565C0" },
        new Category { Name = "Social Events",    Icon = "🎉", Color = "#6A1B9A" },
        new Category { Name = "Education",        Icon = "🎓", Color = "#2E7D32" },
        new Category { Name = "Others",            Icon = "📌", Color = "#5D4037" },
        new Category { Name = "Freelance",        Icon = "🧾", Color = "#00695C" },
        new Category { Name = "Bills",             Icon = "🧾", Color = "#8D6E63" },
        new Category { Name = "Groceries",         Icon = "🛒", Color = "#388E3C" },
        new Category { Name = "Entertainment",    Icon = "🎬", Color = "#6A1B9A" }
    ];
}
