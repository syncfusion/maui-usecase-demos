using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// Drawing-area (page) settings shown by the property panel when no shape is
/// selected. Holds the canvas background, the paper format and the orientation
/// and notifies when the panel edits them so the host can push them onto the
/// live diagram.
/// </summary>
public interface IPageSettingsService
{
    /// <summary>The selected paper format.</summary>
    PaperFormat Format { get; }

    /// <summary>True for portrait, false for landscape.</summary>
    bool IsPortrait { get; }

    /// <summary>Theme key of the canvas background colour.</summary>
    string BackgroundColorKey { get; }

    /// <summary>True when page breaks are shown.</summary>
    bool ShowPageBreaks { get; }

    /// <summary>Raised whenever any page setting changes.</summary>
    event EventHandler? Changed;

    void SetFormat(PaperFormat format);

    void SetOrientation(bool isPortrait);

    void SetBackgroundColor(string colorKey);

    void SetShowPageBreaks(bool show);
}

/// <summary>
/// Default <see cref="IPageSettingsService"/>: an in-memory record of the
/// drawing-area settings, seeded from the theme.
/// </summary>
public class PageSettingsService : IPageSettingsService
{
    private PaperFormat _format = PaperFormat.Default;
    private bool _isPortrait = true;
    private string _backgroundColorKey = Constants.ResourceKeys.CanvasBackground;
    private bool _showPageBreaks;

    public PaperFormat Format => _format;

    public bool IsPortrait => _isPortrait;

    public string BackgroundColorKey => _backgroundColorKey;

    public bool ShowPageBreaks => _showPageBreaks;

    public event EventHandler? Changed;

    public void SetFormat(PaperFormat format)
    {
        if (_format == format)
        {
            return;
        }

        _format = format;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetOrientation(bool isPortrait)
    {
        if (_isPortrait == isPortrait)
        {
            return;
        }

        _isPortrait = isPortrait;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetBackgroundColor(string colorKey)
    {
        if (string.Equals(_backgroundColorKey, colorKey, StringComparison.Ordinal))
        {
            return;
        }

        _backgroundColorKey = colorKey;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetShowPageBreaks(bool show)
    {
        if (_showPageBreaks == show)
        {
            return;
        }

        _showPageBreaks = show;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
