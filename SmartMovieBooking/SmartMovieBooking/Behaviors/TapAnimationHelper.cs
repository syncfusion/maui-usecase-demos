using Microsoft.Maui.Controls;

namespace SmartMovieBooking.Behaviors;

public static class TapAnimationHelper
{
    public static async Task AnimateTapEffectAsync(VisualElement visualElement)
    {
        if (visualElement is null) return;
        if (visualElement.Window is null && visualElement.Handler is null) return;

        var originalBackgroundColor = visualElement.BackgroundColor;
        visualElement.BackgroundColor = Color.FromArgb("#E5E1E3");

        try
        {
            await visualElement.ScaleToAsync(0.92, 60, Easing.CubicOut);
            await visualElement.ScaleToAsync(1, 80, Easing.CubicIn);
        }
        finally
        {
            await Task.Delay(40);
            visualElement.BackgroundColor = originalBackgroundColor;
        }
    }
}