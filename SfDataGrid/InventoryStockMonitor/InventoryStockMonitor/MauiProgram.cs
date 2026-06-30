using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using StockMonitor.Services;
using StockMonitor.ViewModels;
using StockMonitor.Views;

namespace StockMonitor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<InventoryService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<InventoryDetailViewModel>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<InventoryDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
