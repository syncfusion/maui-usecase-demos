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
            // Switch to Theaters tab.
            Shell.Current.GoToAsync("//theaters");
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }

    private void OnAiFabClicked(object? sender, EventArgs e)
    {
        // Phase 4 (T4.2) will wire the AI Copilot sheet here.
        Shell.Current.GoToAsync("ai");
    }
}