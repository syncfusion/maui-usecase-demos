using System.Collections.ObjectModel;

namespace SmartExpense;

/// <summary>
/// Defines a shared in-memory application state.
/// Exposes observable collections that act as the single source of truth
/// for runtime-only data consumed by ViewModels.
/// </summary>
public interface IAppStateService
{
    /// <summary>
    /// Observable collection of all transactions.
    /// Changes propagate automatically to bound UI elements.
    /// </summary>
    ObservableCollection<Transaction> Transactions { get; }

    /// <summary>
    /// Observable collection of all categories.
    /// Used for lookup, filtering, and UI binding.
    /// </summary>
    ObservableCollection<Category> Categories { get; }

    /// <summary>
    /// Initializes the application state by loading data
    /// from the underlying in-memory store.
    /// </summary>
    Task InitializeAsync();
}
