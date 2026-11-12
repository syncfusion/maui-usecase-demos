using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;
using DiagramBuilder.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace DiagramBuilder
{
    public static class MauiProgram
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static MauiApp CreateMauiApp()
        {

            SyncfusionLicenseProvider.RegisterLicense("Syncfusion_License_Key");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    // Google Material Icons (regular) — used for toolbar/menu/panel icons
                    // via the "MaterialIcons" FontFamily and UTF-32 codepoint text.
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if MAUI_DEVFLOW
            builder.AddMauiDevFlowAgent();
#endif

            // Services (mock implementations, in-memory, no backend)
            builder.Services.AddSingleton<IDiagramService, DiagramService>();
            builder.Services.AddSingleton<ITemplateService, TemplateService>();
            builder.Services.AddSingleton<IDiagramFileService, DiagramFileService>();
            builder.Services.AddSingleton<ISelectionService, SelectionService>();
            builder.Services.AddSingleton<IPageSettingsService, PageSettingsService>();
            builder.Services.AddSingleton<IPropertyService, PropertyService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();

            // ViewModels
            builder.Services.AddSingleton<CreateDiagramViewModel>();
            builder.Services.AddSingleton<DiagramDesignerViewModel>();

            // Pages
            builder.Services.AddTransient<CreateDiagramPage>();
            builder.Services.AddTransient<DiagramDesignerPage>();
            builder.Services.AddTransient<Views.Dialogs.ExportDialog>();
            builder.Services.AddTransient<Views.Dialogs.SaveDialog>();
            builder.Services.AddTransient<Views.Dialogs.PrintDialog>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            ServiceProvider = app.Services;
            return app;
        }
    }
}
