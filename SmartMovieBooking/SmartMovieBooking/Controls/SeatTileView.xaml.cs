using SmartMovieBooking.Models;

namespace SmartMovieBooking.Controls;

public partial class SeatTileView : ContentView
{
    public SeatTileView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty SeatProperty = BindableProperty.Create(
        nameof(Seat), typeof(Seat), typeof(SeatTileView),
        propertyChanged: OnSeatChanged);

    public Seat? Seat
    {
        get => (Seat?)GetValue(SeatProperty);
        set => SetValue(SeatProperty, value);
    }

    public event EventHandler<Seat>? Clicked;

    private static void OnSeatChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SeatTileView view && newValue is Seat)
            view.BindingContext = newValue;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Seat is null) return;
        if (Seat.State == SeatState.Occupied || Seat.State == SeatState.Aisle) return;
        Clicked?.Invoke(this, Seat);
    }
}