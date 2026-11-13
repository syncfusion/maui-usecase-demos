using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DiagramBuilder.Models;

/// <summary>
/// In-memory diagram document (items, metadata, dirty flag).
/// </summary>
public class DiagramDocument
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DiagramCategory Category { get; set; }

    public ObservableCollection<ShapeItem> Items { get; set; } = [];

    public ObservableCollection<ConnectorItem> Connectors { get; set; } = [];

    public bool IsDirty { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public string? SavedFilePath { get; set; }
}