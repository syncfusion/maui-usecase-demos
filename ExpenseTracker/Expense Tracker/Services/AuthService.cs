using System.Security.Cryptography;
using System.Text;

namespace SmartExpense;

/// <summary>
/// Provides user authentication, registration, and session management
/// using in-memory user data and secure local storage.
/// </summary>
public class AuthService(IUserRepository userRepository) 
{
    private const string SessionKey = "smartexpense_session_username";

    /// <summary>
    /// Retrieves the username of the active session, if any.
    /// </summary>
    public Task<string?> GetSessionUsernameAsync()
        => SafeSecureGetAsync(SessionKey)!;



    /// <summary>
    /// Stores the session username for display or profile usage.
    /// </summary>
    public Task SetSessionUsernameAsync(string username)
    {
        return SafeSecureSetAsync(SessionKey, username);
    }

    /// <summary>
    /// Stores a value securely with a preferences fallback.
    /// </summary>
    private async Task SafeSecureSetAsync(string key, string value)
    {
        try
        {
            await SecureStorage.SetAsync(key, value);
        }
        catch (Exception)
        {
            Preferences.Set(key, value);
        }
    }

    /// <summary>
    /// Retrieves a securely stored value with a preferences fallback.
    /// </summary>
    private async Task<string?> SafeSecureGetAsync(string key)
    {
        try
        {
            return await SecureStorage.GetAsync(key);
        }
        catch (Exception)
        {
            return Preferences.Get(key, (string?)null);
        }
    }
}
