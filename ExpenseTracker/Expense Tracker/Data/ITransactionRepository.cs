namespace SmartExpense;

/// <summary>
/// Defines the contract for accessing and managing
/// <see cref="Transaction"/> related data operations.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Retrieves all transactions.
    /// </summary>
    Task<List<Transaction>> GetAllAsync();

    /// <summary>
    /// Retrieves transactions for a specific month and year.
    /// </summary>
    /// <param name="year">The year to filter transactions.</param>
    /// <param name="month">The month (1–12) to filter transactions.</param>
    Task<List<Transaction>> GetByMonthAsync(int year, int month);

    /// <summary>
    /// Retrieves transactions within the specified date range.
    /// </summary>
    /// <param name="start">The start date of the range.</param>
    /// <param name="end">The end date of the range.</param>
    Task<List<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end);

    /// <summary>
    /// Retrieves all transactions for a specific year.
    /// </summary>
    /// <param name="year">The year to filter transactions.</param>
    Task<List<Transaction>> GetByYearAsync(int year);

    /// <summary>
    /// Retrieves a single transaction by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the transaction.</param>
    Task<Transaction?> GetByIdAsync(string id);

    /// <summary>
    /// Inserts a new transaction into the data store.
    /// </summary>
    /// <param name="transaction">The transaction to insert.</param>
    Task<int> InsertAsync(Transaction transaction);

    /// <summary>
    /// Updates an existing transaction in the data store.
    /// </summary>
    /// <param name="transaction">The transaction to update.</param>
    Task<int> UpdateAsync(Transaction transaction);

    /// <summary>
    /// Deletes a transaction from the data store.
    /// </summary>
    /// <param name="transaction">The transaction to delete.</param>
    Task<int> DeleteAsync(Transaction transaction);
}