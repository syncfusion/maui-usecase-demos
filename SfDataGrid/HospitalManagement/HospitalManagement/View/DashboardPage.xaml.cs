using HospitalManagement.ViewModels;
using Microsoft.Maui.Controls.Xaml;

namespace HospitalManagement.View;

public partial class DashboardPage : ContentPage
{
    private AppTheme _currentTheme = AppTheme.Light;

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
        _currentTheme = Application.Current?.UserAppTheme ?? AppTheme.Light;

        Loaded += (_, _) => UpdateThemeButtonStyles();
    }

    private void OnSettingsTapped(object sender, TappedEventArgs e)
    {
        UpdateThemeButtonStyles();
        GetSettingsOverlay().IsVisible = true;
    }

    private void OnSettingsOverlayTapped(object sender, TappedEventArgs e)
    {
        GetSettingsOverlay().IsVisible = false;
    }

    private void OnCloseSettingsTapped(object sender, TappedEventArgs e)
    {
        GetSettingsOverlay().IsVisible = false;
    }

    private void OnLightThemeClicked(object sender, EventArgs e)
    {
        ApplyTheme(AppTheme.Light);
    }

    private void OnDarkThemeClicked(object sender, EventArgs e)
    {
        ApplyTheme(AppTheme.Dark);
    }

    private void ApplyTheme(AppTheme theme)
    {
        _currentTheme = theme;
        (Application.Current as App)?.SetAppTheme(theme);
        UpdateThemeButtonStyles();
        GetSettingsOverlay().IsVisible = false;
    }

    private void UpdateThemeButtonStyles()
    {
        var unselectedTextColor = _currentTheme == AppTheme.Dark ? Colors.White : Colors.Black;
        var unselectedBorderColor = _currentTheme == AppTheme.Dark ? Color.FromArgb("#606060") : Color.FromArgb("#CCCCCC");

        var btnLightTheme = GetBtnLightTheme();
        var btnDarkTheme = GetBtnDarkTheme();

        if (btnLightTheme != null)
        {
            if (_currentTheme == AppTheme.Light)
            {
                btnLightTheme.BackgroundColor = Color.FromArgb("#6750A4");
                btnLightTheme.TextColor = Colors.White;
                btnLightTheme.BorderWidth = 0;
                btnLightTheme.BorderColor = Colors.Transparent;
            }
            else
            {
                btnLightTheme.BackgroundColor = Colors.Transparent;
                btnLightTheme.TextColor = unselectedTextColor;
                btnLightTheme.BorderWidth = 1;
                btnLightTheme.BorderColor = unselectedBorderColor;
            }
        }

        if (btnDarkTheme != null)
        {
            if (_currentTheme == AppTheme.Dark)
            {
                btnDarkTheme.BackgroundColor = Color.FromArgb("#6750A4");
                btnDarkTheme.TextColor = Colors.White;
                btnDarkTheme.BorderWidth = 0;
                btnDarkTheme.BorderColor = Colors.Transparent;
            }
            else
            {
                btnDarkTheme.BackgroundColor = Colors.Transparent;
                btnDarkTheme.TextColor = unselectedTextColor;
                btnDarkTheme.BorderWidth = 1;
                btnDarkTheme.BorderColor = unselectedBorderColor;
            }
        }
    }

    private Grid GetSettingsOverlay() => this.FindByName<Grid>("SettingsOverlay");

    private Button GetBtnLightTheme() => this.FindByName<Button>("BtnLightTheme");

    private Button GetBtnDarkTheme() => this.FindByName<Button>("BtnDarkTheme");
}
