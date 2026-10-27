using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// Mock <see cref="IExportService"/>: simulates processing with a small delay and
/// returns a success record. Data URIs are null for PDF (mock-only format).
/// </summary>
public class MockExportService : IExportService
{
    private static readonly Dictionary<ExportFormat, string> Extensions = new()
    {
        [ExportFormat.Png] = "png",
        [ExportFormat.Jpeg] = "jpg",
        [ExportFormat.Pdf] = "pdf",
        [ExportFormat.Svg] = "svg",
    };

    public async Task<ExportResult> ExportAsync(ExportFormat format, string region, string fileName)
    {
        // Simulated processing.
        await Task.Delay(500);

        var safeName = string.IsNullOrWhiteSpace(fileName)
            ? "diagram"
            : fileName;

        // PDF is mock-only: no data URI is produced.
        var dataUri = format == ExportFormat.Pdf
            ? null
            : $"data:image/{Extensions[format]};base64,{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("mock-export"))}";

        return new ExportResult(true, $"{safeName}.{Extensions[format]}", dataUri);
    }
}
