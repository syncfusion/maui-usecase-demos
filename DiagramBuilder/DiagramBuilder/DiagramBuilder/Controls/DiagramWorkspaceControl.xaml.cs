using System.Collections;
using System.Linq;
using DiagramBuilder.Models;
using DiagramBuilder.Behaviors;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Controls;

/// <summary>
/// Workspace canvas (ui-spec.md §5). Hosts the SfDiagram and the shared
/// <see cref="Syncfusion.Maui.Diagram.SymbolPalette"/> as direct Grid siblings
/// (architecture.md §3.3 — sibling discovery walks immediate children only).
/// Attaches <see cref="DiagramBuilder.Behaviors.DiagramSurfaceBehavior"/> declaratively
/// to the SfDiagram. Status strip below shows selection summary and
/// <see cref="ZoomIndicatorControl"/>.
/// </summary>
public partial class DiagramWorkspaceControl : ContentView
{
    public static readonly BindableProperty PaletteGroupsProperty =
        BindableProperty.Create(nameof(PaletteGroups), typeof(IEnumerable), typeof(DiagramWorkspaceControl),
            propertyChanged: OnPaletteGroupsChanged);

    public static readonly BindableProperty SurfaceBehaviorReadyCommandProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorReadyCommand), typeof(System.Windows.Input.ICommand), typeof(DiagramWorkspaceControl));

    public static readonly BindableProperty SurfaceBehaviorSelectionChangedCommandProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorSelectionChangedCommand), typeof(System.Windows.Input.ICommand), typeof(DiagramWorkspaceControl));

    public static readonly BindableProperty SurfaceBehaviorGeometryChangedCommandProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorGeometryChangedCommand), typeof(System.Windows.Input.ICommand), typeof(DiagramWorkspaceControl));

    public static readonly BindableProperty SurfaceBehaviorAnnotationEditedCommandProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorAnnotationEditedCommand), typeof(System.Windows.Input.ICommand), typeof(DiagramWorkspaceControl));

    public static readonly BindableProperty SurfaceBehaviorCollectionChangedCommandProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorCollectionChangedCommand), typeof(System.Windows.Input.ICommand), typeof(DiagramWorkspaceControl));

    public static readonly BindableProperty SurfaceBehaviorZoomFactorProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorZoomFactor), typeof(double), typeof(DiagramWorkspaceControl), 1.0d);

    public IEnumerable? PaletteGroups
    {
        get => (IEnumerable?)GetValue(PaletteGroupsProperty);
        set => SetValue(PaletteGroupsProperty, value);
    }

    public System.Windows.Input.ICommand? SurfaceBehaviorReadyCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SurfaceBehaviorReadyCommandProperty);
        set => SetValue(SurfaceBehaviorReadyCommandProperty, value);
    }

    public System.Windows.Input.ICommand? SurfaceBehaviorSelectionChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SurfaceBehaviorSelectionChangedCommandProperty);
        set => SetValue(SurfaceBehaviorSelectionChangedCommandProperty, value);
    }

    public System.Windows.Input.ICommand? SurfaceBehaviorGeometryChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SurfaceBehaviorGeometryChangedCommandProperty);
        set => SetValue(SurfaceBehaviorGeometryChangedCommandProperty, value);
    }

    public System.Windows.Input.ICommand? SurfaceBehaviorAnnotationEditedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SurfaceBehaviorAnnotationEditedCommandProperty);
        set => SetValue(SurfaceBehaviorAnnotationEditedCommandProperty, value);
    }

    public System.Windows.Input.ICommand? SurfaceBehaviorCollectionChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SurfaceBehaviorCollectionChangedCommandProperty);
        set => SetValue(SurfaceBehaviorCollectionChangedCommandProperty, value);
    }

    public double SurfaceBehaviorZoomFactor
    {
        get => (double)GetValue(SurfaceBehaviorZoomFactorProperty);
        set => SetValue(SurfaceBehaviorZoomFactorProperty, value);
    }

    public DiagramSurfaceBehavior Behavior => SurfaceBehavior;

    public SfDiagram DiagramControl => Diagram;

    public SymbolPalette PaletteControl => Palette;

    public DiagramWorkspaceControl()
    {
        InitializeComponent();
    }

    private static void OnPaletteGroupsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws)
        {
            if (newValue is IEnumerable groups && groups.Cast<object>().Any())
            {
                ws.Palette.Palettes.Clear();
                foreach (var palette in groups)
                {
                    ws.Palette.Palettes.Add(palette);
                }
                ws.Palette.IsVisible = true;
            }
            else
            {
                ws.Palette.IsVisible = false;
            }
        }
    }
}