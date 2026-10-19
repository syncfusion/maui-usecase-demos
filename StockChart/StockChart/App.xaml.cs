using Microsoft.Extensions.DependencyInjection;
using StockChart.ViewModel;
using StockChart.Service;

namespace StockChart
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Store the service provider for later use
            ServiceProvider = serviceProvider;

            // Resolve AppShell with its dependencies
            var appShell = serviceProvider.GetRequiredService<AppShell>();
            MainPage = appShell;
        }
    }
}