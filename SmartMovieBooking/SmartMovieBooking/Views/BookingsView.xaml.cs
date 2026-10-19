using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class BookingsView : ContentPage
{
    public BookingsView(BookingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
