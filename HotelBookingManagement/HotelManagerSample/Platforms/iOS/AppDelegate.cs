using Foundation;

namespace HotelManagerSample;

/// <summary>
/// iOS UIKit application delegate. Bridges into the shared MAUI app
/// produced by <see cref="MauiProgram"/>.
/// </summary>
[Register("AppDelegate")]
/// <summary>
/// iOS UIKit application delegate. Bridges into the shared MAUI app
/// produced by <see cref="MauiProgram"/>.
/// </summary>
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <summary>Builds the shared MAUI app the first time the
    /// UIKit host requests it.</summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
