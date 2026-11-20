
namespace SmartExpense;

/// <summary>
/// Represents a single data point for monthly financial trends.
/// Used for visualizing income, expenses, and savings over time.
/// </summary>
public class MonthlyTrendPoint
{
    /// <summary>
    /// Month label (for example, "Jan 2026").
    /// </summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Total income for the month.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Total expenses for the month.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Calculated savings for the month.
    /// </summary>
    public decimal Savings => Income - Expense;
}
