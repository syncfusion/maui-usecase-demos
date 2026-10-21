using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using Syncfusion.Maui.Buttons;

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
        UpdateSeatsList();
    }

    private void UpdateSeatsList()
    {
        // Re-build flat seat list with one entry per real seat (skip aisle placeholders)
        var flat = _vm.Seats.Where(s => s.State != SeatState.Aisle).ToList();
        _vm.Seats.Clear();
        foreach (var s in flat) _vm.Seats.Add(s);
        UpdatePricing();
    }

    private void OnSeatTapped(object? sender, EventArgs e)
    {
        if (sender is not SfButton btn || btn.BindingContext is not Seat seat) return;
        _vm.ToggleSeat(seat);
        UpdatePricing();
    }

    private void UpdatePricing()
    {
        SeatsSummaryLabel.Text = $"{_vm.SelectedCount} tickets";
        TotalLabel.Text = _vm.FormattedTotal;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//theaters");

    private async void OnConfirmClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("payment-confirmation");
}
