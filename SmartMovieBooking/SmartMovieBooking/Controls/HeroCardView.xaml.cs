using SmartMovieBooking.Models;

namespace SmartMovieBooking.Controls;

public partial class HeroCardView : ContentView
{
    public HeroCardView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty MovieProperty = BindableProperty.Create(
        nameof(Movie), typeof(Movie), typeof(HeroCardView),
        propertyChanged: OnMovieChanged);

    public Movie? Movie
    {
        get => (Movie?)GetValue(MovieProperty);
        set => SetValue(MovieProperty, value);
    }

    public event EventHandler<Movie>? Clicked;

    private static void OnMovieChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeroCardView view && newValue is Movie)
            view.BindingContext = newValue;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Movie is not null)
            Clicked?.Invoke(this, Movie);
    }
}