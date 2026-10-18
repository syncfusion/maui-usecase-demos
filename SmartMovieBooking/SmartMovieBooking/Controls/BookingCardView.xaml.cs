using SmartMovieBooking.Models;

namespace SmartMovieBooking.Controls;

public partial class BookingCardView : ContentView
{
    public BookingCardView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty BookingProperty = BindableProperty.Create(
        nameof(Booking), typeof(Booking), typeof(BookingCardView),
        propertyChanged: OnBookingChanged);

    public Booking? Booking
    {
        get => (Booking?)GetValue(BookingProperty);
        set => SetValue(BookingProperty, value);
    }

    private static void OnBookingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BookingCardView view && newValue is Booking)
            view.BindingContext = newValue;
    }
}