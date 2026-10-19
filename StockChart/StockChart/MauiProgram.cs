using Microsoft.Extensions.Logging;
using StockChart.Service;
using StockChart.ViewModel;
using StockChart.ViewModel.Chart;
using StockChart.ViewModel.Settings;
using StockChart.ViewModel.Stock;
using StockChart.ViewModel.Watchlist;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace StockChart
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            // Register services
            builder.Services.AddSingleton<StockDataService>();
            
            // Register ViewModels
            builder.Services.AddSingleton<ChartViewModel>();
            builder.Services.AddSingleton<WatchlistViewModel>();
            builder.Services.AddSingleton<StockViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<StockChartViewModel>();

            // Register Pages
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<AppShell>();

#if MAUI_DEVFLOW
            builder.AddMauiDevFlowAgent();
#endif

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
