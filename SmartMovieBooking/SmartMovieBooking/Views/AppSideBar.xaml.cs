namespace SmartMovieBooking.Views;

public partial class AppSideBar : ContentView
{
    public AppSideBar()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(string), typeof(AppSideBar), "Movies");

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    private async void OnMoviesTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//home");

    private async void OnTheatersTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//theaters");

    private async void OnBookingsTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//bookings");
}
