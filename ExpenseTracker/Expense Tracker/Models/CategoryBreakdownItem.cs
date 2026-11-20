namespace SmartExpense;

/// <summary>
/// Represents a summarized breakdown of an amount grouped by category.
/// Used for charts, legends, and summary views in the UI.
/// </summary>
public class CategoryBreakdownItem
{
    /// <summary>
    /// Category name for the breakdown entry.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Total amount for the category.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Percentage contribution of the category to the total.
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Pre-formatted legend text combining category and amount.
    /// Used directly by chart legends.
    /// </summary>
    public string LegendText =>
        $"{Category} - {Amount.ToString(AppConstants.CurrencyFormat)}";
}
