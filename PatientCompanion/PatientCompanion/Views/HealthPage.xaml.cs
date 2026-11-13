using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class HealthPage : ContentPage
{
    public HealthPage()
        : this(IPlatformApplication.Current!.Services.GetRequiredService<HealthViewModel>())
    {
    }

    public HealthPage(HealthViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
