using Microsoft.Extensions.DependencyInjection;
using StockChart.ViewModels;
using StockChart.Services;

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
            var appShell = serviceProvider.GetRequiredService&lt;AppShell&gt;();
            MainPage = appShell;
        }
    }
}