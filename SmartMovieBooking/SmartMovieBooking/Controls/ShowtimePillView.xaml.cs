using System.ComponentModel;
using System.Runtime.CompilerServices;
using SmartMovieBooking.Models;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Controls;

public partial class ShowtimePillView : ContentView, INotifyPropertyChanged
{
    public ShowtimePillView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ShowtimeProperty = BindableProperty.Create(
        nameof(Showtime), typeof(Showtime), typeof(ShowtimePillView),
        propertyChanged: OnShowtimeChanged);

    public Showtime? Showtime
    {
        get => (Showtime?)GetValue(ShowtimeProperty);
        set => SetValue(ShowtimeProperty, value);
    }

    public event EventHandler<Showtime>? Clicked;

    private static void OnShowtimeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ShowtimePillView view && newValue is Showtime)
            view.BindingContext = newValue;
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Showtime is not null)
        {
            Clicked?.Invoke(this, Showtime);
            Services.EventBus.PublishShowtimeClicked(Showtime);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}