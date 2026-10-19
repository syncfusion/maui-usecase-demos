using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetAsync(string movieId, string theaterId, DateTime date);
}

public sealed class ShowtimeService : IShowtimeService
{
    public Task<IReadOnlyList<Showtime>> GetAsync(string movieId, string theaterId, DateTime date)
    {
        var list = AppSeed.GetShowtimes(movieId, theaterId, date);
        return Task.FromResult<IReadOnlyList<Showtime>>(list);
    }
}
