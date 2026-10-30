namespace VisitorPass.Controls;

/// <summary>
/// VP-UI sidebar - vertical nav rail injected into every page on
/// desktop / tablet idioms (hidden on phone via OnIdiom).
///
/// The control exposes a single <see cref="ActiveRoute"/> Bindable
/// property and a <see cref="NavItemClicked"/> event:
///   - ActiveRoute = "dashboard" -> Command Center tile highlighted
///     in primary colour, Visitor Directory in muted style.
///   - ActiveRoute = "directory" -> reverse.
///   - When the user taps either tile, the page's code-behind
///     receives NavItemClicked and issues Shell.GoToAsync.
///
/// Visual states are switched via DataTriggers in the XAML so the
/// code-behind stays free of visual-tree walking.
/// </summary>
public partial class SideNavigation : ContentView
{
    /// <summary>Currently highlighted route id. Two-way bindable.</summary>
    public static readonly BindableProperty ActiveRouteProperty = BindableProperty.Create(
        nameof(ActiveRoute),
        typeof(string),
        typeof(SideNavigation),
        defaultValue: "dashboard");

    /// <summary>Raised when the user taps a nav item; arg = route id.</summary>
    public event EventHandler<string>? NavItemClicked;

    /// <summary>Current active route id.</summary>
    public string ActiveRoute
    {
        get => (string)GetValue(ActiveRouteProperty);
        set => SetValue(ActiveRouteProperty, value);
    }

    public SideNavigation()
    {
        InitializeComponent();
    }

    private void OnCommandCenterTapped(object? sender, TappedEventArgs e)
        => NavItemClicked?.Invoke(this, "dashboard");

    private void OnDirectoryTapped(object? sender, TappedEventArgs e)
        => NavItemClicked?.Invoke(this, "directory");
}
