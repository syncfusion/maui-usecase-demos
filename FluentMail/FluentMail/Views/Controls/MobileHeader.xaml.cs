namespace FluentMail.Views.Controls;

public partial class MobileHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MobileHeader), string.Empty);
    public static readonly BindableProperty InitialsProperty = BindableProperty.Create(nameof(Initials), typeof(string), typeof(MobileHeader), "KB");

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Initials { get => (string)GetValue(InitialsProperty); set => SetValue(InitialsProperty, value); }

    public MobileHeader() => InitializeComponent();
}
