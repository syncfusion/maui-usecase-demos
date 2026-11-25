using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace SmartExpense;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureSyncfusionCore()
			.UseMauiCommunityToolkit(options =>
			{
				options.SetShouldEnableSnackbarOnWindows(true);
			}
			)
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("expense_tracker_icon.ttf", "ExpenseTrackerIcons");
                fonts.AddFont("MaterialIconsOutlined-Regular.otf", "MaterialIcons");
            });

		SyncfusionLicenseProvider.RegisterLicense(AppConstants.SyncfusionLicenseKey);

		builder.Services.AddSingleton<InMemoryExpenseStore>();
		builder.Services.AddSingleton<IAppStateService, AppStateService>();
		builder.Services.AddSingleton<ITransactionRepository, TransactionRepository>();
		builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
		builder.Services.AddSingleton<IUserRepository, UserRepository>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<ITransactionService, TransactionService>();

		builder.Services.AddTransient<SetupProfileViewModel>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<AddTransactionViewModel>();
		builder.Services.AddTransient<TransactionsViewModel>();

		builder.Services.AddTransient<SetupProfilePage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<DashboardPage>();
		builder.Services.AddTransient<AddTransactionPage>();
		builder.Services.AddTransient<TransactionsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Initialize shared in-memory state asynchronously to avoid blocking UI thread at startup.
        // AppStateService will marshal UI collection population to the main thread.
        try
        {
            var state = app.Services.GetService<IAppStateService>();
            if (state is not null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await state.InitializeAsync();
                    }
                    catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }
                });
            }
        }
        catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }

        return app;
    }
}
