
namespace SmartExpense;

/// <summary>
/// Defines mappings between expense categories and their supported subcategories.
/// Centralizes category-to-subcategory relationships for consistent reuse.
/// </summary>
public static class CategorySubcategoryMap
{
    /// <summary>
    /// Maps a category name to its list of subcategories.
    /// </summary>
    public static readonly Dictionary<string, List<string>> Map = new()
    {
        ["Food & Dining"] = new() { "Dining Out", "Groceries" },
        ["Transportation"] = new() { "Gasoline", "Public Transit" },
        ["Utilities"] = new() { "Electricity", "Water" },
        ["Housing"] = new() { "Rent", "Mortgage" },
        ["Health & Fitness"] = new() { "Gym", "Medical" },
        ["Travel"] = new() { "Flights", "Accommodations", "Sightseeing" },
        ["Social Events"] = new() { "Parties", "Concerts", "Weddings" },
        ["Education"] = new() { "Tuition", "Books", "Online Courses" },
        ["Others"] = new() { "Others" },
        ["Freelance"] = new() { "Project Based", "Consulting" },
        ["Bills"] = new() { "Electricity Bill", "Water Bill", "Internet Bill" },
        ["Groceries"] = new() { "Fruits", "Vegetables", "Household Items" },
        ["Entertainment"] = new() { "Movies", "Games", "Music Concerts" },
    };
}
