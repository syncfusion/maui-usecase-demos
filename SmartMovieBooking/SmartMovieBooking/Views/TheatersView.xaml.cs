using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class TheatersView : ContentView
{
    private readonly TheatersViewModel _vm;
    private bool _subscribed;

    public TheatersView(TheatersViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;

        if (!_subscribed)
        {
            EventBus.ShowtimeClicked += OnShowtimeClicked;
            EventBus.TheaterContinueClicked += OnTheaterContinueClicked;
            _subscribed = true;
        }
    }

    public async Task ReloadAsync() => await _vm.InitializeAsync();

    private void OnShowtimeClicked(object? sender, Showtime e)
        => _vm.ShowtimeSelectedCommand.Execute(e);

    private void OnTheaterContinueClicked(object? sender, Theater e)
        => _vm.TheaterContinueCommand.Execute(e);

    private void OnDateChipSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is DateChipItem chip)
        {
            _vm.SelectedDateChip = chip;
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private void OnShareClicked(object? sender, EventArgs e)
    {
        // Mock share - sample only.
    }

    private void OnAiFabClicked(object? sender, EventArgs e)
        => Shell.Current.GoToAsync("ai");
}