namespace SmartExpense;

/// <summary>
/// Category data access.
/// </summary>
public class CategoryRepository(InMemoryExpenseStore store) : ICategoryRepository
{
    private readonly InMemoryExpenseStore _store = store;

    /// <summary>
    /// Gets all categories.
    /// </summary>
    public async Task<List<Category>> GetAllAsync()
    {
        return await _store.GetAllCategoriesAsync();
    }
}


