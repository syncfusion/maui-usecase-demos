using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;

namespace PatientCompanion.ViewModels;

public partial class AppointmentReviewViewModel : ObservableObject
{
    private readonly AppointmentViewModel appointmentViewModel;
    private readonly AppointmentService appointmentService;

    public AppointmentReviewViewModel(
        AppointmentViewModel appointmentViewModel,
        AppointmentService appointmentService,
        MockDataService mockDataService)
    {
        this.appointmentViewModel = appointmentViewModel;
        this.appointmentService = appointmentService;

        Booking = appointmentViewModel.Booking;
        PatientName = mockDataService.GetPatient().Name;
    }

    public AppointmentBooking Booking { get; }

    public Doctor? SelectedDoctor => Booking.SelectedDoctor;

    public string DoctorName =>
        Booking.SelectedDoctor?.Name ?? string.Empty;

    public string DoctorImage =>
        Booking.DoctorImage;

    public string Specialization =>
        Booking.SelectedDoctor?.Specialization ?? string.Empty;

    public string HospitalName =>
        Booking.SelectedDoctor?.HospitalName ?? string.Empty;

    public string Address =>
        Booking.SelectedDoctor?.Address ?? string.Empty;

    public double MapLatitude =>
        Booking.SelectedDoctor?.Latitude ?? 0;

    public double MapLongitude =>
        Booking.SelectedDoctor?.Longitude ?? 0;

    public string VisitType =>
        Booking.VisitType;

    public string AppointmentDate =>
        Booking.SelectedDate.ToString("dddd, dd MMM yyyy");

    public string AppointmentTime =>
        Booking.SelectedTimeSlot;

    public string AppointmentSummary =>
        $"{Booking.SelectedDate:dd MMM yyyy} • {Booking.SelectedTimeSlot}";

    public string PatientName { get; }

    public string PurposeOfVisit => "Follow-up on heart rate monitoring";

    [RelayCommand]
    private async Task ConfirmAppointment()
    {
        if (Booking.ScheduledDateTime < DateTime.Now)
        {
            await Shell.Current.DisplayAlert(
                "Cannot book appointment",
                "Please pick a date and time in the future.",
                "OK");
            return;
        }

        appointmentService.AddAppointment(Booking);

        await Shell.Current.GoToAsync("//MainTabs/VisitsPage");
    }

    [RelayCommand]
    private void EditAppointment()
    {
        appointmentViewModel.CurrentStep = 2;

        appointmentViewModel.CurrentStepView =
            new Views.AppointmentDateTimeView(
                appointmentViewModel);
    }
}
