using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class CreateDiagramPage : ContentPage
{
    public CreateDiagramPage(CreateDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Appearing += async (_, _) => await viewModel.InitializeAsync();
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
