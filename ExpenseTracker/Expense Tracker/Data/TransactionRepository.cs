namespace SmartExpense;

/// <summary>
/// In-memory transaction repository.
/// Provides transaction queries and commands backed by <see cref="InMemoryExpenseStore"/>.
/// </summary>
public class TransactionRepository(InMemoryExpenseStore store) : ITransactionRepository
{
    private readonly InMemoryExpenseStore _store = store;

    /// <summary>
    /// Returns all transactions.
    /// </summary>
    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _store.GetAllTransactionsAsync();
    }

    /// <summary>
    /// Returns transactions for a specific month.
    /// </summary>
    public async Task<List<Transaction>> GetByMonthAsync(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        return await _store.GetTransactionsByPredicateAsync(
            t => t.Date >= start && t.Date < end);
    }

    /// <summary>
    /// Returns transactions within the given date range (inclusive).
    /// </summary>
    public async Task<List<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        var endExclusive = end.Date.AddDays(1);

        return await _store.GetTransactionsByPredicateAsync(
            t => t.Date >= start.Date && t.Date < endExclusive);
    }

    /// <summary>
    /// Returns transactions for a specific year.
    /// </summary>
    public async Task<List<Transaction>> GetByYearAsync(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);

        return await _store.GetTransactionsByPredicateAsync(
            t => t.Date >= start && t.Date < end);
    }

    /// <summary>
    /// Retrieves a transaction by its identifier.
    /// </summary>
    public async Task<Transaction?> GetByIdAsync(string id)
    {
        return await _store.GetTransactionByIdAsync(id);
    }

    /// <summary>
    /// Inserts a new transaction.
    /// </summary>
    public async Task<int> InsertAsync(Transaction transaction)
    {
        return await _store.InsertTransactionAsync(transaction);
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    public async Task<int> UpdateAsync(Transaction transaction)
    {
        return await _store.UpdateTransactionAsync(transaction);
    }

    /// <summary>
    /// Deletes a transaction.
    /// </summary>
    public async Task<int> DeleteAsync(Transaction transaction)
    {
        return await _store.DeleteTransactionAsync(transaction);
    }
}
