using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;

namespace Todo.Features.Settings.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    public IReadOnlyList<string> ThemeOptions { get; } =
        new[] { "System Default", "Light", "Dark" };

    [ObservableProperty]
    private string selectedTheme = "System Default";

    public SettingsViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        SelectedTheme = Preferences.Default.Get("AppTheme", "System Default");
        ApplyTheme(SelectedTheme);
    }

    partial void OnSelectedThemeChanged(string value)
    {
        Preferences.Default.Set("AppTheme", value);
        ApplyTheme(value);
    }

    [RelayCommand]
    private Task CloseAsync() => _navigation.PopModalAsync();

    private static void ApplyTheme(string theme)
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = theme switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}