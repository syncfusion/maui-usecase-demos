using DiagramBuilder.Views;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder
{
    public partial class MainPage : ContentPage
    {
        private readonly CreateDiagramPage _createDiagramPage;
        private bool _dialogShown;

        public MainPage(MainViewModel viewModel, CreateDiagramPage createDiagramPage)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _createDiagramPage = createDiagramPage;
            Appearing += OnAppearing;
        }

        private async void OnAppearing(object? sender, EventArgs e)
        {
            if (_dialogShown)
            {
                return;
            }

            _dialogShown = true;
            await Task.Delay(50);
            await Shell.Current.Navigation.PushModalAsync(_createDiagramPage);
        }
    }
}