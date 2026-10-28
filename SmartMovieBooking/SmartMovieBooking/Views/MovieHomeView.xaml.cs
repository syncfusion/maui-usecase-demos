using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

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
    }

    private void CategoryChipGroup_ChipClicked(object sender, EventArgs e)
    {
        var selected = categoryChipGroup.SelectedItem;

        if (selected == null)
            return;

        _vm.SelectedGenre = selected.ToString() ?? "All Genres";
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => _vm.SearchText = e.NewTextValue ?? string.Empty;

    // Called from TapGestureRecognizer on each movie card (whole-card tap).
    private void OnMovieCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Movie movie) return;
        NavigateToTheatersFor(movie);
    }

    // Called from action buttons inside each card. The button BindingContext is the Movie.
    private void OnBookNowClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Movie movie)
            NavigateToTheatersFor(movie);
    }

    private void OnPreviewClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Movie movie)
            NavigateToTheatersFor(movie);
    }

    private void NavigateToTheatersFor(Movie movie)
    {
        _session.SelectedMovie = movie;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();

        // Shell.Current.GoToAsync with "//" prefix navigates to an
        // absolute tab - in this case the Theaters tab.
        MainThread.BeginInvokeOnMainThread(() => _ = Shell.Current.GoToAsync("//theaters"));
    }
}