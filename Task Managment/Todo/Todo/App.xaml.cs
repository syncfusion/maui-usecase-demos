namespace Todo;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = Preferences.Default.Get("AppTheme", "System Default") switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}