using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class Movie : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _tagline = string.Empty;
    [ObservableProperty] private string _synopsis = string.Empty;
    [ObservableProperty] private string _posterUrl = string.Empty;
    [ObservableProperty] private string _bannerUrl = string.Empty;
    [ObservableProperty] private string _primaryGenre = string.Empty;
    [ObservableProperty] private List<string> _genres = new();
    [ObservableProperty] private List<string> _languages = new();
    [ObservableProperty] private string _format = "2D";
    [ObservableProperty] private string _rating = string.Empty;
    [ObservableProperty] private string _duration = string.Empty;
    [ObservableProperty] private string _releaseDate = string.Empty;
    [ObservableProperty] private string _certificate = "UA";
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _priceRange = string.Empty;
    [ObservableProperty] private bool _isNowShowing = true;
    [ObservableProperty] private List<CastMember> _cast = new();
}
