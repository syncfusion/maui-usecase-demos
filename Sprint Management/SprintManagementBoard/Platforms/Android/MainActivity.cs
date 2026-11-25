using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using AndroidX.Core.View;

namespace SprintManagementBoard;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        UpdateSystemBars();
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        UpdateSystemBars();
    }

    // Dark status-bar icons on the light page, light icons on the dark page.
    private void UpdateSystemBars()
    {
        var dark = (Resources?.Configuration?.UiMode & UiMode.NightMask) == UiMode.NightYes;
        if (Window?.DecorView is { } decor)
        {
            var controller = WindowCompat.GetInsetsController(Window, decor);
            if (controller != null) controller.AppearanceLightStatusBars = !dark;
        }
    }
}
