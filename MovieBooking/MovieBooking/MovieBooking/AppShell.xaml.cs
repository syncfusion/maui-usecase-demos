using MovieBooking.Views;

namespace MovieBooking;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Modal/push routes (not tabs)
        Routing.RegisterRoute("seat-selection", typeof(SeatSelectionView));
        Routing.RegisterRoute("ai", typeof(AiCopilotView));
    }
}
