using SmartMovieBooking.Models;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
        ScreenTitleLabel.Text = _vm.ScreenTitle;
        TheaterLabel.Text = $"{_vm.TheaterName} · {_vm.ShowtimeLabel}";
        UpdatePricing();
    }

    private void UpdatePricing()
    {
        SeatsSummaryLabel.Text = _vm.SelectedSeatSummary;
        TotalLabel.Text = _vm.FormattedTotal;
    }

    private void OnSeatTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Border border || border.BindingContext is not Seat seat) return;
        _vm.ToggleSeat(seat);
        UpdatePricing();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//theaters");

    private async void OnConfirmClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("payment-confirmation");
}
