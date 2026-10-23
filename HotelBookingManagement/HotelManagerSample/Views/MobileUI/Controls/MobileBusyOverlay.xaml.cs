namespace HotelManagerSample.Controls;

/// <summary>
/// Wraps SfBusyIndicator inside a centred rounded card. Pages bind
/// <see cref="IsBusy"/> in code-behind and supply a friendly caption via
/// <see cref="BusyMessage"/>.
/// </summary>
public partial class MobileBusyOverlay : ContentView
{
    /// <summary>Builds the overlay and inflates the XAML-inflated tree.</summary>
    public MobileBusyOverlay()
    {
        InitializeComponent();
    }

    /// <summary>
    /// True while async work is happening. Hides/shows the overlay and
    /// toggles <see cref="VisualElement.InputTransparent"/> so the pages
    /// underneath can't capture tap-throughs while the modal is alive.
    /// </summary>
    public bool IsBusy
    {
        get => IsVisible;
        set
        {
            IsVisible = value;
            InputTransparent = !value;
        }
    }

    /// <summary>Caption shown under the spinner. Falls back to "Loading…".</summary>
    public string BusyMessage
    {
        get => BusyMessageLabel.Text;
        set => BusyMessageLabel.Text = string.IsNullOrWhiteSpace(value) ? "Loading…" : value!;
    }
}