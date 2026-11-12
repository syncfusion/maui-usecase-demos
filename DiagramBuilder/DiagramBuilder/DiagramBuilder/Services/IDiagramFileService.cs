using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

public interface IDiagramFileService
{
    Task<DiagramDocument?> PickDiagramAsync();

    Task<string> SaveDiagramAsync(DiagramDocument document, string fileName);

    Task<string> SaveExportAsync(string fileName, string extension, byte[] content);
}
