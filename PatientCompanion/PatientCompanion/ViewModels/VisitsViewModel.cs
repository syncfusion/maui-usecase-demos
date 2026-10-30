using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;
using PatientCompanion.Views;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class VisitsViewModel : BaseViewModel
{
    private readonly MockDataService mockDataService;

    private readonly AppointmentService appointmentService;

    [ObservableProperty]
    private bool isUpcomingSelected = true;

    public ObservableCollection<Appointment> UpcomingVisits { get; } = new();

    public ObservableCollection<Appointment> PastVisits { get; } = new();

    public ObservableCollection<Appointment> DisplayVisits { get; } = new();

    public VisitsViewModel(
        MockDataService mockDataService,
        AppointmentService appointmentService)
    {
        this.mockDataService = mockDataService;
        this.appointmentService = appointmentService;

        Title = "Visits";

        appointmentService.OnAppointmentsChanged += LoadData;

        LoadData();
    }

    public void Subscribe()
    {
        appointmentService.OnAppointmentsChanged -= LoadData;
        appointmentService.OnAppointmentsChanged += LoadData;
        LoadData();
    }

    public void Unsubscribe()
    {
        appointmentService.OnAppointmentsChanged -= LoadData;
    }

    private void LoadData()
    {
        try
        {
            var showUpcoming = IsUpcomingSelected;

            UpcomingVisits.Clear();
            PastVisits.Clear();

            var now = DateTime.Now;

            foreach (var booking in appointmentService
                .GetAppointments())
            {
                if (booking.Status == "Cancelled")
                    continue;

                var appointment = new Appointment
                {
                    AppointmentId = booking.AppointmentId,
                    DoctorName = booking.DoctorName,
                    Specialization = booking.SelectedDoctor?.Specialization ?? "",
                    Hospital = booking.HospitalName,
                    Date = booking.SelectedDate,
                    Time = booking.SelectedTimeSlot,
                    VisitType = booking.VisitType,
                    Status = booking.Status,
                };

                if (appointment.Status is "Confirmed" or "Upcoming"
                    && booking.ScheduledDateTime > now)
                {
                    UpcomingVisits.Add(appointment);
                }
                else
                {
                    PastVisits.Add(appointment);
                }
            }

            // Upcoming: chronological ascending (next first). Past: reverse
            // chronological (most recent past first), matching the standard
            // history-view convention.
            var upcomingOrdered = UpcomingVisits
                .OrderBy(a => a.ScheduledDateTime);
            var pastOrdered = PastVisits
                .OrderByDescending(a => a.ScheduledDateTime);

            DisplayVisits.Clear();
            var visits = showUpcoming ? upcomingOrdered : pastOrdered;
            foreach (var visit in visits)
                DisplayVisits.Add(visit);
        }
        catch
        {
            // Defensive: don't let UI refresh failure escape into service callers.
        }
    }

    [RelayCommand]
    private void Cancel(Appointment? appointment)
    {
        if (appointment != null)
            appointmentService.CancelAppointment(appointment.AppointmentId);
    }

    [RelayCommand]
    private async Task Reschedule(Appointment? appointment)
    {
        if (appointment == null)
            return;

        appointmentService.RequestReschedule(appointment.AppointmentId);
        await Shell.Current.GoToAsync(nameof(AppointmentPage));
    }

    [RelayCommand]
    private async Task BookAppointment()
    {
        await Shell.Current.GoToAsync(nameof(AppointmentPage));
    }

    [RelayCommand]
    private void ShowUpcoming()
    {
        IsUpcomingSelected = true;

        DisplayVisits.Clear();

        foreach (var item in UpcomingVisits.OrderBy(a => a.ScheduledDateTime))
        {
            DisplayVisits.Add(item);
        }
    }

    [RelayCommand]
    private void ShowPast()
    {
        IsUpcomingSelected = false;

        DisplayVisits.Clear();

        foreach (var item in PastVisits.OrderByDescending(a => a.ScheduledDateTime))
        {
            DisplayVisits.Add(item);
        }
    }

    public Color UpcomingBackground =>
        IsUpcomingSelected
            ? Color.FromArgb("#008378")
            : Color.FromArgb("#E5E7EB");

    public Color UpcomingTextColor =>
        IsUpcomingSelected
            ? Colors.White
            : Color.FromArgb("#3D4947");

    public Color PastBackground =>
        !IsUpcomingSelected
            ? Color.FromArgb("#008378")
            : Color.FromArgb("#E5E7EB");

    public Color PastTextColor =>
        !IsUpcomingSelected
            ? Colors.White
            : Color.FromArgb("#3D4947");

    partial void OnIsUpcomingSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(UpcomingBackground));
        OnPropertyChanged(nameof(UpcomingTextColor));
        OnPropertyChanged(nameof(PastBackground));
        OnPropertyChanged(nameof(PastTextColor));
    }
}
