using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Maps;
using PatientCompanion.Services;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentReviewView : ContentView
{
    private readonly WorldMapAssetService? worldMapAsset;

    public AppointmentReviewView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();
        Loaded += OnLoaded;

        var services = IPlatformApplication.Current!.Services;
        worldMapAsset = services.GetService<WorldMapAssetService>();

        BindingContext =
            new AppointmentReviewViewModel(
                appointmentViewModel,
                services.GetRequiredService<AppointmentService>(),
                services.GetRequiredService<MockDataService>());
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (WorldMapLayer.ShapesSource is not null)
            return;

        var asset = worldMapAsset;
        if (asset is null)
            return;

        var stream = asset.ShapesStream;
        if (stream is null)
        {
            // Asset hasn't finished loading yet — wait for it once.
            Dispatcher.Dispatch(async () =>
            {
                var loaded = await asset.GetStreamAsync();
                if (loaded is not null && WorldMapLayer.ShapesSource is null)
                    WorldMapLayer.ShapesSource = loaded;
            });
            return;
        }

        WorldMapLayer.ShapesSource = stream;
    }
}
