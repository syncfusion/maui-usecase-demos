using DiagramBuilder.Models;
using System.Windows.Input;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Behaviors;

/// <summary>
/// Control ↔ VM bridge for <see cref="SfDiagram"/>: subscribes to native events on
/// attach and republishes them through bindable <see cref="ICommand"/> properties —
/// the ViewModel never references the control. Also relays VM requests one-way
/// (VM → behavior → control) by awaiting the control's async methods.
/// Contains zero business logic: event→command relaying and control API calls only.
/// </summary>
public partial class DiagramSurfaceBehavior : Behavior<View>
{
    // ---------- Bindable properties: events out (control → VM) ----------

    public static readonly BindableProperty ReadyCommandProperty =
        BindableProperty.Create(nameof(ReadyCommand), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? ReadyCommand
    {
        get => (ICommand?)GetValue(ReadyCommandProperty);
        set => SetValue(ReadyCommandProperty, value);
    }

    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(nameof(SelectionChangedCommand), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public static readonly BindableProperty GeometryChangedCommandProperty =
        BindableProperty.Create(nameof(GeometryChangedCommand), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? GeometryChangedCommand
    {
        get => (ICommand?)GetValue(GeometryChangedCommandProperty);
        set => SetValue(GeometryChangedCommandProperty, value);
    }

    public static readonly BindableProperty AnnotationEditedCommandProperty =
        BindableProperty.Create(nameof(AnnotationEditedCommand), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? AnnotationEditedCommand
    {
        get => (ICommand?)GetValue(AnnotationEditedCommandProperty);
        set => SetValue(AnnotationEditedCommandProperty, value);
    }

    public static readonly BindableProperty CollectionChangedCommandProperty =
        BindableProperty.Create(nameof(CollectionChangedCommand), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? CollectionChangedCommand
    {
        get => (ICommand?)GetValue(CollectionChangedCommandProperty);
        set => SetValue(CollectionChangedCommandProperty, value);
    }

    // ---------- Bindable properties: requests in (VM → control) ----------

    public static readonly BindableProperty RequestFitToPageProperty =
        BindableProperty.Create(nameof(RequestFitToPage), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    /// <summary>Relay command: VM binds its command here to receive fit-to-page requests.</summary>
    public ICommand? RequestFitToPage
    {
        get => (ICommand?)GetValue(RequestFitToPageProperty);
        set => SetValue(RequestFitToPageProperty, value);
    }

    public static readonly BindableProperty RequestUndoProperty =
        BindableProperty.Create(nameof(RequestUndo), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? RequestUndo
    {
        get => (ICommand?)GetValue(RequestUndoProperty);
        set => SetValue(RequestUndoProperty, value);
    }

    public static readonly BindableProperty RequestRedoProperty =
        BindableProperty.Create(nameof(RequestRedo), typeof(ICommand), typeof(DiagramSurfaceBehavior));

    public ICommand? RequestRedo
    {
        get => (ICommand?)GetValue(RequestRedoProperty);
        set => SetValue(RequestRedoProperty, value);
    }

    // ---------- State passthrough (VM → control) ----------

    public static readonly BindableProperty ZoomFactorProperty =
        BindableProperty.Create(nameof(ZoomFactor), typeof(double), typeof(DiagramSurfaceBehavior), 1.0d,
            propertyChanged: OnZoomFactorChanged);

    /// <summary>Current zoom factor pushed from the ZoomViewModel (1.0 = 100 %).</summary>
    public double ZoomFactor
    {
        get => (double)GetValue(ZoomFactorProperty);
        set => SetValue(ZoomFactorProperty, value);
    }

    /// <summary>
    /// True once the SfDiagram has reported its JS bridge Created event. The
    /// diagram can be queried for layout/camera after this fires — earlier calls
    /// (FitToPage, Zoom, etc.) may silently fail or throw on some platforms.
    /// </summary>
    public bool IsReady { get; private set; }

    // ---------- Attach / detach ----------

    private SfDiagram? _diagram;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        if (bindable is SfDiagram diagram)
        {
            _diagram = diagram;
            diagram.Created += OnDiagramCreated;
            diagram.SelectionChanged += OnSelectionChanged;
            diagram.PositionChanged += OnGeometryChanged;
            diagram.SizeChanged += OnGeometryChanged;
            diagram.RotationChanged += OnGeometryChanged;
            diagram.TextEdited += OnTextEdited;
            diagram.CollectionChanged += OnCollectionChanged;
        }
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (_diagram is not null)
        {
            _diagram.Created -= OnDiagramCreated;
            _diagram.SelectionChanged -= OnSelectionChanged;
            _diagram.PositionChanged -= OnGeometryChanged;
            _diagram.SizeChanged -= OnGeometryChanged;
            _diagram.RotationChanged -= OnGeometryChanged;
            _diagram.TextEdited -= OnTextEdited;
            _diagram.CollectionChanged -= OnCollectionChanged;
            _diagram = null;
        }

        base.OnDetachingFrom(bindable);
    }

    // ---------- Event → command relays (no business logic) ----------

    private void OnDiagramCreated(object? sender, EventArgs e)
    {
        IsReady = true;
        if (ReadyCommand?.CanExecute(null) == true)
        {
            ReadyCommand.Execute(null);
        }

        _ = ApplyZoomFactorAsync();
    }

    // Latest selection ids — control selection state is only observable through
    // SelectionChanged (Node/Connector expose no writable IsSelected in this
    // package version); cached so the delete relay can resolve the selection.
    private IReadOnlyList<string> _selectedNodeIds = [];
    private IReadOnlyList<string> _selectedConnectorIds = [];

    private void OnSelectionChanged(object? sender, DiagramSelectionChangedEventArgs e)
    {
        // Extract ids only — the ViewModel works with plain strings, never control types.
        _selectedNodeIds = e.SelectedNodes?.Select(n => n.Id).ToList() ?? [];
        _selectedConnectorIds = e.SelectedConnectors?.Select(c => c.Id).ToList() ?? [];
        var payload = new SelectionPayload(_selectedNodeIds, _selectedConnectorIds);

        if (SelectionChangedCommand?.CanExecute(payload) == true)
        {
            SelectionChangedCommand.Execute(payload);
        }
    }

    /// <summary>Plain-data selection payload handed to the VM (no control types).</summary>
    public sealed record SelectionPayload(IReadOnlyList<string> NodeIds, IReadOnlyList<string> ConnectorIds);

    private void OnGeometryChanged(object? sender, EventArgs e)
    {
        if (GeometryChangedCommand?.CanExecute(null) == true)
        {
            GeometryChangedCommand.Execute(null);
        }
    }

    private void OnTextEdited(object? sender, DiagramTextEditedEventArgs e)
    {
        if (AnnotationEditedCommand?.CanExecute(null) == true)
        {
            AnnotationEditedCommand.Execute(null);
        }
    }

    private void OnCollectionChanged(object? sender, DiagramCollectionChangedEventArgs e)
    {
        if (CollectionChangedCommand?.CanExecute(null) == true)
        {
            CollectionChangedCommand.Execute(null);
        }
    }

    // ---------- Control API access for the relays ----------

    /// <summary>
    /// Executes a fit-to-page request on the attached diagram (async, one-way VM→control).
    /// The bound VM command calls this via <see cref="RequestFitToPage"/>.
    /// </summary>
    public async Task FitToPageAsync()
    {
        if (_diagram is not null)
        {
            await _diagram.FitToPage();
        }
    }

    // ---------- EDIT relays (VM → control; signatures verified against package 35.1.42) ----------

    public async Task CutAsync()
    {
        if (_diagram is not null)
        {
            await _diagram.Cut();
        }
    }

    public async Task CopyAsync()
    {
        if (_diagram is not null)
        {
            // Copy returns the clipboard payload; the sample only needs the effect.
            await _diagram.Copy();
        }
    }

    public async Task PasteAsync()
    {
        if (_diagram is not null)
        {
            // Paste takes an optional elements parameter; null pastes the control clipboard.
            await _diagram.Paste(null);
        }
    }

    /// <summary>
    /// Deletes the currently selected elements by removing them from the underlying
    /// collections (the MAUI layer exposes no dedicated delete method); the control
    /// raises CollectionChanged for the dispatch table's dirty-tracking. Selection
    /// ids come from the cached SelectionChanged payload.
    ///
    /// Connectors whose SourceID/TargetID reference a deleted node are removed as
    /// well: their associated UI (the line + its endpoints/decorators) must not
    /// survive the node it attaches to.
    /// </summary>
    public Task DeleteSelectionAsync()
    {
        if (_diagram is null)
        {
            return Task.CompletedTask;
        }

        // Snapshot ids before mutating (CollectionChanged handlers run during removal).
        var nodeIds = new HashSet<string>(_selectedNodeIds);
        var connectorIds = new HashSet<string>(_selectedConnectorIds);

        var nodes = _diagram.Nodes.OfType<Node>().Where(n => nodeIds.Contains(n.Id)).ToList();

        // Selected connectors + any connector tied to a deleted node.
        var connectors = _diagram.Connectors.OfType<Connector>()
            .Where(c => connectorIds.Contains(c.Id)
                || (c.SourceID is { } src && nodeIds.Contains(src))
                || (c.TargetID is { } tgt && nodeIds.Contains(tgt)))
            .ToList();

        foreach (var connector in connectors)
        {
            _diagram.Connectors.Remove(connector);
        }

        foreach (var node in nodes)
        {
            _diagram.Nodes.Remove(node);
        }

        return Task.CompletedTask;
    }

    public async Task UndoAsync()
    {
        if (_diagram is not null)
        {
            await _diagram.Undo();
        }
    }

    public async Task RedoAsync()
    {
        if (_diagram is not null)
        {
            await _diagram.Redo();
        }
    }

    public async Task<bool> CanUndoAsync() => _diagram is not null && await _diagram.CanUndoAsync();

    /// <summary>Prints the diagram (Windows-best per architecture.md; status elsewhere).</summary>
    public async Task PrintAsync(DiagramExportOptions? options = null)
    {
        if (_diagram is null || !IsReady)
        {
            throw new InvalidOperationException("The diagram is not ready to print.");
        }

        await _diagram.Print(options);
    }

    public async Task<bool> CanRedoAsync() => _diagram is not null && await _diagram.CanRedoAsync();

    private static async void OnZoomFactorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramSurfaceBehavior behavior && behavior._diagram is not null && behavior.IsReady
            && newValue is double factor && !factor.Equals(oldValue))
        {
            await behavior.ApplyZoomFactorAsync(factor);
        }
    }

    private async Task ApplyZoomFactorAsync(double? factorOverride = null)
    {
        if (_diagram is not { } diagram)
        {
            return;
        }

        var factor = factorOverride ?? ZoomFactor;
        var center = diagram.Width > 0 && diagram.Height > 0
            ? new Microsoft.Maui.Graphics.Point(diagram.Width / 2, diagram.Height / 2)
            : (Microsoft.Maui.Graphics.Point?)null;
        await diagram.Zoom(factor, center);
    }
}
