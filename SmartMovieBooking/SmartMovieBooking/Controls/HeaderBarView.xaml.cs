namespace SmartMovieBooking.Controls;

public partial class HeaderBarView : ContentView
{
    public HeaderBarView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty CityLabelProperty = BindableProperty.Create(
        nameof(CityLabel), typeof(string), typeof(HeaderBarView), "New York, USA");
    public string CityLabel
    {
        get => (string)GetValue(CityLabelProperty);
        set => SetValue(CityLabelProperty, value);
    }

    public static readonly BindableProperty UserAvatarInitialsProperty = BindableProperty.Create(
        nameof(UserAvatarInitials), typeof(string), typeof(HeaderBarView), "AC");
    public string UserAvatarInitials
    {
        get => (string)GetValue(UserAvatarInitialsProperty);
        set => SetValue(UserAvatarInitialsProperty, value);
    }
}