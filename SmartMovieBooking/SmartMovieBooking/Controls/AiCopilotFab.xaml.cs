namespace SmartMovieBooking.Controls;

public partial class AiCopilotFab : ContentView
{
    public AiCopilotFab()
    {
        InitializeComponent();
    }

    public event EventHandler? Clicked;

    private void OnTapped(object? sender, TappedEventArgs e)
        => Clicked?.Invoke(this, EventArgs.Empty);
}