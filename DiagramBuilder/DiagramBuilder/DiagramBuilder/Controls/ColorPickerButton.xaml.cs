using System.Windows.Input;

namespace DiagramBuilder.Controls;

/// <summary>
/// Color swatch + SfColorPicker popup (ui-spec.md §3 group-6 and §6 FILL/BORDER).
/// Bindable <see cref="ColorValue"/> is a theme <c>ResourceKeys</c> string
/// (resolved via <see cref="Services.ResourceLookup"/>); <see cref="ColorChangedCommand"/>
/// fires with the new resource key string when the user picks a color.
/// </summary>
public partial class ColorPickerButton : ContentView
{
    public static readonly BindableProperty ColorValueProperty =
        BindableProperty.Create(nameof(ColorValue), typeof(string), typeof(ColorPickerButton), string.Empty,
            propertyChanged: OnColorValueChanged);

    public static readonly BindableProperty ColorChangedCommandProperty =
        BindableProperty.Create(nameof(ColorChangedCommand), typeof(ICommand), typeof(ColorPickerButton));

    public string ColorValue
    {
        get => (string)GetValue(ColorValueProperty);
        set => SetValue(ColorValueProperty, value);
    }

    public ICommand? ColorChangedCommand
    {
        get => (ICommand?)GetValue(ColorChangedCommandProperty);
        set => SetValue(ColorChangedCommandProperty, value);
    }

    public ColorPickerButton()
    {
        InitializeComponent();
        ApplyToSwatch(ColorValue);
    }

    private static void OnColorValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ColorPickerButton btn && newValue is string key)
        {
            btn.ApplyToSwatch(key);
        }
    }

    private void ApplyToSwatch(string key)
    {
        var color = Services.ResourceLookup.Color(key, Colors.White);
        SwatchFill.BackgroundColor = color;
    }

    private void OnSwatchTapped(object? sender, TappedEventArgs e)
    {
        FlyoutBase.SetContext(this, SwatchFrame);
        PickerFlyout.ShowAttachedFlyout(SwatchFrame);
    }

    private void OnColorChanged(object? sender, ColorChangedEventArgs e)
    {
        // Map back to a theme key by nearest match; fall back to a hex key.
        var newKey = FindNearestKey(e.NewData?.Value ?? Colors.White);
        if (ColorChangedCommand?.CanExecute(newKey) == true)
        {
            ColorChangedCommand.Execute(newKey);
        }
    }

    private static string FindNearestKey(Color color)
    {
        var palette = Services.ResourceLookup.PaletteSnapshot();
        var (bestKey, bestDistance) = (string.Empty, double.MaxValue);
        foreach (var (key, candidate) in palette)
        {
            var d = Math.Abs(color.Red - candidate.Red)
                  + Math.Abs(color.Green - candidate.Green)
                  + Math.Abs(color.Blue - candidate.Blue);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestKey = key;
            }
        }

        return bestKey;
    }
}