using System.Collections;
using System.Diagnostics;
using System.Linq;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Behaviors;
using DiagramBuilder.Services;
using Microsoft.Maui.ApplicationModel;
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
        BindableProperty.Create(nameof(SurfaceBehaviorZoomFactor), typeof(double), typeof(DiagramWorkspaceControl), 1.0d,
            propertyChanged: OnSurfaceBehaviorZoomFactorChanged);

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

        // The drawing-area background. The SfDiagram control serializes
        // BackgroundColor into the EJ2 init payload exactly once
        // (DiagramManager.BuildInitConfigJson → window.SfDiagram.init), and
        // EJ2 paints its canvas div with it (diagramCanvas.style.background =
        // this.backgroundColor) — so this value IS the visible canvas colour.
        //
        // It must be a real, opaque colour: DiagramManager calls the
        // parameterless Color.ToHex(), which drops the alpha channel and turns
        // Colors.Transparent into "#000000" — i.e. BLACK. The default is
        // therefore the theme's canvas background (white), never transparent.
        Diagram.BackgroundColor = Services.ResourceLookup.Color(
            Constants.ResourceKeys.CanvasBackground, Colors.White);

        // Track live additions/removals so elements dropped from the symbol
        // palette (or deleted) are reflected in the property panel's tracked
        // set. The palette writes straight into SfDiagram.Nodes/Connectors,
        // so the ObservableCollection notifications are the only reliable hook.
        Diagram.Nodes.CollectionChanged += OnNodesCollectionChanged;
        Diagram.Connectors.CollectionChanged += OnConnectorsCollectionChanged;

        SurfaceBehavior.ZoomFactor = SurfaceBehaviorZoomFactor;
    }

    private static void OnSurfaceBehaviorZoomFactorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramWorkspaceControl ws && ws.SurfaceBehavior is not null && newValue is double zoom)
        {
            ws.SurfaceBehavior.ZoomFactor = zoom;
        }
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

    public Models.DiagramDocument CreateDocumentSnapshot(Models.DiagramDocument source)
    {
        var snapshot = new Models.DiagramDocument
        {
            Id = source.Id,
            Title = source.Title,
            Category = source.Category,
            CreatedUtc = source.CreatedUtc,
            IsDirty = source.IsDirty,
            SavedFilePath = source.SavedFilePath,
        };

        foreach (var node in Diagram.Nodes.OfType<Node>())
        {
            snapshot.Items.Add(DiagramModelAdapter.ToShapeItem(node));
        }

        foreach (var connector in Diagram.Connectors.OfType<Connector>())
        {
            snapshot.Connectors.Add(DiagramModelAdapter.ToConnectorItem(connector));
        }

        return snapshot;
    }

    public event EventHandler<TemplateLoadPayload>? TemplateLoaded;

    /// <summary>
    /// <summary>
    /// Applies the drawing-area background colour.
    ///
    /// The MAUI Diagram control has no public page/background API for this:
    /// <c>SfDiagram.BackgroundColor</c> is inherited from <c>VisualElement</c>
    /// and is only READ once, when the EJ2 diagram is initialised
    /// (DiagramManager.BuildInitConfigJson). A later property write therefore
    /// never reaches the renderer — which is exactly why the colour picker
    /// used to have no visible effect.
    ///
    /// EJ2 itself DOES support a live background change: its Diagram exposes a
    /// <c>backgroundColor</c> property, and assigning it re-paints the canvas
    /// (diagramCanvas.style.background = this.backgroundColor, triggered through
    /// EJ2's own propertyChange handling). The live instance is reachable in
    /// the hosted page as <c>document.getElementById('diagram').ej2_instances[0]</c>
    /// (EJ2 attaches instances to their DOM element), and the hosted WebView is
    /// reachable from this control through the public visual tree
    /// (SfView.Children → main grid → HybridWebView).
    ///
    /// So the background is applied by pushing the colour into the live EJ2
    /// instance, with the MAUI-side BackgroundColor kept in sync so a later
    /// re-initialisation (e.g. the page being re-created) starts from the same
    /// colour. The host views are painted too, so the canvas edge matches even
    /// before the renderer is ready.
    ///
    /// Paper format/orientation were removed: the MAUI Diagram exposes them only
    /// on <c>DiagramExportOptions</c> (consumed by ExportDiagram/Print), with no
    /// live canvas counterpart, so there is no supported behaviour to expose.
    /// </summary>
    public void ApplyPageSettings(string backgroundColorKey)
    {
        var color = Services.ResourceLookup.Color(backgroundColorKey, Colors.White);

        // Keep the control's own value in sync (used for the init payload and
        // for any re-initialisation of the hosted diagram).
        Diagram.BackgroundColor = color;

        // Paint the host views as well: they frame the canvas and are what
        // shows while the renderer is still starting up.
        DiagramGrid.BackgroundColor = color;
        if (this.Content is Border border)
        {
            border.BackgroundColor = color;
        }

        // Push the colour into the live EJ2 diagram so the canvas repaints now.
        _ = ApplyCanvasBackgroundAsync(color);
    }

    /// <summary>
    /// Sets the live EJ2 diagram's background colour through the hosted WebView.
    /// No-ops until the diagram is ready (the initial colour already came from
    /// the init payload, so nothing is lost while it starts up).
    /// </summary>
    private async Task ApplyCanvasBackgroundAsync(Color color)
    {
        try
        {
            for (var i = 0; i < 40 && !SurfaceBehavior.IsReady; i++)
            {
                await Task.Delay(25);
            }

            if (!SurfaceBehavior.IsReady)
            {
                return;
            }

            var hex = color.ToRgbaHex();
            await ExecuteDiagramScriptAsync(
                "var c='" + hex + "';" +
                "inst.backgroundColor=c;" +
                "var canvas=el&&el.firstElementChild;" +
                "if(canvas){canvas.style.background=c;}" +
                "inst.dataBind&&inst.dataBind();");
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or ObjectDisposedException)
        {
            Debug.WriteLine($"Unable to update the diagram canvas background: {exception}");
        }
    }

    public Task ApplyToolAsync(string toolId)
    {
        var script = toolId switch
        {
            AppConstants.ToolPan =>
                "inst.tool=4;inst.drawingObject=null;inst.dataBind&&inst.dataBind();",
            AppConstants.ToolSelect =>
                "inst.tool=1;inst.drawingObject=null;inst.dataBind&&inst.dataBind();",
            "tool.shape.rectangle" =>
                "inst.tool=16;inst.drawingObject={width:120,height:60,shape:{type:'Basic',shape:'Rectangle'}};inst.dataBind&&inst.dataBind();",
            "tool.shape.ellipse" =>
                "inst.tool=16;inst.drawingObject={width:120,height:60,shape:{type:'Basic',shape:'Ellipse'}};inst.dataBind&&inst.dataBind();",
            "tool.shape.polygon" =>
                "inst.tool=16;inst.drawingObject={width:120,height:110,shape:{type:'Basic',shape:'Polygon',points:[{x:0,y:55},{x:30,y:0},{x:90,y:0},{x:120,y:55},{x:90,y:110},{x:30,y:110}]}};inst.dataBind&&inst.dataBind();",
            AppConstants.ToolConnectorStraight =>
                "inst.tool=16;inst.drawingObject={type:'Straight'};inst.dataBind&&inst.dataBind();",
            AppConstants.ToolConnectorOrthogonal =>
                "inst.tool=16;inst.drawingObject={type:'Orthogonal'};inst.dataBind&&inst.dataBind();",
            AppConstants.ToolConnectorBezier =>
                "inst.tool=16;inst.drawingObject={type:'Bezier'};inst.dataBind&&inst.dataBind();",
            AppConstants.ToolLock =>
                "var nodes=(inst.selectedItems&&inst.selectedItems.nodes)||[];var connectors=(inst.selectedItems&&inst.selectedItems.connectors)||[];" +
                "function resolve(item){return item&&item.id&&inst.nameTable&&inst.nameTable[item.id]?inst.nameTable[item.id]:item;}" +
                "function toggleNodeLock(node){if(!node){return;}if(node.__dbLocked!==true){node.__dbPrevConstraints=node.constraints;node.__dbPrevStyleOpacity=node.style&&node.style.opacity;}" +
                "var current=node.__dbPrevConstraints==null?5240814:node.__dbPrevConstraints;" +
                "if(node.__dbLocked===true){node.constraints=node.__dbPrevConstraints==null?5240814:node.__dbPrevConstraints;if(node.style){node.style.opacity=node.__dbPrevStyleOpacity==null?1:node.__dbPrevStyleOpacity;}node.__dbLocked=false;}else{node.constraints=current&~4;if(node.style){node.style.opacity=0.55;}node.__dbLocked=true;}}" +
                "function toggleConnectorLock(connector){if(!connector){return;}if(connector.__dbLocked!==true){connector.__dbPrevConstraints=connector.constraints;connector.__dbPrevStyleOpacity=connector.style&&connector.style.opacity;}" +
                "var current=connector.__dbPrevConstraints==null?2043454:connector.__dbPrevConstraints;" +
                "if(connector.__dbLocked===true){connector.constraints=connector.__dbPrevConstraints==null?2043454:connector.__dbPrevConstraints;if(connector.style){connector.style.opacity=connector.__dbPrevStyleOpacity==null?1:connector.__dbPrevStyleOpacity;}connector.__dbLocked=false;}else{connector.constraints=current&~8;if(connector.style){connector.style.opacity=0.55;}connector.__dbLocked=true;}}" +
                "for(var i=0;i<nodes.length;i++){toggleNodeLock(resolve(nodes[i]));}" +
                "for(var j=0;j<connectors.length;j++){toggleConnectorLock(resolve(connectors[j]));}" +
                "inst.dataBind&&inst.dataBind();",
            _ => null,
        };

        return script is null ? Task.CompletedTask : ExecuteDiagramScriptAsync(script);
    }

    /// <summary>
    /// Align / distribute / match size / group on the selected items, via the EJ2
    /// instance's public methods (<c>align</c>, <c>distribute</c>, <c>sameSize</c>,
    /// <c>group</c>, <c>unGroup</c>) — the MAUI wrapper does not surface them.
    /// Align is relative to the first selected object; distribute spaces the
    /// shapes evenly between the outer two (RightToLeft / BottomToTop = equal gaps).
    /// </summary>
    public Task ArrangeAsync(string commandId)
    {
        var script = commandId switch
        {
            AppConstants.CmdAlignLeft => "inst.align('Left');",
            AppConstants.CmdAlignRight => "inst.align('Right');",
            AppConstants.CmdAlignCenter => "inst.align('Center');",
            AppConstants.CmdAlignTop => "inst.align('Top');",
            AppConstants.CmdAlignBottom => "inst.align('Bottom');",
            AppConstants.CmdAlignMiddle => "inst.align('Middle');",
            AppConstants.CmdDistributeHorizontally => "inst.distribute('RightToLeft');",
            AppConstants.CmdDistributeVertically => "inst.distribute('BottomToTop');",
            AppConstants.CmdSameWidth => "inst.sameSize('Width');",
            AppConstants.CmdSameHeight => "inst.sameSize('Height');",
            AppConstants.CmdSameWidthAndHeight => "inst.sameSize('Size');",
            AppConstants.CmdSelectAll => "inst.selectAll();",
            // Duplicate = copy + paste: EJ2 pastes the copy offset from the original.
            AppConstants.CmdDuplicate => "if(inst.selectedItems.nodes.length+inst.selectedItems.connectors.length>0){inst.copy();inst.paste();}",
            AppConstants.CmdGroup => "inst.group();",
            AppConstants.CmdUngroup => "inst.unGroup();",
            _ => null,
        };

        return script is null ? Task.CompletedTask : ExecuteDiagramScriptAsync(script);
    }

    /// <summary>
    /// Undoes the last action on the live diagram, then pulls the post-undo state
    /// back into the MAUI-side model objects.
    ///
    /// Why the resync: <see cref="Syncfusion.Maui.Diagram.SfDiagram"/>.Undo() runs
    /// EJ2's UndoRedo module inside the WebView, and the wrapper echoes back only
    /// geometry/collection/text events — never style changes. Without pulling the
    /// state back, the MAUI Node/ShapeStyle objects keep the pre-undo values, so
    /// the next property write (e.g. Fill Color) would flush the whole stale node
    /// — resurrecting values the user had just undone (e.g. BorderThickness).
    /// </summary>
    public async Task UndoWithResyncAsync()
    {
        await Diagram.Undo();
        await SyncModelFromDiagramAsync("undo");
    }

    /// <summary>Redo counterpart of <see cref="UndoWithResyncAsync"/>.</summary>
    public async Task RedoWithResyncAsync()
    {
        await Diagram.Redo();
        await SyncModelFromDiagramAsync("redo");
    }

    /// <summary>
    /// Copies the live EJ2 diagram's node/connector state (position, size, rotation
    /// and styles) into the MAUI-side model objects in
    /// <see cref="Syncfusion.Maui.Diagram.SfDiagram"/>.Nodes/Connectors, so the two
    /// sides agree after the WebView has changed state on its own (undo/redo).
    ///
    /// The copy runs with EJ2 history logging suppressed via
    /// <c>historyManager.canLog</c>: the native model writes the wrapper then flushes
    /// back to EJ2 would otherwise each land on EJ2's undo stack, making the next
    /// user undo revert the resync instead of the user's own last action.
    /// </summary>
    private async Task SyncModelFromDiagramAsync(string operationName)
    {
        try
        {
            // The script returns a plain OBJECT, not JSON.stringify(...) of one.
            // That matters: MAUI's HybridWebView.EvaluateJavaScriptAsync wraps the
            // script result differently per platform (Android returns the raw
            // JSON-encoded value, Windows/iOS wrap it in eval/JSON.stringify) and
            // then trims surrounding quotes. A JS *string* comes back with its inner
            // quotes still escaped ("{\"nodes\":...}"), which is not parseable JSON;
            // an object comes back as clean JSON text on every platform.
            var json = await EvaluateDiagramScriptAsync(
                "var o={nodes:[],connectors:[]};" +
                "var s=inst.nodes||[];" +
                "for(var i=0;i<s.length;i++){var n=s[i];" +
                "o.nodes.push({id:n.id,x:n.offsetX,y:n.offsetY,w:n.width,h:n.height,r:n.rotateAngle," +
                "fill:n.style&&n.style.fill,stroke:n.style&&n.style.strokeColor,sw:n.style&&n.style.strokeWidth,dash:n.style&&n.style.strokeDashArray,op:n.style&&n.style.opacity});}" +
                "var c=inst.connectors||[];" +
                "for(var j=0;j<c.length;j++){var k=c[j];" +
                "o.connectors.push({id:k.id,type:k.type,sw:k.style&&k.style.strokeWidth,dash:k.style&&k.style.strokeDashArray,stroke:k.style&&k.style.strokeColor,op:k.style&&k.style.opacity});}" +
                "return o;");
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            // Belt and braces: a platform that still hands back a quoted/escaped
            // literal is unwrapped here rather than fed to the parser as-is.
            json = UnwrapJsonPayload(json);
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            // Suppress EJ2 history logging while the native model is updated: the
            // wrapper flushes each native property write back to the WebView, and
            // those echoes must not become new undo entries.
            //
            // EJ2 calls historyManager.canLog(transaction) and checks the result's
            // 'cancel' flag (canLog(t).cancel === true aborts the history entry),
            // so the override must return an OBJECT — returning a boolean leaves
            // .cancel undefined and the entry is still logged.
            await ExecuteDiagramScriptAsync(
                "inst.historyManager.canLog=function(){return {cancel:true};};");
            try
            {
                ApplyPulledState(json);
            }
            finally
            {
                // The wrapper flushes native model changes back to EJ2 on the NEXT
                // dispatcher turn (DiagramManager coalesces a burst of property
                // writes into one full-state patch per element, scheduled through
                // MainThread.BeginInvokeOnMainThread). Restoring the history hook
                // has to happen AFTER that flush, otherwise the resync's own echo
                // lands on EJ2's undo stack and the user's next undo would revert
                // the resync instead of their own last action. Queueing the restore
                // on the same dispatcher puts it behind the already-queued flush.
                MainThread.BeginInvokeOnMainThread(() => _ = RestoreHistoryLoggingAsync());
            }

            // The property panel reads the native model; re-read it so the panel
            // reflects the post-undo state instead of the stale one.
            SyncedFromDiagram?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or ObjectDisposedException or OperationCanceledException or System.Text.Json.JsonException)
        {
            Debug.WriteLine($"Unable to {operationName} and resynchronize the diagram model: {exception}");
        }
    }

    /// <summary>
    /// Re-enables EJ2's undo/redo history logging once the resync's own echo has
    /// been flushed to the WebView.
    /// </summary>
    private async Task RestoreHistoryLoggingAsync()
    {
        await ExecuteDiagramScriptAsync(
            "inst.historyManager.canLog=null;inst.dataBind&&inst.dataBind();");
    }

    /// <summary>
    /// Normalizes a JavaScript evaluation result into plain JSON text.
    ///
    /// MAUI trims surrounding quote characters from the value a script returns, so
    /// a JSON object arrives as { ... } on every platform. Should a platform hand
    /// back a quoted JSON literal instead (with the inner quotes escaped), it is
    /// decoded here so the payload is parseable either way.
    /// </summary>
    private static string UnwrapJsonPayload(string payload)
    {
        var trimmed = payload.Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed[0] != '"' && !trimmed.StartsWith("\\\"", StringComparison.Ordinal))
        {
            return trimmed;
        }

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(trimmed);
            return document.RootElement.ValueKind == System.Text.Json.JsonValueKind.String
                ? document.RootElement.GetString() ?? string.Empty
                : trimmed;
        }
        catch (System.Text.Json.JsonException)
        {
            // The outer quotes were trimmed away but the inner ones are still
            // escaped — unescape the payload by hand.
            return trimmed.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }
    }

    /// <summary>Raised after an undo/redo resync so the property panel can re-read the model.</summary>
    public event EventHandler? SyncedFromDiagram;

    /// <summary>
    /// Applies a JSON payload of EJ2-side state (as produced by
    /// <see cref="SyncModelFromDiagramAsync"/>'s script) onto the MAUI-side
    /// Node/Connector model objects, matching by id.
    /// </summary>
    private void ApplyPulledState(string json)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        var root = document.RootElement;

        foreach (var entry in root.GetProperty("nodes").EnumerateArray())
        {
            var node = Diagram.Nodes.OfType<Node>().FirstOrDefault(n =>
                entry.TryGetProperty("id", out var nodeId)
                && nodeId.ValueKind == System.Text.Json.JsonValueKind.String
                && string.Equals(n.Id, nodeId.GetString(), StringComparison.Ordinal));
            if (node is null)
            {
                continue;
            }

            if (entry.TryGetProperty("x", out var x) && x.ValueKind == System.Text.Json.JsonValueKind.Number) node.OffsetX = x.GetDouble();
            if (entry.TryGetProperty("y", out var y) && y.ValueKind == System.Text.Json.JsonValueKind.Number) node.OffsetY = y.GetDouble();
            if (entry.TryGetProperty("w", out var w) && w.ValueKind == System.Text.Json.JsonValueKind.Number) node.Width = w.GetDouble();
            if (entry.TryGetProperty("h", out var h) && h.ValueKind == System.Text.Json.JsonValueKind.Number) node.Height = h.GetDouble();
            if (entry.TryGetProperty("r", out var r) && r.ValueKind == System.Text.Json.JsonValueKind.Number) node.RotateAngle = r.GetDouble();

            if (node.Style is ShapeStyle style)
            {
                if (entry.TryGetProperty("fill", out var fill) && fill.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    style.Fill = ParseEj2Color(fill.GetString());
                }

                if (entry.TryGetProperty("stroke", out var stroke) && stroke.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    style.StrokeColor = ParseEj2Color(stroke.GetString());
                }

                if (entry.TryGetProperty("sw", out var sw) && sw.ValueKind == System.Text.Json.JsonValueKind.Number) style.StrokeWidth = sw.GetDouble();
                if (entry.TryGetProperty("dash", out var dash))
                {
                    style.StrokeDashArray = dash.ValueKind == System.Text.Json.JsonValueKind.String ? dash.GetString() : string.Empty;
                }

                if (entry.TryGetProperty("op", out var op) && op.ValueKind == System.Text.Json.JsonValueKind.Number) style.Opacity = op.GetDouble();
            }

            // Annotations are intentionally NOT synced: the bridge's textEdit/textChange
            // event keeps the native annotation content in step with the EJ2 side for
            // user edits, and the bridge merges annotations by id on full-state flushes,
            // so an EJ2-restored content value would only be re-applied by another
            // property edit's wrapper flush anyway. Re-writing annotation content from
            // the pull has been observed to drop the rendered text in some cases
            // (the wrapper's flush sends id-less annotations that the bridge ignores,
            // so the visible label can be lost between an undo and the next user
            // edit); leaving annotations alone is both correct and idempotent.
        }

        foreach (var entry in root.GetProperty("connectors").EnumerateArray())
        {
            var connector = Diagram.Connectors.OfType<Connector>().FirstOrDefault(c =>
                entry.TryGetProperty("id", out var connectorId)
                && connectorId.ValueKind == System.Text.Json.JsonValueKind.String
                && string.Equals(c.Id, connectorId.GetString(), StringComparison.Ordinal));
            if (connector is null)
            {
                continue;
            }

            if (entry.TryGetProperty("type", out var type) && type.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                connector.Type = type.GetString() == "Orthogonal" ? ConnectorSegmentType.Orthogonal : ConnectorSegmentType.Straight;
            }

            if (connector.Style is ShapeStyle style)
            {
                if (entry.TryGetProperty("stroke", out var stroke) && stroke.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    style.StrokeColor = ParseEj2Color(stroke.GetString());
                }

                if (entry.TryGetProperty("sw", out var sw) && sw.ValueKind == System.Text.Json.JsonValueKind.Number) style.StrokeWidth = sw.GetDouble();
                if (entry.TryGetProperty("op", out var op) && op.ValueKind == System.Text.Json.JsonValueKind.Number) style.Opacity = op.GetDouble();
            }
        }
    }

    /// <summary>Parses an EJ2 color string (#rgb, #rrggbb, #rrggbbaa or a css color name) into a MAUI color.</summary>
    private static Color ParseEj2Color(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Colors.Transparent;
        }

        try
        {
            return Color.FromArgb(value.StartsWith('#') ? value : "#" + value);
        }
        catch (ArgumentException)
        {
            return Colors.Transparent;
        }
    }

    /// <summary>
    /// Pushes the View-menu grid options into the live EJ2 diagram. SnapConstraints:
    /// ShowLines (3) draws the grid, SnapToLines (12) snaps dragged/resized shapes to it.
    /// The MAUI host page only injects UndoRedo / PrintAndExport / ContextMenu, so the
    /// Snapping module is injected here on demand — without it the flag alone is ignored.
    /// </summary>
    public Task ApplyGridSettingsAsync(bool showGrid, bool snapToGrid)
    {
        var constraints = (showGrid ? 3 : 0) | (snapToGrid ? 12 : 0);
        var script =
            (snapToGrid
                ? "try{if(!inst.snappingModule&&ej.diagrams.Snapping){ej.diagrams.Diagram.Inject(ej.diagrams.Snapping);" +
                  "inst.moduleLoader.inject(inst.requiredModules(),inst.injectedModules);}}catch(e){}"
                : string.Empty) +
            "inst.snapSettings.constraints=" + constraints + ";inst.dataBind&&inst.dataBind();";
        return ExecuteDiagramScriptAsync(script);
    }

    /// <summary>
    /// Shows or hides the shape palette. The palette is rendered by the diagram's own web
    /// page (#symbolpalette), so MAUI visibility bindings cannot reach it.
    /// </summary>
    public Task ApplyStencilVisibilityAsync(bool visible) =>
        ExecuteDiagramScriptAsync(
            "var p=document.getElementById('symbolpalette');if(p){p.style.display='" + (visible ? "block" : "none") + "';}" +
            "window.dispatchEvent(new Event('resize'));");

    public Task ApplyStencilSearchStyleAsync() =>
        ExecuteDiagramScriptAsync(
            "var p=document.getElementById('symbolpalette');" +
            "if(p){" +
            "var apply=function(){" +
            "var input=p.querySelector('input');if(!input){return false;}" +
            "var wrapper=input.closest('.e-search,.e-input-group')||input.parentElement;" +
            "if(wrapper){wrapper.style.setProperty('height','38px','important');wrapper.style.setProperty('min-height','38px','important');wrapper.style.setProperty('display','flex','important');wrapper.style.setProperty('align-items','center','important');wrapper.style.setProperty('border-radius','0','important');}" +
            "input.style.setProperty('box-sizing','border-box','important');input.style.setProperty('height','38px','important');input.style.setProperty('min-height','38px','important');input.style.setProperty('width','100%','important');input.style.setProperty('margin','0','important');input.style.setProperty('padding','7px 10px','important');input.style.setProperty('line-height','20px','important');input.style.setProperty('text-indent','0','important');input.style.setProperty('text-align','left','important');input.style.setProperty('border-radius','0','important');" +
            "return true;};" +
            "if(!apply()){var observer=new MutationObserver(function(){if(apply()){observer.disconnect();}});observer.observe(p,{childList:true,subtree:true});window.setTimeout(function(){observer.disconnect();},5000);}" +
            "}");

    public Task BringToFrontAsync() => ExecuteDiagramScriptAsync("inst.bringToFront&&inst.bringToFront();");

    public Task SendToBackAsync() => ExecuteDiagramScriptAsync("inst.sendToBack&&inst.sendToBack();");

    public Task BringForwardAsync() => ExecuteDiagramScriptAsync("inst.moveForward&&inst.moveForward();");

    public Task SendBackwardAsync() => ExecuteDiagramScriptAsync("inst.sendBackward&&inst.sendBackward();");

    private async Task ExecuteDiagramScriptAsync(string script)
    {
        try
        {
            await EvaluateDiagramScriptAsync(script);
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or ObjectDisposedException or OperationCanceledException)
        {
            Debug.WriteLine($"Unable to execute a diagram command: {exception}");
        }
    }

    /// <summary>
    /// Evaluates a script against the live EJ2 diagram instance and returns its raw
    /// JSON result (null when the diagram or WebView is not ready). Unlike
    /// <see cref="ExecuteDiagramScriptAsync"/>, failures propagate to the caller —
    /// callers that depend on the result must be able to react to them.
    /// </summary>
    private async Task<string?> EvaluateDiagramScriptAsync(string script)
    {
        for (var i = 0; i < 40 && !SurfaceBehavior.IsReady; i++)
        {
            await Task.Delay(25);
        }

        if (!SurfaceBehavior.IsReady)
        {
            return null;
        }

        var webView = FindHostedWebView();
        if (webView is null)
        {
            return null;
        }

        // 'inst' is the live EJ2 diagram instance; scripts build on it directly.
        var result = await webView.EvaluateJavaScriptAsync(
            "(function(){var el=document.getElementById('diagram');var inst=el&&el.ej2_instances&&el.ej2_instances[0];if(!inst){return null;}" +
            script +
            "})();");
        return result is null or "null" ? null : result;
    }

    /// <summary>
    /// Locates the hosted HybridWebView inside the diagram control's visual tree
    /// (SfView.Children → internal grid → the WebView). Returns null when the
    /// tree has not been built yet.
    /// </summary>
    private HybridWebView? FindHostedWebView()
    {
        foreach (var child in Diagram.Children)
        {
            if (child is HybridWebView direct)
            {
                return direct;
            }

            if (child is Layout layout)
            {
                foreach (var nested in layout.Children)
                {
                    if (nested is HybridWebView webView)
                    {
                        return webView;
                    }
                }
            }
        }

        return null;
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
        catch (Exception exception) when (exception is InvalidOperationException or ObjectDisposedException)
        {
            Debug.WriteLine($"Unable to fit the diagram to the page: {exception}");
        }
    }
}
