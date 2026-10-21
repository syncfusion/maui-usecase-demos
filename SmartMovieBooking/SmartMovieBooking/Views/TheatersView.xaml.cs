using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using Syncfusion.Maui.ListView;

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

    private void OnDateSelected(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is DateTime d)
        {
            _vm.SelectedDate = d;
            DateList.SelectedItem = null;
        }
    }

    private void OnShowtimeSelected(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Showtime s) return;
        _vm.SelectedShowtime = s;
        if (sender is SfListView list) list.SelectedItem = null;
    }

    private async void OnSelectShowtimeClicked(object? sender, EventArgs e)
    {
        // Navigate to the seat selection popup
        await Shell.Current.GoToAsync("seat-selection");
    }
}
