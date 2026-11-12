namespace FluentMail.Views;

public partial class MailWorkspacePage : ContentPage
{
    public MailWorkspacePage()
    {
        InitializeComponent();
    }

    async void OnCalendarClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//CalendarPage");
}
