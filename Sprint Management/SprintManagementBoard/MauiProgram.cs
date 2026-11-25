using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;

namespace SprintManagementBoard;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

			// Register application services and view models.
			builder.ConfigureSyncfusionCore();
			builder.ConfigureSyncfusionToolkit();

			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
			builder.Services.AddSingleton<ITaskService, MockTaskService>();
			builder.Services.AddSingleton<TasksViewModel>();
#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
			// expose the built service provider for app-level resolution
		SprintManagementBoard.App.Services = app.Services;
		return app;
	}
}
