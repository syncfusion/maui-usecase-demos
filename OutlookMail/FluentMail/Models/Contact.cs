namespace FluentMail.Models;

public class Contact
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsExternal => !Email.EndsWith("@contoso.com", StringComparison.OrdinalIgnoreCase);
}
