using System.Collections.ObjectModel;

namespace DiagramBuilder.Models;

/// <summary>
/// A predefined or blank diagram template shown as a card in the create dialog.
/// </summary>
public class DiagramTemplate
{
    public string Id { get; set; } = string.Empty;

    public DiagramCategory Category { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsBlank { get; set; }

    /// <summary>Theme accent key (ResourceKeys.Accent*) for the category swatch.</summary>
    public string AccentColorKey { get; set; } = string.Empty;

    /// <summary>Resource key of the card thumbnail swatch style.</summary>
    public string ThumbnailKey { get; set; } = string.Empty;

    public ObservableCollection<TemplateNode> Nodes { get; } = [];

    public ObservableCollection<TemplateConnector> Connectors { get; } = [];
}

/// <summary>
/// JSON-mappable node descriptor inside a template payload.
/// </summary>
public class TemplateNode
{
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public ShapeType ShapeType { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public string FillColorKey { get; set; } = string.Empty;

    public bool IsManager { get; set; }
}

/// <summary>
/// JSON-mappable connector descriptor inside a template payload.
/// </summary>
public class TemplateConnector
{
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public ConnectorKind Kind { get; set; }

    public string StrokeColorKey { get; set; } = string.Empty;
}