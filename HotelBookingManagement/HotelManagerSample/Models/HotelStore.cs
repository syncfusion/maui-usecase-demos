using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace HotelManagerSample;

/// <summary>
/// Single source of truth for the live hotel state shared across every page.
/// Registered as a singleton in <see cref="MauiProgram"/> so the dashboard,
/// rooms page and bookings page all see the same <see cref="Rooms"/> and
/// <see cref="Bookings"/> collections. When the user adds a booking from
/// the Add-Booking popup, the page calls <see cref="AddBooking"/> which
/// atomically appends the row, flips the room's status to "Reserved" and
/// raises <see cref="BookingsChanged"/> so every subscriber VM can refresh
/// its derived collections + KPI bindings in lockstep.
/// </summary>
public interface IHotelStore : INotifyPropertyChanged
{
    /// <summary>Master 15-key inventory. Mutations to <see cref="Room.Status"/>
    /// flow through this collection — every page derives its room tiles,
    /// inventory stats and KPI counts from here.</summary>
    ObservableCollection<Room> Rooms { get; }

    /// <summary>Master list of every reservation in the property. The
    /// dashboard's "Today's check-ins" KPI, the bookings page's SfDataGrid,
    /// and the Add-Booking popup's reference-number picker all read from
    /// (and write to) the same list.</summary>
    ObservableCollection<Booking> Bookings { get; }

    /// <summary>Raised after <see cref="AddBooking"/> succeeds, after any
    /// external mutation to <see cref="Bookings"/>, and after a room's
    /// status changes (which can imply folio/occupancy changes). The
    /// <see cref="Booking"/> argument is the row that was added/affected.
    /// Subscribers should reset their grids, refresh KPI tiles and
    /// re-apply any local filter — no need to inspect the collection
    /// contents because they already mirror the master list.</summary>
    event EventHandler<Booking>? BookingsChanged;

    /// <summary>Raised whenever a room's status (or any other property)
    /// changes — either via direct mutation on a <see cref="Room"/> in
    /// <see cref="Rooms"/> or implicitly as part of an
    /// <see cref="AddBooking"/> call (which flips the assigned room to
    /// "Reserved"). Subscribers refresh the rooms board, the dashboard
    /// inventory-donut and KPI keys.</summary>
    event EventHandler<Room>? RoomsChanged;

    /// <summary>Atomically append a booking to the master collection AND
    /// flip the supplied room to "Reserved". Raises
    /// <see cref="BookingsChanged"/> after both mutations so a single
    /// subscription delivers a consistent snapshot.</summary>
    /// <param name="booking">Booking to append.</param>
    /// <param name="reservedRoom">Room the booking reserves. Its status
    /// is set to "Reserved".</param>
    void AddBooking(Booking booking, Room reservedRoom);

    /// <summary>Raise <see cref="RoomsChanged"/> for an arbitrary room that
    /// was mutated directly (not via <see cref="AddBooking"/>). Used by
    /// housekeeping flows that flip statuses outside a booking event.</summary>
    /// <param name="room">Room whose mutation should be broadcast.</param>
    void NotifyRoomChanged(Room room);
}

/// <summary>
/// Default in-memory <see cref="IHotelStore"/>. Holds the collections on
/// instance state (not <c>static</c>) so DI can scope a single shared
/// instance — every ContentPage receives the same store through constructor
/// injection. See <see cref="MauiProgram"/> for the registration.
/// </summary>
public sealed class HotelStore : IHotelStore
{
    /// <summary>Master rooms collection seeded from <see cref="HotelData.BuildRooms"/>.</summary>
    public ObservableCollection<Room> Rooms { get; } = HotelData.BuildRooms();
    /// <summary>Master bookings collection seeded from <see cref="HotelData.BuildBookings"/>.</summary>
    public ObservableCollection<Booking> Bookings { get; } = HotelData.BuildBookings();

    /// <summary>Raised after <see cref="AddBooking"/> succeeds, after any
    /// external mutation to <see cref="Bookings"/>, and after a room's
    /// status changes (which can imply folio/occupancy changes). The
    /// <see cref="Booking"/> argument is the row that was added/affected.</summary>
    public event EventHandler<Booking>? BookingsChanged;
    /// <summary>Raised whenever a room's status (or any other property)
    /// changes — either via direct mutation on a <see cref="Room"/> in
    /// <see cref="Rooms"/> or implicitly as part of an
    /// <see cref="AddBooking"/> call (which flips the assigned room to
    /// "Reserved").</summary>
    public event EventHandler<Room>? RoomsChanged;

    /// <summary>Standard <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event raised whenever <see cref="Rooms"/> or <see cref="Bookings"/>
    /// mutates so XAML bindings can re-read the collection identity.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Used by the CollectionChanged forwarders below to
    /// notify XAML that the bound property may have a new identity.</summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Constructs the store and wires CollectionChanged
    /// forwarders so that the higher-level
    /// <see cref="BookingsChanged"/> / <see cref="RoomsChanged"/>
    /// events fire from every list mutation.</summary>
    public HotelStore()
    {
        // Forward CollectionChanged as the higher-level BookingsChanged so
        // every subscriber gets one notification carrying the relevant row.
        Bookings.CollectionChanged += OnBookingsCollectionChanged;
        Rooms.CollectionChanged += OnRoomsCollectionChanged;
    }

    /// <summary>Forwards <see cref="ObservableCollection{T}.CollectionChanged"/>
    /// on <see cref="Bookings"/> as a typed <see cref="BookingsChanged"/>
    /// event. Reset / replace / remove operations surface the first
    /// surviving row so subscribers always have a concrete payload.</summary>
    /// <param name="sender">The bookings collection. Unused.</param>
    /// <param name="e">Standard CollectionChanged payload.</param>
    private void OnBookingsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is { Count: > 0 })
        {
            foreach (var item in e.NewItems.OfType<Booking>())
                BookingsChanged?.Invoke(this, item);
        }
        else if (e.NewItems is { Count: > 0 })
        {
            foreach (var item in e.NewItems.OfType<Booking>())
                BookingsChanged?.Invoke(this, item);
        }
        else
        {
            // Reset / replace / remove — surface the first current row so
            // subscribers can refresh even though there is no "new" item.
            var first = Bookings.FirstOrDefault();
            if (first is not null)
                BookingsChanged?.Invoke(this, first);
        }
        OnPropertyChanged(nameof(Bookings));
    }

    /// <summary>Forwards <see cref="ObservableCollection{T}.CollectionChanged"/>
    /// on <see cref="Rooms"/> as a typed <see cref="RoomsChanged"/>
    /// event. Reset / replace / remove operations surface the first
    /// surviving room so subscribers always have a concrete payload.</summary>
    /// <param name="sender">The rooms collection. Unused.</param>
    /// <param name="e">Standard CollectionChanged payload.</param>
    private void OnRoomsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is { Count: > 0 })
        {
            foreach (var item in e.NewItems.OfType<Room>())
                RoomsChanged?.Invoke(this, item);
        }
        else
        {
            // Reset / replace / remove — pick the first surviving room so
            // subscribers get a concrete ref to react to.
            var first = Rooms.FirstOrDefault();
            if (first is not null)
                RoomsChanged?.Invoke(this, first);
        }
        OnPropertyChanged(nameof(Rooms));
    }

    /// <summary>Atomically appends <paramref name="booking"/> to
    /// <see cref="Bookings"/> and flips <paramref name="reservedRoom"/>'s
    /// status to "Reserved". Raises <see cref="RoomsChanged"/> explicitly
    /// because <see cref="Room"/> is not observable.</summary>
    /// <param name="booking">Booking to append.</param>
    /// <param name="reservedRoom">Room the booking reserves.</param>
    public void AddBooking(Booking booking, Room reservedRoom)
    {
        Bookings.Add(booking);
        if (reservedRoom is not null)
            reservedRoom.Status = "Reserved";

        // Raise RoomsChanged explicitly because Room is not an observable
        // model — its Status setter mutates a plain backing field, so the
        // ObservableCollection itself does not fire. Subscribers watching
        // RoomsChanged will rebuild their derived tiles/legends and then
        // BookingsChanged (raised by the collection forwarder above) will
        // run a moment later when they process the Bookings.Add side.
        if (reservedRoom is not null)
            RoomsChanged?.Invoke(this, reservedRoom);
    }

    /// <summary>Raise <see cref="RoomsChanged"/> for the supplied
    /// <paramref name="room"/>. Used by housekeeping flows that flip
    /// statuses outside a booking event.</summary>
    /// <param name="room">Room whose mutation should be broadcast.</param>
    public void NotifyRoomChanged(Room room)
    {
        RoomsChanged?.Invoke(this, room);
    }
}
