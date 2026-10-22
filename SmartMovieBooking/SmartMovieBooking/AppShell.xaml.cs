using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Modal/push routes (not tabs)
        Routing.RegisterRoute("seat-selection", typeof(SeatSelectionView));
        Routing.RegisterRoute("payment-confirmation", typeof(PaymentConfirmationView));
        Routing.RegisterRoute("ai", typeof(AiCopilotView));
    }
}
