using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class AiCopilotSheet : ContentPage
{
    private readonly AiCopilotViewModel _vm;

    public AiCopilotSheet(AiCopilotViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OnSendClicked(object? sender, EventArgs e)
        => await _vm.SendAsync();
}