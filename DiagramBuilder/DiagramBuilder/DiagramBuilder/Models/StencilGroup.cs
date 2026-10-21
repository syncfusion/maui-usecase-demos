using System.Collections.ObjectModel;

namespace DiagramBuilder.Models;

/// <summary>
/// Stencil category (collapsible group in the left panel).
/// </summary>
public class StencilGroup
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string IconKey { get; set; } = string.Empty;

    public bool IsConnectorGroup { get; set; }

    public ObservableCollection<StencilItem> Items { get; } = [];
}

/// <summary>
/// A stencil tile: a shape or a connector tool.
/// </summary>
public class StencilItem
{
    public string Id { get; set; } = string.Empty;

    public ShapeType? ShapeType { get; set; }

    public ConnectorKind? ConnectorKind { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string[] SearchTags { get; set; } = [];

    public string IconKey { get; set; } = string.Empty;
}