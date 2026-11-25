namespace SmartExpense;

/// <summary>
/// Defines contract for accessing and managing
/// <see cref="Category"/> related data operations.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves all categories from the data source.
    /// </summary>
    Task<List<Category>> GetAllAsync();
}