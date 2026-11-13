using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCompanion.Models;
using PatientCompanion.Services;
using PatientCompanion.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PatientCompanion.ViewModels;

public partial class AppointmentViewModel : BaseViewModel
{
    [ObservableProperty]
    private int currentStep;

    [ObservableProperty]
    private View currentStepView;

    [ObservableProperty]
    private AppointmentBooking booking = new();

    public AppointmentViewModel()
        : this(null, null, null, null, null)
    {
    }

    public AppointmentViewModel(
        AppointmentBooking? existingBooking,
        Func<AppointmentViewModel, AppointmentSpecialtyView>? specialtyFactory = null,
        Func<AppointmentViewModel, AppointmentDoctorSelectionView>? doctorFactory = null,
        Func<AppointmentViewModel, AppointmentDateTimeView>? dateTimeFactory = null,
        Func<AppointmentViewModel, AppointmentReviewView>? reviewFactory = null)
    {
        specialtyFactory ??= vm => new AppointmentSpecialtyView(vm);
        doctorFactory ??= vm => new AppointmentDoctorSelectionView(vm);
        dateTimeFactory ??= vm => new AppointmentDateTimeView(vm);
        reviewFactory ??= vm => new AppointmentReviewView(vm);

        if (existingBooking != null)
        {
            Booking = existingBooking;
            CurrentStep = 2;
            CurrentStepView = dateTimeFactory(this);
            return;
        }

        CurrentStep = 0;
        CurrentStepView = specialtyFactory(this);
    }

    // Convenience accessors that forward to the Booking object
    public Doctor? SelectedDoctor
    {
        get => Booking.SelectedDoctor;
        set
        {
            Booking.SelectedDoctor = value;
            OnPropertyChanged(nameof(SelectedDoctor));
        }
    }

    public SpecialtyItem? SelectedSpecialty
    {
        get => Booking.SelectedSpecialty;
        set
        {
            Booking.SelectedSpecialty = value;
            OnPropertyChanged(nameof(SelectedSpecialty));
        }
    }

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(StepText));
        OnPropertyChanged(nameof(StepTitle));

        OnPropertyChanged(nameof(Step1Color));
        OnPropertyChanged(nameof(Step2Color));
        OnPropertyChanged(nameof(Step3Color));
        OnPropertyChanged(nameof(Step4Color));
    }

    public string StepText => $"STEP {CurrentStep + 1} OF 4";

    public string StepTitle =>
        CurrentStep switch
        {
            0 => "Select Specialty",
            1 => "Select Doctor",
            2 => "Date & Time",
            3 => "Review & Confirm",
            _ => string.Empty
        };

    public Color Step1Color =>
        CurrentStep >= 0
            ? Color.FromArgb("#00685F")
            : Color.FromArgb("#D9D9D9");

    public Color Step2Color =>
        CurrentStep >= 1
            ? Color.FromArgb("#00685F")
            : Color.FromArgb("#D9D9D9");

    public Color Step3Color =>
        CurrentStep >= 2
            ? Color.FromArgb("#00685F")
            : Color.FromArgb("#D9D9D9");

    public Color Step4Color =>
        CurrentStep >= 3
            ? Color.FromArgb("#00685F")
            : Color.FromArgb("#D9D9D9");

    [RelayCommand]
    private async Task GoBack()
    {
        // Step 1 - Select Specialty: exit the flow and go back to Visits Page
        if (CurrentStep == 0)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        CurrentStep = CurrentStep switch
        {
            1 => 0,
            2 => 1,
            _ => 2
        };

        CurrentStepView = CurrentStep switch
        {
            0 => new AppointmentSpecialtyView(this),
            1 => new AppointmentDoctorSelectionView(this),
            _ => new AppointmentDateTimeView(this)
        };
    }
}
