using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    public DiagramDesignerPage(DiagramDesignerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
