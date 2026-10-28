using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class PaymentConfirmationViewModel : ObservableObject
{
    private readonly BookingSession _session;
    private readonly IBookingRepository _repo;

    public PaymentConfirmationViewModel(BookingSession session, IBookingRepository repo)
    {
        _session = session;
        _repo = repo;
    }

    [ObservableProperty] private string _bookingId = "BK-PENDING";
    [ObservableProperty] private string _movieTitle = string.Empty;
    [ObservableProperty] private string _theaterName = string.Empty;
    [ObservableProperty] private string _showtimeLabel = string.Empty;
    [ObservableProperty] private string _seatsDisplay = string.Empty;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _isProcessing;

    public string FormattedTotal => $"${Total:F2}";

    public event EventHandler? Completed;

    public void PopulateFromSession()
    {
        if (_session.SelectedMovie is not null)
            MovieTitle = _session.SelectedMovie.Title;
        if (_session.SelectedTheater is not null)
            TheaterName = _session.SelectedTheater.Name;
        if (_session.SelectedShowtime is not null)
            ShowtimeLabel = $"{_session.SelectedDate:ddd, MMM d} · {_session.SelectedShowtime.StartTime}";
        SeatsDisplay = string.Join(", ", _session.SelectedSeats.OrderBy(s => s.Id).Select(s => s.Id));
        Total = _session.Total;
        BookingId = $"BK-{Random.Shared.Next(100000, 999999)}";
        OnPropertyChanged(nameof(FormattedTotal));
    }

    [RelayCommand]
    public async Task ProcessAsync()
    {
        IsProcessing = true;
        try
        {
            await Task.Delay(1500); // simulated processing

            var booking = new Booking
            {
                BookingId = BookingId,
                CreatedAt = DateTime.Now,
                Movie = _session.SelectedMovie,
                Theater = _session.SelectedTheater,
                Showtime = _session.SelectedShowtime,
                Subtotal = _session.Subtotal,
                ConvenienceFee = _session.ConvenienceFee,
                Total = _session.Total,
                PaymentMethod = "Card",
                Status = "Confirmed"
            };
            foreach (var s in _session.SelectedSeats) booking.Seats.Add(s);

            _repo.Add(booking);
            _session.Reset();

            Completed?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
