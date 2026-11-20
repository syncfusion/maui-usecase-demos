using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace SmartExpense;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Use AdjustResize so the window resizes when the keyboard appears.
        // This keeps top UI (AppBar) fixed while only the content area adjusts.
        Window.SetSoftInputMode(SoftInput.AdjustResize);
    }
}
