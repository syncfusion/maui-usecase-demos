using System.Text.Json;
using System.Text.Json.Serialization;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

public sealed class DiagramFileService : IDiagramFileService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<DiagramDocument?> PickDiagramAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = AppStrings.OpenDialogTitle,
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".json"],
                [DevicePlatform.MacCatalyst] = ["public.json"],
                [DevicePlatform.iOS] = ["public.json"],
                [DevicePlatform.Android] = ["application/json"],
            }),
        });

        if (result is null)
        {
            return null;
        }

        if (!string.Equals(Path.GetExtension(result.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(AppStrings.OpenJsonOnlyMessage);
        }

        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        using var jsonDocument = JsonDocument.Parse(json);
        var root = jsonDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object
            || !HasArray(root, "Items")
            || !HasArray(root, "Connectors")
            || !TryGetProperty(root, "Category", out _))
        {
            throw new InvalidDataException(AppStrings.OpenInvalidDiagramMessage);
        }

        var document = JsonSerializer.Deserialize<DiagramDocument>(json, JsonOptions);
        if (document is null
            || !Enum.IsDefined(document.Category)
            || document.Items is null
            || document.Connectors is null
            || document.Items.Any(item => item is null || !Enum.IsDefined(item.ShapeType))
            || document.Connectors.Any(item => item is null || !Enum.IsDefined(item.ConnectorKind)))
        {
            throw new InvalidDataException(AppStrings.OpenInvalidDiagramMessage);
        }

        document.Title = Path.GetFileNameWithoutExtension(result.FileName);
        document.Id = string.IsNullOrWhiteSpace(document.Id)
            ? "doc-" + Guid.NewGuid().ToString("N")
            : document.Id;
        document.IsDirty = false;
        return document;
    }

    public Task<string> SaveDiagramAsync(DiagramDocument document, string fileName)
    {
        ArgumentNullException.ThrowIfNull(document);

        var path = string.IsNullOrWhiteSpace(document.SavedFilePath)
            ? GetAvailableDiagramPath(fileName)
            : document.SavedFilePath;
        return WriteDiagramAsync(document, path);
    }

    public Task<string> SaveDiagramAsAsync(DiagramDocument document, string fileName)
    {
        ArgumentNullException.ThrowIfNull(document);

        return WriteDiagramAsync(document, GetAvailableDiagramPath(fileName));
    }

    private static async Task<string> WriteDiagramAsync(DiagramDocument document, string path)
    {
        var title = Path.GetFileNameWithoutExtension(path);
        var previousTitle = document.Title;
        var wasDirty = document.IsDirty;
        var saved = false;

        try
        {
            document.Title = title;
            document.IsDirty = false;
            var content = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
            await WriteAsync(path, content);
            document.SavedFilePath = path;
            saved = true;
            return path;
        }
        finally
        {
            if (!saved)
            {
                document.Title = previousTitle;
                document.IsDirty = wasDirty;
            }
        }
    }

    public Task<string> SaveExportAsync(string fileName, string extension, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var path = GetOutputPath(fileName, extension);
        return WriteAsync(path, content);
    }

    private static string GetOutputPath(string fileName, string extension)
    {
        if (extension is not ("json" or "png" or "jpg" or "pdf" or "svg"))
        {
            throw new ArgumentException(AppStrings.FileNameInvalidMessage, nameof(extension));
        }

        var trimmedName = fileName.Trim();
        var safeName = Path.GetFileNameWithoutExtension(trimmedName);
        if (string.IsNullOrWhiteSpace(safeName)
            || !string.Equals(Path.GetFileName(trimmedName), trimmedName, StringComparison.Ordinal)
            || safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || trimmedName.Contains('/') || trimmedName.Contains('\\'))
        {
            throw new ArgumentException(AppStrings.FileNameInvalidMessage, nameof(fileName));
        }

        var directory = GetDownloadsDirectory();
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{safeName}.{extension}");
    }

    private static string GetAvailableDiagramPath(string fileName)
    {
        var requestedPath = GetOutputPath(fileName, "json");
        if (!File.Exists(requestedPath))
        {
            return requestedPath;
        }

        var directory = Path.GetDirectoryName(requestedPath)!;
        var baseName = Path.GetFileNameWithoutExtension(requestedPath);
        for (var index = 1; ; index++)
        {
            var candidate = Path.Combine(directory, $"{baseName} {index}.json");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }

    private static bool HasArray(JsonElement element, string propertyName) =>
        TryGetProperty(element, propertyName, out var property)
        && property.ValueKind == JsonValueKind.Array;

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }

    private static string GetDownloadsDirectory()
    {
#if WINDOWS
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
        {
            return Path.Combine(userProfile, "Downloads");
        }
#endif
        return Path.Combine(FileSystem.AppDataDirectory, "Downloads");
    }

    private static async Task<string> WriteAsync(string path, byte[] content)
    {
        await File.WriteAllBytesAsync(path, content);
        return path;
    }
}
