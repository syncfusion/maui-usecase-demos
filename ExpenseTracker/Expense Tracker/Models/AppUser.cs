
namespace SmartExpense;

/// <summary>
/// Represents an application user stored in memory.
/// </summary>
public class AppUser
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique username used for login.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password value.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
