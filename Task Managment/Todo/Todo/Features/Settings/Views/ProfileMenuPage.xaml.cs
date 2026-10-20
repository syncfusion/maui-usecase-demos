using Todo.Common;
using Todo.Features.Settings.ViewModels;

namespace Todo.Features.Settings.Views;

public partial class ProfileMenuPage : ContentPage
{
    private readonly INavigationService _navigation;

    public ProfileMenuPage(INavigationService navigation)
    {
        InitializeComponent();
        _navigation = navigation;
    }

    private async void OnManageAccountsClicked(object sender, EventArgs e)
        => await DisplayAlert("Manage Accounts", "No additional accounts are connected.", "OK");

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await _navigation.PopModalAsync();
        await _navigation.PushModalAsync(new SettingsPage(new SettingsViewModel(_navigation)));
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        var confirmed = await DisplayAlert("Sign out?", "Are you sure you want to sign out?", "Sign Out", "Cancel");
        if (confirmed)
            await _navigation.PopModalAsync();
    }

    private async void OnOutsideTapped(object? sender, TappedEventArgs e)
        => await _navigation.PopModalAsync();
}