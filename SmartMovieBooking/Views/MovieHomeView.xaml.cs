using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class MovieHomeView : ContentPage
{
    public MovieHomeView(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
