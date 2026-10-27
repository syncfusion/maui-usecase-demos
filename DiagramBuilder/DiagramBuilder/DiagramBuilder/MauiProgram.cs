using DiagramBuilder.Services;
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

            // Services (mock implementations, in-memory, no backend)
            builder.Services.AddSingleton<IDiagramService, DiagramService>();
            builder.Services.AddSingleton<ITemplateService, TemplateService>();
            builder.Services.AddSingleton<IExportService, MockExportService>();
            builder.Services.AddSingleton<ISelectionService, SelectionService>();
            builder.Services.AddSingleton<IPropertyService, PropertyService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();

            // ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<CreateDiagramViewModel>();
            builder.Services.AddSingleton<DiagramDesignerViewModel>();

            // Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CreateDiagramPage>();
            builder.Services.AddTransient<DiagramDesignerPage>();
            builder.Services.AddTransient<Views.Dialogs.ExportDialog>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
