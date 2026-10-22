using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Register the Syncfusion license key. The placeholder string below
        // is valid as a community-license trial; replace with your own key
        // for production builds to remove the trial watermark.
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            "Ngo9BigBOggjHTQxAR8/V1JNaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWX5dcXRXRGZcUUBwXUJZWEM=");

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore() // wires up SfButton, SfListView, SfPopup, ...
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services (singletons - mock data, no network)
        builder.Services.AddSingleton<IMovieService, MovieService>();
        builder.Services.AddSingleton<ITheaterService, TheaterService>();
        builder.Services.AddSingleton<IShowtimeService, ShowtimeService>();
        builder.Services.AddSingleton<ISeatLayoutFactory, SeatLayoutFactory>();
        builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
        builder.Services.AddSingleton<BookingSession>();
        builder.Services.AddSingleton<IAiCopilotService, AiCopilotService>();

        // ViewModels (transient)
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TheatersViewModel>();
        builder.Services.AddTransient<BookingsViewModel>();
        builder.Services.AddTransient<SeatSelectionViewModel>();
        builder.Services.AddTransient<PaymentConfirmationViewModel>();
        builder.Services.AddTransient<AiCopilotViewModel>();

        // Views (transient - DI constructs with their VM)
        builder.Services.AddTransient<MovieHomeView>();
        builder.Services.AddTransient<TheatersView>();
        builder.Services.AddTransient<BookingsView>();
        builder.Services.AddTransient<SeatSelectionView>();
        builder.Services.AddTransient<PaymentConfirmationView>();
        builder.Services.AddTransient<AiCopilotView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
