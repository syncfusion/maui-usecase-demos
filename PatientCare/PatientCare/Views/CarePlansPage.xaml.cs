using PatientCare.Services;
using PatientCare.ViewModels;

namespace PatientCare.Views;

public partial class CarePlansPage : ContentPage
{
    private readonly CarePlansViewModel viewModel;

    // Shell resolves DataTemplate pages through the app's DI container, so constructor injection is the only path.
    public CarePlansPage(CarePlanDataService carePlanDataService, PatientDataService patientDataService, NotificationService notificationService)
    {
        InitializeComponent();
        viewModel = new CarePlansViewModel(carePlanDataService, patientDataService);
        BindingContext = viewModel;
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
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
