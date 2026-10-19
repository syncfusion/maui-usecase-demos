using Microsoft.Extensions.Logging;
using PatientCare.Services;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;

namespace PatientCare
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore();

            builder.Services.AddSingleton<DashboardDataService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<PatientDataService>();
            builder.Services.AddSingleton<ScheduleDataService>();
            builder.Services.AddSingleton<CarePlanDataService>();
            
            // Register pages for DI
            builder.Services.AddTransient<Views.SplashPage>();
            builder.Services.AddTransient<Views.DashboardPage>();
            builder.Services.AddTransient<Views.PatientsPage>();
            builder.Services.AddTransient<Views.AppointmentsPage>();
            builder.Services.AddTransient<Views.CarePlansPage>();

            builder
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // App icons use the bundled Google Material Icons font directly; MauiMaterialAssets (Syncfusion's
                    // internal icon font) was not rendering reliably and is only kept for Syncfusion controls' own icons.
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

