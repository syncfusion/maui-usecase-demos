namespace SmartMovieBooking.Controls;

public partial class ConciergePromoView : ContentView
{
    public ConciergePromoView()
    {
        InitializeComponent();
    }

    public event EventHandler? Clicked;

    private void OnTapped(object? sender, TappedEventArgs e)
        => Clicked?.Invoke(this, EventArgs.Empty);
}