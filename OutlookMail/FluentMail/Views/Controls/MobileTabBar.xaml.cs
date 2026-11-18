using System.Windows.Input;

namespace FluentMail.Views.Controls;

public partial class MobileTabBar : ContentView
{
    public static readonly BindableProperty SelectedProperty = BindableProperty.Create(nameof(Selected), typeof(string), typeof(MobileTabBar), "Mail");
    public static readonly BindableProperty BadgeProperty = BindableProperty.Create(nameof(Badge), typeof(int), typeof(MobileTabBar), 0, propertyChanged: (b, _, _) => ((MobileTabBar)b).OnPropertyChanged(nameof(HasBadge)));

    public string Selected { get => (string)GetValue(SelectedProperty); set => SetValue(SelectedProperty, value); }
    public int Badge { get => (int)GetValue(BadgeProperty); set => SetValue(BadgeProperty, value); }
    public bool HasBadge => Badge > 0;

    public ICommand GoCommand { get; } = new Command<string>(route => Shell.Current.GoToAsync($"//{route}"));

    public MobileTabBar() => InitializeComponent();
}
