namespace SmartExpense;

/// <summary>
/// Interface defining contract for accessing and managing
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their username. Returns null if no user is found with the given username.
    /// </summary>
    Task<AppUser?> GetByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user in the database. Returns the ID of the newly created user.
    /// </summary>
    Task<int> CreateUserAsync(AppUser user);

    /// <summary>
    /// Updates an existing user's information in the database. Returns true if the update was successful, false otherwise.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
}
