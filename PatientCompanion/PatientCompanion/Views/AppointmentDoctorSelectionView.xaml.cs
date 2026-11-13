using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentDoctorSelectionView : ContentView
{
    public AppointmentDoctorSelectionView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();

        BindingContext =
            new DoctorsViewModel(
                IPlatformApplication.Current!.Services.GetRequiredService<MockDataService>(),
                appointmentViewModel);
    }
}
