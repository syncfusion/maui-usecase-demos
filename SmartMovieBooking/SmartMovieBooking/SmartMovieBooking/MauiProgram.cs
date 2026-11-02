using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;
using Syncfusion.Maui.Toolkit.Hosting;

namespace SmartMovieBooking;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var syncfusionLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
        }

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionToolkit() // wires up SfCardView, SfChipGroup, SfTextInputLayout, ...
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
