using PatientCare.Models;
using PatientCare.Services;
using PatientCare.ViewModels;

namespace PatientCare.Views;

public partial class CarePlansPage : ContentPage
{
    private readonly CarePlansViewModel viewModel;

    public CarePlansPage()
    {
        InitializeComponent();
        var services = Application.Current?.Handler?.MauiContext?.Services;
        var patientDataService = services?.GetService<PatientDataService>() ?? new PatientDataService();
        var carePlanDataService = services?.GetService<CarePlanDataService>() ?? new CarePlanDataService();
        var notificationService = services?.GetService<NotificationService>() ?? new NotificationService();
        viewModel = new CarePlansViewModel(carePlanDataService, patientDataService);
        BindingContext = viewModel;
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
    }

    private void OnPatientChipTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Patient patient)
        {
            viewModel.SelectPatientCommand.Execute(patient);
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