using CommunityToolkit.Mvvm.ComponentModel;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaRegular");
                fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakartaSemiBold");
                fonts.AddFont("PlusJakartaSans-Bold.ttf", "PlusJakartaBold");
                fonts.AddFont("PlusJakartaSans-ExtraBold.ttf", "PlusJakartaExtraBold");
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            })
            // Register Syncfusion MAUI
            .ConfigureSyncfusionCore();

        // Services (singletons)
        builder.Services.AddSingleton<IMovieService, MovieService>();
        builder.Services.AddSingleton<ITheaterService, TheaterService>();
        builder.Services.AddSingleton<IShowtimeService, ShowtimeService>();
        builder.Services.AddSingleton<ISeatLayoutFactory, SeatLayoutFactory>();
        builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
        builder.Services.AddSingleton<BookingSession>();
        builder.Services.AddSingleton<IAiCopilotService, AiCopilotService>();

        // ViewModels (transient)
        builder.Services.AddTransient<MainWindowViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<TheatersViewModel>();
        builder.Services.AddTransient<BookingsViewModel>();
        builder.Services.AddTransient<SeatSelectionViewModel>();
        builder.Services.AddTransient<PaymentConfirmationViewModel>();
        builder.Services.AddTransient<AiCopilotViewModel>();

        // Views (transient)
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddTransient<MovieHomeView>();
        builder.Services.AddTransient<TheatersView>();
        builder.Services.AddTransient<BookingsView>();
        builder.Services.AddTransient<SeatSelectionView>();
        builder.Services.AddTransient<PaymentConfirmationView>();
        builder.Services.AddTransient<AiCopilotSheet>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}