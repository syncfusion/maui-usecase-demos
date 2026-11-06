using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// Drawing-area (page) settings shown by the property panel when no shape is
/// selected. Holds only the canvas background — the one drawing-area property
/// the MAUI Diagram control genuinely supports at runtime — and notifies when
/// the panel edits it so the host can repaint the canvas.
///
/// Paper format and orientation were removed: the Syncfusion.Maui.Diagram
/// assembly exposes no page-settings API on SfDiagram (page settings exist only
/// on <c>DiagramExportOptions</c>, consumed by ExportDiagram/Print), so there
/// is nothing for a live panel edit to act on.
/// </summary>
public interface IPageSettingsService
{
    /// <summary>Theme key of the canvas background colour.</summary>
    string BackgroundColorKey { get; }

    /// <summary>Raised whenever any page setting changes.</summary>
    event EventHandler? Changed;

    void SetBackgroundColor(string colorKey);
}

/// <summary>
/// Default <see cref="IPageSettingsService"/>: an in-memory record of the
/// drawing-area background, seeded from the theme.
/// </summary>
public class PageSettingsService : IPageSettingsService
{
    private string _backgroundColorKey = Constants.ResourceKeys.CanvasBackground;

    public string BackgroundColorKey => _backgroundColorKey;

    public event EventHandler? Changed;

    public void SetBackgroundColor(string colorKey)
    {
        if (string.Equals(_backgroundColorKey, colorKey, StringComparison.Ordinal))
        {
            return;
        }

        _backgroundColorKey = colorKey;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}