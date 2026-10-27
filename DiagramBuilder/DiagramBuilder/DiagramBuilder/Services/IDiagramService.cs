using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// In-memory document store: holds the current <see cref="DiagramDocument"/>,
/// supports save/save-as and a mock recent list.
/// </summary>
public interface IDiagramService
{
    /// <summary>Current document, or null before the first create.</summary>
    DiagramDocument? CurrentDocument { get; }

    /// <summary>Raised with a status message (e.g. after save).</summary>
    event EventHandler<string>? StatusChanged;

    Task<DiagramDocument> CreateDocumentAsync(DiagramTemplate template);

    Task SaveAsync();

    Task SaveAsAsync(string title);

    Task<IReadOnlyList<string>> GetRecentDocumentsAsync();

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
/// Export pipeline (mock): simulates processing and reports success.
/// </summary>
public interface IExportService
{
    Task<ExportResult> ExportAsync(ExportFormat format, string region, string fileName);
}

/// <summary>
/// Result of a mock export operation.
/// </summary>
public record ExportResult(bool Success, string FileName, string? DataUri)
{
    /// <summary>True for formats whose data URI is not produced (PDF is mock-only).</summary>
    public bool IsMockOnly => DataUri is null;
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
