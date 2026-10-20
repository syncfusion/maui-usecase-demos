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

    /// <summary>
    /// Read-only view of the in-memory store. Mutations go through Add().
    /// We surface the inner _store.CollectionChanged event through this
    /// repository's INotifyCollectionChanged event so external ViewModels
    /// can subscribe through the interface (required when the interface
    /// extends INotifyCollectionChanged - otherwise CS0535 fires).
    /// </summary>
    public ReadOnlyObservableCollection<Booking> All { get; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public BookingRepository()
    {
        All = new ReadOnlyObservableCollection<Booking>(_store);

        // Forward every change from the inner store to our own event so
        // that BookingsViewModel (which subscribes through
        // IBookingRepository) receives Add/Remove notifications.
        _store.CollectionChanged += (s, e) => CollectionChanged?.Invoke(s, e);
    }

    public void Add(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.BookingId))
            booking.BookingId = $"BK-{Random.Shared.Next(100000, 999999)}";
        _store.Insert(0, booking);
    }

    public Booking? GetById(string id) => _store.FirstOrDefault(b => b.BookingId == id);
}
