using PatientCare.Services;
using PatientCare.ViewModels;

namespace PatientCare.Views;

public partial class DashboardPage : ContentPage, IQueryAttributable
{
	private readonly DashboardViewModel _viewModel;

	// Shell resolves DataTemplate pages through the app's DI container, so constructor injection is the only path.
	public DashboardPage(DashboardDataService dataService)
	{
		InitializeComponent();
		_viewModel = new DashboardViewModel(dataService);
		BindingContext = _viewModel;
	}

	private void OnViewAllClicked(object? sender, EventArgs e)
	{
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
		{
			MobileAppointmentsSheet.IsVisible = true;
			return;
		}

		AppointmentsPopup.IsOpen = true;
	}

	private void OnNotificationsTapped(object? sender, TappedEventArgs e)
	{
		OpenNotifications();
	}

	private void OpenNotifications()
	{
		if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone || DeviceInfo.Current.Idiom == DeviceIdiom.Tablet)
		{
			MobileCareAlertsSheet.IsVisible = true;
			return;
		}

		NotificationsPopup.IsOpen = true;
	}

	private void OnCloseAppointmentsPopupTapped(object? sender, TappedEventArgs e)
	{
		AppointmentsPopup.IsOpen = false;
		MobileAppointmentsSheet.IsVisible = false;
	}

	private void OnCloseAppointmentsPopupClicked(object? sender, EventArgs e)
	{
		AppointmentsPopup.IsOpen = false;
		MobileAppointmentsSheet.IsVisible = false;
	}

	private void OnCloseNotificationsPopupClicked(object? sender, EventArgs e)
	{
		NotificationsPopup.IsOpen = false;
		MobileCareAlertsSheet.IsVisible = false;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("showNotifications", out var value)
			&& string.Equals(value?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
		{
			OpenNotifications();
		}
	}
}
