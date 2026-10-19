using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface ITheaterService
{
    Task<IReadOnlyList<Theater>> GetAllAsync();
    Task<Theater?> GetByIdAsync(string id);
    Task<IReadOnlyList<Theater>> GetTheatersForMovieAsync(string movieId);
}

public sealed class TheaterService : ITheaterService
{
    public Task<IReadOnlyList<Theater>> GetAllAsync() => Task.FromResult(AppSeed.Theaters);

    public Task<Theater?> GetByIdAsync(string id) =>
        Task.FromResult(AppSeed.Theaters.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<Theater>> GetTheatersForMovieAsync(string movieId) =>
        Task.FromResult(AppSeed.Theaters);
}
