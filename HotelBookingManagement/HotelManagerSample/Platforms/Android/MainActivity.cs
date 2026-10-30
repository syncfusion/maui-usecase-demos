using Android.App;
using Android.Content.PM;
using Android.OS;

namespace HotelManagerSample;

/// <summary>
/// Single MAUI activity hosted on Android. The activity is a thin host —
/// every platform-specific work is performed inside the MAUI runtime via
/// the registered <see cref="MainApplication"/>.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
/// <summary>
/// Single MAUI activity hosted on Android. The activity is a thin host —
/// every platform-specific work is performed inside the MAUI runtime via
/// the registered <see cref="MainApplication"/>.
/// </summary>
public class MainActivity : MauiAppCompatActivity
{
}
