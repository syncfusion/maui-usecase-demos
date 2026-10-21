using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class SeatSelectionViewModel : ObservableObject
{
    private readonly ISeatLayoutFactory _factory;
    private readonly BookingSession _session;

    public SeatSelectionViewModel(ISeatLayoutFactory factory, BookingSession session)
    {
        _factory = factory;
        _session = session;
        Seats = new ObservableCollection<Seat>();
        SeatRows = new ObservableCollection<SeatRow>();
    }

    public ObservableCollection<Seat> Seats { get; }
    public ObservableCollection<SeatRow> SeatRows { get; } = new();

    [ObservableProperty] private string _screenTitle = "Select Your Seats";
    [ObservableProperty] private string _theaterName = string.Empty;
    [ObservableProperty] private string _showtimeLabel = string.Empty;
    [ObservableProperty] private string _showtimeStartTime = string.Empty;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _convenienceFee;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _canConfirm;

    public string FormattedSubtotal       => $"${Subtotal:F2}";
    public string FormattedConvenienceFee => $"${ConvenienceFee:F2}";
    public string FormattedTotal          => $"${Total:F2}";

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public async Task InitializeAsync()
    {
        if (_session.SelectedShowtime is null) return;

        TheaterName = _session.SelectedTheater?.Name ?? "";
        ShowtimeStartTime = _session.SelectedShowtime.StartTime;
        ShowtimeLabel = $"{_session.SelectedDate:ddd, MMM d} · {_session.SelectedShowtime.StartTime}";
        ScreenTitle = $"Auditorium · {_session.SelectedShowtime.StartTime}";

        var basePrice = _session.SelectedShowtime.Price;
        var seats = _factory.Build(_session.SelectedShowtime.Id, basePrice);
        Seats.Clear();
        foreach (var s in seats) Seats.Add(s);

        SeatRows.Clear();
        var rows = seats.Where(s => s.State != SeatState.Aisle)
            .GroupBy(s => s.Row)
            .OrderBy(g => g.Key);
        foreach (var rg in rows)
        {
            SeatRows.Add(new SeatRow
            {
                Label = rg.Key,
                Seats = rg.OrderBy(s => s.Column).ToList()
            });
        }
    }

    /// <summary>
    /// Public so the view code-behind can invoke it via the generated
    /// ToggleSeatCommand. The source generator strips the "On" prefix, so
    /// this method named "ToggleSeat" produces "ToggleSeatCommand".
    /// </summary>
    [RelayCommand]
    public void ToggleSeat(Seat seat)
    {
        if (seat is null || seat.State == SeatState.Occupied || seat.State == SeatState.Aisle) return;
        seat.IsSelected = !seat.IsSelected;
        seat.State = seat.IsSelected ? SeatState.Selected : SeatState.Available;

        if (seat.IsSelected) _session.SelectedSeats.Add(seat);
        else _session.SelectedSeats.Remove(seat);

        SelectedCount = _session.SelectedSeats.Count;
        Subtotal = _session.Subtotal;
        ConvenienceFee = _session.ConvenienceFee;
        Total = _session.Total;
        CanConfirm = SelectedCount > 0;
    }

    [RelayCommand]
    public void Confirm() => Confirmed?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    public void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}

public class SeatRow
{
    public string Label { get; set; } = string.Empty;
    public List<Seat> Seats { get; set; } = new();
}
