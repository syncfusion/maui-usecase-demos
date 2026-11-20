
namespace SmartExpense;

/// <summary>
/// Category model used in memory.
/// Represents a basic expense category with visual metadata.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique category identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Icon or symbol representing the category.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Hex color code associated with the category.
    /// </summary>
    public string Color { get; set; } = "#4CAF50";
}
