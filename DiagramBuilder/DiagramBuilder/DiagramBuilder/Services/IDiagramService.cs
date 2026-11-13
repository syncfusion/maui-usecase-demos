using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// Document store: holds the current <see cref="DiagramDocument"/> and supports
/// loading, save, and save-as operations.
/// </summary>
public interface IDiagramService
{
    /// <summary>Current document, or null before the first create.</summary>
    DiagramDocument? CurrentDocument { get; }

    /// <summary>Raised with a status message (e.g. after save).</summary>
    event EventHandler<string>? StatusChanged;

    /// <summary>Raised after a document is created or opened so the designer can load it.</summary>
    event EventHandler<DiagramDocument>? DocumentLoaded;

    Task<DiagramDocument> CreateDocumentAsync(DiagramTemplate template);

    Task LoadDocumentAsync(DiagramDocument document);

    Task SaveAsync(DiagramDocument snapshot, string fileName);

    Task SaveAsAsync(DiagramDocument snapshot, string title);

    void SetDirty();
}

/// <summary>
/// Template provider: preloads JSON payloads from Resources/Raw/Templates and
/// exposes the six built-in templates (3 blank + 3 predefined). Falls back to
/// C# seed templates when JSON parsing fails so the UI is never empty.
/// </summary>
public interface ITemplateService
{
    Task PreloadAsync();

    Task<IReadOnlyList<DiagramTemplate>> GetAllAsync();

    Task<DiagramTemplate?> GetByIdAsync(string id);
}

/// <summary>
/// Selection → property bridge: writes property edits to the live Syncfusion
/// diagram models and reads snapshots for the property panel.
/// </summary>
public interface IPropertyService
{
    void Apply(string elementId, PropertyModel property);

    IReadOnlyList<PropertyModel> Read(string elementId);

    /// <summary>Registers the live Syncfusion element collection for write-through.</summary>
    void Track(IEnumerable<object> nodes, IEnumerable<object> connectors);

    /// <summary>
    /// Registers a single element that was added to the diagram after the initial
    /// track (e.g. a shape dragged in from the symbol palette) so it can be
    /// inspected and edited from the property panel.
    /// </summary>
    void TrackAdded(object element);

    /// <summary>Removes an element that was deleted from the diagram.</summary>
    void Untrack(string elementId);

    /// <summary>
    /// Reads the drawing-area (page) properties. Used when nothing is selected,
    /// so the panel can offer page format, orientation and background instead of
    /// shape properties.
    /// </summary>
    IReadOnlyList<PropertyModel> ReadPage();
}

/// <summary>
/// Tracks the current selection (node ids + connector ids) and publishes changes.
/// </summary>
public interface ISelectionService
{
    IReadOnlyList<string> SelectedNodeIds { get; }

    IReadOnlyList<string> SelectedConnectorIds { get; }

    /// <summary>Human-readable summary, e.g. "2 shapes selected" or "Nothing selected".</summary>
    string SelectionSummary { get; }

    event EventHandler? SelectionChanged;

    void ApplySelection(IEnumerable<string> nodeIds, IEnumerable<string> connectorIds);

    void Clear();
}

/// <summary>
/// View-agnostic alert/confirm wrappers used by ViewModels.
/// </summary>
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message);

    Task<bool> ConfirmAsync(string title, string message);
}
