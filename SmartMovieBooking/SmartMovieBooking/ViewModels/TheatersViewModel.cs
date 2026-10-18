using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        BuildDateStrip();
    }

    public ObservableCollection<DateTime> Dates { get; } = new();
    public ObservableCollection<Theater> Theaters { get; } = new();

    [ObservableProperty] private Movie? _movie;
    [ObservableProperty] private DateTime _selectedDate;
    [ObservableProperty] private string _selectedFormat = "All Formats";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _venuesCount;

    public event EventHandler<Showtime>? ShowtimeSelected;

    public async Task InitializeAsync()
    {
        if (_session.SelectedMovie is null) return;
        Movie = _session.SelectedMovie;
        await LoadTheatersAsync();
    }

    private void BuildDateStrip()
    {
        Dates.Clear();
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++) Dates.Add(today.AddDays(i));
        SelectedDate = today;
    }

    partial void OnSelectedDateChanged(DateTime value) => _ = LoadTheatersAsync();

    public async Task LoadTheatersAsync()
    {
        IsLoading = true;
        try
        {
            var movieId = Movie?.Id ?? "";
            var theaters = await _theaters.GetTheatersForMovieAsync(movieId);
            foreach (var t in theaters)
            {
                t.Showtimes = (await _showtimes.GetAsync(movieId, t.Id, SelectedDate)).ToList();
            }

            Theaters.Clear();
            foreach (var t in theaters) Theaters.Add(t);
            VenuesCount = Theaters.Count;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void SelectShowtime(Showtime showtime)
    {
        if (showtime is null || showtime.Status == ShowtimeStatus.SoldOut) return;

        // Deselect previous
        foreach (var t in Theaters)
            foreach (var s in t.Showtimes)
                s.IsSelected = false;

        showtime.IsSelected = true;
        _session.SelectedTheater = Theaters.FirstOrDefault(t => t.Showtimes.Contains(showtime));
        _session.SelectedShowtime = showtime;
        _session.SelectedDate = SelectedDate;

        ShowtimeSelected?.Invoke(this, showtime);
    }
}