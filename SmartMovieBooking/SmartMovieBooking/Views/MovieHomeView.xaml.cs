using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class MovieHomeView : ContentView
{
    private readonly HomeViewModel _vm;
    private readonly BookingSession _session;
    private readonly MainWindowViewModel _main;

    public MovieHomeView(HomeViewModel vm, BookingSession session, MainWindowViewModel main)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        _main = main;
        BindingContext = _vm;
    }

    private void OnMovieSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Movie m)
        {
            _session.SelectedMovie = m;
            _session.SelectedTheater = null;
            _session.SelectedShowtime = null;
            _session.SelectedDate = DateTime.Today;
            _session.SelectedSeats.Clear();
            _main.NavigateToTheaters();

            // clear selection so the same item can be re-selected later
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }
}