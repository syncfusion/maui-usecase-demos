using StockChart.ViewModel;

namespace StockChart
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();
            Items.Clear();
            
            var shellContent = new ShellContent
            {
                Title = "Home",
                Content = mainPage,
                Route = "MainPage"
            };
            
            Items.Add(shellContent);
        }

        public AppShell() : this(App.ServiceProvider.GetRequiredService<MainPage>())
        {
            // Fallback constructor for design-time support
        }
    }
}
