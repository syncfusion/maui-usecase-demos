using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class SeatSelectionView : ContentPage
{
    private readonly SeatSelectionViewModel _vm;

    public SeatSelectionView(SeatSelectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public Task InitializeAsync() => _vm.InitializeAsync();

    private void OnCancelClicked(object? sender, EventArgs e)
        => EventBus.PublishSeatSelectionCancelled();

    private void OnConfirmClicked(object? sender, EventArgs e)
        => EventBus.PublishSeatSelectionConfirmed();

    private async void OnAiClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("ai");
}