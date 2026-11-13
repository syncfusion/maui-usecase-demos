using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PatientCompanion.Services;

/// <summary>
/// Loads the bundled world-map JSON asset from the app package once at startup
/// and exposes it as a fresh <see cref="MemoryStream"/> per call.
///
/// Syncfusion's <c>MapShapeLayer.ShapesSource</c> setter validates
/// <c>stream.CanRead</c> on the assigned value and throws
/// <c>ArgumentException("Stream was not readable.")</c> when the stream is
/// not readable. <see cref="FileSystem.OpenAppPackageFileAsync(string)"/>
/// returns a platform stream wrapper whose <c>CanRead</c> is unreliable
/// (false on some Android wrappers, false once consumed on iOS asset
/// streams, and not always seekable). A <see cref="MemoryStream"/> is
/// always readable and seekable, so we materialise the asset bytes once
/// and hand callers a fresh copy.
/// </summary>
public class WorldMapAssetService
{
    public const string AssetName = "world-map.json";

    private readonly Task<byte[]?> loadTask;

    public WorldMapAssetService()
        : this(logger: null)
    {
    }

    public WorldMapAssetService(ILogger<WorldMapAssetService>? logger)
    {
        loadTask = LoadAsync(logger);
    }

    /// <summary>
    /// Returns a fresh, readable, seekable <see cref="MemoryStream"/>
    /// positioned at 0 containing the world-map JSON, or <c>null</c> if
    /// the asset is unavailable.
    /// </summary>
    public Stream? ShapesStream
    {
        get
        {
            if (!loadTask.IsCompletedSuccessfully)
                return null;

            var bytes = loadTask.Result;
            return bytes is null ? null : new MemoryStream(bytes, writable: false);
        }
    }

    /// <summary>
    /// Async accessor for callers that need to wait for the asset to
    /// finish loading. Returns a fresh <see cref="MemoryStream"/>.
    /// </summary>
    public async Task<Stream?> GetStreamAsync()
    {
        var bytes = await loadTask;
        return bytes is null ? null : new MemoryStream(bytes, writable: false);
    }

    private static async Task<byte[]?> LoadAsync(ILogger<WorldMapAssetService>? logger)
    {
        try
        {
            await using var source = await FileSystem.OpenAppPackageFileAsync(AssetName);
            using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer);
            return buffer.ToArray();
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Bundled map asset '{Asset}' could not be loaded; the location preview will be empty.",
                AssetName);
            return null;
        }
    }
}

