namespace PatientCompanion.Controls;

public partial class BottomNavigationView : ContentView
{
    private static readonly Color SelectedColor = Color.FromArgb("#00685F");
    private static readonly Color UnselectedColor = Color.FromArgb("#64748B");

    private EventHandler<ShellNavigatedEventArgs>? shellNavigatedHandler;

    public BottomNavigationView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (shellNavigatedHandler is null && Shell.Current is not null)
        {
            shellNavigatedHandler = OnShellNavigated;
            Shell.Current.Navigated += shellNavigatedHandler;
        }

        UpdateSelectedTab();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        if (shellNavigatedHandler is not null && Shell.Current is not null)
        {
            Shell.Current.Navigated -= shellNavigatedHandler;
            shellNavigatedHandler = null;
        }
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        UpdateSelectedTab();
    }

    private void UpdateSelectedTab()
    {
        var route = Shell.Current?.CurrentState?.Location?.OriginalString ?? string.Empty;

        SetTabSelected(route.Contains("HomePage", StringComparison.OrdinalIgnoreCase), HomeIcon, HomeText);
        SetTabSelected(route.Contains("VisitsPage", StringComparison.OrdinalIgnoreCase), VisitsIcon, VisitsText);
        SetTabSelected(route.Contains("HealthPage", StringComparison.OrdinalIgnoreCase), HealthIcon, HealthText);
        SetTabSelected(route.Contains("MedicationsPage", StringComparison.OrdinalIgnoreCase), MedicationsIcon, MedicationsText);
        SetTabSelected(route.Contains("RecordsPage", StringComparison.OrdinalIgnoreCase), HelpIcon, HelpText);
    }

    private static void SetTabSelected(bool isSelected, Label icon, Label text)
    {
        var color = isSelected ? SelectedColor : UnselectedColor;
        icon.TextColor = color;
        text.TextColor = color;
    }

    private static Task NavigateAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    private void HomeTapped(object? sender, TappedEventArgs e)
    {
        _ = NavigateAsync("//MainTabs/HomePage");
    }

    private void VisitsTapped(object? sender, TappedEventArgs e)
    {
        _ = NavigateAsync("//MainTabs/VisitsPage");
    }

    private void HealthTapped(object? sender, TappedEventArgs e)
    {
        _ = NavigateAsync("//MainTabs/HealthPage");
    }

    private void MedicationsTapped(object? sender, TappedEventArgs e)
    {
        _ = NavigateAsync("//MainTabs/MedicationsPage");
    }

    private void HelpTapped(object? sender, TappedEventArgs e)
    {
        _ = NavigateAsync("//MainTabs/RecordsPage");
    }
}
