using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Movie Home",
            Route = "home",
            Content = services.GetRequiredService<MovieHomeView>()
        });
    }
}
