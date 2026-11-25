namespace SmartExpense;

/// <summary>
/// Defines the contract for transaction-related operations.
/// Provides query, aggregation, and persistence capabilities
/// for financial data used throughout the application.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Retrieves all stored transactions.
    /// </summary>
    Task<List<Transaction>> GetAllTransactionsAsync();

    /// <summary>
    /// Retrieves transactions for the specified month and year.
    /// </summary>
    Task<List<Transaction>> GetTransactionsByMonthAsync(int year, int month);

    /// <summary>
    /// Retrieves transactions within the specified date range.
    /// </summary>
    Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end);

    /// <summary>
    /// Normalizes stored transactions to ensure data consistency.
    /// </summary>
    Task NormalizeStoredTransactionsAsync();

    /// <summary>
    /// Retrieves transactions for the specified year.
    /// </summary>
    Task<List<Transaction>> GetTransactionsByYearAsync(int year);

    /// <summary>
    /// Retrieves all available categories.
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Saves a transaction (create or update).
    /// </summary>
    Task SaveTransactionAsync(Transaction transaction);

    /// <summary>
    /// Deletes the specified transaction.
    /// </summary>
    Task DeleteTransactionAsync(Transaction transaction);

    /// <summary>
    /// Returns aggregated values for the dashboard summary.
    /// </summary>
    Task<DashboardSummary> GetDashboardSummaryAsync();

    /// <summary>
    /// Returns monthly income, expense, and savings trend data.
    /// </summary>
    Task<List<MonthlyTrendPoint>> GetMonthlyTrendAsync(int year);

    /// <summary>
    /// Returns a category-wise breakdown for the given month.
    /// If <paramref name="income"/> is true, returns income breakdown;
    /// otherwise, returns expense breakdown.
    /// </summary>
    Task<List<CategoryBreakdownItem>> GetCategoryBreakdownAsync(
        int year,
        int month,
        bool income = false);
}
