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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _vm.PopulateFromSession();
        BookingIdLabel.Text = _vm.BookingId;
        TotalLabel.Text = _vm.FormattedTotal;
        MovieLabel.Text = _vm.MovieTitle;
        TheaterLabel.Text = _vm.TheaterName;
        ShowtimeLabel.Text = _vm.ShowtimeLabel;
        SeatsLabel.Text = $"Seats: {_vm.SeatsDisplay}";
        await _vm.ProcessAsync();
    }

    private async void OnDoneClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//home");
    }
}
