using System.Collections.ObjectModel;

namespace SmartExpense;

/// <summary>
/// In-memory application state service.
/// Holds observable collections shared across ViewModels so the UI
/// stays in sync when data changes.
/// </summary>
public class AppStateService : IAppStateService
{
    private readonly InMemoryExpenseStore _store;

    /// <summary>
    /// Observable collection of all transactions.
    /// Bound directly to UI elements for automatic updates.
    /// </summary>
    public ObservableCollection<Transaction> Transactions { get; } = new();

    /// <summary>
    /// Observable collection of all categories.
    /// Bound directly to UI elements for automatic updates.
    /// </summary>
    public ObservableCollection<Category> Categories { get; } = new();

    /// <summary>
    /// Creates a new instance of the app state service.
    /// </summary>
    public AppStateService(InMemoryExpenseStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Loads transactions and categories from the in-memory store
    /// and initializes observable collections.
    /// </summary>
    public async Task InitializeAsync()
    {
        var transactions = await _store.GetAllTransactionsAsync();

        Transactions.Clear();
        foreach (var transaction in transactions.OrderByDescending(t => t.Date))
        {
            Transactions.Add(transaction);
        }

        var categories = await _store.GetAllCategoriesAsync();

        Categories.Clear();
        foreach (var category in categories.OrderBy(c => c.Name))
        {
            Categories.Add(category);
        }
    }
}
