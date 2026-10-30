using VisitorPass.Models;

namespace VisitorPass.Controls;

/// <summary>
/// VP-020 — reusable status pill built on <c>SfBadgeView</c>.
///
/// Drop it into a grid cell with one property:
///
///     &lt;controls:VisitorStatusBadge Status="{Binding Status}" /&gt;
///
/// The wrapper handles:
///
///   1. The correct foreground / background brush via the StaticResource
///      keys returned by <see cref="VisitorStatusExtensions.BrushKey"/> /
///      <see cref="VisitorStatusExtensions.BackgroundBrushKey"/>.
///   2. Per-idiom label: full <see cref="VisitorStatusExtensions.Display"/>
///      on desktop / tablet, compact <see cref="VisitorStatusExtensions.Short"/>
///      on phone. The two labels are bound directly via
///      <c>VisitorStatusToDisplayLabelConverter</c> /
///      <c>VisitorStatusToShortLabelConverter</c> from App.xaml and their
///      visibility switches via <c>OnIdiom</c> in XAML — per AGENTS.md
///      the idiom check never lives in code-behind.
/// </summary>
public partial class VisitorStatusBadge : ContentView
{
    /// <summary>Bindable status (required).</summary>
    public static readonly BindableProperty StatusProperty = BindableProperty.Create(
        nameof(Status),
        typeof(VisitorStatus),
        typeof(VisitorStatusBadge),
        defaultValue: VisitorStatus.Expected,
        propertyChanged: OnStatusChanged);

    /// <summary>The status to render.</summary>
    public VisitorStatus Status
    {
        get => (VisitorStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly BindableProperty BackgroundKeyProperty =
    BindableProperty.Create(
        nameof(BackgroundKey),
        typeof(string),
        typeof(VisitorStatusBadge),
        string.Empty,
        propertyChanged: OnStatusChanged);

    public string BackgroundKey
    {
        get => (string)GetValue(BackgroundKeyProperty);
        set => SetValue(BackgroundKeyProperty, value);
    }

    public VisitorStatusBadge()
    {
        InitializeComponent();
        // Apply once at construction so the first paint is correct
        // even before Status is bound.
        ApplyStatus();
    }

    private static void OnStatusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisitorStatusBadge self)
            self.ApplyStatus();
    }

    private void ApplyStatus()
    {
        // The badge's BackgroundColor is the pale shade (background
        // brush). The inner Label sets its own foreground (white)
        // for contrast — see VisitorStatusBadge.xaml.
        if (Application.Current?.Resources.TryGetValue(BackgroundKey, out var resource) == true)
        {
            if (resource is Color color)
            {
                Badge.BackgroundColor = color;
            }
            else if (resource is SolidColorBrush brush)
            {
                Badge.BackgroundColor = brush.Color;
            }
        }
    }
}
