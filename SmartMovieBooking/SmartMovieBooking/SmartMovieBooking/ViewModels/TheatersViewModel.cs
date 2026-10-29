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

    [ObservableProperty] private string _moviePosterUrl = string.Empty;
    [ObservableProperty] private string _movieBannerUrl = string.Empty;
    [ObservableProperty] private string _movieTagline = string.Empty;
    [ObservableProperty] private string _movieSynopsis = string.Empty;
    [ObservableProperty] private string _movieRatingText = string.Empty;
    [ObservableProperty] private string _movieDurationText = string.Empty;
    [ObservableProperty] private string _movieCertificateText = string.Empty;
    [ObservableProperty] private string _movieFormatText = string.Empty;
    [ObservableProperty] private string _movieGenresText = string.Empty;
    [ObservableProperty] private string _movieReleaseText = string.Empty;
    [ObservableProperty] private string _moviePriceRangeText = string.Empty;

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
            {
                var movie = _session.SelectedMovie;
                MovieTitle = movie.Title;
                MoviePosterUrl = movie.PosterUrl;
                MovieBannerUrl = movie.BannerUrl;
                MovieTagline = movie.Tagline;
                MovieSynopsis = movie.Synopsis;
                MovieRatingText = movie.Rating;
                MovieDurationText = movie.Duration;
                MovieCertificateText = movie.Certificate;
                MovieFormatText = movie.Format;
                MovieGenresText = movie.Genres.Count > 0 ? string.Join(" • ", movie.Genres) : movie.PrimaryGenre;
                MovieReleaseText = movie.ReleaseDate;
                MoviePriceRangeText = movie.PriceRange;
            }

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
