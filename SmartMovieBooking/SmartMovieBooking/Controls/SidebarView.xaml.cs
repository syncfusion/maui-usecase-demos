using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Controls;

public partial class SidebarView : ContentView, INotifyPropertyChanged
{
    public SidebarView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create(
        nameof(CurrentPage),
        typeof(MainPage),
        typeof(SidebarView),
        defaultValue: MainPage.MovieHome,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnCurrentPageChanged);

    public MainPage CurrentPage
    {
        get => (MainPage)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public bool IsMovieHomeSelected => CurrentPage == MainPage.MovieHome;
    public bool IsTheatersSelected  => CurrentPage == MainPage.Theaters;
    public bool IsBookingsSelected  => CurrentPage == MainPage.Bookings;

    private static void OnCurrentPageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SidebarView view)
        {
            view.OnPropertyChanged(nameof(IsMovieHomeSelected));
            view.OnPropertyChanged(nameof(IsTheatersSelected));
            view.OnPropertyChanged(nameof(IsBookingsSelected));
        }
    }

    private void OnMovieHomeTapped(object? sender, TappedEventArgs e) => CurrentPage = MainPage.MovieHome;
    private void OnTheatersTapped(object? sender, TappedEventArgs e) => CurrentPage = MainPage.Theaters;
    private void OnBookingsTapped(object? sender, TappedEventArgs e) => CurrentPage = MainPage.Bookings;

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}