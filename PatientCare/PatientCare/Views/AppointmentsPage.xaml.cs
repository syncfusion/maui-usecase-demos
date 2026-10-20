using System.Collections.ObjectModel;
using PatientCare.Models;
using PatientCare.Services;
using PatientCare.ViewModels;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Scheduler;

namespace PatientCare.Views;

public partial class AppointmentsPage : ContentPage
{
    private readonly AppointmentsViewModel viewModel;

    // Shell resolves DataTemplate pages through the app's DI container, so constructor injection is the only path.
    public AppointmentsPage(ScheduleDataService dataService, NotificationService notificationService)
    {
        InitializeComponent();
        viewModel = new AppointmentsViewModel(dataService);
        BindingContext = viewModel;
        DesktopNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();
        MobileNotificationBadge.BadgeText = notificationService.CareAlertCount.ToString();

        DesktopScheduler.DaysView.TimeRegions = CreateLunchBreakRegions();
        MobileScheduler.DaysView.TimeRegions = CreateLunchBreakRegions();
    }

    private static ObservableCollection<SchedulerTimeRegion> CreateLunchBreakRegions() =>
    [
        new SchedulerTimeRegion
        {
            StartTime = DateTime.Today.AddHours(12),
            EndTime = DateTime.Today.AddHours(13),
            Text = "LUNCH BREAK",
            EnablePointerInteraction = false,
            RecurrenceRule = "FREQ=DAILY;INTERVAL=1"
        }
    ];

    private void OnSchedulerViewChanged(object? sender, SchedulerViewChangedEventArgs e)
    {
        if (e.NewVisibleDates is null || e.NewVisibleDates.Count == 0)
        {
            return;
        }

        viewModel.SynchronizeSchedulerState(e.NewView, e.NewVisibleDates.First());
    }

    private void OnClinicianCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Clinician clinician)
        {
            viewModel.SelectClinicianCommand.Execute(clinician);
        }
    }

    private void OnClinicianSelectionChanged(object? sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        if (e.AddedItem is Clinician clinician)
        {
            viewModel.SelectClinicianCommand.Execute(clinician);
        }
    }

    private void OnSchedulerTapped(object? sender, SchedulerTappedEventArgs e)
    {
        var appointment = e.Appointments?
            .Select(item => item is ScheduleAppointment scheduleAppointment ? scheduleAppointment
                : item is SchedulerAppointment schedulerAppointment ? schedulerAppointment.DataItem as ScheduleAppointment
                : null)
            .FirstOrDefault(item => item is not null);

        if (appointment is not null)
        {
            viewModel.SelectedAppointment = appointment;
        }
    }

    private void OnCloseAppointmentDetailsTapped(object? sender, TappedEventArgs e)
    {
        viewModel.CloseAppointmentDetailsCommand.Execute(null);
    }

    private void OnCloseAppointmentDetailsClicked(object? sender, EventArgs e)
    {
        viewModel.CloseAppointmentDetailsCommand.Execute(null);
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