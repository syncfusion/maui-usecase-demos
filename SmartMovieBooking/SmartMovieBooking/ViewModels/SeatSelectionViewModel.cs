using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class SeatSelectionViewModel : ObservableObject
{
    private readonly BookingSession _session;

    public SeatSelectionViewModel(BookingSession session)
    {
        _session = session;
    }

    [ObservableProperty] private string _screenTitle = "Select Tickets";
    [ObservableProperty] private string _theaterName = string.Empty;
    [ObservableProperty] private string _showtimeLabel = string.Empty;
    [ObservableProperty] private string _showtimeStartTime = string.Empty;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private decimal _subtotal;
    [ObservableProperty] private decimal _convenienceFee;
    [ObservableProperty] private decimal _total;
    [ObservableProperty] private bool _canConfirm;

    public string FormattedSubtotal => $"${Subtotal:F2}";
    public string FormattedConvenienceFee => $"${ConvenienceFee:F2}";
    public string FormattedTotal => $"${Total:F2}";

    public event EventHandler? Confirmed;
    public event EventHandler? Cancelled;

    public Task InitializeAsync()
    {
        if (_session.SelectedShowtime is null) return Task.CompletedTask;

        TheaterName = _session.SelectedTheater?.Name ?? string.Empty;
        ShowtimeStartTime = _session.SelectedShowtime.StartTime;
        ShowtimeLabel = $"{_session.SelectedDate:ddd, MMM d} · {_session.SelectedShowtime.StartTime}";
        ScreenTitle = $"Tickets · {_session.SelectedShowtime.StartTime}";
        SelectedCount = Math.Max(1, _session.SelectedTicketCount);
        Recalculate();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Increase()
    {
        SelectedCount++;
        Recalculate();
    }

    [RelayCommand]
    private void Decrease()
    {
        if (SelectedCount <= 1) return;
        SelectedCount--;
        Recalculate();
    }

    private void Recalculate()
    {
        _session.SelectedTicketCount = SelectedCount;
        Subtotal = _session.Subtotal;
        ConvenienceFee = _session.ConvenienceFee;
        Total = _session.Total;
        CanConfirm = SelectedCount > 0;
        OnPropertyChanged(nameof(FormattedSubtotal));
        OnPropertyChanged(nameof(FormattedConvenienceFee));
        OnPropertyChanged(nameof(FormattedTotal));
    }

    [RelayCommand]
    public void Confirm() => Confirmed?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    public void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);
}