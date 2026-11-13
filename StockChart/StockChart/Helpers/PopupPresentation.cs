using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Popup;

namespace StockChart.Helpers;

/// <summary>
/// Centralizes sizing, positioning, and theming of the sample's <see cref="SfPopup"/> instances
/// so popup presentation logic stays out of page code-behind.
/// </summary>
internal static class PopupPresentation
{
    /// <summary>Styles a popup as a full-width bottom sheet anchored to the screen bottom.</summary>
    /// <param name="popup">The popup to style.</param>
    /// <param name="screenWidth">Screen width in density-independent units.</param>
    /// <param name="screenHeight">Screen height in density-independent units.</param>
    /// <param name="height">Sheet height; <c>null</c> keeps the popup's current <c>HeightRequest</c>.</param>
    /// <param name="topCornerRadius">Corner radius applied to the sheet's top corners.</param>
    public static void ApplyBottomSheetLayout(SfPopup popup, double screenWidth, double screenHeight, double? height = null, double topCornerRadius = 0)
    {
        popup.WidthRequest = screenWidth;
        if (height.HasValue)
        {
            popup.HeightRequest = height.Value;
        }

        popup.StartX = 0;
        popup.StartY = (int)Math.Max(0, screenHeight - popup.HeightRequest);
        popup.PopupStyle.PopupBackground = Colors.Transparent;
        popup.PopupStyle.Stroke = Colors.Transparent;
        popup.PopupStyle.StrokeThickness = 0;
        popup.PopupStyle.CornerRadius = topCornerRadius > 0
            ? new CornerRadius(topCornerRadius, topCornerRadius, 0, 0)
            : new CornerRadius(0);
        popup.AnimationMode = PopupAnimationMode.SlideOnBottom;
    }

    /// <summary>Applies the rounded card look shared by desktop popups.</summary>
    public static void ApplyDesktopCardStyle(SfPopup popup)
    {
        popup.PopupStyle.CornerRadius = 20;
        popup.PopupStyle.StrokeThickness = 1;
    }

    /// <summary>Applies the rounded card look with light/dark theme-aware colors for desktop popups.</summary>
    public static void ApplyThemedDesktopCardStyle(SfPopup popup)
    {
        ApplyDesktopCardStyle(popup);
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        popup.PopupStyle.PopupBackground = isDark
            ? Color.FromArgb("#1E293B")
            : Colors.White;
        popup.PopupStyle.Stroke = isDark
            ? Color.FromArgb("#334155")
            : Color.FromArgb("#E2E8F0");
    }
}
