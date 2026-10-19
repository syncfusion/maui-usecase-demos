using Microsoft.Extensions.Logging;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using SmartMovieBooking.Views;

namespace SmartMovieBooking;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
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

        // Views (transient - DI constructs with their VM)
        builder.Services.AddTransient<MovieHomeView>();
        builder.Services.AddTransient<TheatersView>();
        builder.Services.AddTransient<BookingsView>();
        builder.Services.AddTransient<SeatSelectionView>();
        builder.Services.AddTransient<PaymentConfirmationView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
