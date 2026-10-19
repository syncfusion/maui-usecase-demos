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
    public ObservableCollection<Movie> HeroMovies { get; } = new();
    public ObservableCollection<Movie> NowShowingMovies { get; } = new();
    public ObservableCollection<Movie> ComingSoonMovies { get; } = new();
    public ObservableCollection<Movie> PopularMovies { get; } = new();
    public ObservableCollection<string> Genres { get; }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedGenre = "All Genres";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _nowShowingCount;

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _movies.GetAllAsync();
            Movies.Clear();
            HeroMovies.Clear();
            NowShowingMovies.Clear();
            ComingSoonMovies.Clear();
            PopularMovies.Clear();
            foreach (var m in list) Movies.Add(m);

            // First 5 → hero carousel
            foreach (var m in list.Take(5)) HeroMovies.Add(m);

            // Now Showing = first 10
            foreach (var m in list.Where(m => m.IsNowShowing).Take(10))
                NowShowingMovies.Add(m);

            // Coming Soon = items where IsNowShowing is false
            foreach (var m in list.Where(m => !m.IsNowShowing).Take(8))
                ComingSoonMovies.Add(m);

            // Popular = top 6 by rating
            foreach (var m in list
                         .OrderByDescending(m => double.TryParse(m.Rating, out var r) ? r : 0)
                         .Take(6))
                PopularMovies.Add(m);

            NowShowingCount = NowShowingMovies.Count;
        }
        finally { IsLoading = false; }
    }
}