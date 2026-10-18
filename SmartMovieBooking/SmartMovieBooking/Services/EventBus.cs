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

    public static void PublishShowtimeClicked(Showtime s)
        => ShowtimeClicked?.Invoke(null, s);
}