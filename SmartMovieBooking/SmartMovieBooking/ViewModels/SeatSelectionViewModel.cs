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
        FrontRows = new ObservableCollection<SeatRowGroup>();
        PrimeRows = new ObservableCollection<SeatRowGroup>();
        UpperRows = new ObservableCollection<SeatRowGroup>();

        EventBus.SeatToggled += OnSeatToggled;
    }

    /// <summary>Raw seats in row-major order (kept for inspection).</summary>
    public ObservableCollection<Seat> Seats { get; }

    /// <summary>Grouped into 14 rows (Front A-C, Prime D-F, Upper G-N).</summary>
    public ObservableCollection<SeatRowGroup> FrontRows { get; }
    public ObservableCollection<SeatRowGroup> PrimeRows { get; }
    public ObservableCollection<SeatRowGroup> UpperRows { get; }

    [ObservableProperty] private string _screenTitle = "Auditorium 4 · IMAX";
    [ObservableProperty] private string _theaterLine = string.Empty;
    [ObservableProperty] private string _showtimeLine = "Today, Oct 24";
    [ObservableProperty] private string _showtimeStartTime = "07:15 PM";
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _convenienceFee;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _canConfirm;

    public string FormattedSubtotal       => $"${Subtotal:F2}";
    public string FormattedConvenienceFee => $"${ConvenienceFee:F2}";
    public string FormattedTotal          => $"${Total:F2}";

    public string SelectedSeatsSummary
    {
        get
        {
            if (_session.SelectedSeats.Count == 0) return "—";
            var row = _session.SelectedSeats.First().Row;
            var seats = string.Join(", ", _session.SelectedSeats
                .Where(s => s.Row == row).OrderBy(s => s.Column).Select(s => s.Id.Substring(1)));
            return $"Row {row} ({row}{seats}) · {_session.SelectedSeats.Count} Ticket{(_session.SelectedSeats.Count == 1 ? "" : "s")}";
        }
    }

    public string PriceSummaryLine
    {
        get
        {
            if (_session.SelectedSeats.Count == 0) return "—";
            var subtotal = _session.SelectedSeats.Sum(s => s.Price);
            var fee = ConvenienceFee;
            return $"{_session.SelectedSeats.Count} × ${subtotal / _session.SelectedSeats.Count:F2} = ${subtotal:F2} (+ ${fee:F2} fees)";
        }
    }

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public async Task InitializeAsync()
    {
        if (_session.SelectedShowtime is null || _session.SelectedTheater is null) return;

        var basePrice = _session.SelectedShowtime.Price;
        var seats = _factory.Build(_session.SelectedShowtime.Id, basePrice);
        Seats.Clear();
        foreach (var s in seats) Seats.Add(s);

        // Rebuild row groups.
        FrontRows.Clear();
        PrimeRows.Clear();
        UpperRows.Clear();

        var byRow = seats
            .Where(s => s.State != SeatState.Aisle)
            .GroupBy(s => s.Row)
            .OrderBy(g => g.Key);

        foreach (var group in byRow)
        {
            var rowGroup = new SeatRowGroup
            {
                Label = group.Key,
                Seats = new ObservableCollection<Seat>(group.OrderBy(s => s.Column)),
                IsPrime = group.Key is "D" or "E" or "F"
            };

            if (group.Key is "A" or "B" or "C")
                FrontRows.Add(rowGroup);
            else if (group.Key is "D" or "E" or "F")
                PrimeRows.Add(rowGroup);
            else
                UpperRows.Add(rowGroup);
        }

        // Headers
        var theater = _session.SelectedTheater;
        TheaterLine = $"{theater.Name} · Auditorium 4 (Prime Floor)";
        ScreenTitle = _session.SelectedShowtime.Format is null
            ? theater.Name
            : $"{theater.Name}";

        ShowtimeStartTime = _session.SelectedShowtime.StartTime;

        // Reset session selection (start fresh).
        _session.SelectedSeats.Clear();
        Recompute();
    }

    private void OnSeatToggled(object? sender, Seat seat)
    {
        if (seat.State == SeatState.Occupied || seat.State == SeatState.Aisle) return;
        if (seat.IsSelected)
        {
            if (_session.SelectedSeats.Count >= 10) return;
            seat.IsSelected = true;
            seat.State = SeatState.Selected;
            _session.SelectedSeats.Add(seat);
        }
        else
        {
            seat.IsSelected = false;
            seat.State = SeatState.Available;
            _session.SelectedSeats.Remove(seat);
        }
        Recompute();
    }

    private void Recompute()
    {
        Subtotal = _session.SelectedSeats.Sum(s => s.Price);
        SelectedCount = _session.SelectedSeats.Count;
        ConvenienceFee = SelectedCount > 0 ? 3.50m : 0m;
        Total = Subtotal + ConvenienceFee;
        CanConfirm = SelectedCount > 0;

        OnPropertyChanged(nameof(SelectedSeatsSummary));
        OnPropertyChanged(nameof(PriceSummaryLine));
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedConvenienceFee));
        OnPropertyChanged(nameof(FormattedTotal));
    }

    [RelayCommand]
    private void Confirm() => Confirmed?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}