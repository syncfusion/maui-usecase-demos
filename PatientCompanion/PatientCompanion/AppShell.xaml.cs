using PatientCompanion.Views;

namespace PatientCompanion;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Custom BottomNavigationView is rendered inside AppPageLayout on every
        // main page; the Shell's built-in tab bar would double-render and steal
        // hit-testing on the custom nav.
        Shell.SetTabBarIsVisible(this, false);

        Routing.RegisterRoute(nameof(AppointmentPage), typeof(AppointmentPage));
    }
}
