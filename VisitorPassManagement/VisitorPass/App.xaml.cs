using VisitorPass.Views;

namespace VisitorPass;

/// <summary>
/// VP-UI nav-refactor — the app now uses a plain NavigationPage
/// instead of MAUI Shell. Shell's relative routing crashes when
/// invoked from inside content pages ("Relative routing to shell
/// elements is currently not supported"), and the user wants
/// register to live inside an SfPopup rather than a pushed
/// page — neither of which fit Shell's content-template model.
///
/// This class returns a NavigationPage hosting the Dashboard as
/// the root, so all subsequent navigation is plain
/// <c>Navigation.PushAsync</c> against the root.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
        // Resolve the singleton DashboardPage from DI so the
        // constructor-injected VM + ThemeService are wired up.
        //var services = Handler!.MauiContext!.Services;
        //var dashboard = services.GetRequiredService<DashboardPage>();
        //return new Window(new NavigationPage(dashboard)
        //{
        //    BarBackgroundColor = Color.FromArgb("#F6F8FD"),
        //    BarTextColor = Color.FromArgb("#17243A")
        //});
    }
}
