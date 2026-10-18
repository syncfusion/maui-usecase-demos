using SmartMovieBooking.Services;

namespace SmartMovieBooking.Views;

public partial class BookingsView : ContentView
{
    public BookingsView(BookingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}