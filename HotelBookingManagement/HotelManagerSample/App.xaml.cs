using HotelManagerSample;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagerSample;

/// <summary>
/// MAUI application root. Resolves a root page based on the running
/// platform (mobile shell vs. NavigationPage + DashboardPage) and
/// captures the <see cref="IServiceProvider"/> so MAUI-inflated pages
/// (which Shell doesn't construct via DI) can still reach the shared
/// <see cref="HotelManagerSample.IHotelStore"/> and view-models.
/// </summary>
public partial class App : Application
{
    /// <summary>Application-wide service provider. Exposed so content
    /// pages (which MAUI instantiates via Shell's DataTemplate, not via
    /// DI) can still resolve the shared <see cref="HotelManagerSample.IHotelStore"/>
    /// and the singleton view-models. Populated from the MAUI host
    /// context via <see cref="CaptureServices"/>.</summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>Constructs the root MAUI <see cref="App"/> and
    /// inflates the shared <c>App.xaml</c> resource dictionary.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>Builds the root <see cref="Window"/> for the
    /// process. Picks the mobile shell on phones / iOS / Android and
    /// the desktop NavigationPage-based surface on every other idiom.
    /// </summary>
    /// <param name="activationState">MAUI-supplied activation
    /// arguments. Unused by the override.</param>
    /// <returns>The configured <see cref="Window"/> that will host
    /// the rest of the UI tree.</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var root = IsMobileRuntime()
            ? (Page)new MobileShell()
            : new NavigationPage(new DashboardPage());

        var window = new Window(root);
        window.Created += (_, _) => CaptureServices();
        return window;
    }

    /// <summary>
    /// Returns true when the current platform should be served the
    /// Mobile UI. iOS and Android (the brief's supported target list)
    /// always take the mobile path. Phones in any platform family —
    /// detected via <see cref="DeviceInfo.Idiom"/> = Phone — also
    /// take the mobile path even when running on a Windows device
    /// (some tablet chassis surface Phone enough to warrant it).
    /// </summary>
    /// <returns><c>true</c> when the mobile shell should be the
    /// root page; <c>false</c> when the desktop surface is
    /// appropriate.</returns>
    public static bool IsMobileRuntime()
    {
        // Phone = always mobile.
        if (DeviceInfo.Idiom == DeviceIdiom.Phone)
            return true;

        // Tablet / desktop / unknown — keep the existing desktop UI
        // unless we're on iOS/Android, where the page widths are too
        // narrow for the sidebar.
        if (DeviceInfo.Platform == DevicePlatform.iOS
         || DeviceInfo.Platform == DevicePlatform.Android)
            return true;

        return false;
    }

    /// <summary><see cref="Window.Created"/> only fires after the
    /// handler chain is attached. The pages themselves call this
    /// from their constructor so DI works even on the very first
    /// page that loads before <c>window.Created</c> has had a chance
    /// to run. Idempotent — multiple calls are safe.</summary>
    public static void CaptureServices()
    {
        if (Services is not null)
            return;
        Services = Current?.Handler?.MauiContext?.Services;
    }

    /// <summary>Resolves <typeparamref name="T"/> from the DI container,
    /// or throws if the host context has not yet been initialised.</summary>
    /// <typeparam name="T">The service type to resolve. Constrained
    /// to non-nullable reference types because the resolved instance
    /// is always dereferenced.</typeparam>
    /// <returns>The resolved instance of <typeparamref name="T"/>
    /// from the captured service provider.</returns>
    public static T Resolve<T>() where T : notnull
    {
        CaptureServices();
        return Services!.GetRequiredService<T>();
    }
}
