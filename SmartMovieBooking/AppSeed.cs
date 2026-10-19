using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

/// <summary>
/// Static catalog of movies, theaters, and showtime generator.
/// All data is mock — no network calls, no JSON loading.
///
/// The primary movie (Chronicles of Nebula: Horizon) and the four
/// theaters (AMC Empire 25, Regal E-Walk 4DX, AMC Lincoln Square 13,
/// Alamo Drafthouse Downtown) come from the Stitch Windows design.
/// </summary>
public static class AppSeed
{
    public static IReadOnlyList<Movie> Movies { get; } = BuildMovies();
    public static IReadOnlyList<Theater> Theaters { get; } = BuildTheaters();

    public static IEnumerable<string> Genres =>
        Movies.SelectMany(m => m.Genres).Distinct().OrderBy(g => g);

    public static IEnumerable<string> Languages =>
        Movies.SelectMany(m => m.Languages).Distinct().OrderBy(l => l);

    /// <summary>
    /// Build a deterministic list of showtimes for a given (movie, theater, date).
    /// </summary>
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
                Id = $"{theaterId}-{movieId}-{date:yyyyMMdd}-{start.Replace(":", "").Replace(" ", "")}",
                MovieId = movieId,
                TheaterId = theaterId,
                Date = date,
                StartTime = start,
                Format = fmt,
                Price = price,
                AvailableSeats = 40 + rng.Next(120),
                Status = status,
                IsPrimeTime = start == "07:15 PM"
            });
        }
        return list;
    }

    private static List<Movie> BuildMovies()
    {
        var movies = new List<Movie>
        {
            new()
            {
                Id = "m-001",
                Title = "Chronicles of Nebula: Horizon",
                Tagline = "One signal. Infinite worlds.",
                Synopsis = "When a mysterious signal is detected from the edge of the observable universe, a small crew of scientists and pilots must journey beyond the heliopause to make first contact — and discover that the signal is not a message, but a mirror.",
                PrimaryGenre = "Sci-Fi",
                Genres = new() { "Sci-Fi", "Drama", "Adventure" },
                Languages = new() { "English", "Hindi", "Tamil", "Telugu" },
                Format = "IMAX 3D",
                Rating = "9.2",
                Duration = "2h 45m",
                ReleaseDate = "Oct 24, 2025",
                Certificate = "UA",
                Description = "An epic space adventure that redefines the limits of human imagination. Winner of the 2025 Stellar Award for Best Visual Effects.",
                PriceRange = "$18 - $32",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Ava Chen",    Character = "Dr. Lena Mirov" },
                    new() { Name = "Marcus Vale", Character = "Cmdr. Ezra Quill" },
                    new() { Name = "Priya Rao",   Character = "Pilot Anika Soto" },
                    new() { Name = "David Yu",    Character = "Dr. Henrik Soder" }
                }
            },
            new()
            {
                Id = "m-002",
                Title = "The Last Cartographer",
                Tagline = "Some maps lead to treasure. Others lead home.",
                Synopsis = "An aging mapmaker discovers a hand-drawn route to a city that doesn't exist — and a journey he never made, but somehow remembers.",
                PrimaryGenre = "Drama",
                Genres = new() { "Drama", "Mystery", "Adventure" },
                Languages = new() { "English", "French" },
                Format = "Standard 2D",
                Rating = "8.6",
                Duration = "2h 10m",
                ReleaseDate = "Oct 17, 2025",
                Certificate = "PG-13",
                PriceRange = "$14 - $22",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Iris Bloom",  Character = "Walt Carver" },
                    new() { Name = "Theo Marsh",  Character = "Young Walt" }
                }
            },
            new()
            {
                Id = "m-003",
                Title = "Skyfall Protocol",
                Tagline = "When the sky falls, only the prepared survive.",
                Synopsis = "A disgraced air traffic controller must land a commercial flight during a city-wide blackout — with no radar, no radio, and a storm closing in.",
                PrimaryGenre = "Thriller",
                Genres = new() { "Thriller", "Action", "Drama" },
                Languages = new() { "English", "Hindi" },
                Format = "IMAX Laser",
                Rating = "8.9",
                Duration = "1h 58m",
                ReleaseDate = "Oct 24, 2025",
                Certificate = "PG-13",
                PriceRange = "$18 - $28",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Nina Park",    Character = "Dana Yoon" },
                    new() { Name = "Owen Reyes",   Character = "Captain Reyes" },
                    new() { Name = "Sam Carter",   Character = "ATC Hayes" }
                }
            },
            new()
            {
                Id = "m-004",
                Title = "Echoes of Aurora",
                Tagline = "Love is the only frequency that survives the silence.",
                Synopsis = "Two radio astronomers on opposite sides of the planet discover they hear the same impossible song — a melody encoded in cosmic background radiation.",
                PrimaryGenre = "Romance",
                Genres = new() { "Romance", "Sci-Fi", "Drama" },
                Languages = new() { "English", "Spanish" },
                Format = "Dolby Cinema",
                Rating = "8.4",
                Duration = "2h 02m",
                ReleaseDate = "Oct 10, 2025",
                Certificate = "PG-13",
                PriceRange = "$16 - $24",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Maya Lin",     Character = "Dr. Eun Park" },
                    new() { Name = "Carlos Diaz",  Character = "Dr. Miguel Soto" }
                }
            },
            new()
            {
                Id = "m-005",
                Title = "Ironclad: Apex",
                Tagline = "Built to break. Forged to last.",
                Synopsis = "In a near-future where corporations run their own armies, a retired combat engineer is pulled back into the only fight that matters.",
                PrimaryGenre = "Action",
                Genres = new() { "Action", "Sci-Fi", "Thriller" },
                Languages = new() { "English", "Japanese" },
                Format = "4DX",
                Rating = "8.1",
                Duration = "2h 22m",
                ReleaseDate = "Oct 24, 2025",
                Certificate = "UA",
                PriceRange = "$20 - $32",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Rex Tanaka",   Character = "Col. Marcus Hale" },
                    new() { Name = "Vera Lin",    Character = "Lt. Aiko Chen" }
                }
            },
            new()
            {
                Id = "m-006",
                Title = "The Baker's Dozen",
                Tagline = "Thirteen stories. One recipe. Endless crumbs.",
                Synopsis = "An ensemble comedy-drama set in a small-town bakery where every regular has a secret only the bread knows.",
                PrimaryGenre = "Comedy",
                Genres = new() { "Comedy", "Drama", "Family" },
                Languages = new() { "English" },
                Format = "Standard 2D",
                Rating = "7.9",
                Duration = "1h 52m",
                ReleaseDate = "Oct 03, 2025",
                Certificate = "PG",
                PriceRange = "$12 - $18",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Joy Sanders",  Character = "Mrs. Baker" },
                    new() { Name = "Ethan Cole",  Character = "Mr. Baker" }
                }
            },
            new()
            {
                Id = "m-007",
                Title = "Crimson Peak: Reborn",
                Tagline = "Every house remembers. This one remembers you.",
                Synopsis = "A modern architect inherits a crumbling Victorian estate and discovers the blueprints of a room that hasn't been built yet — and shouldn't exist.",
                PrimaryGenre = "Horror",
                Genres = new() { "Horror", "Mystery", "Thriller" },
                Languages = new() { "English", "French" },
                Format = "Dolby Cinema",
                Rating = "8.7",
                Duration = "2h 08m",
                ReleaseDate = "Oct 24, 2025",
                Certificate = "A",
                PriceRange = "$16 - $26",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Lila Frost",   Character = "Eleanor Vey" },
                    new() { Name = "Nathan Cross", Character = "Daniel Vey" }
                }
            },
            new()
            {
                Id = "m-008",
                Title = "Velocity Drift",
                Tagline = "The fastest way to win is to never slow down.",
                Synopsis = "A street-racing prodigy is recruited by a covert motorsport league where the circuits are designed by artificial intelligence and the finish line is always moving.",
                PrimaryGenre = "Action",
                Genres = new() { "Action", "Sport" },
                Languages = new() { "English", "Japanese" },
                Format = "IMAX Laser",
                Rating = "8.0",
                Duration = "1h 48m",
                ReleaseDate = "Oct 17, 2025",
                Certificate = "PG-13",
                PriceRange = "$16 - $24",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Kai Brooks",   Character = "Nina Cortez" }
                }
            },
            new()
            {
                Id = "m-009",
                Title = "Whispers of the Tide",
                Tagline = "The ocean keeps its promises — even the ones we never made.",
                Synopsis = "A marine biologist returns to her estranged coastal hometown to find the fishermen vanishing and the sea singing back.",
                PrimaryGenre = "Drama",
                Genres = new() { "Drama", "Mystery", "Family" },
                Languages = new() { "English", "Korean" },
                Format = "Standard 2D",
                Rating = "8.3",
                Duration = "2h 00m",
                ReleaseDate = "Oct 10, 2025",
                Certificate = "PG",
                PriceRange = "$14 - $22",
                IsNowShowing = true,
                Cast = new()
                {
                    new() { Name = "Hailey Moon", Character = "Dr. Soo-yeon Park" }
                }
            },
            new()
            {
                Id = "m-010",
                Title = "Arcane Heir",
                Tagline = "Magic always leaves a debt.",
                Synopsis = "When a young cartographer accidentally redraws the boundaries of an ancient kingdom, the only person who can help her unmake the mistake is the heir she just exiled.",
                PrimaryGenre = "Fantasy",
                Genres = new() { "Fantasy", "Adventure", "Drama" },
                Languages = new() { "English" },
                Format = "IMAX 3D",
                Rating = "8.5",
                Duration = "2h 18m",
                ReleaseDate = "Nov 07, 2025",
                Certificate = "PG-13",
                PriceRange = "$18 - $28",
                IsNowShowing = false,
                Cast = new()
                {
                    new() { Name = "Rina Voss",   Character = "Elara Kaine" },
                    new() { Name = "Jude Aron",  Character = "Prince Theron" }
                }
            }
        };
        return movies;
    }

    private static List<Theater> BuildTheaters()
    {
        return new List<Theater>
        {
            new()
            {
                Id = "t-empire",
                Name = "AMC Empire 25",
                Location = "234 W 42nd St, Times Square",
                City = "New York, NY",
                DistanceKm = 0.5,
                DistanceLabel = "(0.3 mi away)",
                PriceFrom = "$18.00",
                AvailableSeats = 142,
                Badge = "Dolby Atmos",
                Icon = "🎬",
                Description = "Flagship 25-screen multiplex with IMAX Laser, Dolby Cinema and 4DX Motion auditoriums.",
                Facilities = new()
                {
                    new() { Label = "IMAX Laser",   Icon = "📽️", IsProminent = true },
                    new() { Label = "Dolby Cinema", Icon = "📽️", IsProminent = true },
                    new() { Label = "4DX Motion",   Icon = "🎞️", IsProminent = false },
                    new() { Label = "Reserved Parking", Icon = "🅿️", IsProminent = false }
                }
            },
            new()
            {
                Id = "t-ewalk",
                Name = "Regal E-Walk 4DX & RPX",
                Location = "247 W 42nd St, Times Square",
                City = "New York, NY",
                DistanceKm = 0.9,
                DistanceLabel = "(0.9 mi away)",
                PriceFrom = "$17.00",
                AvailableSeats = 98,
                Badge = "Motion Immersion",
                Icon = "🎞️",
                Description = "Premium 4DX motion seats and RPX laser projection with full bar service.",
                Facilities = new()
                {
                    new() { Label = "4DX Motion",       Icon = "🎞️", IsProminent = true },
                    new() { Label = "RPX Laser",        Icon = "📽️", IsProminent = true },
                    new() { Label = "Dolby Atmos",      Icon = "📢", IsProminent = false },
                    new() { Label = "Reserved Parking", Icon = "🅿️", IsProminent = false }
                }
            },
            new()
            {
                Id = "t-lincoln",
                Name = "AMC Lincoln Square 13",
                Location = "1998 Broadway",
                City = "New York, NY",
                DistanceKm = 4.8,
                DistanceLabel = "(3.0 mi away)",
                PriceFrom = "$22.00",
                AvailableSeats = 67,
                Badge = "IMAX 70mm",
                Icon = "🎥",
                Description = "Iconic IMAX 70mm screen — the largest in the city — with reserved seating and balcony boxes.",
                Facilities = new()
                {
                    new() { Label = "IMAX 70mm",      Icon = "📽️", IsProminent = true },
                    new() { Label = "Dolby Atmos",    Icon = "📢", IsProminent = false },
                    new() { Label = "Reserved Seats", Icon = "💺", IsProminent = false },
                    new() { Label = "Cafe Lounge",    Icon = "☕", IsProminent = false }
                }
            },
            new()
            {
                Id = "t-alamo",
                Name = "Alamo Drafthouse Cinema Downtown",
                Location = "28 Liberty St, Financial District",
                City = "New York, NY",
                DistanceKm = 2.1,
                DistanceLabel = "(2.1 mi away)",
                PriceFrom = "$20.00",
                AvailableSeats = 85,
                Badge = "Curated Dining",
                Icon = "🍽️",
                Description = "Signature culinary dishes delivered to your seat, zero-talking adherence, and craft beer curation.",
                Facilities = new()
                {
                    new() { Label = "Dine-In at Seat", Icon = "🍽️", IsProminent = true },
                    new() { Label = "Sony 4K Digital", Icon = "📽️", IsProminent = true },
                    new() { Label = "Zero Talking Policy", Icon = "🤫", IsProminent = false },
                    new() { Label = "Craft Kitchen", Icon = "🍺", IsProminent = false }
                }
            }
        };
    }
}
