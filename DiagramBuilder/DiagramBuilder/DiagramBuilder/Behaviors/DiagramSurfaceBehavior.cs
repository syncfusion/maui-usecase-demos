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
        if (ReadyCommand?.CanExecute(null) == true)
        {
            ReadyCommand.Execute(null);
        }
    }

    private void OnSelectionChanged(object? sender, DiagramSelectionChangedEventArgs e)
    {
        // Extract ids only — the ViewModel works with plain strings, never control types.
        var nodeIds = e.SelectedNodes?.Select(n => n.Id).ToList() ?? [];
        var connectorIds = e.SelectedConnectors?.Select(c => c.Id).ToList() ?? [];
        var payload = new SelectionPayload(nodeIds, connectorIds);

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

    public async Task<bool> CanRedoAsync() => _diagram is not null && await _diagram.CanRedoAsync();

    private static async void OnZoomFactorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DiagramSurfaceBehavior behavior && behavior._diagram is { } diagram
            && newValue is double factor && !factor.Equals(oldValue))
        {
            var center = diagram.Width > 0 && diagram.Height > 0
                ? new Microsoft.Maui.Graphics.Point(diagram.Width / 2, diagram.Height / 2)
                : (Microsoft.Maui.Graphics.Point?)null;
            await diagram.Zoom(factor, center);
        }
    }
}
