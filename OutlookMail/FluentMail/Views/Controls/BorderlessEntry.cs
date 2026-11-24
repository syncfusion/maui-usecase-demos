#if ANDROID
using Android.Content.Res;
#endif
using Microsoft.Maui.Platform;

namespace FluentMail.Views.Controls;

/// <summary>
/// Plain <see cref="Entry"/> subclass (not a Syncfusion control) that removes the
/// native underline on Android, the focus border on Windows and the rounded border
/// on Mac Catalyst. Use it inside a styled container (e.g. a <c>Border</c>) when
/// you want a custom input without platform chrome.
/// </summary>
internal sealed class BorderlessEntry : Entry
{
    static bool registered;

    /// <summary>
    /// Appends the per-platform mapper once for the whole app (called from
    /// <c>MauiProgram</c>). The mapper only affects <see cref="BorderlessEntry"/>
    /// instances; regular <see cref="Entry"/> controls keep their native look.
    /// </summary>
    internal static void RegisterHandlerMappings()
    {
        if (registered) return;
        registered = true;

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
            if (v is BorderlessEntry)
            {
                h.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            }
        });
#endif
#if WINDOWS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
            if (v is BorderlessEntry)
            {
                h.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                // MAUI's default WinUI TextBox padding (≈10 px top) drops the
                // entry text below the surrounding icon's center line. Reset
                // the padding and let VerticalContentAlignment = Center float
                // the text in the middle of the field.
                h.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
                h.PlatformView.VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center;
                h.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
            }
        });
#endif
#if MACCATALYST
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
            if (v is BorderlessEntry)
            {
                h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            }
        });
#endif
    }
}
