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

    public event EventHandler<Showtime>? ShowtimeSelected;

    private static void OnTheaterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TheaterCardView view && newValue is Theater)
            view.BindingContext = newValue;

        if (bindable is TheaterCardView v && newValue is Theater t)
        {
            // Wire each pill's click to bubble up.
            v.BindingContext = t;
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        // Forward showtime click events when CollectionView instantiates children.
    }
}