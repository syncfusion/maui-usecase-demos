using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetAsync(string movieId, string theaterId, DateTime date);
}

public sealed class ShowtimeService : IShowtimeService
{
    public Task<IReadOnlyList<Showtime>> GetAsync(string movieId, string theaterId, DateTime date)
        => Task.FromResult<IReadOnlyList<Showtime>>(AppSeed.GetShowtimes(movieId, theaterId, date));
}
