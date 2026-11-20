namespace SmartExpense;

/// <summary>
/// In-memory store for expense-related data.
/// Maintains transactions, categories, and users for the app lifetime.
/// Thread-safe via a semaphore.
/// </summary>
public class InMemoryExpenseStore
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly List<Transaction> _transactions = new();
    private readonly List<Category> _categories = new();
    private readonly List<AppUser> _users = new();

    /// <summary>
    /// Initializes the store and seeds default categories.
    /// </summary>
    public InMemoryExpenseStore()
    {
        _categories.AddRange(CategorySeedData.GetDefaultCategories());
    }

    // Transactions

    /// <summary>
    /// Returns all transactions sorted by date (descending).
    /// </summary>
    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _transactions
                .OrderByDescending(t => t.Date)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Returns transactions matching the given predicate.
    /// </summary>
    public async Task<List<Transaction>> GetTransactionsByPredicateAsync(Func<Transaction, bool> predicate)
    {
        await _lock.WaitAsync();
        try
        {
            return _transactions
                .Where(predicate)
                .OrderByDescending(t => t.Date)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Finds a transaction by its identifier.
    /// </summary>
    public async Task<Transaction?> GetTransactionByIdAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            return _transactions.FirstOrDefault(t => t.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Adds a new transaction to the store.
    /// </summary>
    public async Task<int> InsertTransactionAsync(Transaction tx)
    {
        await _lock.WaitAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(tx.Id))
                tx.Id = Guid.NewGuid().ToString("N");

            _transactions.Add(tx);
            return 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Updates an existing transaction.
    /// </summary>
    public async Task<int> UpdateTransactionAsync(Transaction tx)
    {
        await _lock.WaitAsync();
        try
        {
            var idx = _transactions.FindIndex(t => t.Id == tx.Id);
            if (idx == -1)
                return 0;

            _transactions[idx] = tx;
            return 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Removes a transaction from the store.
    /// </summary>
    public async Task<int> DeleteTransactionAsync(Transaction tx)
    {
        await _lock.WaitAsync();
        try
        {
            return _transactions.RemoveAll(t => t.Id == tx.Id) > 0 ? 1 : 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    // Categories

    /// <summary>
    /// Returns all categories sorted by name.
    /// </summary>
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _categories
                .OrderBy(c => c.Name)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    // Users

    /// <summary>
    /// Retrieves a user by username.
    /// </summary>
    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        await _lock.WaitAsync();
        try
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Adds a new user to the store.
    /// </summary>
    public async Task<int> CreateUserAsync(AppUser user)
    {
        await _lock.WaitAsync();
        try
        {
            _users.Add(user);
            return 1;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Checks whether a username already exists.
    /// </summary>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        await _lock.WaitAsync();
        try
        {
            return _users.Any(u => u.Username == username);
        }
        finally
        {
            _lock.Release();
        }
    }
}
