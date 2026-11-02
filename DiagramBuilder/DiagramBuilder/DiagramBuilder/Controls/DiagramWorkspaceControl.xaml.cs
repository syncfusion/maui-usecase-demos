using System.Collections;
using System.Linq;
using DiagramBuilder.Models;
using DiagramBuilder.Behaviors;
using DiagramBuilder.Services;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Controls;

/// <summary>
/// Workspace canvas (ui-spec.md §5). Hosts the SfDiagram; the SymbolPalette
/// lives on the host page as a direct Grid sibling (architecture.md §3.3).
/// Attaches <see cref="Behaviors.DiagramSurfaceBehavior"/> declaratively to
/// the SfDiagram.
/// </summary>
public partial class DiagramWorkspaceControl : ContentView
{
    /// <summary>Raised once when the SfDiagram control reports its JS instance is ready.</summary>
    public static readonly BindableProperty ReadyCommandProperty =
        BindableProperty.Create(nameof(ReadyCommand), typeof(System.Windows.Input.ICommand),
            typeof(DiagramWorkspaceControl), null, propertyChanged: OnReadyCommandChanged);

    public System.Windows.Input.ICommand? ReadyCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(ReadyCommandProperty);
        set => SetValue(ReadyCommandProperty, value);
    }

    /// <summary>Raised when the user changes the canvas selection.</summary>
    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(nameof(SelectionChangedCommand), typeof(System.Windows.Input.ICommand),
            typeof(DiagramWorkspaceControl), null, propertyChanged: OnSelectionChangedCommandChanged);

    public System.Windows.Input.ICommand? SelectionChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    /// <summary>Raised when a node's position, size or rotation changes (drags, resizes).</summary>
    public static readonly BindableProperty GeometryChangedCommandProperty =
        BindableProperty.Create(nameof(GeometryChangedCommand), typeof(System.Windows.Input.ICommand),
            typeof(DiagramWorkspaceControl), null, propertyChanged: OnGeometryChangedCommandChanged);

    public System.Windows.Input.ICommand? GeometryChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(GeometryChangedCommandProperty);
        set => SetValue(GeometryChangedCommandProperty, value);
    }

    /// <summary>Raised when a label edit completes (double-click annotation commit).</summary>
    public static readonly BindableProperty AnnotationEditedCommandProperty =
        BindableProperty.Create(nameof(AnnotationEditedCommand), typeof(System.Windows.Input.ICommand),
            typeof(DiagramWorkspaceControl), null, propertyChanged: OnAnnotationEditedCommandChanged);

    public System.Windows.Input.ICommand? AnnotationEditedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(AnnotationEditedCommandProperty);
        set => SetValue(AnnotationEditedCommandProperty, value);
    }

    /// <summary>Raised when nodes or connectors are added or removed.</summary>
    public static readonly BindableProperty CollectionChangedCommandProperty =
        BindableProperty.Create(nameof(CollectionChangedCommand), typeof(System.Windows.Input.ICommand),
            typeof(DiagramWorkspaceControl), null, propertyChanged: OnCollectionChangedCommandChanged);

    public System.Windows.Input.ICommand? CollectionChangedCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(CollectionChangedCommandProperty);
        set => SetValue(CollectionChangedCommandProperty, value);
    }

    public static readonly BindableProperty SurfaceBehaviorZoomFactorProperty =
        BindableProperty.Create(nameof(SurfaceBehaviorZoomFactor), typeof(double), typeof(DiagramWorkspaceControl), 1.0d);

    public double SurfaceBehaviorZoomFactor
    {
        get => (double)GetValue(SurfaceBehaviorZoomFactorProperty);
        set => SetValue(SurfaceBehaviorZoomFactorProperty, value);
    }

    public DiagramSurfaceBehavior Behavior => SurfaceBehavior;

    public SfDiagram DiagramControl => Diagram;

    public DiagramWorkspaceControl()
    {
        InitializeComponent();
    }

    private static void OnReadyCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is { } sb)
        {
            sb.ReadyCommand = (System.Windows.Input.ICommand?)newValue;
        }
    }

    private static void OnSelectionChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is { } sb)
        {
            sb.SelectionChangedCommand = (System.Windows.Input.ICommand?)newValue;
        }
    }

    private static void OnGeometryChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is { } sb)
        {
            sb.GeometryChangedCommand = (System.Windows.Input.ICommand?)newValue;
        }
    }

    private static void OnAnnotationEditedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is { } sb)
        {
            sb.AnnotationEditedCommand = (System.Windows.Input.ICommand?)newValue;
        }
    }

    private static void OnCollectionChangedCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is { } sb)
        {
            sb.CollectionChangedCommand = (System.Windows.Input.ICommand?)newValue;
        }
    }

    /// <summary>
    /// Loads a domain template's nodes and connectors into the live SfDiagram.
    /// Tracking for the property panel happens via <see cref="DiagramModelAdapter"/>.
    /// </summary>
    public void LoadTemplate(Models.DiagramDocument document)
    {
        try
        {
            Diagram.Nodes.Clear();
            Diagram.Connectors.Clear();

            var createdNodes = new List<object>();
            var createdConnectors = new List<object>();

            foreach (var shape in document.Items.OfType<Models.ShapeItem>())
            {
                var node = DiagramModelAdapter.ToNode(shape);
                createdNodes.Add(node);
                Diagram.Nodes.Add(node);
            }

            foreach (var connector in document.Connectors)
            {
                var c = DiagramModelAdapter.ToConnector(connector);
                createdConnectors.Add(c);
                Diagram.Connectors.Add(c);
            }

            // Inform any registered property service of the new live nodes/connectors
            // so the property panel can read live state for the selected element.
            TemplateLoaded?.Invoke(this, new TemplateLoadPayload(createdNodes, createdConnectors));
        }
        finally
        {
            // Always try to fit-to-page after loading; SafeFitToPageAsync waits for
            // the diagram to be ready and swallows any errors so loading can't be
            // blocked by a not-yet-initialized diagram.
            _ = SafeFitToPageAsync();
        }
    }

    public event EventHandler<TemplateLoadPayload>? TemplateLoaded;

    public sealed record TemplateLoadPayload(IReadOnlyList<object> Nodes, IReadOnlyList<object> Connectors);

    /// <summary>
    /// Fits the diagram content to the viewport. Waits for the SfDiagram's JS
    /// bridge to report its <c>Created</c> event before invoking FitToPage, so
    /// the call lands on an initialized renderer (otherwise some platforms
    /// silently drop the call or throw).
    /// </summary>
    private async Task SafeFitToPageAsync()
    {
        try
        {
            // Poll until the diagram reports ready or a short deadline elapses.
            // The template-load race (nodes added before Created) needs both:
            //   1. the JS bridge initialised, and
            //   2. the node/connector changes flushed through.
            for (var i = 0; i < 40 && !SurfaceBehavior.IsReady; i++)
            {
                await Task.Delay(25);
            }

            if (SurfaceBehavior.IsReady)
            {
                await Diagram.FitToPage();
            }
        }
        catch
        {
            // Swallow — the user can still press VIEW ▸ Fit To Screen manually.
        }
    }
}
