using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IBookingRepository : INotifyCollectionChanged
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
    }

    public void Add(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.BookingId))
            booking.BookingId = $"BK-{Random.Shared.Next(100000, 999999)}";
        _store.Insert(0, booking);
    }

    public Booking? GetById(string id) => _store.FirstOrDefault(b => b.BookingId == id);
}
