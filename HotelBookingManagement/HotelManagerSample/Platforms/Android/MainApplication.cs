using Android.App;
using Android.Runtime;

namespace HotelManagerSample;

/// <summary>
/// Android <see cref="MauiApplication"/> bootstrapper. Constructs the
/// shared <see cref="MauiApp"/> built by <see cref="MauiProgram"/>.
/// </summary>
[Application]
/// <summary>
/// Android <see cref="MauiApplication"/> bootstrapper. Constructs the
/// shared <see cref="MauiApp"/> built by <see cref="MauiProgram"/>.
/// </summary>
public class MainApplication : MauiApplication
{
    /// <summary>
    /// Native Android constructor. Forwards the handle and ownership to
    /// <see cref="MauiApplication"/> which handles the managed/native
    /// bridging for the lifetime of the process.
    /// </summary>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <summary>Returns the shared MAUI app instance configured by
    /// <see cref="MauiProgram.CreateMauiApp"/>.</summary>
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
