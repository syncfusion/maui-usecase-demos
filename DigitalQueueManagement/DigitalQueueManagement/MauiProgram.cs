using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using DigitalQueueManagement.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;
#if MAUI_DEVFLOW
using Microsoft.Maui.DevFlow.Agent;
#endif

namespace DigitalQueueManagement;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
		if (!string.IsNullOrWhiteSpace(licenseKey))
		{
			SyncfusionLicenseProvider.RegisterLicense(licenseKey);
		}

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureSyncfusionCore()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("UIFontIcons.ttf", "UIFontIcons");
				fonts.AddFont("MauiMaterialAssets.ttf", "MauiMaterialAssets");
				fonts.AddFont("materialicon-regular.ttf", "MaterialIcons");
			});

#if MAUI_DEVFLOW
		builder.AddMauiDevFlowAgent();
#endif

		builder.Services.AddSingleton<QueueStore>();
		builder.Services.AddSingleton<QueueSessionViewModel>();
		builder.Services.AddTransient<AppShellViewModel>();
		builder.Services.AddTransient<QueueOverviewViewModel>();
		builder.Services.AddTransient<TokenManagementViewModel>();
		builder.Services.AddTransient<CounterOperationsViewModel>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
