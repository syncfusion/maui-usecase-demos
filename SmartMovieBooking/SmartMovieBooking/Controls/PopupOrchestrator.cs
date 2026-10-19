using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;
using Syncfusion.Maui.Popup;

namespace SmartMovieBooking.Controls;

/// <summary>
/// Owns the two popups (Seat Selection, Payment Confirmation) and wires
/// them to the in-app event bus so the Theaters page can trigger them.
/// </summary>
public class PopupOrchestrator
{
    private readonly SeatSelectionView _seatSelection;
    private readonly SeatSelectionViewModel _seatVm;
    private readonly PaymentConfirmationView _paymentConfirmation;
    private readonly PaymentConfirmationViewModel _paymentVm;
    private readonly BookingSession _session;
    private readonly MainWindowViewModel _main;

    private readonly PopupShell _seatPopup = new("Select Your Seats");
    private readonly PopupShell _paymentPopup = new("Booking Confirmed");

    public PopupOrchestrator(SeatSelectionView seatSelection,
                             SeatSelectionViewModel seatVm,
                             PaymentConfirmationView paymentConfirmation,
                             PaymentConfirmationViewModel paymentVm,
                             BookingSession session,
                             MainWindowViewModel main)
    {
        _seatSelection = seatSelection;
        _seatVm = seatVm;
        _paymentConfirmation = paymentConfirmation;
        _paymentVm = paymentVm;
        _session = session;
        _main = main;

        _seatPopup.SetContent(_seatSelection);
        _paymentPopup.SetContent(_paymentConfirmation);

        // Wire the bus.
        EventBus.ShowtimeClicked += OnShowtimeClicked;
        EventBus.TheaterContinueClicked += OnTheaterContinueClicked;
        EventBus.SeatSelectionConfirmed += OnSeatSelectionConfirmed;
        EventBus.SeatSelectionCancelled += OnSeatSelectionCancelled;
        EventBus.BookingConfirmed += OnBookingConfirmed;
        EventBus.BookingPopupClosed += OnBookingPopupClosed;
        EventBus.NavigateToBookings += OnNavigateToBookings;
    }

    public void AttachTo(Grid host)
    {
        // Stack the popups on top of the host. They start collapsed.
        host.Children.Add(_seatPopup.Popup);
        host.Children.Add(_paymentPopup.Popup);
    }

    private async void OnShowtimeClicked(object? sender, Models.Showtime e)
    {
        // Selection state is already set in the BookingSession by Theaters VM.
        await _seatVm.InitializeAsync();
        _seatPopup.Show();
    }

    private async void OnTheaterContinueClicked(object? sender, Models.Theater e)
    {
        // Use the most recently selected showtime for this theater; if none,
        // pick the first available one.
        var s = _session.SelectedShowtime;
        if (s is null || s.TheaterId != e.Id)
        {
            s = e.Showtimes.FirstOrDefault(x => x.Status != Models.ShowtimeStatus.SoldOut);
            if (s is null) return;
            _session.SelectedShowtime = s;
            _session.SelectedTheater = e;
        }

        await _seatVm.InitializeAsync();
        _seatPopup.Show();
    }

    private async void OnSeatSelectionConfirmed(object? sender, EventArgs e)
    {
        _seatPopup.Close();

        // Hand off to the payment popup.
        _paymentVm.PopulateFromSession();
        _paymentPopup.Show();
        await _paymentVm.ProcessAsync();
    }

    private void OnSeatSelectionCancelled(object? sender, EventArgs e)
    {
        _seatPopup.Close();
    }

    private void OnBookingConfirmed(object? sender, EventArgs e)
    {
        // Booking persisted; popup remains open showing the confirmation card.
    }

    private void OnBookingPopupClosed(object? sender, EventArgs e)
    {
        _paymentPopup.Close();
    }

    private void OnNavigateToBookings(object? sender, EventArgs e)
    {
        _main.NavigateToBookings();
    }
}