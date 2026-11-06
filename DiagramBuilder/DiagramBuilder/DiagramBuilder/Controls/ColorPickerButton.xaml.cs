using System.Windows.Input;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;

namespace DiagramBuilder.Controls;

/// <summary>
/// Color swatch + SfColorPicker popup (ui-spec.md §3 group-6 and §6 FILL/BORDER).
/// Bindable <see cref="ColorValue"/> is a theme <c>ResourceKeys</c> string
/// (resolved via <see cref="Services.ResourceLookup"/>); <see cref="ColorChangedCommand"/>
/// fires with the nearest theme key string when the user picks a color.
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

    /// <summary>
    /// CLR event raised with the new color key when the user picks a color.
    /// Used by code-behind callers; <see cref="ColorChangedCommand"/> is the
    /// MVVM-friendly equivalent for XAML binding.
    /// </summary>
    public event EventHandler<string>? ColorChanged;

    private SfPopup? _popup;

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
        ShowPicker(SwatchFrame);
    }

    /// <summary>Opens the colour picker popup anchored to the given element.</summary>
    private void ShowPicker(View anchor)
    {
        _popup = BuildPopup();
        PopupXamlRootHelper.AttachIfAvailable(_popup, anchor);
        _popup.ShowRelativeToView(anchor, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
    }

    private SfPopup BuildPopup()
    {
        var pendingColorKey = string.IsNullOrWhiteSpace(ColorValue)
            ? FindNearestKey(Colors.White)
            : ColorValue;

        var picker = new SfColorPicker
        {
            ColorMode = ColorPickerMode.Spectrum,
            ShowAlphaSlider = true,
            ShowInputArea = true,
        };

        SetPickerColor(picker, Services.ResourceLookup.Color(pendingColorKey, Colors.White));
        picker.ColorChanged += (_, args) =>
        {
            pendingColorKey = FindNearestKey(args.NewColor ?? Colors.White);
        };

        return new SfPopup
        {
            ContentTemplate = new DataTemplate(() => picker),
            ShowHeader = false,
            ShowFooter = true,
            ShowCloseButton = false,
            AppearanceMode = PopupButtonAppearanceMode.TwoButton,
            AcceptButtonText = "Apply",
            DeclineButtonText = "Cancel",
            AcceptCommand = new Command(() =>
            {
                ColorValue = pendingColorKey;
                ApplyToSwatch(pendingColorKey);
                ColorChanged?.Invoke(this, pendingColorKey);
                if (ColorChangedCommand?.CanExecute(pendingColorKey) == true)
                {
                    ColorChangedCommand.Execute(pendingColorKey);
                }
            }),
            DeclineCommand = new Command(() => { }),
        };
    }

    private static void SetPickerColor(SfColorPicker picker, Color color)
    {
        // SfColorPicker exposes a public writable SelectedColor (verified in the
        // Syncfusion.Maui.Inputs API surface) — no reflection needed.
        picker.SelectedColor = color;
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