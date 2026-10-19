using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class ProfileView : ContentView
{
    public ProfileView(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}