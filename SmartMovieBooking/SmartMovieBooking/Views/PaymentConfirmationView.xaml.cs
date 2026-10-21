using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class PaymentConfirmationView : ContentPage
{
    private readonly PaymentConfirmationViewModel _vm;
    private readonly IBookingRepository _repo;

    public PaymentConfirmationView(PaymentConfirmationViewModel vm, IBookingRepository repo)
    {
        InitializeComponent();
        _vm = vm;
        _repo = repo;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.PopulateFromSession();
        await _vm.ProcessAsync();
        MovieTitleLabel.Text = _vm.MovieTitle;
        TheaterLabel.Text = _vm.TheaterName;
        ShowtimeLabel.Text = _vm.ShowtimeLabel;
        SeatsLabel.Text = $"Seats: {_vm.SeatsDisplay}";
        BookingIdLabel.Text = _vm.BookingId;
        TotalLabel.Text = _vm.FormattedTotal;
    }

    private async void OnDoneClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//theaters");

    private async void OnViewBookingsClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("//bookings");
}
