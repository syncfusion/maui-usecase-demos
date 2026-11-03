using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Syncfusion.Licensing;

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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
