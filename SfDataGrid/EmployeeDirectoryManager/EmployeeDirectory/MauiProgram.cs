using Microsoft.Extensions.Logging;
using EmployeeDirectory.Views;
using Syncfusion.Maui.Core.Hosting;

namespace EmployeeDirectory
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                // Register Syncfusion MAUI
                .ConfigureSyncfusionCore()
                // Register Views for navigation
                .ConfigureRouting();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static MauiAppBuilder ConfigureRouting(this MauiAppBuilder builder)
        {
            // Register pages for navigation
            return builder;
        }
    }
}
