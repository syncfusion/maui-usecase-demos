using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

/// <summary>
/// Static catalog of movies, theaters, and a deterministic showtime
/// generator. All data is mock — no network calls, no JSON loading.
///
/// The primary record (Chronicles of Nebula: Horizon) and the four
/// theaters (AMC Empire 25, Regal E-Walk 4DX, AMC Lincoln Square 13,
/// Alamo Drafthouse Downtown) are taken verbatim from the Stitch
/// Windows design HTML.
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
    /// Build a stable list of showtimes for a given (movie, theater, date).
    /// Deterministic per call so the same showtimes reappear on each refresh.
    /// </summary>
    public static IReadOnlyList<Showtime> GetShowtimes(string movieId, string theaterId, DateTime date)
    {
        var theater = Theaters.First(t => t.Id == theaterId);
        var rng = new Random(HashCode.Combine(movieId, theaterId, date.DayOfYear));
        var templates = new[]
        {
            ("10:30 AM", "IMAX Laser",   24m, ShowtimeStatus.Available),
            ("01:15 PM", "Dolby Cinema", 22m, ShowtimeStatus.Available),
            ("04:00 PM", "Standard 2D",  16m, ShowtimeStatus.AlmostFull),
            ("07:15 PM", "IMAX Laser",   26.5m, ShowtimeStatus.SellingFast),
            ("10:00 PM", "IMAX Laser",   24m, ShowtimeStatus.Available),
            ("11:30 PM", "Late Night 2D", 18.5m, ShowtimeStatus.Available)
        };

        var list = new List<Showtime>();
        var pick = templates.OrderBy(_ => rng.Next()).Take(4 + rng.Next(0, 2)).ToList();
        foreach (var (start, format, price, status) in pick)
        {
            list.Add(new Showtime
            {
                Id = $"ss-{movieId}-{theaterId}-{date:yyyyMMdd}-{start.Replace(":", "").Replace(" ", "")}",
                MovieId = movieId,
                TheaterId = theaterId,
                Date = date,
                StartTime = start,
                EndTime = AddMinutes(start, 130),
                Format = format,
                Price = price,
                AvailableSeats = status switch
                {
                    ShowtimeStatus.SoldOut => 0,
                    ShowtimeStatus.SellingFast => rng.Next(5, 20),
                    ShowtimeStatus.AlmostFull => rng.Next(15, 50),
                    _ => rng.Next(50, 120)
                },
                Status = status,
                IsPrimeTime = start.Contains("07:")
            });
        }
        return list;
    }

    private static string AddMinutes(string hhmm, int minutes)
    {
        if (!TimeSpan.TryParse(hhmm, out var t)) return hhmm;
        var end = t.Add(TimeSpan.FromMinutes(minutes));
        var hh = (end.Hours % 12 == 0 ? 12 : end.Hours % 12).ToString("D2");
        var mm = end.Minutes.ToString("D2");
        var ampm = end.Hours < 12 ? "AM" : "PM";
        return $"{hh}:{mm} {ampm}";
    }

    private static List<Movie> BuildMovies()
    {
        var nebula = new Movie
        {
            Id = "mv-nebula-horizon",
            Title = "Chronicles of Nebula: Horizon",
            Tagline = "Beyond the edge of space, beyond time itself.",
            Synopsis = "An interstellar adventure following Commander Vega as she leads a fractured crew " +
                       "across the frontier of a dying galaxy to confront an ancient intelligence hidden " +
                       "within the nebula's heart.",
            PosterUrl = "https://images.unsplash.com/photo-1518709268805-4e9042af2176?w=600",
            BannerUrl = "https://images.unsplash.com/photo-1462331940025-496dfbfc7564?w=1600",
            PrimaryGenre = "Sci-Fi",
            Genres = new() { "Sci-Fi", "Adventure", "Drama" },
            Languages = new() { "English", "Hindi", "Tamil" },
            Format = "IMAX 3D",
            Rating = "8.4",
            Duration = "2h 38m",
            ReleaseDate = "Oct 24, 2025",
            Certificate = "UA",
            Description = "Directed by Mira Solano · Visual masterpiece · Critics' choice",
            PriceRange = "$14.00 – $32.50",
            IsNowShowing = true,
            Cast = new()
            {
                new CastMember { Name = "Ava Reyes",       Character = "Cmdr. Vega",     AvatarUrl = "" },
                new CastMember { Name = "Idris Khan",      Character = "Lt. Aurelio",    AvatarUrl = "" },
                new CastMember { Name = "Hana Mori",       Character = "Dr. Selene",     AvatarUrl = "" },
                new CastMember { Name = "Lucas Brandt",    Character = "The Cartographer", AvatarUrl = "" }
            }
        };

        return new()
        {
            nebula,
            new Movie
            {
                Id = "mv-last-light",
                Title = "The Last Light Keeper",
                Tagline = "Some promises outlive empires.",
                PrimaryGenre = "Drama",
                Genres = new() { "Drama", "Romance" },
                Languages = new() { "English", "Spanish" },
                Format = "Standard 2D",
                Rating = "7.8",
                Duration = "2h 12m",
                ReleaseDate = "Oct 17, 2025",
                PriceRange = "$12.00 – $22.00",
                PosterUrl = "https://images.unsplash.com/photo-1485846234645-a62644f84728?w=600",
                Description = "An evocative period drama set on the coast of 1920s Portugal."
            },
            new Movie
            {
                Id = "mv-glitch-protocol",
                Title = "Glitch Protocol",
                Tagline = "Reality is the first casualty.",
                PrimaryGenre = "Thriller",
                Genres = new() { "Thriller", "Sci-Fi" },
                Languages = new() { "English", "French" },
                Format = "Dolby Cinema",
                Rating = "8.1",
                Duration = "2h 04m",
                ReleaseDate = "Oct 10, 2025",
                PriceRange = "$16.00 – $28.00",
                PosterUrl = "https://images.unsplash.com/photo-1502134249126-9f3755a50d78?w=600",
                Description = "A hacker stumbles on a government simulation that shouldn't exist."
            },
            new Movie
            {
                Id = "mv-paper-lanterns",
                Title = "Paper Lanterns",
                Tagline = "Light a thousand wishes.",
                PrimaryGenre = "Animation",
                Genres = new() { "Animation", "Family" },
                Languages = new() { "English", "Japanese", "Korean" },
                Format = "3D",
                Rating = "9.0",
                Duration = "1h 56m",
                ReleaseDate = "Oct 03, 2025",
                PriceRange = "$11.00 – $24.00",
                PosterUrl = "https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=600",
                Description = "A hand-drawn Studio Ghibli-style fable of courage and kindness."
            },
            new Movie
            {
                Id = "mv-iron-tide",
                Title = "Iron Tide",
                Tagline = "The sea remembers every name.",
                PrimaryGenre = "Action",
                Genres = new() { "Action", "Adventure" },
                Languages = new() { "English", "Hindi", "Telugu" },
                Format = "IMAX",
                Rating = "7.4",
                Duration = "2h 21m",
                ReleaseDate = "Sep 26, 2025",
                PriceRange = "$14.00 – $26.00",
                PosterUrl = "https://images.unsplash.com/photo-1440404653325-ab127d49abc1?w=600",
                Description = "A submarine commander faces impossible choices beneath the Arctic."
            },
            new Movie
            {
                Id = "mv-cobalt-sky",
                Title = "Cobalt Sky",
                Tagline = "Two strangers, one flight, no turning back.",
                PrimaryGenre = "Romance",
                Genres = new() { "Romance", "Drama" },
                Languages = new() { "English", "Spanish" },
                Format = "Standard 2D",
                Rating = "7.0",
                Duration = "1h 48m",
                ReleaseDate = "Sep 19, 2025",
                PriceRange = "$10.00 – $20.00",
                PosterUrl = "https://images.unsplash.com/photo-1489599735734-e6c91e2d4f0a?w=600",
                Description = "An unexpected romance blossoms mid-air at 35,000 feet."
            },
            new Movie
            {
                Id = "mv-quantum-leap",
                Title = "Quantum Leap: Reloaded",
                Tagline = "Time will not be contained.",
                PrimaryGenre = "Sci-Fi",
                Genres = new() { "Sci-Fi", "Action" },
                Languages = new() { "English" },
                Format = "IMAX 3D",
                Rating = "8.6",
                Duration = "2h 44m",
                ReleaseDate = "Oct 31, 2025",
                PriceRange = "$18.00 – $34.00",
                PosterUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=600",
                Description = "Sequel to the cult time-travel franchise, leap forward into the future.",
                IsNowShowing = false
            },
            new Movie
            {
                Id = "mv-silent-orchard",
                Title = "Silent Orchard",
                Tagline = "Every garden hides a story.",
                PrimaryGenre = "Mystery",
                Genres = new() { "Mystery", "Drama" },
                Languages = new() { "English", "German" },
                Format = "Standard 2D",
                Rating = "7.9",
                Duration = "2h 02m",
                ReleaseDate = "Nov 07, 2025",
                PriceRange = "$11.00 – $22.00",
                PosterUrl = "https://images.unsplash.com/photo-1500964757637-c85e8a162699?w=600",
                Description = "A forensic botanist uncovers decades of secrets in a remote alpine village.",
                IsNowShowing = false
            },
            new Movie
            {
                Id = "mv-kings-of-static",
                Title = "Kings of Static",
                Tagline = "The loudest music ever silenced.",
                PrimaryGenre = "Documentary",
                Genres = new() { "Documentary", "Music" },
                Languages = new() { "English" },
                Format = "Dolby Cinema",
                Rating = "8.2",
                Duration = "1h 52m",
                ReleaseDate = "Oct 24, 2025",
                PriceRange = "$12.00 – $24.00",
                PosterUrl = "https://images.unsplash.com/photo-1493225457124-a3eb161ffa5f?w=600",
                Description = "An intimate look at the rise and fall of 90s analog synth culture."
            },
            new Movie
            {
                Id = "mv-stormchasers",
                Title = "Stormchasers",
                Tagline = "Catch the wind, before it catches you.",
                PrimaryGenre = "Adventure",
                Genres = new() { "Adventure", "Family" },
                Languages = new() { "English", "Hindi" },
                Format = "IMAX",
                Rating = "7.6",
                Duration = "1h 58m",
                ReleaseDate = "Nov 14, 2025",
                PriceRange = "$12.00 – $26.00",
                PosterUrl = "https://images.unsplash.com/photo-1527489622319-810b78c3d1ce?w=600",
                Description = "A family of tornado researchers races storms across Tornado Alley.",
                IsNowShowing = false
            }
        };
    }

    private static List<Theater> BuildTheaters()
    {
        return new()
        {
            new Theater
            {
                Id = "th-amc-empire-25",
                Name = "AMC Empire 25",
                Location = "234 W 42nd St, Times Square",
                City = "New York",
                DistanceKm = 1.2,
                DistanceLabel = "0.5 mi away",
                PriceFrom = "$22.00",
                AvailableSeats = 142,
                Badge = "Premium Flagship",
                Icon = "theaters",
                Description = "Dolby Vision 4K projection certified for auditorium 1 & 3",
                Facilities = new()
                {
                    new() { Label = "IMAX Laser",        Icon = "videocam",       IsProminent = true },
                    new() { Label = "Dolby Cinema",      Icon = "surround_sound", IsProminent = true },
                    new() { Label = "Reserved Seating",  Icon = "event_seat",     IsProminent = false },
                    new() { Label = "Snack Bar",         Icon = "restaurant",     IsProminent = false }
                }
            },
            new Theater
            {
                Id = "th-regal-ewalk",
                Name = "Regal E-Walk 4DX & RPX",
                Location = "247 W 42nd St, Times Square",
                City = "New York",
                DistanceKm = 1.4,
                DistanceLabel = "0.9 mi away",
                PriceFrom = "$17.00",
                AvailableSeats = 98,
                Badge = "Motion Immersion",
                Icon = "cinematic_blur",
                Description = "4DX motion seats, RPX laser projection, and Dolby Atmos.",
                Facilities = new()
                {
                    new() { Label = "4DX Motion",        Icon = "waves",          IsProminent = true },
                    new() { Label = "RPX Laser",         Icon = "videocam",       IsProminent = true },
                    new() { Label = "Dolby Atmos",       Icon = "surround_sound", IsProminent = false },
                    new() { Label = "Reserved Parking",  Icon = "local_parking",  IsProminent = false }
                }
            },
            new Theater
            {
                Id = "th-amc-lincoln",
                Name = "AMC Lincoln Square 13",
                Location = "1998 Broadway, Upper West Side",
                City = "New York",
                DistanceKm = 3.6,
                DistanceLabel = "3.2 mi away",
                PriceFrom = "$26.00",
                AvailableSeats = 76,
                Badge = "70mm IMAX Icon",
                Icon = "movie",
                Description = "Home of the iconic 70mm IMAX screen — a New York institution.",
                Facilities = new()
                {
                    new() { Label = "IMAX 70mm",         Icon = "videocam",       IsProminent = true },
                    new() { Label = "Sony 4K Digital",   Icon = "cloud_download", IsProminent = false },
                    new() { Label = "Hearing Assistance", Icon = "hearing",       IsProminent = false }
                }
            },
            new Theater
            {
                Id = "th-alamo-downtown",
                Name = "Alamo Drafthouse Cinema Downtown",
                Location = "28 Liberty St, Financial District",
                City = "New York",
                DistanceKm = 3.4,
                DistanceLabel = "2.1 mi away",
                PriceFrom = "$20.00",
                AvailableSeats = 85,
                Badge = "Curated Dining",
                Icon = "dinner_dining",
                Description = "Signature culinary dishes delivered to your seat, zero-talking policy.",
                Facilities = new()
                {
                    new() { Label = "Dine-In at Seat",   Icon = "restaurant",     IsProminent = true },
                    new() { Label = "Sony 4K Digital",   Icon = "cloud_download", IsProminent = true },
                    new() { Label = "Zero Talking Policy", Icon = "volume_off",   IsProminent = false },
                    new() { Label = "Craft Kitchen",     Icon = "local_bar",      IsProminent = false }
                }
            }
        };
    }
}