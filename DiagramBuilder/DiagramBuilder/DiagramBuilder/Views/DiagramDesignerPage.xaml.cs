using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    private readonly DiagramDesignerViewModel _viewModel;
    private readonly IDiagramService _diagramService;
    private readonly IPropertyService _propertyService;
    private readonly CreateDiagramPage _createDiagramPage;
    private readonly Views.Dialogs.ExportDialog _exportDialog;
    private readonly ContentPage _exportDialogHost;
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

        // Binding-context resolution + data mapping only (no business logic):
        // the palette is now bound through the VM (StencilViewModel.PaletteGroups).
        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;

        // Wire diagram surface relay commands (control → VM, one-way).
        Workspace.ReadyCommand = new Command(() => viewModel.SetStatus(Constants.AppStrings.StatusReady));
        Workspace.SelectionChangedCommand = new Command(payload =>
        {
            if (payload is DiagramBuilder.Behaviors.DiagramSurfaceBehavior.SelectionPayload sp)
            {
                viewModel.ApplySelection(sp.NodeIds, sp.ConnectorIds);
            }
        });
        // CommunityToolkit.Mvvm strips the leading "On" when generating [RelayCommand] names,
        // so OnGeometryChanged / OnAnnotationEdited / OnCollectionChanged produce commands
        // named GeometryChangedCommand / AnnotationEditedCommand / CollectionChangedCommand.
        Workspace.GeometryChangedCommand = viewModel.GeometryChangedCommand;
        Workspace.AnnotationEditedCommand = viewModel.AnnotationEditedCommand;
        Workspace.CollectionChangedCommand = viewModel.CollectionChangedCommand;

        // Wire panel-header chevrons to the show/hide relay commands.
        StencilPanel.Header.ToggleCommand = new Command(() => viewModel.ShowStencil = !viewModel.ShowStencil);
        PropertyPanel.Header.ToggleCommand = new Command(() => viewModel.ShowProperties = !viewModel.ShowProperties);

        // Wire surface request relays (VM → control, one-way; the VM stays
        // control-agnostic — each delegate only awaits a behavior method).
        viewModel.UndoRequested = () => Workspace.Behavior.UndoAsync();
        viewModel.RedoRequested = () => Workspace.Behavior.RedoAsync();
        viewModel.CutRequested = () => Workspace.Behavior.CutAsync();
        viewModel.CopyRequested = () => Workspace.Behavior.CopyAsync();
        viewModel.PasteRequested = () => Workspace.Behavior.PasteAsync();
        viewModel.DeleteRequested = () => Workspace.Behavior.DeleteSelectionAsync();
        viewModel.PrintRequested = () => Workspace.Behavior.PrintAsync();
        viewModel.Zoom.FitToPageRequested = new Command(() => _ = Workspace.Behavior.FitToPageAsync());

        // Zoom state passthrough: ZoomViewModel.CurrentZoom → behavior ZoomFactor →
        // SfDiagram.Zoom (data mapping relay only, no business logic).
        _zoomSubscription = OnZoomPropertyChanged;
        Workspace.SurfaceBehaviorZoomFactor = viewModel.Zoom.CurrentZoom;

        // Event subscriptions are made in OnAppearing / removed in OnDisappearing
        // only (symmetric pairs — see those handlers).
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
    }

    private readonly System.ComponentModel.PropertyChangedEventHandler _zoomSubscription;

    private void OnZoomPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ViewModels.ZoomViewModel.CurrentZoom)
            && sender is ViewModels.ZoomViewModel zoom)
        {
            Workspace.SurfaceBehaviorZoomFactor = zoom.CurrentZoom;
        }
    }

    private void OnTemplateLoaded(object? sender, DiagramWorkspaceControl.TemplateLoadPayload payload)
    {
        _propertyService.Track(payload.Nodes, payload.Connectors);
    }

    /// <summary>Document loaded by service → push to live SfDiagram (template load).</summary>
    private void OnDocumentLoaded(object? sender, Models.DiagramDocument document)
    {
        Workspace.LoadTemplate(document);
        _viewModel.SetDocumentTitle(document.Title);
        _viewModel.SetStatus(string.Format(Constants.AppStrings.StatusOpenedFormat, document.Title));
    }

    /// <summary>VM request → modal push relay (no business logic).</summary>
    private async void OnExportDialogRequested(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(_exportDialogHost);
    }

    /// <summary>FILE ▸ New → push the create-diagram modal (same flow as launch).</summary>
    private async void OnCreateDialogRequested(object? sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushModalAsync(_createDiagramPage);
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        // Re-establish subscriptions torn down in OnDisappearing — pushing the
        // create/export modals triggers Disappearing on this page, and the
        // document loaded on return must still reach the canvas.
        _diagramService.DocumentLoaded += OnDocumentLoaded;
        _viewModel.ExportDialogRequested += OnExportDialogRequested;
        _viewModel.CreateDialogRequested += OnCreateDialogRequested;
        _viewModel.Zoom.PropertyChanged += _zoomSubscription;
        Workspace.TemplateLoaded += OnTemplateLoaded;

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
        // Symmetric teardown — transient page outlives the singleton VMs, so its
        // subscriptions must be removed to avoid duplicate-handler leaks across
        // page instances (validation.md §2).
        _diagramService.DocumentLoaded -= OnDocumentLoaded;
        _viewModel.ExportDialogRequested -= OnExportDialogRequested;
        _viewModel.CreateDialogRequested -= OnCreateDialogRequested;
        _viewModel.Zoom.PropertyChanged -= _zoomSubscription;
        Workspace.TemplateLoaded -= OnTemplateLoaded;
    }
}
