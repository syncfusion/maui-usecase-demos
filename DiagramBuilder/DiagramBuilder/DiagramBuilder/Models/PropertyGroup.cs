namespace DiagramBuilder.Models;

/// <summary>
/// Sections of the property inspector panel.
/// </summary>
public enum PropertyGroup
{
    /// <summary>Shape position and size.</summary>
    Dimensions,

    /// <summary>Reserved for future insert actions.</summary>
    Insert,

    /// <summary>Shape fill colour.</summary>
    Fill,

    /// <summary>Shape border style, colour and thickness.</summary>
    Border,

    /// <summary>Shape label typography.</summary>
    Text,

    /// <summary>Drawing-area paper format.</summary>
    Format,

    /// <summary>Drawing-area orientation.</summary>
    Orientation,

    /// <summary>Drawing-area background colour.</summary>
    Background,

    /// <summary>Drawing-area page breaks.</summary>
    PageBreaks,
}
