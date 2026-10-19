using SmartMovieBooking.Models;

namespace SmartMovieBooking;

/// <summary>
/// Static catalog of movies, theaters, and a deterministic showtime
/// generator. All data is mock — no network, no JSON.
/// </summary>
public static class AppSeed
{
    public static IReadOnlyList<Movie> Movies { get; } = BuildMovies();
    public static IReadOnlyList<Theater> Theaters { get; } = BuildTheaters();

    public static IEnumerable<string> Genres =>
        Movies.SelectMany(m => m.Genres).Distinct().OrderBy(g => g);

    public static IEnumerable<string> Languages =>
        Movies.SelectMany(m => m.Languages).Distinct().OrderBy(l => l);

    /// <summary>Deterministic showtimes per (movie, theater, date).</summary>
    public static IReadOnlyList<Showtime> GetShowtimes(string movieId, string theaterId, DateTime date)
    {
        var rng = new Random(HashCode.Combine(movieId, theaterId, date.DayOfYear));
        var templates = new (string Start, string Format, decimal Price, ShowtimeStatus Status)[]
        {
            ("10:30 AM", "IMAX Laser",   24m,   ShowtimeStatus.Available),
            ("01:15 PM", "Dolby Cinema", 22m,   ShowtimeStatus.Available),
            ("04:00 PM", "Standard 2D",  16m,   ShowtimeStatus.AlmostFull),
            ("07:15 PM", "IMAX Laser",   26.5m, ShowtimeStatus.SellingFast),
            ("10:00 PM", "IMAX Laser",   24m,   ShowtimeStatus.Available),
            ("11:30 PM", "Late Night 2D", 18.5m, ShowtimeStatus.Available)
        };

        var pick = templates.OrderBy(_ => rng.Next()).Take(4 + rng.Next(0, 2)).ToList();
        var list = new List<Showtime>();
        foreach (var (start, fmt, price, status) in pick)
        {
            list.Add(new Showtime
            {
                Id = $"ss-{movieId}-{theaterId}-{date:yyyyMMdd}-{start.Replace(":", "").Replace(" ", "")}",
                MovieId = movieId,
                TheaterId = theaterId,
                Date = date,
                StartTime = start,
                EndTime = ComputeEndTime(start),
                Format = fmt,
                Price = price,
                AvailableSeats = 30 + rng.Next(80),
                Status = status,
                IsPrimeTime = start == "07:15 PM"
            });
        }
        return list;
    }

    private static string ComputeEndTime(string start)
    {
        var parts = start.Split(' ');
        var time = parts[0].Split(':');
        var hour = int.Parse(time[0]) + 2;
        var ampm = parts[1];
        if (hour > 12) { hour -= 12; ampm = ampm == "AM" ? "PM" : "AM"; }
        return $"{hour:D2}:{time[1]} {ampm}";
    }

    private static List<Movie> BuildMovies()
    {
        return new List<Movie>
        {
            new()
            {
                Id = "nebula-horizon",
                Title = "Chronicles of Nebula: Horizon",
                Tagline = "Across the void, beyond time.",
                Synopsis = "An interstellar crew discovers a signal from the edge of the universe — and the past they tried to leave behind.",
                PrimaryGenre = "Sci-Fi",
                Genres = new() { "Sci-Fi", "Adventure", "Drama" },
                Languages = new() { "English", "Hindi", "Tamil" },
                Format = "IMAX 3D",
                Rating = "9.2",
                Duration = "2h 45m",
                ReleaseDate = "Oct 24",
                Certificate = "UA",
                PriceRange = "$18.50 - $26.50",
                IsNowShowing = true,
                Description = "A breathtaking sci-fi epic that takes audiences on a journey through space and time.",
                Cast = new()
                {
                    new() { Name = "Marcus Chen", Character = "Capt. Aris Vey", AvatarUrl = "" },
                    new() { Name = "Elena Rodriguez", Character = "Dr. Lira Osei", AvatarUrl = "" },
                    new() { Name = "Takeshi Yamamoto", Character = "Cmdr. Hiro Tanaka", AvatarUrl = "" }
                }
            },
            new()
            {
                Id = "midnight-protocol",
                Title = "Midnight Protocol",
                Tagline = "Trust no one. Remember everything.",
                Synopsis = "A retired spy wakes up with no memory and a list of names — including her own.",
                PrimaryGenre = "Thriller",
                Genres = new() { "Thriller", "Action", "Mystery" },
                Languages = new() { "English" },
                Format = "Dolby Cinema",
                Rating = "8.7",
                Duration = "2h 10m",
                ReleaseDate = "Oct 17",
                Certificate = "A",
                PriceRange = "$16.00 - $22.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "echoes-of-aurora",
                Title = "Echoes of Aurora",
                Tagline = "Some songs remember what we forget.",
                Synopsis = "A street musician's composition unlocks memories of a city that no longer exists.",
                PrimaryGenre = "Drama",
                Genres = new() { "Drama", "Music", "Fantasy" },
                Languages = new() { "English", "Spanish" },
                Format = "Standard 2D",
                Rating = "8.4",
                Duration = "1h 55m",
                ReleaseDate = "Oct 10",
                Certificate = "UA",
                PriceRange = "$14.00 - $18.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "iron-summit",
                Title = "Iron Summit",
                Tagline = "The mountain does not forgive.",
                Synopsis = "Two estranged brothers must climb the world's deadliest peak to save their father.",
                PrimaryGenre = "Action",
                Genres = new() { "Action", "Adventure", "Drama" },
                Languages = new() { "English", "French" },
                Format = "IMAX Laser",
                Rating = "8.1",
                Duration = "2h 25m",
                ReleaseDate = "Oct 3",
                Certificate = "UA",
                PriceRange = "$18.00 - $24.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "paper-flowers",
                Title = "Paper Flowers",
                Tagline = "The garden remembers.",
                Synopsis = "An elderly botanist discovers a flower that blooms only at midnight — and only for the lonely.",
                PrimaryGenre = "Drama",
                Genres = new() { "Drama", "Romance" },
                Languages = new() { "English", "Japanese" },
                Format = "Standard 2D",
                Rating = "7.9",
                Duration = "1h 48m",
                ReleaseDate = "Sep 26",
                Certificate = "U",
                PriceRange = "$12.00 - $16.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "neon-blade",
                Title = "Neon Blade",
                Tagline = "The future runs on blood and circuitry.",
                Synopsis = "In a megacity ruled by AI, the last human detective hunts a killer that doesn't exist.",
                PrimaryGenre = "Sci-Fi",
                Genres = new() { "Sci-Fi", "Thriller", "Action" },
                Languages = new() { "English" },
                Format = "4DX",
                Rating = "8.5",
                Duration = "2h 05m",
                ReleaseDate = "Sep 19",
                Certificate = "A",
                PriceRange = "$22.00 - $30.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "family-table",
                Title = "The Family Table",
                Tagline = "Three generations. One recipe. Endless stories.",
                Synopsis = "When grandma decides to pass down her legendary kitchen, every Sunday becomes a memory.",
                PrimaryGenre = "Comedy",
                Genres = new() { "Comedy", "Drama", "Family" },
                Languages = new() { "English", "Spanish" },
                Format = "Standard 2D",
                Rating = "8.0",
                Duration = "1h 42m",
                ReleaseDate = "Sep 12",
                Certificate = "U",
                PriceRange = "$12.00 - $16.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "shadow-court",
                Title = "Shadow Court",
                Tagline = "Justice has a price. He paid in full.",
                Synopsis = "A lawyer takes on a case that could expose the city's darkest secret — or cost him everything.",
                PrimaryGenre = "Thriller",
                Genres = new() { "Thriller", "Drama" },
                Languages = new() { "English" },
                Format = "Dolby Cinema",
                Rating = "8.6",
                Duration = "2h 18m",
                ReleaseDate = "Sep 5",
                Certificate = "A",
                PriceRange = "$16.00 - $22.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "cartographers",
                Title = "The Cartographers",
                Tagline = "Every map hides a secret.",
                Synopsis = "A young cartographer inherits a map that draws itself — and the places it shows aren't supposed to exist.",
                PrimaryGenre = "Adventure",
                Genres = new() { "Adventure", "Fantasy", "Mystery" },
                Languages = new() { "English", "Portuguese" },
                Format = "Standard 2D",
                Rating = "7.8",
                Duration = "2h 02m",
                ReleaseDate = "Aug 29",
                Certificate = "UA",
                PriceRange = "$14.00 - $18.00",
                IsNowShowing = true
            },
            new()
            {
                Id = "lullaby-engine",
                Title = "Lullaby Engine",
                Tagline = "Sing the song. Forget the words.",
                Synopsis = "A composer builds a machine that sings your forgotten memories back to you.",
                PrimaryGenre = "Sci-Fi",
                Genres = new() { "Sci-Fi", "Drama", "Music" },
                Languages = new() { "English", "Korean" },
                Format = "Dolby Cinema",
                Rating = "8.3",
                Duration = "1h 58m",
                ReleaseDate = "Nov 14",
                Certificate = "UA",
                PriceRange = "$16.00 - $22.00",
                IsNowShowing = false
            }
        };
    }

    private static List<Theater> BuildTheaters()
    {
        return new List<Theater>
        {
            new()
            {
                Id = "amc-empire-25",
                Name = "AMC Empire 25",
                Location = "234 W 42nd St, Times Square",
                City = "New York, NY",
                DistanceKm = 0.5,
                DistanceLabel = "0.3 mi away",
                PriceFrom = "$22.00",
                AvailableSeats = 124,
                Badge = "Dolby Certified",
                Icon = "🎬",
                Description = "Flagship IMAX theater with reserved seating and 4K laser projection.",
                Facilities = new()
                {
                    new() { Label = "IMAX Laser", Icon = "🎥", IsProminent = true },
                    new() { Label = "Dolby Atmos", Icon = "🔊", IsProminent = true },
                    new() { Label = "Recliner", Icon = "💺", IsProminent = false },
                    new() { Label = "Reserved Parking", Icon = "🅿️", IsProminent = false }
                }
            },
            new()
            {
                Id = "regal-ewalk-4dx",
                Name = "Regal E-Walk 4DX & RPX",
                Location = "247 W 42nd St, Times Square",
                City = "New York, NY",
                DistanceKm = 0.9,
                DistanceLabel = "0.9 mi away",
                PriceFrom = "$17.00",
                AvailableSeats = 98,
                Badge = "Motion Immersion",
                Icon = "🎟",
                Description = "4DX motion seats and RPX laser projection for a fully immersive experience.",
                Facilities = new()
                {
                    new() { Label = "4DX Motion", Icon = "💫", IsProminent = true },
                    new() { Label = "RPX Laser", Icon = "📽", IsProminent = true },
                    new() { Label = "Dolby Atmos", Icon = "🔊", IsProminent = false },
                    new() { Label = "Reserved Parking", Icon = "🅿️", IsProminent = false }
                }
            },
            new()
            {
                Id = "amc-lincoln-square",
                Name = "AMC Lincoln Square 13",
                Location = "1998 Broadway, Upper West Side",
                City = "New York, NY",
                DistanceKm = 3.4,
                DistanceLabel = "3.4 mi away",
                PriceFrom = "$26.00",
                AvailableSeats = 156,
                Badge = "IMAX 70mm",
                Icon = "🎞",
                Description = "Home of the legendary IMAX 70mm projector — the gold standard for film purists.",
                Facilities = new()
                {
                    new() { Label = "IMAX 70mm", Icon = "🎬", IsProminent = true },
                    new() { Label = "Sony 4K", Icon = "📺", IsProminent = true },
                    new() { Label = "Heated Seats", Icon = "🔥", IsProminent = false },
                    new() { Label = "Cafe", Icon = "☕", IsProminent = false }
                }
            },
            new()
            {
                Id = "alamo-drafthouse",
                Name = "Alamo Drafthouse Downtown",
                Location = "28 Liberty St, Financial District",
                City = "New York, NY",
                DistanceKm = 2.1,
                DistanceLabel = "2.1 mi away",
                PriceFrom = "$20.00",
                AvailableSeats = 85,
                Badge = "Curated Dining",
                Icon = "🍽",
                Description = "Signature culinary dishes delivered to your seat with zero-talking policy.",
                Facilities = new()
                {
                    new() { Label = "Dine-In at Seat", Icon = "🍴", IsProminent = true },
                    new() { Label = "Sony 4K Digital", Icon = "☁", IsProminent = true },
                    new() { Label = "Zero Talking Policy", Icon = "🔇", IsProminent = false },
                    new() { Label = "Craft Kitchen", Icon = "🍺", IsProminent = false }
                }
            }
        };
    }
}
