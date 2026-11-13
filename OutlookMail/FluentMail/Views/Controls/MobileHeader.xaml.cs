using System.Windows.Input;
using Syncfusion.Maui.Popup;

namespace FluentMail.Views.Controls;

public partial class MobileHeader : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(MobileHeader), string.Empty);
    public static readonly BindableProperty FilterCommandProperty = BindableProperty.Create(nameof(FilterCommand), typeof(ICommand), typeof(MobileHeader), null, propertyChanged: (b, _, _) => ((MobileHeader)b).OnPropertyChanged(nameof(HasFilter)));

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public ICommand? FilterCommand { get => (ICommand?)GetValue(FilterCommandProperty); set => SetValue(FilterCommandProperty, value); }
    public bool HasFilter => FilterCommand is not null;

    public MobileHeader() => InitializeComponent();

    void OnFilterTapped(object? sender, TappedEventArgs e) => FilterPopup.ShowRelativeToView(FilterIcon, PopupRelativePosition.AlignBottom);

    void OnFilterOptionTapped(object? sender, TappedEventArgs e)
    {
        FilterPopup.Dismiss();
        if (FilterCommand?.CanExecute(e.Parameter) == true) FilterCommand.Execute(e.Parameter);
    }
}
