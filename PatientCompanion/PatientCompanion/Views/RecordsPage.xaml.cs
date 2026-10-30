using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class RecordsPage : ContentPage
{
    public RecordsPage()
        : this(IPlatformApplication.Current!.Services.GetRequiredService<RecordsViewModel>())
    {
    }

    public RecordsPage(RecordsViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
