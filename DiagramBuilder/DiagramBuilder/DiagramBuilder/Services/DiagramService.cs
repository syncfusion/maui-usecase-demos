using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// Mock in-memory <see cref="IDiagramService"/> implementation.
/// </summary>
public class DiagramService : IDiagramService
{
    private readonly ITemplateService _templateService;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public DiagramService(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    public DiagramDocument? CurrentDocument { get; private set; }

    public event EventHandler<string>? StatusChanged;

    public async Task<DiagramDocument> CreateDocumentAsync(DiagramTemplate template)
    {
        await _gate.WaitAsync();
        try
        {
            var doc = new DiagramDocument
            {
                Id = "doc-" + Guid.NewGuid().ToString("N"),
                Title = template.IsBlank ? AppStrings.UntitledDocument : template.Title,
                Category = template.Category,
                IsDirty = false,
            };

            foreach (var node in template.Nodes)
            {
                doc.Items.Add(new ShapeItem
                {
                    Id = node.Id,
                    Text = node.Text,
                    ShapeType = node.ShapeType,
                    PositionX = node.X,
                    PositionY = node.Y,
                    Width = node.Width,
                    Height = node.Height,
                    FillColorKey = node.FillColorKey,
                });
            }

            foreach (var connector in template.Connectors)
            {
                doc.Connectors.Add(new ConnectorItem
                {
                    Id = connector.Id,
                    Text = connector.Text,
                    SourceId = connector.SourceId,
                    TargetId = connector.TargetId,
                    ConnectorKind = connector.Kind,
                    StrokeColorKey = connector.StrokeColorKey,
                });
            }

            CurrentDocument = doc;
            return doc;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task SaveAsync()
    {
        if (CurrentDocument is null)
        {
            return Task.CompletedTask;
        }

        CurrentDocument.IsDirty = false;
        RaiseStatus(string.Format(AppStrings.StatusSavedFormat, CurrentDocument.Title));
        return Task.CompletedTask;
    }

    public Task SaveAsAsync(string title)
    {
        if (CurrentDocument is null)
        {
            return Task.CompletedTask;
        }

        CurrentDocument.Title = title;
        CurrentDocument.IsDirty = false;
        RaiseStatus(string.Format(AppStrings.StatusSavedFormat, title));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetRecentDocumentsAsync() =>
        Task.FromResult(RecentDocumentCatalog.All);

    public void SetDirty()
    {
        if (CurrentDocument is not null)
        {
            CurrentDocument.IsDirty = true;
        }
    }

    private void RaiseStatus(string message) =>
        StatusChanged?.Invoke(this, message);
}
