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
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureSyncfusionCore();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Services (singletons — mock data, no network)
        builder.Services.AddSingleton<IMovieService, MovieService>();
        builder.Services.AddSingleton<ITheaterService, TheaterService>();
        builder.Services.AddSingleton<IShowtimeService, ShowtimeService>();
        builder.Services.AddSingleton<ISeatLayoutFactory, SeatLayoutFactory>();
        builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
        builder.Services.AddSingleton<BookingSession>();
        builder.Services.AddSingleton<IAiCopilotService, AiCopilotService>();

        // ViewModels (transient)
        builder.Services.AddTransient<HomeViewModel>();

        // Views (transient — DI constructs with their VM)
        builder.Services.AddTransient<MovieHomeView>();

        return builder.Build();
    }
}
