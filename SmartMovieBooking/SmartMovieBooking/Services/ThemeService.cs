using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Themes;

namespace SmartMovieBooking.Services;

/// <summary>
/// Manages the app-wide theme tokens (light/dark).
/// All <c>{DynamicResource Theme*}</c> lookups in XAML resolve through
/// the keys defined in <c>Resources/Styles/Theme.xaml</c>; this service
/// rewrites those values at runtime to switch themes.
/// </summary>
public sealed class ThemeService : INotifyPropertyChanged
{
    private static readonly Lazy<ThemeService> _lazy = new(() => new ThemeService());
    public static ThemeService Instance => _lazy.Value;

    private bool _isDarkTheme;
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (_isDarkTheme == value) return;
            _isDarkTheme = value;
            OnPropertyChanged();
            ApplyTheme();
        }
    }

    private ThemeService() { }

    /// <summary>Pushes the current token set into <see cref="Application.Resources"/>.</summary>
    public void Initialize()
    {
        ApplyTheme();
    }

    public void Toggle() => IsDarkTheme = !IsDarkTheme;

    private void ApplyTheme()
    {
        if (Application.Current is null) return;

        // MAUI's UserAppTheme drives the default resource resolution.
        Application.Current.UserAppTheme = _isDarkTheme ? AppTheme.Dark : AppTheme.Light;

        // Manually rewrite the Theme* keys so DynamicResource lookups update.
        var (suffix, _) = _isDarkTheme ? ("Dark", "Light") : ("Light", "Dark");
        Swap("ThemePrimary", $"Primary{suffix}");
        Swap("ThemePrimaryContainer", $"PrimaryContainer{suffix}");
        Swap("ThemeOnPrimary", $"OnPrimary{suffix}");
        Swap("ThemeOnPrimaryContainer", $"OnPrimaryContainer{suffix}");

        Swap("ThemeSecondary", $"Secondary{suffix}");
        Swap("ThemeSecondaryContainer", $"SecondaryContainer{suffix}");
        Swap("ThemeOnSecondary", $"OnSecondary{suffix}");
        Swap("ThemeOnSecondaryContainer", $"OnSecondaryContainer{suffix}");

        Swap("ThemeTertiary", $"Tertiary{suffix}");
        Swap("ThemeTertiaryContainer", $"TertiaryContainer{suffix}");
        Swap("ThemeOnTertiary", $"OnTertiary{suffix}");
        Swap("ThemeOnTertiaryContainer", $"OnTertiaryContainer{suffix}");

        Swap("ThemeError", $"Error{suffix}");
        Swap("ThemeOnError", $"OnError{suffix}");

        Swap("ThemeBackground", $"Background{suffix}");
        Swap("ThemeOnBackground", $"OnBackground{suffix}");

        Swap("ThemeSurface", $"Surface{suffix}");
        Swap("ThemeOnSurface", $"OnSurface{suffix}");
        Swap("ThemeOnSurfaceVariant", $"OnSurfaceVariant{suffix}");

        Swap("ThemeSurfaceContainerLowest", $"SurfaceContainerLowest{suffix}");
        Swap("ThemeSurfaceContainerLow", $"SurfaceContainerLow{suffix}");
        Swap("ThemeSurfaceContainer", $"SurfaceContainer{suffix}");
        Swap("ThemeSurfaceContainerHigh", $"SurfaceContainerHigh{suffix}");
        Swap("ThemeSurfaceContainerHighest", $"SurfaceContainerHighest{suffix}");

        Swap("ThemeOutline", $"Outline{suffix}");
        Swap("ThemeOutlineVariant", $"OutlineVariant{suffix}");

        Swap("ThemeInverseSurface", $"InverseSurface{suffix}");
        Swap("ThemeInverseOnSurface", $"InverseOnSurface{suffix}");

        Swap("ThemeSecondaryFixed", $"SecondaryFixed{suffix}");
        Swap("ThemeSecondaryFixedDim", $"SecondaryFixedDim{suffix}");

        Swap("ThemeSuccess", $"Success{suffix}");
        Swap("ThemeAmber", $"Amber{suffix}");

        // Syncfusion theme
        try
        {
            var merged = Application.Current.Resources.MergedDictionaries;
            var theme = merged?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
            if (theme is not null)
            {
                theme.VisualTheme = _isDarkTheme ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;
            }
        }
        catch
        {
            // ignore: theme dictionary may be missing in some test scenarios
        }
    }

    private static void Swap(string aliasKey, string sourceKey)
    {
        if (Application.Current is null) return;
        var resources = Application.Current.Resources;

        if (!resources.TryGetValue(sourceKey, out var value)) return;

        if (resources.ContainsKey(aliasKey))
            resources[aliasKey] = value;
        else
            resources.Add(aliasKey, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}