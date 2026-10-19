using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;

namespace SmartMovieBooking.Controls;

/// <summary>
/// Orchestrates the popup-style navigation. On mobile, sheets are pushed as
/// Shell modal routes (the routes use full-screen pages styled as bottom sheets
/// with rounded top corners and a drag handle). On Windows desktop, the same
/// routes render as centered modals.
/// </summary>
public class PopupOrchestrator
{
    private readonly SeatSelectionViewModel _seatVm;
    private readonly PaymentConfirmationViewModel _paymentVm;
    private readonly BookingSession _session;

    public PopupOrchestrator(SeatSelectionViewModel seatVm,
                             PaymentConfirmationViewModel paymentVm,
                             BookingSession session)
    {
        _seatVm = seatVm;
        _paymentVm = paymentVm;
        _session = session;

        // Wire the bus.
        EventBus.ShowtimeClicked += OnShowtimeClicked;
        EventBus.TheaterContinueClicked += OnTheaterContinueClicked;
        EventBus.SeatSelectionConfirmed += OnSeatSelectionConfirmed;
        EventBus.SeatSelectionCancelled += OnSeatSelectionCancelled;
        EventBus.BookingConfirmed += OnBookingConfirmed;
        EventBus.BookingPopupClosed += OnBookingPopupClosed;
        EventBus.NavigateToBookings += OnNavigateToBookings;
    }

    /// <summary>No-op for Shell-based popups. Kept for API symmetry.</summary>
    public void AttachTo(Grid host) { }

    private async void OnShowtimeClicked(object? sender, Showtime e)
    {
        await _seatVm.InitializeAsync();
        await Shell.Current.GoToAsync("seat-selection");
    }

    private async void OnTheaterContinueClicked(object? sender, Theater e)
    {
        var s = _session.SelectedShowtime;
        if (s is null || s.TheaterId != e.Id)
        {
            s = e.Showtimes.FirstOrDefault(x => x.Status != ShowtimeStatus.SoldOut);
            if (s is null) return;
            _session.SelectedShowtime = s;
            _session.SelectedTheater = e;
        }

        await _seatVm.InitializeAsync();
        await Shell.Current.GoToAsync("seat-selection");
    }

    private async void OnSeatSelectionConfirmed(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("payment-confirmation");
        _paymentVm.PopulateFromSession();
        await _paymentVm.ProcessAsync();
    }

    private async void OnSeatSelectionCancelled(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("..");

    private void OnBookingConfirmed(object? sender, EventArgs e) { }

    private async void OnBookingPopupClosed(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OnNavigateToBookings(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//bookings");
}