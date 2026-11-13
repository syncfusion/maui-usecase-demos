using Microsoft.Extensions.DependencyInjection;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class HomePage : ContentPage
{
    private HomeViewModel? viewModel;

    public HomePage()
        : this(IPlatformApplication.Current!.Services.GetRequiredService<HomeViewModel>())
    {
    }

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        viewModel?.Subscribe();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        viewModel?.Unsubscribe();
    }
}
