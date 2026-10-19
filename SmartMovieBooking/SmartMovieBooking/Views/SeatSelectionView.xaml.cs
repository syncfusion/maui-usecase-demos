using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class SeatSelectionView : ContentView
{
    private readonly SeatSelectionViewModel _vm;

    public SeatSelectionView(SeatSelectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;

        // Forward tile clicks to the VM.
        _vm.Confirmed += (_, _) => EventBus.PublishSeatSelectionConfirmed();
        _vm.Cancelled += (_, _) => EventBus.PublishSeatSelectionCancelled();
    }

    public Task InitializeAsync() => _vm.InitializeAsync();

    private void OnCloseClicked(object? sender, EventArgs e)
        => EventBus.PublishSeatSelectionCancelled();

    private void OnCancelClicked(object? sender, EventArgs e)
        => EventBus.PublishSeatSelectionCancelled();

    private void OnConfirmClicked(object? sender, EventArgs e)
        => EventBus.PublishSeatSelectionConfirmed();
}