using DiagramBuilder.Constants;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = Shell.Current.GoToAsync($"//{AppConstants.RouteCreateDiagram}");
        }
    }
}
