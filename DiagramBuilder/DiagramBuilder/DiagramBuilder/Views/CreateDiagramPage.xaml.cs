using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class CreateDiagramPage : ContentPage
{
    public CreateDiagramPage(CreateDiagramViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
