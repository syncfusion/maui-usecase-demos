using System.Collections.ObjectModel;
using System.Linq;
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

    public string MovieTitle => Movie?.Title ?? "Movie";
    public string TheaterName => Theater?.Name ?? "Theater";
    public string ShowtimeText => Showtime is null
        ? string.Empty
        : $"{Showtime.Date:ddd, MMM d} · {Showtime.StartTime}";
    public string SeatsText => Seats.Count == 0
        ? "No seats"
        : string.Join(", ", Seats.OrderBy(s => s.Id).Select(s => s.Id));
    public string SeatCountText => $"{Seats.Count} seat{(Seats.Count == 1 ? string.Empty : "s")}";
    public string CreatedAtText => CreatedAt == default
        ? string.Empty
        : CreatedAt.ToString("MMM d, yyyy · h:mm tt");
    public string StatusText => Status;
}
