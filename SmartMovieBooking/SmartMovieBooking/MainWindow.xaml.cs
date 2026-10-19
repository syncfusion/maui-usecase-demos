using System.ComponentModel;
using SmartMovieBooking.Controls;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly PopupOrchestrator _popups;
    private readonly TheatersView _theatersView;

    public MainWindow(MainWindowViewModel viewModel,
                      PopupOrchestrator popups,
                      TheatersView theatersView)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _popups = popups;
        _theatersView = theatersView;
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is not null)
        {
            _popups.AttachTo(PopupHost);
        }
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.CurrentPage))
        {
            // Reload theaters data whenever the user lands on Theaters.
            if (_viewModel.CurrentPage == ViewModels.MainPage.Theaters)
                await _theatersView.ReloadAsync();
        }
    }

    private void OnAiFabClicked(object? sender, EventArgs e)
    {
        _viewModel.IsAiCopilotOpen = !_viewModel.IsAiCopilotOpen;
    }
}