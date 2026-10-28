using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IMovieService _movies;
    private readonly BookingSession _session;

    public HomeViewModel(IMovieService movies, BookingSession session)
    {
        _movies = movies;
        _session = session;
        Genres = new ObservableCollection<string> { "All Genres" };
        foreach (var g in AppSeed.Genres) Genres.Add(g);
        _ = LoadAsync();
    }

    public ObservableCollection<Movie> Movies { get; } = new();
    public ObservableCollection<Movie> FilteredMovies { get; } = new();
    public ObservableCollection<Movie> FeaturedMovies { get; } = new();
    public ObservableCollection<Movie> PopularMovies { get; } = new();
    public ObservableCollection<Movie> ComingSoonMovies { get; } = new();
    public ObservableCollection<string> Genres { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedGenre = "All Genres";
    [ObservableProperty] private bool _isLoading;

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _movies.GetAllAsync();
            Movies.Clear();
            foreach (var m in list)
                Movies.Add(m);

            ApplyFilters();
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedGenreChanged(string value) => ApplyFilters();

    [RelayCommand]
    private void SelectGenre(string genre) => SelectedGenre = genre ?? "All Genres";

    [RelayCommand]
    private void SelectMovie(Movie movie)
    {
        _session.SelectedMovie = movie;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();
        Shell.Current.GoToAsync("theaters");
    }

    private void ApplyFilters()
    {
        IEnumerable<Movie> query = Movies;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            query = query.Where(m =>
                m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.PrimaryGenre.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SelectedGenre) && SelectedGenre != "All Genres")
            query = query.Where(m => m.Genres.Contains(SelectedGenre, StringComparer.OrdinalIgnoreCase));

        var filtered = query.ToList();

        ReplaceItems(FilteredMovies, filtered);
        ReplaceItems(FeaturedMovies, filtered.Where(m => m.IsNowShowing)
            .OrderByDescending(GetRating)
            .ThenBy(m => m.Title)
            .Take(4));
        ReplaceItems(PopularMovies, filtered.Where(m => m.IsNowShowing)
            .OrderByDescending(GetRating)
            .ThenBy(m => m.Title)
            .Take(6));
        ReplaceItems(ComingSoonMovies, filtered.Where(m => !m.IsNowShowing)
            .OrderBy(m => ParseReleaseRank(m.ReleaseDate))
            .ThenBy(m => m.Title)
            .Take(6));
    }

    private static void ReplaceItems(ObservableCollection<Movie> target, IEnumerable<Movie> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }

    private static decimal GetRating(Movie movie)
        => decimal.TryParse(movie.Rating, out var rating) ? rating : 0m;

    private static int ParseReleaseRank(string releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate))
            return int.MaxValue;

        var parts = releaseDate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !int.TryParse(parts[1], out var day))
            return int.MaxValue;

        return day;
    }
}
