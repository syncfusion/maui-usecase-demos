using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using Syncfusion.Maui.Chips;
using Syncfusion.Maui.ListView;

namespace SmartMovieBooking.Views;

public partial class MovieHomeView : ContentPage
{
    private readonly HomeViewModel _vm;
    private readonly BookingSession _session;

    public MovieHomeView(HomeViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => _vm.SearchText = e.NewTextValue ?? string.Empty;

    private void OnGenreSelected(object? sender, ChipSelectionChangedEventArgs e)
    {
        if (e.AddedItem is string g)
            _vm.SelectedGenre = g;
    }

    private async void OnMovieTapped(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Movie m) return;

        // Store selection and navigate to the Theaters tab
        _session.SelectedMovie = m;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();

        MoviesList.SelectedItem = null; // reset so the same movie can be tapped again
        await Shell.Current.GoToAsync("//theaters");
    }
}
