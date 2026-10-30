using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HotelManagerSample;

/// <summary>
/// Wraps a <see cref="Booking"/> with a bindable <see cref="IsSelected"/> flag
/// so the DataGrid checkbox column can flip it without losing the underlying
/// record identity. A Reset wipes selections across the whole list.
/// </summary>
public sealed class SelectableBooking : INotifyPropertyChanged
{
    /// <summary>The wrapped <see cref="Booking"/>. Never null after
    /// construction.</summary>
    public Booking Booking { get; }

    /// <summary>Wraps the supplied <paramref name="booking"/> and
    /// exposes each of its public fields as a property so the
    /// SfDataGrid can bind against individual columns.</summary>
    /// <param name="booking">Booking to wrap.</param>
    public SelectableBooking(Booking booking) => Booking = booking;

    /// <summary>Booking reference number.</summary>
    public string Reference => Booking.Reference;
    /// <summary>Guest full name.</summary>
    public string Guest => Booking.Guest;
    /// <summary>Guest email address.</summary>
    public string GuestEmail => Booking.GuestEmail;
    /// <summary>Guest phone number.</summary>
    public string Phone => Booking.Phone;
    /// <summary>Human-readable room label.</summary>
    public string Room => Booking.Room;
    /// <summary>Room type label.</summary>
    public string RoomType => Booking.RoomType;
    /// <summary>Room segment (floor + wing).</summary>
    public string RoomSegment => Booking.RoomSegment;
    /// <summary>Date-range label.</summary>
    public string Dates => Booking.Dates;
    /// <summary>Check-in date.</summary>
    public DateTime CheckIn => Booking.CheckIn;
    /// <summary>Check-out date.</summary>
    public DateTime CheckOut => Booking.CheckOut;
    /// <summary>Adult count.</summary>
    public int Adults => Booking.Adults;
    /// <summary>Children count.</summary>
    public int Children => Booking.Children;
    /// <summary>Source label.</summary>
    public string Source => Booking.Source;
    /// <summary>Source segment.</summary>
    public string SourceSegment => Booking.SourceSegment;
    /// <summary>Hex colour used for the source tag background.</summary>
    public string SourceTagColor => Booking.SourceTagColor;
    /// <summary>Lifecycle status string.</summary>
    public string Status => Booking.Status;
    /// <summary>Follow-up note.</summary>
    public string FollowUp => Booking.FollowUp;
    /// <summary>Loyalty tier label.</summary>
    public string Tier => Booking.Tier;
    /// <summary>Hex colour used for the loyalty tier tag.</summary>
    public string TierColor => Booking.TierColor;
    /// <summary>Free-form notes.</summary>
    public string Notes => Booking.Notes;
    /// <summary>Total folio value.</summary>
    public decimal Total => Booking.Total;
    /// <summary>Night count.</summary>
    public int Nights => Booking.Nights;
    /// <summary>Two-letter guest monogram.</summary>
    public string Initials => Booking.Initials;
    /// <summary>Party label ("2 Adults, 1 Kids").</summary>
    public string PartyLabel => Booking.PartyLabel;
    /// <summary>Three-letter day name for the check-in date.</summary>
    public string CheckInDayLabel => Booking.CheckInDayLabel;
    /// <summary>Three-letter day name for the check-out date.</summary>
    public string CheckOutDayLabel => Booking.CheckOutDayLabel;

    /// <summary>Backing field for <see cref="IsSelected"/>.</summary>
    private bool _isSelected;
    /// <summary>True when the row's checkbox is checked.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Standard <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event the SfDataGrid uses to observe <see cref="IsSelected"/>
    /// toggles.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
