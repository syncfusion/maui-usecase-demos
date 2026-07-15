using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using HospitalManagement.ViewModels;

namespace HospitalManagement
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
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                });

            // Register ViewModels
            builder.Services.AddSingleton<PatientRecordsViewModel>();
            builder.Services.AddSingleton<PatientDetailsViewModel>();
            builder.Services.AddSingleton<DashboardViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
