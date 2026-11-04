using System.Collections.ObjectModel;

namespace DiagramBuilder.Models;

/// <summary>
/// In-memory diagram document (items, metadata, dirty flag).
/// </summary>
public class DiagramDocument
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DiagramCategory Category { get; set; }

    public ObservableCollection<ShapeItem> Items { get; } = [];

    public ObservableCollection<ConnectorItem> Connectors { get; } = [];

    public bool IsDirty { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}