
namespace SmartExpense;

/// <summary>
/// Encapsulates key financial metrics displayed on the dashboard.
/// Aggregates overall and current-month income, expenses, and balance values.
/// </summary>
public class DashboardSummary
{
    /// <summary>
    /// Net balance calculated from total income and expenses.
    /// </summary>
    public int TotalBalance { get; set; }

    /// <summary>
    /// Cumulative income across all recorded transactions.
    /// </summary>
    public int TotalIncome { get; set; }

    /// <summary>
    /// Cumulative expenses across all recorded transactions.
    /// </summary>
    public int TotalExpense { get; set; }

    /// <summary>
    /// Income recorded for the current month.
    /// </summary>
    public int CurrentMonthIncome { get; set; }

    /// <summary>
    /// Expenses recorded for the current month.
    /// </summary>
    public int CurrentMonthExpense { get; set; }
}
