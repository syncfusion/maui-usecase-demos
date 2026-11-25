
namespace SmartExpense;

/// <summary>
/// Represents basic profile information for the current user.
/// Used for personalization and dashboard display.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// User's first name. Defaults to "Guest".
    /// </summary>
    public string FirstName { get; set; } = "Guest";

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's date of birth, if provided.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's gender, if specified.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Preferred currency code or symbol.
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Computed initials used by the dashboard profile avatar.
    /// </summary>
    public string Initials
    {
        get
        {
            if (FirstName == "Guest")
                return "G";

            if (string.IsNullOrWhiteSpace(LastName))
                return FirstName.Substring(0, 1).ToUpper();

            return $"{FirstName[0]}{LastName[0]}".ToUpper();
        }
    }
}
