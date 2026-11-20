using MovieBooking.ViewModels;

namespace MovieBooking.Views;

public partial class BookingsView : ContentPage
{
    public BookingsView(BookingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
