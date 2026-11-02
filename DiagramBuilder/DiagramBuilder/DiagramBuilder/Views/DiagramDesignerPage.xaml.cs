using System.Collections.Specialized;
using System.ComponentModel;
using DiagramBuilder.Controls;
using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    private readonly DiagramDesignerViewModel _viewModel;
    private readonly IDiagramService _diagramService;
    private readonly IPropertyService _propertyService;
    private readonly CreateDiagramPage _createDiagramPage;
    private readonly Views.Dialogs.ExportDialog _exportDialog;
    private readonly ContentPage _exportDialogHost;
    private readonly System.ComponentModel.PropertyChangedEventHandler _zoomSubscription;
    private bool _dialogShown;

    public DiagramDesignerPage(
        DiagramDesignerViewModel viewModel,
        IDiagramService diagramService,
        IPropertyService propertyService,
        CreateDiagramPage createDiagramPage,
        Views.Dialogs.ExportDialog exportDialog)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _diagramService = diagramService;
        _propertyService = propertyService;
        _createDiagramPage = createDiagramPage;
        _exportDialog = exportDialog;
        _exportDialog.BindingContext = viewModel.Export;
        _exportDialogHost = new ContentPage
        {
            Title = Constants.AppStrings.ExportDialogTitle,
            Content = _exportDialog
        };

        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;

        // Wire both Workspace instances so the same commands work whether the
        // shapes panel is visible or the canvas is full-bleed.
        WireWorkspace(WorkspaceWithShapes);
        WireWorkspace(WorkspaceFullBleed);

        // Mirror the shared Stencil VM palette groups into the native SymbolPalette
        // (Shapes panel). Syncfusion handles drag-and-drop from there to the
        // SfDiagram, which is the direct Grid sibling it expects.
        CopyPaletteGroups(viewModel.Stencil.PaletteGroups, ShapesPalette.Palettes);
        ((INotifyCollectionChanged)viewModel.Stencil.PaletteGroups).CollectionChanged +=
            (_, _) => CopyPaletteGroups(viewModel.Stencil.PaletteGroups, ShapesPalette.Palettes);

        // Wire panel-header chevrons to the show/hide relay commands.
        ShapesHeader.ToggleCommand = new Command(() => viewModel.ShowStencil = !viewModel.ShowStencil);
        PropertyPanel.Header.ToggleCommand = new Command(() => viewModel.ShowProperties = !viewModel.ShowProperties);

        // Wire surface request relays (VM → control, one-way; the VM stays
        // control-agnostic — each delegate only awaits a behavior method).
        viewModel.UndoRequested = () => ActiveWorkspace().Behavior.UndoAsync();
        viewModel.RedoRequested = () => ActiveWorkspace().Behavior.RedoAsync();
        viewModel.CutRequested = () => ActiveWorkspace().Behavior.CutAsync();
        viewModel.CopyRequested = () => ActiveWorkspace().Behavior.CopyAsync();
        viewModel.PasteRequested = () => ActiveWorkspace().Behavior.PasteAsync();
        viewModel.DeleteRequested = () => ActiveWorkspace().Behavior.DeleteSelectionAsync();
        viewModel.PrintRequested = () => ActiveWorkspace().Behavior.PrintAsync();
        viewModel.Zoom.FitToPageRequested = new Command(() => _ = ActiveWorkspace().Behavior.FitToPageAsync());

        _zoomSubscription = OnZoomPropertyChanged;
        WorkspaceWithShapes.SurfaceBehaviorZoomFactor = viewModel.Zoom.CurrentZoom;
        WorkspaceFullBleed.SurfaceBehaviorZoomFactor = viewModel.Zoom.CurrentZoom;

        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
    }

    /// <summary>
    /// Routes diagram-surface relay commands to the visible workspace. There are
    /// two Workspace instances in the visual tree (one in each layout mode), so
    /// commands dispatch to whichever is currently shown.
    /// </summary>
    private DiagramWorkspaceControl ActiveWorkspace() =>
        _viewModel.ShowStencil ? WorkspaceWithShapes : WorkspaceFullBleed;

    private void WireWorkspace(DiagramWorkspaceControl workspace)
    {
        workspace.ReadyCommand = new Command(() => _viewModel.SetStatus(Constants.AppStrings.StatusReady));
        workspace.SelectionChangedCommand = new Command(payload =>
        {
            if (payload is DiagramBuilder.Behaviors.DiagramSurfaceBehavior.SelectionPayload sp)
            {
                _viewModel.ApplySelection(sp.NodeIds, sp.ConnectorIds);
            }
        });
        workspace.GeometryChangedCommand = _viewModel.GeometryChangedCommand;
        workspace.AnnotationEditedCommand = _viewModel.AnnotationEditedCommand;
        workspace.CollectionChangedCommand = _viewModel.CollectionChangedCommand;
    }

    private static void CopyPaletteGroups(
        System.Collections.ObjectModel.ObservableCollection<object> source,
        System.Collections.IList target)
    {
        target.Clear();
        foreach (var palette in source.OfType<Syncfusion.Maui.Diagram.Palette>())
        {
            target.Add(palette);
        }
    }

    private void OnZoomPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ZoomViewModel.CurrentZoom)
            && sender is ZoomViewModel zoom)
        {
            WorkspaceWithShapes.SurfaceBehaviorZoomFactor = zoom.CurrentZoom;
            WorkspaceFullBleed.SurfaceBehaviorZoomFactor = zoom.CurrentZoom;
        }
    }

    private void OnTemplateLoaded(object? sender, DiagramWorkspaceControl.TemplateLoadPayload payload)
    {
        _propertyService.Track(payload.Nodes, payload.Connectors);
    }

    /// <summary>Document loaded by service → push to live SfDiagram (template load).</summary>
    private void OnDocumentLoaded(object? sender, Models.DiagramDocument document)
    {
        // Mirror the template into both workspaces so the visible one shows it.
        WorkspaceWithShapes.LoadTemplate(document);
        WorkspaceFullBleed.LoadTemplate(document);
        _viewModel.SetDocumentTitle(document.Title);
        _viewModel.SetStatus(string.Format(Constants.AppStrings.StatusOpenedFormat, document.Title));
    }

    private async void OnExportDialogRequested(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(_exportDialogHost);
    }

    private async void OnCreateDialogRequested(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(_createDiagramPage);
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        // The diagram service is a singleton whose DocumentLoaded event fires from
        // the CreateDiagram modal. Pushing the modal triggers OnDisappearing on
        // this page (per MAUI Shell modal lifecycle), which would tear the
        // subscription down before the user picks a template — leaving the event
        // with no listener. We keep the service-level subscription alive for the
        // lifetime of the page (the page is transient so the handler is GC'd with
        // it), and only re-establish the page-scoped UI subscriptions each time.
        // Idempotent subscribe: -= first, then += to avoid double-subscription
        // when OnAppearing fires multiple times (e.g. after a modal pops).
        _diagramService.DocumentLoaded -= OnDocumentLoaded;
        _diagramService.DocumentLoaded += OnDocumentLoaded;

        _viewModel.ExportDialogRequested -= OnExportDialogRequested;
        _viewModel.ExportDialogRequested += OnExportDialogRequested;

        _viewModel.CreateDialogRequested -= OnCreateDialogRequested;
        _viewModel.CreateDialogRequested += OnCreateDialogRequested;

        _viewModel.Zoom.PropertyChanged -= _zoomSubscription;
        _viewModel.Zoom.PropertyChanged += _zoomSubscription;

        WorkspaceWithShapes.TemplateLoaded -= OnTemplateLoaded;
        WorkspaceWithShapes.TemplateLoaded += OnTemplateLoaded;

        WorkspaceFullBleed.TemplateLoaded -= OnTemplateLoaded;
        WorkspaceFullBleed.TemplateLoaded += OnTemplateLoaded;

        if (_dialogShown)
        {
            return;
        }

        _dialogShown = true;
        await Task.Delay(50);
        await Shell.Current.Navigation.PushModalAsync(_createDiagramPage);
    }

    private void OnDisappearing(object? sender, EventArgs e)
    {
        // Symmetric teardown for page-scoped UI subscriptions only. Service-level
        // subscriptions are kept (see OnAppearing for the rationale) so a
        // DocumentLoaded event fired while the modal is up still reaches the page.
        WorkspaceWithShapes.TemplateLoaded -= OnTemplateLoaded;
        WorkspaceFullBleed.TemplateLoaded -= OnTemplateLoaded;
        _viewModel.Zoom.PropertyChanged -= _zoomSubscription;
    }
}
