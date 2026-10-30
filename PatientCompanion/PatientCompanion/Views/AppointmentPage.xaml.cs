using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentPage : ContentPage
{
    public AppointmentPage()
        : this(
            IPlatformApplication.Current!.Services.GetRequiredService<AppointmentService>(),
            null)
    {
    }

    public AppointmentPage(AppointmentService appointmentService, AppointmentViewModel? viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel ?? new AppointmentViewModel(appointmentService.ConsumePendingReschedule());
    }
}
