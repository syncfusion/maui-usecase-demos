using Syncfusion.Maui.Toolkit.Hosting;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace FileExplorerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<PopupService>();
            builder.ConfigureSyncfusionCore();
            return builder.Build();
        }
    }
}
