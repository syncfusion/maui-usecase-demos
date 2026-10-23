using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HotelManagerSample.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default
/// Application class. On Windows the WinUI host must subclass
/// <see cref="MauiWinUIApplication"/> so MAUI can take over after the
/// native bootstrapper finishes.
/// </summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>Builds the shared MAUI app produced by
    /// <see cref="MauiProgram.CreateMauiApp"/>.</summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /// <summary>Runs after the WinUI host has launched. Applies the
    /// desktop title-bar customisation so the MAUI window no longer
    /// shows the OS chrome strip on Windows.</summary>
    /// <param name="args">Activation arguments forwarded by WinUI.
    /// Unused — present for the base contract.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        // Subscribe to WindowCreated BEFORE the first MAUI window is
        // created. TitleBarController.Apply() then collapses the OS
        // title bar (ExtendsContentIntoTitleBar = true) and recolors
        // the chrome in #F6F8FD so the empty strip disappears.
        TitleBarController.Apply();
    }
}
