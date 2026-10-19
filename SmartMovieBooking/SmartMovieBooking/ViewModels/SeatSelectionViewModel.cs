using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

/// <summary>
/// Owns the in-progress seat-picking session. Reads the active
/// Showtime from <see cref="BookingSession"/>, builds a 14×14 seat
/// layout, and tracks the user's selection.
/// </summary>
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

    /// <summary>All 14×14 seats (including aisle slots), in row-major order.</summary>
    public ObservableCollection<Seat> Seats { get; }

    /// <summary>Grouped into 14 rows for the grid template.</summary>
    public ObservableCollection<SeatRow> SeatRows { get; }

    [ObservableProperty] private string _screenTitle = "Select Your Seats";
    [ObservableProperty] private string _theaterLine = string.Empty;
    [ObservableProperty] private string _showtimeLine = string.Empty;
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
        if (_session.SelectedShowtime is null || _session.SelectedTheater is null) return;

        TheaterLine = _session.SelectedTheater.Name;
        ShowtimeLine = $"{_session.SelectedShowtime.Format} · {_session.SelectedShowtime.StartTime}";

        var basePrice = _session.SelectedShowtime.Price;
        var seats = _factory.Build(_session.SelectedShowtime.Id, basePrice);

        Seats.Clear();
        foreach (var s in seats) Seats.Add(s);

        SeatRows.Clear();
        var rowGroups = seats.GroupBy(s => s.Row).OrderBy(g => g.Key);
        foreach (var g in rowGroups)
        {
            var row = new SeatRow { RowLabel = g.Key };
            foreach (var s in g.OrderBy(x => x.Column)) row.Seats.Add(s);
            SeatRows.Add(row);
        }

        Recompute();
    }

    [RelayCommand]
    private void ToggleSeat(Seat? seat)
    {
        if (seat is null) return;
        if (seat.State == SeatState.Occupied || seat.State == SeatState.Aisle) return;

        if (seat.IsSelected)
        {
            seat.IsSelected = false;
            seat.State = SeatState.Available;
            _session.SelectedSeats.Remove(seat);
        }
        else
        {
            // Limit to 10 seats per booking.
            if (_session.SelectedSeats.Count >= 10) return;
            seat.IsSelected = true;
            seat.State = SeatState.Selected;
            _session.SelectedSeats.Add(seat);
        }
        Recompute();
    }

    [RelayCommand]
    private void Confirm()
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        // Clear partial selection.
        foreach (var s in _session.SelectedSeats.ToList())
        {
            s.IsSelected = false;
            s.State = SeatState.Available;
        }
        _session.SelectedSeats.Clear();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void Recompute()
    {
        SelectedCount = _session.SelectedSeats.Count;
        Subtotal = _session.SelectedSeats.Sum(s => s.Price);
        ConvenienceFee = SelectedCount > 0 ? 2.50m : 0m;
        Total = Subtotal + ConvenienceFee;
        CanConfirm = SelectedCount > 0;

        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedConvenienceFee));
        OnPropertyChanged(nameof(FormattedTotal));
    }
}

public partial class SeatRow : ObservableObject
{
    public string RowLabel { get; set; } = string.Empty;
    public ObservableCollection<Seat> Seats { get; } = new();
}