using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class TheatersView : ContentPage
{
    private readonly TheatersViewModel _vm;
    private readonly BookingSession _session;

    public TheatersView(TheatersViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        MovieLabel.Text = _vm.MovieTitle ?? "Choose a theater and showtime";
    }

    private async void OnDateTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not DateTime d) return;

        _vm.SelectedDate = d;
        await _vm.LoadAsync();
        MovieLabel.Text = _vm.MovieTitle ?? "Choose a theater and showtime";
    }

    private async void OnShowtimeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Showtime s) return;

        _vm.SelectedShowtime = s;
        _session.SelectedShowtime = s;
        _session.SelectedDate = _vm.SelectedDate;
        _session.SelectedTheater = _vm.Theaters.FirstOrDefault(t => t.Showtimes.Contains(s));
        await Shell.Current.GoToAsync("seat-selection");
    }
}
