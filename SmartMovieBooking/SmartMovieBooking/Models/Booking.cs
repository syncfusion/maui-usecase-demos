using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class Booking : ObservableObject
{
    [ObservableProperty] private string _bookingId = string.Empty;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private Movie? _movie;
    [ObservableProperty] private Theater? _theater;
    [ObservableProperty] private Showtime? _showtime;
    [ObservableProperty] private ObservableCollection<Seat> _seats = new();
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _convenienceFee = 2.50m;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private string _paymentMethod = "Card";
    [ObservableProperty] private string _status = "Confirmed";
}
