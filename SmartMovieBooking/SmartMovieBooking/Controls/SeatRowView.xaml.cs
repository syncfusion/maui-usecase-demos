using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Controls;

public partial class SeatRowView : ContentView
{
    public SeatRowView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty RowProperty = BindableProperty.Create(
        nameof(Row), typeof(SeatRowGroup), typeof(SeatRowView),
        propertyChanged: OnRowChanged);

    public SeatRowGroup? Row
    {
        get => (SeatRowGroup?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    private static void OnRowChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SeatRowView view && newValue is SeatRowGroup row)
            view.BindingContext = row;
    }
}