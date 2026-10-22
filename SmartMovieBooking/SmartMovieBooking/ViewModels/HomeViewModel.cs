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
            FilteredMovies.Clear();
            foreach (var m in list) { Movies.Add(m); FilteredMovies.Add(m); }
            ApplyFilters();
        }
        finally { IsLoading = false; }
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
        // Navigation to Theaters happens in code-behind via Shell route
        Shell.Current.GoToAsync("theaters");
    }

    private void ApplyFilters()
    {
        FilteredMovies.Clear();
        IEnumerable<Movie> q = Movies;
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim();
            q = q.Where(m =>
                m.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                m.PrimaryGenre.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(SelectedGenre) && SelectedGenre != "All Genres")
            q = q.Where(m => m.Genres.Contains(SelectedGenre, StringComparer.OrdinalIgnoreCase));
        foreach (var m in q) FilteredMovies.Add(m);
    }
}
