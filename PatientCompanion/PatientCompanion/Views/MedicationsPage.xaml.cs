using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class MedicationsPage : ContentPage
{
    public MedicationsPage()
        : this(IPlatformApplication.Current!.Services.GetRequiredService<MedicationsViewModel>())
    {
    }

    public MedicationsPage(MedicationsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
