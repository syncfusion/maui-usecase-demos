using CommunityToolkit.Mvvm.ComponentModel;

namespace VisitorPass.Services;

/// <summary>
/// VP-080 — Theme toggle singleton.
///
/// Wraps MAUI's <see cref="Application.Current"/> request-user-theme
/// API in an observable view-model-friendly service so pages can
/// surface a Light/Dark toggle affordance. The actual token swap
/// happens automatically through AppThemeBinding on the brush
/// entries in <c>Colors.xaml</c>: SurfaceBrush / SurfaceAltBrush /
/// BorderBrush / TextPrimaryBrush / TextSecondaryBrush / TextMutedBrush
/// flip without code when the OS theme changes or the user taps the
/// toggle.
///
/// Registered as a singleton in <c>MauiProgram.cs</c> so the toggle
/// state survives page navigations and the same instance backs
/// every surface that exposes a theme switch.
/// </summary>
public partial class ThemeService : ObservableObject
{
    /// <summary>Tri-state; <c>Unspecified</c> follows the OS theme.</summary>
    [ObservableProperty]
    private AppTheme _currentTheme = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;

    /// <summary>Human-readable summary (used by the toggle button label).</summary>
    public string ThemeLabel => CurrentTheme switch
    {
        AppTheme.Light => "Light",
        AppTheme.Dark => "Dark",
        _ => "System",
    };

    /// <summary>True when the app is currently rendering in dark mode.</summary>
    public bool IsDarkMode =>
        (Application.Current?.RequestedTheme ?? Application.Current?.PlatformAppTheme) == AppTheme.Dark;

    /// <summary>Apply a new theme + persist via MAUI's user-theme API.</summary>
    public void SetTheme(AppTheme theme)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = theme;
        CurrentTheme = theme;
        OnPropertyChanged(nameof(IsDarkMode));
        OnPropertyChanged(nameof(ThemeLabel));
    }

    /// <summary>Flip between Light and Dark (no-op when currently System).</summary>
    public void Toggle()
    {
        var next = IsDarkMode ? AppTheme.Light : AppTheme.Dark;
        SetTheme(next);
    }
}
