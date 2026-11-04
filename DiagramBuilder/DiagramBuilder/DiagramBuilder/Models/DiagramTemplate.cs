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

    /// <summary>Optional layout configuration for auto-layout algorithms (e.g., OrganizationalChart, MindMap).</summary>
    public DiagramLayoutTemplate? Layout { get; set; }

    public ObservableCollection<TemplateNode> Nodes { get; } = [];

    public ObservableCollection<TemplateConnector> Connectors { get; } = [];
}

/// <summary>
/// Layout configuration stored in template JSON.
/// </summary>
public class DiagramLayoutTemplate
{
    public string Type { get; set; } = string.Empty;

    public string Orientation { get; set; } = "TopToBottom";

    public double HorizontalSpacing { get; set; } = 30;

    public double VerticalSpacing { get; set; } = 30;

    public string? Root { get; set; }
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

    public double StrokeThickness { get; set; } = 2.0;  // Default thickness

    public double SourceX { get; set; }

    public double SourceY { get; set; }

    public double TargetX { get; set; }

    public double TargetY { get; set; }
}