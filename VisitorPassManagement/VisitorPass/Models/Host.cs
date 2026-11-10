namespace VisitorPass.Models;

/// <summary>
/// Static list of predefined hosts (employees that visitors can be
/// checked in against). Per SPEC §5:
///
///   Sarah Johnson
///   Michael Lee
///   David Wilson
///   Emma Parker
///   John Anderson
///
/// The Register Visitor form renders this list in its Host combo box
/// (VP-072). The Directory + Details screens display the chosen host
/// as plain text. The order below matches SPEC's listing.
/// </summary>
public static class Hosts
{
    /// <summary>Read-only list of every predefined host.</summary>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        "Sarah Johnson",
        "Michael Lee",
        "David Wilson",
        "Emma Parker",
        "John Anderson",
    };

    /// <summary>True when the supplied host name is one of the predefined hosts (case-sensitive).</summary>
    public static bool IsKnownHost(string host) => All.Contains(host);
}
