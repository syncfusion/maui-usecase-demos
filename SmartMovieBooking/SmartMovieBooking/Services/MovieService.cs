using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IMovieService
{
    Task<IReadOnlyList<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(string id);
    Task<IReadOnlyList<Movie>> SearchAsync(string query, string? genre = null);
    Task<IReadOnlyList<string>> GetGenresAsync();
}

public sealed class MovieService : IMovieService
{
    public Task<IReadOnlyList<Movie>> GetAllAsync() => Task.FromResult(AppSeed.Movies);

    public Task<Movie?> GetByIdAsync(string id) =>
        Task.FromResult(AppSeed.Movies.FirstOrDefault(m => m.Id == id));

    public Task<IReadOnlyList<Movie>> SearchAsync(string query, string? genre = null)
    {
        IEnumerable<Movie> result = AppSeed.Movies;
        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            result = result.Where(m =>
                m.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                m.PrimaryGenre.Contains(q, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(genre) && genre != "All Genres")
            result = result.Where(m => m.Genres.Contains(genre, StringComparer.OrdinalIgnoreCase));
        return Task.FromResult<IReadOnlyList<Movie>>(result.ToList());
    }

    public Task<IReadOnlyList<string>> GetGenresAsync() =>
        Task.FromResult<IReadOnlyList<string>>(AppSeed.Genres.ToList());
}
