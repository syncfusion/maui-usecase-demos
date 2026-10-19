using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public partial class BookingSession : ObservableObject
{
    [ObservableProperty] private Movie? _selectedMovie;
    [ObservableProperty] private Theater? _selectedTheater;
    [ObservableProperty] private Showtime? _selectedShowtime;
    [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty] private ObservableCollection<Seat> _selectedSeats = new();

    public void Reset()
    {
        SelectedMovie = null;
        SelectedTheater = null;
        SelectedShowtime = null;
        SelectedDate = DateTime.Today;
        SelectedSeats.Clear();
    }

    public decimal Subtotal => SelectedSeats.Sum(s => s.Price);
    public decimal ConvenienceFee => SelectedSeats.Count > 0 ? 2.50m : 0m;
    public decimal Total => Subtotal + ConvenienceFee;
}
