namespace SmartExpense;

/// <summary>
/// In-memory user repository backed by <see cref="InMemoryExpenseStore"/>.
/// Provides user lookup and creation without persistent storage.
/// </summary>
public class UserRepository(InMemoryExpenseStore store) : IUserRepository
{
    private readonly InMemoryExpenseStore _store = store;

    /// <summary>
    /// Retrieves a user by username.
    /// </summary>
    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
        return await _store.GetUserByUsernameAsync(username);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public async Task<int> CreateUserAsync(AppUser user)
    {
        return await _store.CreateUserAsync(user);
    }

    /// <summary>
    /// Checks whether the username already exists.
    /// </summary>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _store.UsernameExistsAsync(username);
    }
}
