using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.Controls;

public partial class MovieCardView : ContentView, INotifyPropertyChanged
{
    public MovieCardView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty MovieProperty = BindableProperty.Create(
        nameof(Movie), typeof(Movie), typeof(MovieCardView),
        propertyChanged: OnMovieChanged);

    public Movie? Movie
    {
        get => (Movie?)GetValue(MovieProperty);
        set => SetValue(MovieProperty, value);
    }

    public event EventHandler<Movie>? Clicked;

    private static void OnMovieChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MovieCardView view && newValue is Movie)
        {
            view.BindingContext = newValue;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (BindingContext is Movie m && Movie != m)
            Movie = m;
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (Movie is not null)
            Clicked?.Invoke(this, Movie);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}