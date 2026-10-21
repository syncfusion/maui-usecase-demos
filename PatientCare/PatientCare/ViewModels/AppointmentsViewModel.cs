using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using PatientCare.Models;
using PatientCare.Services;
using Syncfusion.Maui.Scheduler;

namespace PatientCare.ViewModels;

public sealed class AppointmentsViewModel : INotifyPropertyChanged
{
    private readonly ScheduleDataService scheduleDataService;
    private bool isBusy;
    private string errorMessage = string.Empty;
    private DateTime selectedDate = DateTime.Today;
    private SchedulerView viewMode = SchedulerView.Day;
    private Clinician? selectedClinician;
    private ScheduleAppointment? selectedAppointment;
    private bool hasNoAppointments;

    public AppointmentsViewModel(ScheduleDataService scheduleDataService)
    {
        this.scheduleDataService = scheduleDataService;

        PreviousDateCommand = new Command(() => SelectedDate = SelectedDate.AddDays(ViewMode == SchedulerView.Week ? -7 : -1));
        NextDateCommand = new Command(() => SelectedDate = SelectedDate.AddDays(ViewMode == SchedulerView.Week ? 7 : 1));
        GoToTodayCommand = new Command(() => SelectedDate = DateTime.Today);
        SelectClinicianCommand = new Command<Clinician>(SelectClinician);
        CloseAppointmentDetailsCommand = new Command(() => SelectedAppointment = null);
        RetryCommand = new Command(Load);

        Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => isBusy;
        private set => SetField(ref isBusy, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetField(ref errorMessage, value);
    }

    public DateTime SelectedDate
    {
        get => selectedDate;
        set
        {
            if (SetField(ref selectedDate, value))
            {
                OnPropertyChanged(nameof(SelectedDateDisplay));
                RefreshVisibleAppointments();
            }
        }
    }

    public string SelectedDateDisplay
    {
        get
        {
            if (ViewMode != SchedulerView.Week)
            {
                return SelectedDate.ToString("MMM dd, yyyy");
            }

            var weekStart = SelectedDate.Date.AddDays(-(int)SelectedDate.DayOfWeek + (int)DayOfWeek.Sunday);
            var weekEnd = weekStart.AddDays(6);
            return SelectedDate.Year == weekEnd.Year
                ? $"{SelectedDate:MMM dd} - {weekEnd:MMM dd, yyyy}"
                : $"{SelectedDate:MMM dd, yyyy} - {weekEnd:MMM dd, yyyy}";
        }
    }

    public SchedulerView ViewMode
    {
        get => viewMode;
        set
        {
            if (SetField(ref viewMode, value))
            {
                OnPropertyChanged(nameof(ViewModeIndex));
                OnPropertyChanged(nameof(SelectedDateDisplay));
                RefreshVisibleAppointments();
            }
        }
    }

    /// <summary>0=Day, 1=Week. Two-way bound to the header's segmented control: tapping writes here,
    /// which changes ViewMode and updates the scheduler through its View binding — no code-behind event needed.</summary>
    public int ViewModeIndex
    {
        get => ViewMode == SchedulerView.Week ? 1 : 0;
        set => ViewMode = value == 1 ? SchedulerView.Week : SchedulerView.Day;
    }

    public Clinician? SelectedClinician
    {
        get => selectedClinician;
        private set => SetField(ref selectedClinician, value);
    }

    public ScheduleAppointment? SelectedAppointment
    {
        get => selectedAppointment;
        set
        {
            if (SetField(ref selectedAppointment, value))
            {
                OnPropertyChanged(nameof(IsAppointmentDetailsOpen));
            }
        }
    }

    public bool IsAppointmentDetailsOpen => SelectedAppointment is not null;

    public bool HasNoAppointments
    {
        get => hasNoAppointments;
        private set => SetField(ref hasNoAppointments, value);
    }

    public ObservableCollection<Clinician> Clinicians { get; } = [];

    public ObservableCollection<ScheduleAppointment> VisibleAppointments { get; } = [];

    public Command PreviousDateCommand { get; }
    public Command NextDateCommand { get; }
    public Command GoToTodayCommand { get; }
    public Command<Clinician> SelectClinicianCommand { get; }
    public Command CloseAppointmentDetailsCommand { get; }
    public Command RetryCommand { get; }

    public void Refresh() => Load();

    public void SynchronizeSchedulerState(SchedulerView view, DateTime visibleDate)
    {
        ViewMode = view;
        SelectedDate = visibleDate.Date;
    }

    private void Load()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var clinicianList = scheduleDataService.GetClinicians();
            Clinicians.Clear();
            foreach (var clinician in clinicianList)
            {
                Clinicians.Add(clinician);
            }

            SelectClinician(Clinicians.FirstOrDefault());
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectClinician(Clinician? clinician)
    {
        if (clinician is null)
        {
            return;
        }

        SelectedClinician = clinician;

        RefreshVisibleAppointments();
    }

    private void RefreshVisibleAppointments()
    {
        if (SelectedClinician is null)
        {
            return;
        }

        // Scheduler week view starts on Sunday (first day of week), so anchor the data filter to the same week.
        var selectedDay = SelectedDate.Date;
        var weekStart = selectedDay.Date.AddDays(-(int)selectedDay.DayOfWeek);

        var appointments = scheduleDataService.GetAppointments(SelectedClinician.ClinicianId)
            .Where(appointment => ViewMode == SchedulerView.Week
                ? appointment.StartTime.Date >= weekStart.Date && appointment.StartTime.Date < weekStart.Date.AddDays(7)
                : appointment.StartTime.Date == selectedDay)
            .OrderBy(appointment => appointment.StartTime);

        VisibleAppointments.Clear();
        foreach (var appointment in appointments)
        {
            VisibleAppointments.Add(appointment);
        }

        HasNoAppointments = VisibleAppointments.Count == 0;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
