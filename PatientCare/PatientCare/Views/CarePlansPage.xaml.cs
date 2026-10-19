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
        var carePlanDataService = services?.GetService<CarePlanDataService>();
        var patientDataService = services?.GetService<PatientDataService>();
        var notificationService = services?.GetService<NotificationService>();
        
        if (carePlanDataService != null && patientDataService != null && notificationService != null)
        {
            viewModel = new CarePlansViewModel(carePlanDataService, patientDataService);
            DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
            MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        }
        else
        {
            // Fallback for direct instantiation
            patientDataService = new PatientDataService();
            carePlanDataService = new CarePlanDataService();
            notificationService = new NotificationService();
            viewModel = new CarePlansViewModel(carePlanDataService, patientDataService);
            DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
            MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        }

        BindingContext = viewModel;
    }

    public CarePlansPage(CarePlanDataService carePlanDataService, PatientDataService patientDataService, NotificationService notificationService)
    {
        InitializeComponent();
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
