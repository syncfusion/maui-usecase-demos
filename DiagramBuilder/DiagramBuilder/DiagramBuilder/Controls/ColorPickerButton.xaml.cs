using System.Windows.Input;
using DiagramBuilder.Services;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Inputs;

namespace DiagramBuilder.Controls;

public partial class ColorPickerButton : ContentView
{
    public static readonly BindableProperty ColorValueProperty =
        BindableProperty.Create(
            nameof(ColorValue),
            typeof(string),
            typeof(ColorPickerButton),
            "#FFFFFFFF",
            BindingMode.TwoWay,
            propertyChanged: OnColorValueChanged);

    public static readonly BindableProperty ColorChangedCommandProperty =
        BindableProperty.Create(
            nameof(ColorChangedCommand),
            typeof(ICommand),
            typeof(ColorPickerButton));

    private bool _updatingSelection;

    public ColorPickerButton()
    {
        InitializeComponent();
        UpdateSelectedColor();
        Picker.ColorChanged += OnPickerColorChanged;
    }

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

    public event EventHandler<string>? ColorChanged;

    private static void OnColorValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((ColorPickerButton)bindable).UpdateSelectedColor();
    }

    private void UpdateSelectedColor()
    {
        _updatingSelection = true;
        try
        {
            Picker.SelectedColor = ResourceLookup.Color(ColorValue, Colors.White);
        }
        finally
        {
            _updatingSelection = false;
        }
    }

    private void OnPickerColorChanged(object? sender, ColorChangedEventArgs args)
    {
        if (_updatingSelection || args.NewColor is not Color color)
        {
            return;
        }

        var value = ResourceLookup.ToArgbHex(color);
        ColorValue = value;
        ColorChanged?.Invoke(this, value);

        if (ColorChangedCommand?.CanExecute(value) == true)
        {
            ColorChangedCommand.Execute(value);
        }
    }
}
