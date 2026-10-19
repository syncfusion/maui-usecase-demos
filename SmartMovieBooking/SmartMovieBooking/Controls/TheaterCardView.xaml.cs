using SmartMovieBooking.Models;

namespace SmartMovieBooking.Controls;

public partial class TheaterCardView : ContentView
{
    public TheaterCardView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TheaterProperty = BindableProperty.Create(
        nameof(Theater), typeof(Theater), typeof(TheaterCardView),
        propertyChanged: OnTheaterChanged);

    public Theater? Theater
    {
        get => (Theater?)GetValue(TheaterProperty);
        set => SetValue(TheaterProperty, value);
    }

    public event EventHandler<Theater>? ContinueClicked;

    private static void OnTheaterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TheaterCardView view && newValue is Theater t)
            view.BindingContext = t;
    }

    private void OnContinueClicked(object? sender, EventArgs e)
    {
        if (Theater is not null)
        {
            ContinueClicked?.Invoke(this, Theater);
            Services.EventBus.PublishTheaterContinueClicked(Theater);
        }
    }
}