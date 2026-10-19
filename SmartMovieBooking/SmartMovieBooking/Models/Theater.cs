using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class Theater : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _location = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    [ObservableProperty] private double _distanceKm;
    [ObservableProperty] private string _distanceLabel = string.Empty;
    [ObservableProperty] private string _priceFrom = string.Empty;
    [ObservableProperty] private int _availableSeats;
    [ObservableProperty] private string _badge = string.Empty;
    [ObservableProperty] private string _icon = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private List<TheaterFacility> _facilities = new();
    [ObservableProperty] private List<Showtime> _showtimes = new();
}

public partial class TheaterFacility : ObservableObject
{
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _icon = string.Empty;
    [ObservableProperty] private bool _isProminent;
}
