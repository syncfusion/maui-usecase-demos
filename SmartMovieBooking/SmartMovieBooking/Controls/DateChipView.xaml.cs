using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Controls;

public partial class DateChipView : ContentView
{
    public DateChipView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date), typeof(DateTime), typeof(DateChipView),
        propertyChanged: OnDateChanged);

    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(DateChipView), false);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public event EventHandler<DateTime>? Clicked;

    private static void OnDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DateChipView view && newValue is DateTime dt)
        {
            view.BindingContext = new DateChipModel
            {
                Date = dt,
                Label = ComputeLabel(dt),
                DayNumber = dt.Day.ToString(),
                Weekday = $"{dt.ToString("ddd", CultureInfo.InvariantCulture)} • {dt.ToString("MMM", CultureInfo.InvariantCulture)}",
                IsSelected = view.IsSelected
            };
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsSelected) && BindingContext is DateChipModel m)
        {
            m.IsSelected = IsSelected;
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        Clicked?.Invoke(this, Date);
    }

    private static string ComputeLabel(DateTime dt)
    {
        var today = DateTime.Today;
        var days = (dt.Date - today.Date).Days;
        return days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            >= 2 and <= 6 when dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday => "Weekend",
            >= 2 and <= 6 => "Weekday",
            _ => string.Empty
        };
    }
}

public partial class DateChipModel : ObservableObject
{
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _dayNumber = string.Empty;
    [ObservableProperty] private string _weekday = string.Empty;
    [ObservableProperty] private bool _isSelected;
}