namespace DigitalQueueManagement.Controls;

public partial class SummaryCardIcon : ContentView
{
    public static readonly BindableProperty IconNameProperty = BindableProperty.Create(
        nameof(IconName),
        typeof(string),
        typeof(SummaryCardIcon),
        string.Empty);

    public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
        nameof(TintColor),
        typeof(Color),
        typeof(SummaryCardIcon),
        Colors.Transparent);

    public static readonly BindableProperty IconColorProperty = BindableProperty.Create(
        nameof(IconColor),
        typeof(Color),
        typeof(SummaryCardIcon),
        Colors.Transparent);

    public SummaryCardIcon()
    {
        InitializeComponent();
    }

    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
}
