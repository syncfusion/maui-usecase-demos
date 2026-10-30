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
            // ----- Syncfusion hosts (HARNESS §3, Architecture Rule 5) -----
            // ConfigureSyncfusionCore loads the Syncfusion.Maui.Core
            // assembly AND the Syncfusion.Maui.Popup assembly (SfPopup
            // ships under the same Syncfusion.Maui.Core umbrella in
            // this release). It registers control themes, handlers,
            // and makes SfBadgeView / SfAvatarView / SfPopup etc.
            // available with the MaterialLight palette pulled in from
            // App.xaml's SyncfusionThemeResourceDictionary.
            .ConfigureSyncfusionCore()
            // ConfigureSyncfusionToolkit loads the Syncfusion.Maui.Toolkit
            // assembly (SfStepProgressBar, SfChips, SfEffectsView, etc.).
            // Both calls are required so that every Syncfusion control
            // registered in Resources/Styles/Colors.xaml can resolve
            // its theme brushes at inflation time.
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
            });

        // ----- Services (HARNESS §6 rule 1 — single source of truth) -----
        // IVisitorStore is the single source of truth for visitor records.
        // Registered as a SINGLETON so every page + ViewModel that resolves
        // it gets the same instance — mutations on one screen (e.g. a
        // Check In on the Digital Pass page) are immediately visible to
        // every other screen bound to the same collection.
        builder.Services.AddSingleton<IVisitorStore, VisitorStore>();

        // PassCodeGenerator is also a SINGLETON. It holds the per-day
        // running counter that hands out distinct VP-yyyyMMdd-#### codes.
        // A second instance would race the first one and mint duplicate
        // codes; singleton ensures every AddVisitor sees the same
        // counter state.
        builder.Services.AddSingleton<PassCodeGenerator>();

        // ThemeService (VP-080) — singleton so the toggle state
        // persists across page navigations and every page that
        // binds to its CurrentTheme property sees the same value.
        builder.Services.AddSingleton<ThemeService>();

        // ----- ViewModels (VP-030 — Phase 3 Shell wiring) -----
        // Each ViewModel is a singleton so the same instance backs the
        // page it inflates for and survives any
        // Navigation.PushAsync / GoToAsync round-trip. The
        // IVisitorStore dependency is resolved from DI.
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<DirectoryViewModel>();
        builder.Services.AddSingleton<VisitorDetailsViewModel>();

        // RegisterVisitorViewModel (VP-070 — Phase 7). NOT a singleton
        // — each opening of the registration surface gets a fresh
        // VM so the wizard's fields reset between consecutive
        // registrations.
        builder.Services.AddTransient<RegisterVisitorViewModel>();

        // ----- Pages (VP-030) -----
        // Registered as singletons (HARNESS §6 rule 4: single page,
        // two form factors). ShellContent.ContentTemplate inflates
        // each once and keeps it cached. DI resolves the
        // view-model into the constructor.
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<DirectoryPage>();
        builder.Services.AddSingleton<VisitorDetailsPage>();

        // RegisterVisitorPage (VP-071 — Phase 7). Transient because
        // it inflates inside SfPopup / Navigation.PushModalAsync on
        // every "Register Visitor" tap.
        builder.Services.AddTransient<RegisterVisitorPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
