using Syncfusion.Maui.Popup;

namespace SmartMovieBooking.Controls;

/// <summary>
/// Wraps a Syncfusion <see cref="SfPopup"/> and exposes a typed Open/Close API.
/// </summary>
public sealed class PopupShell
{
    private readonly SfPopup _popup;

    public PopupShell(string headerTitle = "")
    {
        _popup = new SfPopup
        {
            HeaderTitle = headerTitle,
            ShowHeader = false,
            ShowCloseButton = false,
            IsOpen = false,
            AutoSizeMode = PopupAutoSizeMode.None,
            PopupStyle = new PopupStyle
            {
                PopupBackground = Colors.Transparent,
                PopupStroke = Colors.Transparent,
                OverlayOpacity = 0.45,
            },
        };
    }

    public SfPopup Popup => _popup;
    public bool IsOpen => _popup.IsOpen;

    public void SetContent(View content)
        => _popup.Content = content;

    public void Show()
        => _popup.IsOpen = true;

    public void Close()
        => _popup.IsOpen = false;
}