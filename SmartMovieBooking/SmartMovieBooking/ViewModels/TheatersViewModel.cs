using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class TheatersViewModel : ObservableObject
{
    private readonly ITheaterService _theaters;
    private readonly IShowtimeService _showtimes;
    private readonly BookingSession _session;

    public TheatersViewModel(ITheaterService theaters, IShowtimeService showtimes, BookingSession session)
    {
        _theaters = theaters;
        _showtimes = showtimes;
        _session = session;
        Dates = new ObservableCollection<DateTime>();
        Theaters = new ObservableCollection<Theater>();
        BuildDateStrip();
    }

    public ObservableCollection<DateTime> Dates { get; }
    public ObservableCollection<Theater> Theaters { get; }

    [ObservableProperty] private DateTime _selectedDate;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private Theater? _selectedTheater;
    [ObservableProperty] private Showtime? _selectedShowtime;
    [ObservableProperty] private string _movieTitle = string.Empty;

    private void BuildDateStrip()
    {
        Dates.Clear();
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++) Dates.Add(today.AddDays(i));
        SelectedDate = today;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            if (_session.SelectedMovie is not null)
                MovieTitle = _session.SelectedMovie.Title;

            var movieId = _session.SelectedMovie?.Id;
            if (string.IsNullOrEmpty(movieId)) { Theaters.Clear(); return; }

            var list = await _theaters.GetTheatersForMovieAsync(movieId);
            Theaters.Clear();
            foreach (var t in list)
            {
                t.Showtimes = (await _showtimes.GetAsync(movieId, t.Id, SelectedDate)).ToList();
                Theaters.Add(t);
            }
        }
        finally { IsLoading = false; }
    }
}
