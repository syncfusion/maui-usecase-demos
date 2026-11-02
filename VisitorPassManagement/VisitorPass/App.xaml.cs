using Microsoft.Extensions.DependencyInjection;
using VisitorPass.Views;

namespace VisitorPass;

/// <summary>
/// Application root.
///
/// The app uses a plain <see cref="NavigationPage"/> rather than MAUI
/// Shell. Two reasons drive that choice:
///   1. Register Visitor lives inside an <c>SfPopup</c> on
///      desktop / tablet, and an in-page overlay on phone. Shell's
///      content-template model does not host that idiom-aware layout
///      cleanly.
///   2. Navigation events from within content pages
///      (<c>Navigation.PushAsync</c>) are the simplest mental model
///      for a sample reader — every page directly handles the
///      Register / Detail / Sidebar taps without Shell route plumbing.
///
/// Dashboard is resolved from DI so the singleton view-model +
/// <see cref="Services.ThemeService"/> are wired up via constructor
/// injection.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var services = Handler!.MauiContext!.Services;
        var dashboard = services.GetRequiredService<DashboardPage>();
        return new Window(new NavigationPage(dashboard)
        {
            BarBackgroundColor = Color.FromArgb("#F6F8FD"),
            BarTextColor = Color.FromArgb("#17243A"),
        });
    }
}
