using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class PaymentConfirmationView : ContentPage
{
    private readonly PaymentConfirmationViewModel _vm;

    public PaymentConfirmationView(PaymentConfirmationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    public void PopulateFromSession() => _vm.PopulateFromSession();
    public Task ProcessAsync() => _vm.ProcessAsync();

    private async void OnDoneClicked(object? sender, EventArgs e)
    {
        EventBus.PublishBookingPopupClosed();
        await Shell.Current.GoToAsync("//home");
    }

    private void OnViewBookingsClicked(object? sender, EventArgs e)
        => EventBus.PublishNavigateToBookings();
}