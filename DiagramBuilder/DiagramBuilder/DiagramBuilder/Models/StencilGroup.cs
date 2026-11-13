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

    /// <summary>Initial expansion state shown in the stencil panel accordion.</summary>
    public bool IsExpanded { get; set; }

    /// <summary>Human-readable item count for the group header.</summary>
    public string ItemCountText => Items.Count == 0 ? string.Empty : Items.Count.ToString();

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