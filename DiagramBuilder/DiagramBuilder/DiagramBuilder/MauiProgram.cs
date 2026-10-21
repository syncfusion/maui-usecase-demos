using DiagramBuilder.ViewModels;
using DiagramBuilder.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;

namespace DiagramBuilder
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

            // ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CreateDiagramViewModel>();
            builder.Services.AddSingleton<DiagramDesignerViewModel>();

            // Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CreateDiagramPage>();
            builder.Services.AddTransient<DiagramDesignerPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
