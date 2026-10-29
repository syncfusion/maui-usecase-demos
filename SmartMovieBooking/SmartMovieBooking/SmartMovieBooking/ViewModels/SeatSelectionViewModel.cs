using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class SeatSelectionViewModel : ObservableObject
{
    private readonly BookingSession _session;
    private readonly ISeatLayoutFactory _seatLayoutFactory;
    private readonly IBookingRepository _repo;

    public SeatSelectionViewModel(BookingSession session, ISeatLayoutFactory seatLayoutFactory, IBookingRepository repo)
    {
        _session = session;
        _seatLayoutFactory = seatLayoutFactory;
        _repo = repo;
        SeatRows = new ObservableCollection<SeatRowViewModel>();
    }

    [ObservableProperty] private string _screenTitle = "Select Seats";
    [ObservableProperty] private string _theaterName = string.Empty;
    [ObservableProperty] private string _showtimeLabel = string.Empty;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _convenienceFee;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _canConfirm;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private bool _isConfirmationVisible;
    [ObservableProperty] private string _bookingId = "BK-PENDING";
    [ObservableProperty] private string _movieTitle = string.Empty;
    [ObservableProperty] private string _seatsDisplay = string.Empty;

    public ObservableCollection<SeatRowViewModel> SeatRows { get; }
    public ObservableCollection<Seat> SelectedSeats { get; } = new();

    public string FormattedSubtotal => $"${Subtotal:F2}";
    public string FormattedConvenienceFee => $"${ConvenienceFee:F2}";
    public string FormattedTotal => $"${Total:F2}";
    public string SelectedSeatSummary => SelectedSeats.Count == 0
        ? "0 seats selected"
        : $"{SelectedSeats.Count} seat{(SelectedSeats.Count == 1 ? string.Empty : "s")} selected";

    public Task InitializeAsync()
    {
        if (_session.SelectedShowtime is null) return Task.CompletedTask;

        MovieTitle = _session.SelectedMovie?.Title ?? string.Empty;
        TheaterName = _session.SelectedTheater?.Name ?? string.Empty;
        ShowtimeLabel = $"{_session.SelectedDate:ddd, MMM d} · {_session.SelectedShowtime.StartTime}";
        ScreenTitle = $"{TheaterName} Seats";

        var seats = _seatLayoutFactory.Build(_session.SelectedShowtime.Id, _session.SelectedShowtime.Price)
            .Where(s => s.State != SeatState.Aisle)
            .ToList();

        SeatRows.Clear();
        SelectedSeats.Clear();
        _session.SelectedSeats.Clear();

        foreach (var row in seats.GroupBy(s => s.Row))
            SeatRows.Add(new SeatRowViewModel(row.Key, new ObservableCollection<Seat>(row)));

        Recalculate();
        return Task.CompletedTask;
    }

    public void ToggleSeat(Seat seat)
    {
        if (seat.State != SeatState.Available && seat.State != SeatState.Selected) return;

        if (seat.State == SeatState.Selected)
        {
            seat.State = SeatState.Available;
            seat.IsSelected = false;
            SelectedSeats.Remove(seat);
            _session.SelectedSeats.Remove(seat);
        }
        else
        {
            seat.State = SeatState.Selected;
            seat.IsSelected = true;
            SelectedSeats.Add(seat);
            if (!_session.SelectedSeats.Contains(seat))
                _session.SelectedSeats.Add(seat);
        }

        Recalculate();
        OnPropertyChanged(nameof(SelectedSeatSummary));
    }

    private void Recalculate()
    {
        Subtotal = SelectedSeats.Sum(s => s.Price);
        ConvenienceFee = SelectedSeats.Count > 0 ? 2.50m : 0m;
        Total = Subtotal + ConvenienceFee;
        CanConfirm = SelectedSeats.Count > 0;
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedConvenienceFee));
        OnPropertyChanged(nameof(FormattedTotal));
    }

    [RelayCommand]
    public async Task ConfirmAsync()
    {
        if (SelectedSeats.Count == 0)
            return;

        IsProcessing = true;
        try
        {
            await Task.Delay(1500);

            BookingId = $"BK-{Random.Shared.Next(100000, 999999)}";
            SeatsDisplay = string.Join(", ", SelectedSeats.OrderBy(s => s.Id).Select(s => s.Id));

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

            foreach (var s in _session.SelectedSeats)
                booking.Seats.Add(s);

            _repo.Add(booking);
            _session.Reset();
            IsConfirmationVisible = true;
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    public void CloseConfirmation() => IsConfirmationVisible = false;

}

public sealed class SeatRowViewModel
{
    public SeatRowViewModel(string rowLabel, ObservableCollection<Seat> seats)
    {
        RowLabel = rowLabel;
        Seats = seats;
    }

    public string RowLabel { get; }
    public ObservableCollection<Seat> Seats { get; }
}