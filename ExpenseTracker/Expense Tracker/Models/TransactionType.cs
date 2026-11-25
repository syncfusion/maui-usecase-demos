
namespace SmartExpense;

/// <summary>
/// Defines the supported types of financial transactions.
/// Used for categorization, filtering, and reporting.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Represents money spent.
    /// </summary>
    Expense = 0,

    /// <summary>
    /// Represents money received.
    /// </summary>
    Income = 1
}
