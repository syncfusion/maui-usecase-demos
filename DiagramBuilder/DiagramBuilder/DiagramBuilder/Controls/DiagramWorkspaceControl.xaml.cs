using System.Collections;
using System.Linq;
using DiagramBuilder.Models;
using DiagramBuilder.Behaviors;
using DiagramBuilder.Services;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Controls;

/// <summary>
/// Workspace canvas (ui-spec.md §5). Hosts the SfDiagram and a hidden
/// SymbolPalette as direct siblings inside <c>DiagramGrid</c> — Syncfusion's
/// palette discovery requires the SymbolPalette to be among
/// <c>SfDiagram.Parent</c>'s immediate children (architecture.md §3.3).
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

    /// <summary>
    /// The hidden SymbolPalette sibling of the SfDiagram inside
    /// <c>DiagramGrid</c>. It is a config carrier: Syncfusion discovers it via
    /// <c>FindSiblingSymbolPalette()</c> and renders the shapes inside the
    /// SfDiagram's own webview. Assign <see cref="SymbolPalette.Palettes"/> to
    /// populate it.
    /// </summary>
    public SymbolPalette ShapesPalette => ShapesPaletteView;

    public DiagramWorkspaceControl()
    {
        InitializeComponent();

        // The SfDiagram ctor hardcodes BackgroundColor=White, which
        // BuildInitConfigJson serializes into the EJ2 init payload
        // (overriding the JS 'transparent' default). The EJ2 canvas would
        // then be an opaque white rectangle covering every MAUI view behind
        // it, and page-background edits would never be visible. Forcing
        // transparency here — BEFORE the WebView loads, while the value is
        // still picked up by init — makes the EJ2 canvas see-through, so the
        // painted host views (DiagramGrid / workspace Border) show through.
        Diagram.BackgroundColor = Colors.Transparent;

        // Track live additions/removals so elements dropped from the symbol
        // palette (or deleted) are reflected in the property panel's tracked
        // set. The palette writes straight into SfDiagram.Nodes/Connectors,
        // so the ObservableCollection notifications are the only reliable hook.
        Diagram.Nodes.CollectionChanged += OnNodesCollectionChanged;
        Diagram.Connectors.CollectionChanged += OnConnectorsCollectionChanged;
    }

    private void OnNodesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaiseElementsChanged(e, ElementKind.Node);
    }

    private void OnConnectorsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaiseElementsChanged(e, ElementKind.Connector);
    }

    private void RaiseElementsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e, ElementKind kind)
    {
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is not null)
                        {
                            ElementAdded?.Invoke(this, new ElementChangePayload(kind, item, Added: true));
                        }
                    }
                }

                break;

            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                {
                    foreach (var item in e.OldItems)
                    {
                        var id = item switch
                        {
                            Syncfusion.Maui.Diagram.Node node => node.Id,
                            Syncfusion.Maui.Diagram.Connector connector => connector.Id,
                            _ => string.Empty,
                        };

                        if (!string.IsNullOrEmpty(id))
                        {
                            ElementAdded?.Invoke(this, new ElementChangePayload(kind, id, Added: false));
                        }
                    }
                }

                break;

            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                // Bulk clear (e.g. Nodes.Clear() during a template load) — the
                // subsequent Track() call re-registers everything.
                ElementsReset?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    /// <summary>Raised when an element is added to or removed from the diagram.</summary>
    public event EventHandler<ElementChangePayload>? ElementAdded;

    /// <summary>Raised when the whole node/connector collection is cleared.</summary>
    public event EventHandler? ElementsReset;

    /// <summary>Identifies which live collection an element change came from.</summary>
    public enum ElementKind
    {
        Node,
        Connector,
    }

    /// <summary>
    /// Describes an element entering or leaving the diagram. <see cref="Element"/>
    /// carries the live object for additions and its id for removals.
    /// </summary>
    public sealed record ElementChangePayload(ElementKind Kind, object Element, bool Added)
    {
        /// <summary>Id of the affected element (empty when the element has no id).</summary>
        public string ElementId => Element as string ?? string.Empty;
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

            // TODO: Apply layout after diagram is fully initialized
            // Layout application requires the diagram to be fully ready (Created event fired).
            // For now, templates use manual positioning with pre-calculated node coordinates.
            // if (document.Layout != null)
            // {
            //     await SafeApplyLayoutAsync(document.Layout);
            // }

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

    /// <summary>
    /// <summary>
    /// Applies the drawing-area background colour.
    ///
    /// The SfDiagram control serializes its BackgroundColor into the EJ2 init
    /// payload exactly once (DiagramManager.BuildInitConfigJson →
    /// window.SfDiagram.init) and offers no runtime update channel, so a later
    /// property write never reaches the renderer. The constructor is therefore
    /// given a TRANSPARENT background (see DiagramWorkspaceControl ctor) so the
    /// EJ2 canvas stays see-through, and the chosen colour is painted on the MAUI
    /// host views behind it (DiagramGrid + the workspace Border). Those views ARE
    /// the visible canvas background, and repainting them works at runtime.
    ///
    /// Paper format/orientation were removed: the MAUI Diagram exposes them only
    /// on <c>DiagramExportOptions</c> (consumed by ExportDiagram/Print), with no
    /// live canvas counterpart, so there is no supported behaviour to expose.
    /// </summary>
    public void ApplyPageSettings(string backgroundColorKey)
    {
        var color = Services.ResourceLookup.Color(backgroundColorKey, Colors.White);
        // The HybridWebView itself must stay transparent; paint the host views.
        DiagramGrid.BackgroundColor = color;
        if (this.Content is Border border)
        {
            border.BackgroundColor = color;
        }
    }


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
