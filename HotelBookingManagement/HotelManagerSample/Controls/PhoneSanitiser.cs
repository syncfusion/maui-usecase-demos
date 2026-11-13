namespace HotelManagerSample.Controls;

/// <summary>
/// Phone-number sanitisation used by every booking entry surface
/// (the desktop Add-Booking popup, the mobile New-Booking wizard).
/// Keeps the rule — strip everything that isn't a digit, clamp to 10
/// characters — in a single place so the two entry points never drift.
/// </summary>
internal static class PhoneSanitiser
{
    /// <summary>The canonical 10-digit US-phone length. Both entry
    /// surfaces clip past this to keep validators honest.</summary>
    public const int PhoneLength = 10;

    /// <summary>Returns the supplied raw text with every non-digit
    /// character stripped and the result truncated to
    /// <see cref="PhoneLength"/> characters.</summary>
    /// <param name="raw">Raw text from the entry control. May be null
    /// or empty.</param>
    /// <returns>A cleaned digit-only string of length
    /// <c>0..<see cref="PhoneLength"/></c>.</returns>
    public static string Clean(string? raw)
    {
        var text = raw ?? string.Empty;
        var digitsOnly = new string(text.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length > PhoneLength)
            digitsOnly = digitsOnly[..PhoneLength];
        return digitsOnly;
    }
}