using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HotelManagerSample;

namespace HotelManagerSample.Dashboard;

/// <summary>
/// View-model for the mobile dashboard page. Reuses the singleton
/// <see cref="IHotelStore"/> registered in MauiProgram so all four
/// mobile pages render the same room/booking data. Most properties are
/// projections from the master store; the page binds them directly and
/// the VM listens to <see cref="IHotelStore.RoomsChanged"/> /
/// <see cref="IHotelStore.BookingsChanged"/> to propagate changes.
///
/// KPI shape mirrors the Stitch design:
///   - TotalBookings, AvailableRooms, OccupiedKeyCount, ArrivingKeyCount,
///     CleanedKeyCount, MaintenanceKeyCount.
///   - OccupancyPercent (0-100), shown in the donut upper text.
///   - InventorySegments (IReadOnlyList&lt;InventoryBreakdownPoint&gt;).
///   - ShiftTurnover (CheckIns / CheckOuts / In Cleaning / Maintenance).
///
/// RecentBookings powers the bottom SfListView.
/// </summary>
public sealed class MobileDashboardViewModel : INotifyPropertyChanged
{
    private readonly IHotelStore _store;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Constructs the VM, seeds every bound collection from
    /// the master store and subscribes to the store's mutation
    /// events.</summary>
    /// <param name="store">Shared <see cref="IHotelStore"/> resolved
    /// from DI. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <paramref name="store"/> is null.</exception>
    public MobileDashboardViewModel(IHotelStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        // -- Seed initial collections from the live store --
        foreach (var b in _store.Bookings)
            RecentBookings.Add(b);

        RebuildInventorySegments();

        // -- Subscribe to the store so changes propagate to this VM --
        _store.BookingsChanged += OnBookingsChanged;
        _store.RoomsChanged    += OnRoomsChanged;

        // -- Notify any startup surfaces --
        NotifyKpis();
        NotifyShiftTurnover();
        NotifyGreeting();
    }

    // =================== Stats & KPIs ===================

    /// <summary>Raises <see cref="PropertyChanged"/> for every KPI
    /// bound property so XAML re-evaluates them after a store change.</summary>
    private void NotifyKpis()
    {
        OnPropertyChanged(nameof(TotalBookings));
        OnPropertyChanged(nameof(AvailableRooms));
        OnPropertyChanged(nameof(AvailableRoomsRatio));
        OnPropertyChanged(nameof(OccupiedKeyCount));
        OnPropertyChanged(nameof(ArrivingKeyCount));
        OnPropertyChanged(nameof(CleanedKeyCount));
        OnPropertyChanged(nameof(MaintenanceKeyCount));
        OnPropertyChanged(nameof(AvailabilityPercent));
        OnPropertyChanged(nameof(OccupancyTierLabel));
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the
    /// shift-turnover matrix entries.</summary>
    private void NotifyShiftTurnover()
    {
        OnPropertyChanged(nameof(CheckInsCount));
        OnPropertyChanged(nameof(CheckOutsCount));
        OnPropertyChanged(nameof(MaintenanceCount));
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the greeting
    /// card copy.</summary>
    private void NotifyGreeting()
    {
        OnPropertyChanged(nameof(Greeting));
        OnPropertyChanged(nameof(DateLabel));
        OnPropertyChanged(nameof(Subtitle));
        OnPropertyChanged(nameof(ShiftLabel));
    }

    /// <summary>Stripe at the top of the welcome card.</summary>
    public string DateLabel => DateTime.Now.ToString("dddd, MMM d, yyyy");
    /// <summary>"Good Morning, Alex" — capitalised for the hero card.</summary>
    public string Greeting => "Good Morning, Alex";
    /// <summary>"Today's Overview · Grand Horizon Palace"</summary>
    public string Subtitle => "Today's Overview";
    /// <summary>Active-shift chip on the welcome card.</summary>
    public string ShiftLabel => "Duty Active: Shift AM";

    /// <summary>Total active bookings (any status other than
    /// "Cancelled") in the master store.</summary>
    public int TotalBookings =>
        _store.Bookings.Count(b => !string.Equals(b.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));

    /// <summary>Rooms whose housekeeping status is "Available" or
    /// "Cleaning" — i.e. ready or soon-to-be-ready for the next guest.</summary>
    public int AvailableRooms =>
        _store.Rooms.Count(r =>
            string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Status, "Cleaning",   StringComparison.OrdinalIgnoreCase));

    /// <summary>"42 / 120" caption rendered under the Available tile.</summary>
    public string AvailableRoomsRatio => $"{AvailableRooms} / {HotelConfig.TotalRooms}";

    /// <summary>Number of rooms currently flagged "Occupied".</summary>
    public int OccupiedKeyCount =>
        _store.Rooms.Count(r => string.Equals(r.Status, "Occupied", StringComparison.OrdinalIgnoreCase));

    /// <summary>Number of rooms flagged "Reserved" (future arrivals).</summary>
    public int ArrivingKeyCount =>
        _store.Rooms.Count(r => string.Equals(r.Status, "Reserved", StringComparison.OrdinalIgnoreCase));

    /// <summary>Number of rooms that are either Available or currently
    /// being cleaned (the "cleaned" tile on the dashboard).</summary>
    public int CleanedKeyCount =>
        _store.Rooms.Count(r =>
            string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r.Status, "Cleaning",   StringComparison.OrdinalIgnoreCase));

    /// <summary>Number of rooms currently flagged "Maintenance".</summary>
    public int MaintenanceKeyCount =>
        _store.Rooms.Count(r => string.Equals(r.Status, "Maintenance", StringComparison.OrdinalIgnoreCase));

    /// <summary>Occupancy percentage shown above the inventory donut.
    /// Returns 0 when the property has zero keys (avoids divide-by-zero).</summary>
    public double AvailabilityPercent =>
        HotelConfig.TotalRooms == 0
            ? 0
            : Math.Round((double)OccupiedKeyCount / HotelConfig.TotalRooms * 100.0, 0);

    /// <summary>"OCCUPANCY ACTIVE" tag floating under the donut %.</summary>
    public string OccupancyTierLabel => "ACTIVE";

    /// <summary>Inventory donut segments (Occupied, Arriving, Vacant Clean, Maintenance).</summary>
    public IReadOnlyList<InventoryBreakdownPoint> InventorySegments => MobileInventorySegments;

    private IReadOnlyList<InventoryBreakdownPoint> _mobileInventorySegments = Array.Empty<InventoryBreakdownPoint>();
    /// <summary>Backing storage for <see cref="InventorySegments"/>.
    /// Setting this raises <see cref="PropertyChanged"/> so the chart
    /// re-renders.</summary>
    public IReadOnlyList<InventoryBreakdownPoint> MobileInventorySegments
    {
        get => _mobileInventorySegments;
        private set
        {
            _mobileInventorySegments = value;
            OnPropertyChanged(nameof(InventorySegments));
        }
    }

    /// <summary>Recomputes the donut segments from the live room
    /// collection. Called on every <see cref="IHotelStore.RoomsChanged"/>
    /// delivery so the chart tracks the master inventory.</summary>
    private void RebuildInventorySegments()
    {
        int occupied    = OccupiedKeyCount;
        int reserved    = ArrivingKeyCount;
        int cleaning    = _store.Rooms.Count(r => string.Equals(r.Status, "Cleaning", StringComparison.OrdinalIgnoreCase));
        int vacantClean = _store.Rooms.Count(r => string.Equals(r.Status, "Available", StringComparison.OrdinalIgnoreCase)) + cleaning;
        int maintenance = MaintenanceKeyCount;

        // Stitch legend mapping: Occupied / Cleaned / Arriving / Maintenance
        MobileInventorySegments = new[]
        {
            new InventoryBreakdownPoint { Category = "Occupied",    Value = occupied,    Color = "#4A43E6" },
            new InventoryBreakdownPoint { Category = "Cleaned",     Value = vacantClean, Color = "#22C55E" },
            new InventoryBreakdownPoint { Category = "Arriving",    Value = reserved,    Color = "#38BDF8" },
            new InventoryBreakdownPoint { Category = "Maintenance", Value = maintenance, Color = "#EF4444" },
        };
    }

    // =================== Recent Bookings ===================

    /// <summary>
    /// Recent reservations rendered as cards on the bottom of the page.
    /// Caught up to the master <see cref="IHotelStore.Bookings"/>
    /// collection via the <see cref="IHotelStore.BookingsChanged"/>
    /// event so a new booking added from the wizard appears here too.
    /// </summary>
    public ObservableCollection<Booking> RecentBookings { get; } = new();

    // =================== Shift turnover matrix ===================

    /// <summary>Number of non-cancelled bookings arriving today.</summary>
    public int CheckInsCount =>
        _store.Bookings.Count(b => b.CheckIn.Date == HotelConfig.ReferenceToday.Date
                                    && !string.Equals(b.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));

    /// <summary>Number of non-cancelled bookings departing today.</summary>
    public int CheckOutsCount =>
        _store.Bookings.Count(b => b.CheckOut.Date == HotelConfig.ReferenceToday.Date
                                    && !string.Equals(b.Status, "CANCELLED", StringComparison.OrdinalIgnoreCase));

    /// <summary>Number of rooms currently flagged "Maintenance".</summary>
    public int MaintenanceCount =>
        _store.Rooms.Count(r => string.Equals(r.Status, "Maintenance", StringComparison.OrdinalIgnoreCase));

    // =================== INotifyPropertyChanged ===================

    /// <summary>Master-store callback for booking changes. Inserts the
    /// new row at the top of <see cref="RecentBookings"/> (when not
    /// already present) and refreshes the KPI / shift-turnover
    /// properties.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The booking that changed.</param>
    private void OnBookingsChanged(object? sender, Booking e)
    {
        // Insert at top so the most recent booking shows on top.
        if (e is not null && !RecentBookings.Any(b => b.Reference == e.Reference))
            RecentBookings.Insert(0, e);
        NotifyKpis();
        NotifyShiftTurnover();
    }

    /// <summary>Master-store callback for room changes. Rebuilds the
    /// donut, then re-notifies KPI / shift-turnover derived properties
    /// so the page repaints.</summary>
    /// <param name="sender">The store raising the event. Unused.</param>
    /// <param name="e">The room that changed.</param>
    private void OnRoomsChanged(object? sender, Room e)
    {
        RebuildInventorySegments();
        NotifyKpis();
        NotifyShiftTurnover();
    }

    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
