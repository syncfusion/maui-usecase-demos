#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
// Microsoft.UI.Xaml provides the WinUI Application type
// (Application.Current). It's referenced via a fully-qualified name
// inside Apply() because the MAUI host also adds a using for
// Microsoft.Maui.Controls.Application, which makes the bare
// "Application" identifier ambiguous.
using WinRT.Interop;

namespace HotelManagerSample.WinUI;

/// <summary>
/// Strips the OS title-bar strip from MAUI's Windows window and paints
/// the chrome in the page background color (#F6F8FD) so the white gap
/// that previously appeared above every page disappears.
/// </summary>
internal static class TitleBarController
{
    // Matches <ContentPage BackgroundColor="#F6F8FD"> in the desktop
    // pages. Keep both ends in sync.
    private static readonly Windows.UI.Color TitleBarBackground =
        Windows.UI.Color.FromArgb(0xFF, 0xF6, 0xF8, 0xFD);

    /// <summary>Foreground colour used for the title-bar text and
    /// caption glyphs.</summary>
    private static readonly Windows.UI.Color TitleBarForeground =
        Windows.UI.Color.FromArgb(0xFF, 0x17, 0x24, 0x3A);

    /// <summary>
    /// Subscribes to the WinUI host's <see cref="Application.WindowCreated"/>
    /// event (MAUI's <see cref="global::Microsoft.Maui.Controls.Application"/>
    /// does not expose one). The runtime instance is
    /// <see cref="MauiWinUIApplication"/> — reachable as
    /// <see cref="Application.Current"/>.
    /// </summary>
    public static void Apply()
    {
        // Application.Current lives on the WinUI App type — fully-qualified
        // so MAUI's Application doesn't shadow it.
        var winUiApp = Microsoft.UI.Xaml.Application.Current;
        if (winUiApp is null)
            return;

        // Use reflection to find the WindowCreated event regardless of which
        // WinAppSDK version is referenced — its argument type moved from
        // Microsoft.UI.Xaml.WindowCreatedEventArgs to Microsoft.UI.WindowCreatedEventArgs
        // across SDK releases, so we don't bind the parameter type statically.
        var eventInfo = winUiApp.GetType().GetEvent("WindowCreated");
        if (eventInfo is null)
            return;

        // Capture our handler in a local so reflection's dynamic invoke
        // doesn't allocate a typed delegate every time.
        EventHandler handler = (_, _) =>
        {
            var windowProp = winUiApp.GetType().GetProperty("MainWindow");
            if (windowProp?.GetValue(winUiApp) is Microsoft.UI.Xaml.Window win)
                Customize(win);
        };
        eventInfo.AddEventHandler(winUiApp, handler);
        _handler = handler;
    }

    /// <summary>Kept around so the JIT doesn't drop the registration.</summary>
    private static EventHandler? _handler;

    /// <summary>Applies the custom-title-bar colour overrides to the
    /// supplied WinUI <paramref name="winUiWindow"/>. Called once per
    /// <see cref="Application.WindowCreated"/> event delivery.</summary>
    /// <param name="winUiWindow">The newly-created WinUI window to
    /// customise. Must be non-null.</param>
    private static void Customize(Microsoft.UI.Xaml.Window winUiWindow)
    {
        var hwnd    = WindowNative.GetWindowHandle(winUiWindow);
        var id      = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWin  = AppWindow.GetFromWindowId(id);

        // 1) Opt the window into custom title-bar drawing. This is
        //    what removes the empty strip from the MAUI layout.
        appWin.TitleBar.ExtendsContentIntoTitleBar = true;

        // 2) Paint every chrome element in our page background color
        //    so the title-bar strip becomes visually invisible.
        var tb = appWin.TitleBar;
        tb.BackgroundColor            = TitleBarBackground;
        tb.InactiveBackgroundColor    = TitleBarBackground;
        tb.ButtonBackgroundColor      = TitleBarBackground;
        tb.ButtonHoverBackgroundColor = TitleBarBackground;
        tb.ButtonPressedBackgroundColor = TitleBarBackground;
        tb.ButtonInactiveBackgroundColor = TitleBarBackground;

        // 3) Foreground / icon colors
        tb.ForegroundColor            = TitleBarForeground;
        tb.InactiveForegroundColor    = TitleBarForeground;
        tb.ButtonForegroundColor      = TitleBarForeground;
        tb.ButtonHoverForegroundColor = TitleBarForeground;
        tb.ButtonPressedForegroundColor = TitleBarForeground;
        tb.ButtonInactiveForegroundColor = TitleBarForeground;
    }
}
#endif
