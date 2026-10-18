using SmartMovieBooking.Services;

namespace SmartMovieBooking;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Initialize theme tokens in app resources so XAML {DynamicResource} works.
        ThemeService.Instance.Initialize();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new MainWindow();
        window.Title = "Smart Movie Ticket Booking v2.5 (x64)";

        // Desktop-friendly default window size
        window.Width = 1440;
        window.Height = 900;
        window.MinimumWidth = 1024;
        window.MinimumHeight = 720;

        return window;
    }
}