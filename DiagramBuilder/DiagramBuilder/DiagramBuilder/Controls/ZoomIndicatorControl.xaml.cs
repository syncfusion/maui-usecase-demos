using DiagramBuilder.Constants;

namespace DiagramBuilder.Controls;

/// <summary>
/// Status-bar zoom/snap/grid indicator trio. Binds to the same string properties
/// exposed by <see cref="DiagramBuilder.ViewModels.ZoomViewModel"/> (1.0 = 100 %).
/// All values rendered via theme keys — no hardcoded colors or labels.
/// </summary>
public partial class ZoomIndicatorControl : ContentView
{
    public static readonly BindableProperty ZoomPercentProperty =
        BindableProperty.Create(nameof(ZoomPercent), typeof(string), typeof(ZoomIndicatorControl), "100%",
            propertyChanged: OnZoomPercentChanged);

    public static readonly BindableProperty ShowGridProperty =
        BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ZoomIndicatorControl), true,
            propertyChanged: OnShowGridChanged);

    public static readonly BindableProperty SnapToGridProperty =
        BindableProperty.Create(nameof(SnapToGrid), typeof(bool), typeof(ZoomIndicatorControl), true,
            propertyChanged: OnSnapToGridChanged);

    public string ZoomPercent
    {
        get => (string)GetValue(ZoomPercentProperty);
        set => SetValue(ZoomPercentProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public bool SnapToGrid
    {
        get => (bool)GetValue(SnapToGridProperty);
        set => SetValue(SnapToGridProperty, value);
    }

    public ZoomIndicatorControl()
    {
        InitializeComponent();
        ZoomLabel.Text = ZoomPercent;
        GridLabel.Text = AppStrings.GridOnLabel;
        SnapLabel.Text = AppStrings.SnapOnLabel;
    }

    private static void OnZoomPercentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ZoomIndicatorControl ctrl && newValue is string pct)
        {
            ctrl.ZoomLabel.Text = pct;
        }
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ZoomIndicatorControl ctrl && newValue is bool on)
        {
            ctrl.GridLabel.Text = on ? AppStrings.GridOnLabel : AppStrings.GridOffLabel;
        }
    }

    private static void OnSnapToGridChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ZoomIndicatorControl ctrl && newValue is bool on)
        {
            ctrl.SnapLabel.Text = on ? AppStrings.SnapOnLabel : AppStrings.SnapOffLabel;
        }
    }
}