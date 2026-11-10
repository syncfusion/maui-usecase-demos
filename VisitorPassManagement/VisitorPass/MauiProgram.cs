using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using VisitorPass.Services;
using VisitorPass.ViewModels;
using VisitorPass.Views;

namespace VisitorPass;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // ConfigureSyncfusionCore loads the Syncfusion.Maui.Core
            // assembly (SfBadgeView, SfAvatarView, etc.) and registers
            // the MaterialLight palette + handlers.
            .ConfigureSyncfusionCore()
            // ConfigureSyncfusionToolkit loads the Syncfusion.Maui.Toolkit
            // assembly (SfStepProgressBar, SfChips, SfEffectsView,
            // SfTextInputLayout, SfDatePicker, SfTimePicker, etc.). Both
            // calls are required so every control registered through
            // SyncfusionThemeResourceDictionary resolves at inflation time.
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
            });

        // ----- Services -----
        // IVisitorStore is the single source of truth for visitor
        // records. Registered as a SINGLETON so every page +
        // view-model that resolves it gets the same instance —
        // mutations on one screen (e.g. a Check In on the Digital
        // Pass page) are immediately visible to every other screen
        // bound to the same collection.
        builder.Services.AddSingleton<IVisitorStore, VisitorStore>();

        // PassCodeGenerator is also a SINGLETON. It holds the
        // per-day running counter that hands out distinct
        // VP-{yyyyMMdd}-{####} codes. A second instance would race the
        // first and mint duplicate codes.
        builder.Services.AddSingleton<PassCodeGenerator>();

        // ThemeService is a singleton so the toggle state survives
        // page navigations and every surface that exposes a theme
        // switch reads the same instance.
        builder.Services.AddSingleton<ThemeService>();

        // ----- ViewModels -----
        // Dashboard, Directory, and VisitorDetails view-models are
        // singletons so the same instance backs the page it inflates
        // for and survives any Navigation.PushAsync round-trip.
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<DirectoryViewModel>();
        builder.Services.AddSingleton<VisitorDetailsViewModel>();

        // RegisterVisitorViewModel is TRANSIENT — each opening of
        // the registration surface (SfPopup on desktop/tablet or
        // push page on phone) gets a fresh view-model so the
        // wizard's fields reset between consecutive registrations.
        builder.Services.AddTransient<RegisterVisitorViewModel>();

        // ----- Pages -----
        // DashboardPage is the singleton root resolved by
        // App.CreateWindow. DirectoryPage and VisitorDetailsPage are
        // also singletons because Navigation.PushAsync reuses an
        // existing instance when available.
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<DirectoryPage>();
        builder.Services.AddSingleton<VisitorDetailsPage>();

        // RegisterVisitorPage is transient because it inflates a
        // fresh registration form on every registration open.
        builder.Services.AddTransient<RegisterVisitorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
