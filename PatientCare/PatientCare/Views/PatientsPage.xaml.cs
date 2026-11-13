using PatientCare.Models;
using PatientCare.Services;
using PatientCare.ViewModels;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Core;

namespace PatientCare.Views;

public partial class PatientsPage : ContentPage
{
    private readonly PatientsViewModel viewModel;

    // Shell resolves DataTemplate pages through the app's DI container, so constructor injection is the only path.
    public PatientsPage(PatientDataService patientDataService, ScheduleDataService scheduleDataService, NotificationService notificationService)
    {
        InitializeComponent();
        viewModel = new PatientsViewModel(patientDataService, scheduleDataService);
        BindingContext = viewModel;
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();

        foreach (var chip in MobileFilterChips.Items)
        {
            MobileFilterChips.SelectedItem = chip;
            break;
        }
        SetDesktopFilterTab("All Patients");
    }

    private void OnPatientCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Patient patient)
        {
            viewModel.OpenPatientCommand.Execute(patient);
        }
    }

    private async void OnPatientCarePlanTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (e.Parameter is Patient patient)
            {
                await Shell.Current.GoToAsync($"//careplans?patientId={patient.PatientId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private void OnCloseProfileTapped(object? sender, TappedEventArgs e)
    {
        CloseProfile();
    }

    private void OnCloseProfileClicked(object? sender, EventArgs e)
    {
        CloseProfile();
    }

    private void CloseProfile()
    {
        try
        {
            viewModel.CloseProfileCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Close profile error: {ex.Message}");
        }
    }

    private void OnFilterSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (e.AddedItem is SfChip chip)
        {
            viewModel.SelectedFilter = chip.Text;
        }
    }

    private void OnDesktopFilterTabClicked(object? sender, EventArgs e)
    {
        if (sender is SfButton button && button.CommandParameter is string filter)
        {
            viewModel.SelectedFilter = filter;
            SetDesktopFilterTab(filter);
        }
    }

    private void SetDesktopFilterTab(string filter)
    {
        var activeBackground = Color.FromArgb("#087F73");
        var inactiveBackground = Colors.Transparent;
        var activeText = Colors.White;
        var inactiveText = Color.FromArgb("#14213D");

        foreach (var tab in new[] { DesktopAllPatientsTab, DesktopHighPriorityTab, DesktopFollowUpTab })
        {
            var isActive = (string)tab.CommandParameter == filter;
            tab.Background = isActive ? activeBackground : inactiveBackground;
            tab.Stroke = isActive ? activeBackground : Color.FromArgb("#DCE4F2");
            tab.TextColor = isActive ? activeText : inactiveText;
        }
    }

    private async void OnNotificationsTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//dashboard?showNotifications=true");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }
}
