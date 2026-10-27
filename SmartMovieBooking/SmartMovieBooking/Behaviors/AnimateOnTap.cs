using Microsoft.Maui.Controls;

namespace SmartMovieBooking.Behaviors;

public static class AnimateOnTap
{
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.CreateAttached(
        "IsEnabled",
        typeof(bool),
        typeof(AnimateOnTap),
        false,
        propertyChanged: OnIsEnabledChanged);

    public static bool GetIsEnabled(BindableObject view) =>
        (bool)view.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(BindableObject view, bool value) =>
        view.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view) return;

        for (int i = view.Behaviors.Count - 1; i >= 0; i--)
        {
            if (view.Behaviors[i] is TapAnimationBehavior)
                view.Behaviors.RemoveAt(i);
        }

        if (newValue is bool enabled && enabled)
        {
            view.Behaviors.Add(new TapAnimationBehavior());
        }
    }
}