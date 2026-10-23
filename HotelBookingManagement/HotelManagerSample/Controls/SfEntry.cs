#if ANDROID
using Android.Content.Res;
#endif
using Microsoft.Maui.Platform;

namespace HotelManagerSample.Controls;

/// <summary>
/// Plain <see cref="Entry"/> subclass that removes the native underline on
/// Android, the focus border on Windows and the rounded border on Mac
/// Catalyst. Use it inside a styled container (e.g. a <c>Border</c>) when
/// you want to render a custom search/filter input without any platform
/// chrome on top of your visuals.
/// </summary>
internal sealed class SfEntry : Entry
{
    /// <summary>Static constructor registers the per-platform
    /// <c>NoUnderline</c> mapper that strips native chrome from the
    /// entry on the current target. The mapper only fires for
    /// <see cref="SfEntry"/> instances — regular <see cref="Entry"/>
    /// controls keep their native look.</summary>
    public SfEntry()
    {

#if ANDROID
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
            if (v is SfEntry)
            {
                h.PlatformView.BackgroundTintList = ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            }
        });
#endif
#if WINDOWS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (h, v) =>
        {
            if (v is SfEntry)
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
            if (v is SfEntry)
            {
                h.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
            }
        });
#endif
    }
}
