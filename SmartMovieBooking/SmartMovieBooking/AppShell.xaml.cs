using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("ai", typeof(AiCopilotSheet));
        Routing.RegisterRoute("payment-confirmation", typeof(PaymentConfirmationView));
        Routing.RegisterRoute("seat-selection", typeof(SeatSelectionView));
    }
}