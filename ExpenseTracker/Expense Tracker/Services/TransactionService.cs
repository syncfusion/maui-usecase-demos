namespace SmartExpense;

/// <summary>
/// Provides high-level transaction operations, aggregations, and summaries.
/// Acts as the orchestration layer between repositories and in-memory app state,
/// ensuring UI collections remain synchronized with underlying data.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAppStateService _appState;

    /// <summary>
    /// Initializes the transaction service with required dependencies.
    /// </summary>
    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IAppStateService appState)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _appState = appState;
    }

    /// <summary>
    /// Retrieves all transactions.
    /// </summary>
    public Task<List<Transaction>> GetAllTransactionsAsync()
        => _transactionRepository.GetAllAsync();

    /// <summary>
    /// Retrieves transactions for a specific month.
    /// </summary>
    public Task<List<Transaction>> GetTransactionsByMonthAsync(int year, int month)
        => _transactionRepository.GetByMonthAsync(year, month);

    /// <summary>
    /// Retrieves transactions within a date range.
    /// </summary>
    public Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
        => _transactionRepository.GetByDateRangeAsync(start, end);

    /// <summary>
    /// Retrieves transactions for a specific year.
    /// </summary>
    public Task<List<Transaction>> GetTransactionsByYearAsync(int year)
        => _transactionRepository.GetByYearAsync(year);

    /// <summary>
    /// Retrieves all available categories.
    /// </summary>
    public Task<List<Category>> GetCategoriesAsync()
        => _categoryRepository.GetAllAsync();

    /// <summary>
    /// Creates or updates a transaction and synchronizes UI state.
    /// </summary>
    public async Task SaveTransactionAsync(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Id))
            transaction.Id = Guid.NewGuid().ToString("N");

        var existing = await _transactionRepository.GetByIdAsync(transaction.Id);

        if (existing is null)
        {
            await _transactionRepository.InsertAsync(transaction);

            MainThread.BeginInvokeOnMainThread(() =>
                _appState.Transactions.Insert(0, transaction));
        }
        else
        {
            await _transactionRepository.UpdateAsync(transaction);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var index = _appState.Transactions.IndexOf(existing);
                if (index >= 0)
                    _appState.Transactions[index] = transaction;
            });
        }
    }

    /// <summary>
    /// Deletes a transaction and updates UI state.
    /// </summary>
    public async Task DeleteTransactionAsync(Transaction transaction)
    {
        await _transactionRepository.DeleteAsync(transaction);

        MainThread.BeginInvokeOnMainThread(() =>
            _appState.Transactions.Remove(transaction));
    }

    /// <summary>
    /// Ensures stored transactions have normalized type and amount values.
    /// </summary>
    public async Task NormalizeStoredTransactionsAsync()
    {
        var items = await _transactionRepository.GetAllAsync();

        foreach (var transaction in items)
        {
            var desiredType =
                (int)transaction.Type == (int)TransactionType.Income
                    ? TransactionType.Income
                    : TransactionType.Expense;

            var desiredAmount = Math.Abs(transaction.Amount);

            if (transaction.Type != desiredType || transaction.Amount != desiredAmount)
            {
                transaction.Type = desiredType;
                transaction.Amount = desiredAmount;
                await _transactionRepository.UpdateAsync(transaction);
            }
        }
    }

    /// <summary>
    /// Computes aggregated values for the dashboard summary.
    /// </summary>
    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        var items = await _transactionRepository.GetAllAsync();
        var now = DateTime.Now;

        var totalIncome = items
            .Where(t => (int)t.Type == (int)TransactionType.Income)
            .Sum(t => Math.Abs(t.Amount));

        var totalExpense = items
            .Where(t => (int)t.Type != (int)TransactionType.Income)
            .Sum(t => Math.Abs(t.Amount));

        var currentMonthItems = items
            .Where(t => t.Date.Year == now.Year && t.Date.Month == now.Month)
            .ToList();

        var currentMonthIncome = currentMonthItems
            .Where(t => (int)t.Type == (int)TransactionType.Income)
            .Sum(t => Math.Abs(t.Amount));

        var currentMonthExpense = currentMonthItems
            .Where(t => (int)t.Type != (int)TransactionType.Income)
            .Sum(t => Math.Abs(t.Amount));

        return new DashboardSummary
        {
            TotalIncome = (int)totalIncome,
            TotalExpense = (int)totalExpense,
            TotalBalance = (int)(totalIncome - totalExpense),
            CurrentMonthIncome = (int)currentMonthIncome,
            CurrentMonthExpense = (int)currentMonthExpense
        };
    }

    /// <summary>
    /// Generates monthly income and expense trend data for the given year.
    /// </summary>
    public async Task<List<MonthlyTrendPoint>> GetMonthlyTrendAsync(int year)
    {
        var items = await _transactionRepository.GetByYearAsync(year);
        var points = new List<MonthlyTrendPoint>();

        var now = DateTime.Now;
        var lastMonth = year == now.Year ? now.Month : 12;

        for (var month = 1; month <= lastMonth; month++)
        {
            var monthItems = items
                .Where(t => t.Date.Month == month)
                .ToList();

            points.Add(new MonthlyTrendPoint
            {
                Month = new DateTime(year, month, 1).ToString("MMM"),
                Income = monthItems
                    .Where(t => (int)t.Type == (int)TransactionType.Income)
                    .Sum(t => Math.Abs(t.Amount)),
                Expense = monthItems
                    .Where(t => (int)t.Type != (int)TransactionType.Income)
                    .Sum(t => Math.Abs(t.Amount))
            });
        }

        return points;
    }

    /// <summary>
    /// Computes a category-wise breakdown for a given month.
    /// </summary>
    public async Task<List<CategoryBreakdownItem>> GetCategoryBreakdownAsync(
        int year,
        int month,
        bool income = false)
    {
        var items = await _transactionRepository.GetByMonthAsync(year, month);

        var filtered = income
            ? items.Where(t => (int)t.Type == (int)TransactionType.Income).ToList()
            : items.Where(t => (int)t.Type != (int)TransactionType.Income).ToList();

        var total = filtered.Sum(t => Math.Abs(t.Amount));
        if (total <= 0)
            return new List<CategoryBreakdownItem>();

        return filtered
            .GroupBy(t => t.Category)
            .Select(group =>
            {
                var amount = group.Sum(t => Math.Abs(t.Amount));
                return new CategoryBreakdownItem
                {
                    Category = group.Key,
                    Amount = amount,
                    Percentage = (double)(amount / total * 100)
                };
            })
            .OrderByDescending(item => item.Amount)
            .ToList();
    }
}
