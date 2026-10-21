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

            SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore();

            builder.Services.AddSingleton<DashboardDataService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddSingleton<PatientDataService>();
            builder.Services.AddSingleton<ScheduleDataService>();
            builder.Services.AddSingleton<CarePlanDataService>();

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

