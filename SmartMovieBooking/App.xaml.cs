namespace SmartMovieBooking;

public partial class App : Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_shell)
        {
            Title = "Smart Movie Ticket Booking v2.5"
        };
}
