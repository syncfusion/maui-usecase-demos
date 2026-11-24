using FluentMail.Data;
using FluentMail.Services;
using FluentMail.ViewModels;
using FluentMail.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace FluentMail;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
            });

        // Strip native Entry chrome once, for BorderlessEntry instances only.
        Views.Controls.BorderlessEntry.RegisterHandlerMappings();

        builder.Services.AddSingleton(MockData.Mailbox);
        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<ComposeViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<MailWorkspaceViewModel>();
        builder.Services.AddTransient<MailWorkspacePage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<CalendarPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
