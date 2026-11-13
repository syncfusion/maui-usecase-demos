using HotelManagerSample.Bookings;
using HotelManagerSample.Dashboard;
using HotelManagerSample.Rooms;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace HotelManagerSample;

/// <summary>
/// Static MAUI host bootstrapper. Configures Syncfusion, fonts,
/// lifecycle events (Windows title bar), and every service the
/// desktop / mobile pages resolve from DI.
/// </summary>
public static class MauiProgram
{
    /// <summary>Builds the shared <see cref="MauiApp"/> instance and
    /// registers every page + view-model that the rest of the app
    /// reaches through <see cref="App.Services"/>.</summary>
    /// <returns>A configured, unbuilt <see cref="MauiApp"/> ready
    /// to host the rest of the MAUI runtime.</returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                HotelManagerSample.WinUI.TitleBarController.Apply();
            }));
#endif
        });

        // ----- Shared hotel state -----
        builder.Services.AddSingleton<IHotelStore, HotelStore>();

        // ----- Desktop page view-models -----
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddSingleton<RoomsViewModel>();
        builder.Services.AddSingleton<BookingsViewModel>();
        builder.Services.AddTransient<AddBookingViewModel>();

        // ----- Mobile pages -----
        builder.Services.AddSingleton<MobileDashboardViewModel>();
        builder.Services.AddSingleton<MobileBookingsViewModel>();
        builder.Services.AddSingleton<MobileRoomsViewModel>();
        builder.Services.AddTransient<MobileNewBookingViewModel>();
        builder.Services.AddSingleton<MobileShellViewModel>();
        builder.Services.AddSingleton<MobileDashboardPage>();
        builder.Services.AddSingleton<MobileBookingsPage>();
        builder.Services.AddSingleton<MobileRoomsPage>();
        builder.Services.AddTransient<MobileNewBookingPage>();
        builder.Services.AddSingleton<MobileShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
