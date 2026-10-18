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
    }

    public ObservableCollection<Seat> Seats { get; }

    [ObservableProperty] private string _screenTitle = "Select Your Seats";
    [ObservableProperty] private string _subTitle = "Tap an available seat to add it to your booking.";
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _canConfirm;

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public async Task InitializeAsync()
    {
        if (_session.SelectedShowtime is null) return;
        var basePrice = _session.SelectedShowtime.Price;
        var seats = _factory.Build(_session.SelectedShowtime.Id, basePrice);
        Seats.Clear();
        foreach (var s in seats) Seats.Add(s);

        if (_session.SelectedShowtime is not null)
        {
            ScreenTitle = $"{_session.SelectedTheater?.Name} · {_session.SelectedShowtime.StartTime}";
        }
    }

    [RelayCommand]
    private void ToggleSeat(Seat seat)
    {
        if (seat is null || seat.State == SeatState.Occupied || seat.State == SeatState.Aisle) return;

        seat.IsSelected = !seat.IsSelected;
        seat.State = seat.IsSelected ? SeatState.Selected : SeatState.Available;
        Recompute();
    }

    private void Recompute()
    {
        SelectedCount = Seats.Count(s => s.IsSelected);
        Subtotal = Seats.Where(s => s.IsSelected).Sum(s => s.Price);
        Total = Subtotal + (SelectedCount > 0 ? 2.5m : 0m);
        CanConfirm = SelectedCount > 0;

        // Sync to BookingSession
        _session.SelectedSeats.Clear();
        foreach (var s in Seats.Where(x => x.IsSelected)) _session.SelectedSeats.Add(s);
    }

    [RelayCommand]
    private void Confirm() => Confirmed?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}