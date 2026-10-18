using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        BindingContext = _viewModel;
    }

    private void OnAiFabClicked(object? sender, EventArgs e)
    {
        // Phase 4 (T4.2) will replace this with SfBottomSheet containing AIAssistView.
        _viewModel.IsAiCopilotOpen = !_viewModel.IsAiCopilotOpen;
        DisplayAlert("Cinema AI Copilot",
            "AI Copilot sheet opens in Phase 4 (T4.2).\n\nCurrent status: " +
            (_viewModel.IsAiCopilotOpen ? "Open" : "Closed"), "OK");
    }
}