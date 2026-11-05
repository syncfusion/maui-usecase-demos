namespace DiagramBuilder.Models;

/// <summary>
/// A paper format offered by the drawing-area "Format" dropdown. Width and
/// height are in inches, matching the MAUI Diagram export API
/// (<c>DiagramExportOptions.PageWidth</c> / <c>PageHeight</c>).
/// </summary>
/// <param name="Label">Display text, e.g. "A3 (11.7 in x 16.53 in)".</param>
/// <param name="Width">Paper width in inches (portrait).</param>
/// <param name="Height">Paper height in inches (portrait).</param>
public sealed record PaperFormat(string Label, double Width, double Height)
{
    /// <summary>Formats offered by the drawing-area Format dropdown.</summary>
    public static readonly IReadOnlyList<PaperFormat> All =
    [
        new("Letter (8.5 in x 11 in)", 8.5, 11),
        new("Legal (8.5 in x 14 in)", 8.5, 14),
        new("Tabloid (11 in x 17 in)", 11, 17),
        new("A3 (11.7 in x 16.53 in)", 11.7, 16.53),
        new("A4 (8.27 in x 11.7 in)", 8.27, 11.7),
        new("A5 (5.82 in x 8.27 in)", 5.82, 8.27),
        new("A6 (4.1 in x 5.8 in)", 4.1, 5.8),
    ];

    /// <summary>The format used when nothing has been chosen yet.</summary>
    public static PaperFormat Default => All[3]; // A3

    /// <summary>Finds a format by its display label.</summary>
    public static PaperFormat? FromLabel(string? label) =>
        All.FirstOrDefault(f => string.Equals(f.Label, label, StringComparison.Ordinal));
}
