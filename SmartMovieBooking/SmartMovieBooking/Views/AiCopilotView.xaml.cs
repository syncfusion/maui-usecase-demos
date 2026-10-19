using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class AiCopilotView : ContentPage
{
    public AiCopilotView(AiCopilotViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");
}
