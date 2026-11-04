using System.Globalization;

namespace VisitorPass.Converters;

/// <summary>
/// VP-024 — converts a person's full name into a 1-2 character
/// monogram used by SfAvatarView / VisitorAvatar.
///
///   "Sarah Johnson"  -> "SJ"
///   "Madonna"        -> "M"
///   ""               -> "?"
///
/// The logic is shared with Visitor.Initials (in Models/Visitor.cs);
/// this converter exists so XAML bindings that only have a name string
/// (not a full Visitor) can still render an avatar.
/// </summary>
public sealed class InitialsConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
            return "?";

        var joined = string.Join(string.Empty,
            name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(part => part.Length > 0 ? part[0].ToString() : string.Empty));

        return joined.Length switch
        {
            0 => "?",
            _ => joined.Length > 2 ? joined[..2].ToUpperInvariant() : joined.ToUpperInvariant()
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
