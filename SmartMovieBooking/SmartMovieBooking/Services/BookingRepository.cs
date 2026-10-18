using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IBookingRepository
{
    ReadOnlyObservableCollection<Booking> All { get; }
    void Add(Booking booking);
    Booking? GetById(string id);
}

public sealed class BookingRepository : IBookingRepository
{
    private readonly ObservableCollection<Booking> _store = new();

    public ReadOnlyObservableCollection<Booking> All { get; }

    public BookingRepository()
    {
        All = new ReadOnlyObservableCollection<Booking>(_store);
        _store.CollectionChanged += OnStoreChanged;
        Seed();
    }

    public void Add(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.BookingId))
            booking.BookingId = $"BK-{Random.Shared.Next(100000, 999999)}";
        _store.Insert(0, booking);
    }

    public Booking? GetById(string id) => _store.FirstOrDefault(b => b.BookingId == id);

    private void OnStoreChanged(object? sender, NotifyCollectionChangedEventArgs e) { }

    private void Seed()
    {
        // One historical booking so the Bookings tab is not empty on first launch.
        var sample = new Booking
        {
            BookingId = "BK-841203",
            CreatedAt = DateTime.Now.AddDays(-3),
            Movie = AppSeed.Movies.Count > 1 ? AppSeed.Movies[1] : AppSeed.Movies[0],
            Theater = AppSeed.Theaters.First(),
            Showtime = new Showtime
            {
                Id = "ss-seed-1",
                StartTime = "07:30 PM",
                Format = "Dolby Cinema",
                Price = 22m,
                AvailableSeats = 0,
                Status = ShowtimeStatus.SoldOut
            },
            Subtotal = 44m,
            ConvenienceFee = 2.5m,
            Total = 46.5m,
            PaymentMethod = "Card",
            Status = "Completed"
        };
        sample.Seats.Add(new Seat { Id = "C7", Price = 22m });
        sample.Seats.Add(new Seat { Id = "C8", Price = 22m });
        _store.Add(sample);
    }
}