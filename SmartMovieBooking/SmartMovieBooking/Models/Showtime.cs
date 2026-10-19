using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class Showtime : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _movieId = string.Empty;
    [ObservableProperty] private string _theaterId = string.Empty;
    [ObservableProperty] private string _screenId = string.Empty;
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private string _startTime = string.Empty;
    [ObservableProperty] private string _endTime = string.Empty;
    [ObservableProperty] private string _format = string.Empty;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private int _availableSeats;
    [ObservableProperty] private ShowtimeStatus _status = ShowtimeStatus.Available;
    [ObservableProperty] private bool _isPrimeTime;
    [ObservableProperty] private bool _isSelected;
}

public enum ShowtimeStatus { Available, AlmostFull, SellingFast, SoldOut }
