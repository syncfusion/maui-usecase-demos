using FluentMail.Views;

namespace FluentMail;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ComposePage), typeof(ComposePage));
    }
}
