using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class BookingsView : ContentView
{
    private readonly BookingsViewModel _vm;

    public BookingsView(BookingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private void OnViewTicketClicked(object? sender, EventArgs e)
    {
        // Sample only: navigate to the booking confirmation popup.
        Shell.Current.GoToAsync("payment-confirmation");
    }
}