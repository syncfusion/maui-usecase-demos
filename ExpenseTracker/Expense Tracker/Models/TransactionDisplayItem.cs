
namespace SmartExpense;

/// <summary>
/// Represents a simplified transaction model used for UI display.
/// Contains only the data required for lists, summaries, and visuals.
/// </summary>
public class TransactionDisplayItem
{
    /// <summary>
    /// Unique transaction identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Date when the transaction occurred.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Short description or title of the transaction.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Category name associated with the transaction.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Icon glyph used to visually represent the category.
    /// </summary>
    public string IconGlyph { get; set; } = string.Empty;

    /// <summary>
    /// Transaction type (for example, Income or Expense).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Background color associated with the category icon.
    /// </summary>
    public Color IconBackground { get; set; }
}