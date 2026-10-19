using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

/// <summary>
/// Lightweight static event bus for forwarding virtualized child events
/// (like tapped showtime pills inside a BindableLayout) up to the page-level
/// ViewModel. Sample-only — production code should use a proper messenger.
/// </summary>
public static class EventBus
{
    public static event EventHandler<Showtime>? ShowtimeClicked;
    public static event EventHandler<Theater>? TheaterContinueClicked;
    public static event EventHandler<Seat>? SeatToggled;
    public static event EventHandler? SeatSelectionConfirmed;
    public static event EventHandler? SeatSelectionCancelled;
    public static event EventHandler? BookingConfirmed;
    public static event EventHandler? BookingPopupClosed;
    public static event EventHandler? NavigateToBookings;

    public static void PublishShowtimeClicked(Showtime s)
        => ShowtimeClicked?.Invoke(null, s);

    public static void PublishTheaterContinueClicked(Theater t)
        => TheaterContinueClicked?.Invoke(null, t);

    public static void PublishSeatToggled(Seat s)
        => SeatToggled?.Invoke(null, s);

    public static void PublishSeatSelectionConfirmed()
        => SeatSelectionConfirmed?.Invoke(null, EventArgs.Empty);

    public static void PublishSeatSelectionCancelled()
        => SeatSelectionCancelled?.Invoke(null, EventArgs.Empty);

    public static void PublishBookingConfirmed()
        => BookingConfirmed?.Invoke(null, EventArgs.Empty);

    public static void PublishBookingPopupClosed()
        => BookingPopupClosed?.Invoke(null, EventArgs.Empty);

    public static void PublishNavigateToBookings()
        => NavigateToBookings?.Invoke(null, EventArgs.Empty);
}