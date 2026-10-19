using Microsoft.Extensions.DependencyInjection;
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
        var services = Handler!.MauiContext!.Services;
        var shell = services.GetRequiredService<AppShell>();

        var window = new Window(shell)
        {
            Title = "Smart Movie Ticket Booking v2.5"
        };

        // Desktop-friendly default window size on Windows / Mac Catalyst.
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
        {
            window.Width = 420;
            window.Height = 900;
            window.MinimumWidth = 360;
            window.MinimumHeight = 720;
        }

        return window;
    }
}