namespace SmartMovieBooking.WinUI;

/// <summary>
/// Windows-specific MAUI application bootstrap.
/// On Windows, MAUI uses <see cref="MauiWinUIApplication"/> as the
/// entry point. This class is instantiated by the WinUI runtime and
/// returns the shared <see cref="MauiProgram.CreateMauiApp"/> builder.
/// </summary>
public partial class App : MauiWinUIApplication
{
    public App()
    {
        // InitializeComponent is generated from Platforms/Windows/App.xaml
        // by the WinUI XAML source generator.
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}