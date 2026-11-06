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
    private readonly IPageSettingsService _pageSettings;
    private readonly CreateDiagramPage _createDiagramPage;
    private readonly Views.Dialogs.ExportDialog _exportDialog;
    private readonly ContentPage _exportDialogHost;
    private readonly System.ComponentModel.PropertyChangedEventHandler _zoomSubscription;
    private bool _dialogShown;

    public DiagramDesignerPage(
        DiagramDesignerViewModel viewModel,
        IDiagramService diagramService,
        IPropertyService propertyService,
        IPageSettingsService pageSettings,
        CreateDiagramPage createDiagramPage,
        Views.Dialogs.ExportDialog exportDialog)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
        _diagramService = diagramService;
        _propertyService = propertyService;
        _pageSettings = pageSettings;
        _createDiagramPage = createDiagramPage;
        _exportDialog = exportDialog;
        _exportDialog.BindingContext = viewModel.Export;
        _exportDialogHost = new ContentPage
        {
            Title = Constants.AppStrings.ExportDialogTitle,
            Content = _exportDialog
        };

        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;

        // Wire the single workspace. The SymbolPalette it discovers lives inside
        // the workspace control's DiagramGrid alongside the SfDiagram, so palette
        // discovery succeeds regardless of how the host page is laid out.
        WireWorkspace(Workspace);

        // Template-loaded wiring is constructor-scoped (never torn down): pushing
        // the Create-Diagram modal fires this page's OnDisappearing immediately
        // after OnAppearing, which would unsubscribe the handler below before the
        // user ever picks a template. With no listener, PropertyService.Track is
        // never called, every Read() returns no rows and the property panel
        // renders empty sections. The page is transient, so the subscription is
        // released with the page itself (same lifetime policy as DocumentLoaded).
        Workspace.TemplateLoaded += OnTemplateLoaded;

        // Live element changes (palette drop, delete, duplicate) keep the property
        // service's tracked set in step with the diagram. Without this, a shape
        // dropped from the palette is never tracked, so selecting it yields no
        // property rows and the panel renders empty sections.
        Workspace.ElementAdded += OnElementChanged;
        Workspace.ElementsReset += OnElementsReset;

        // Page settings: the canvas background is the only drawing-area property
        // the MAUI Diagram control supports at runtime (format/orientation have
        // no API), so it is the only one exposed. Applied whenever it changes…
        _pageSettings.Changed += OnPageSettingsChanged;
        // …and once here so the canvas matches the service's seed.
        Workspace.ApplyPageSettings(_pageSettings.BackgroundColorKey);

        // Mirror the shared Stencil VM palette groups into the native SymbolPalette
        // (inside the workspace control). Syncfusion's JS bridge caches the Palettes
        // reference at initialisation, so the collection must be ASSIGNED to
        // SymbolPalette.Palettes rather than mutated via Clear()/Add() — per the
        // Syncfusion diagram-sdk MAUI getting-started docs.
        var paletteGroups = viewModel.Stencil.PaletteGroups;
        Workspace.ShapesPalette.Palettes = paletteGroups;
        ((INotifyCollectionChanged)paletteGroups).CollectionChanged +=
            (_, _) => Workspace.ShapesPalette.Palettes = paletteGroups;

        // The property panel gets an explicit collapse button in the trailing slot
        // so the header uses a clearer close/collapse affordance than the default
        // chevron, while the stencil keeps its own chevron header.
        PropertyPanel.Header.ShowChevron = false;
        PropertyPanel.Header.Trailing = BuildPropertyPanelTrailingButton();

        // Wire surface request relays (VM → control, one-way; the VM stays
        // control-agnostic — each delegate only awaits a behavior method).
        viewModel.UndoRequested = () => Workspace.Behavior.UndoAsync();
        viewModel.RedoRequested = () => Workspace.Behavior.RedoAsync();
        viewModel.CutRequested = () => Workspace.Behavior.CutAsync();
        viewModel.CopyRequested = () => Workspace.Behavior.CopyAsync();
        viewModel.PasteRequested = () => Workspace.Behavior.PasteAsync();
        viewModel.DeleteRequested = () => Workspace.Behavior.DeleteSelectionAsync();
        viewModel.ToolRequested = Workspace.ApplyToolAsync;
        viewModel.BringToFrontRequested = () => Workspace.BringToFrontAsync();
        viewModel.SendToBackRequested = () => Workspace.SendToBackAsync();
        viewModel.BringForwardRequested = () => Workspace.BringForwardAsync();
        viewModel.SendBackwardRequested = () => Workspace.SendBackwardAsync();
        // Print/export use the control's defaults: the MAUI Diagram exposes page
        // settings only on DiagramExportOptions, and there is no live page
        // model backing them, so no unsupported overrides are passed.
        viewModel.PrintRequested = () => Workspace.Behavior.PrintAsync();
        viewModel.Zoom.FitToPageRequested = new Command(() => _ = Workspace.Behavior.FitToPageAsync());

        _zoomSubscription = OnZoomPropertyChanged;
        Workspace.SurfaceBehaviorZoomFactor = viewModel.Zoom.CurrentZoom;

        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
    }

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

    private void OnZoomPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ZoomViewModel.CurrentZoom)
            && sender is ZoomViewModel zoom)
        {
            Workspace.SurfaceBehaviorZoomFactor = zoom.CurrentZoom;
        }
    }

    private void OnTemplateLoaded(object? sender, DiagramWorkspaceControl.TemplateLoadPayload payload)
    {
        _propertyService.Track(payload.Nodes, payload.Connectors);
    }

    /// <summary>
    /// Keeps the property service's tracked elements in step with the diagram so
    /// shapes added after the template load (palette drop, duplicate, connector
    /// tool) can be edited from the property panel.
    /// </summary>
    private void OnElementChanged(object? sender, DiagramWorkspaceControl.ElementChangePayload payload)
    {
        if (payload.Added)
        {
            _propertyService.TrackAdded(payload.Element);
        }
        else
        {
            _propertyService.Untrack(payload.ElementId);
        }
    }

    /// <summary>Pushes the drawing-area background colour onto the canvas host views.</summary>
    private void OnPageSettingsChanged(object? sender, EventArgs e)
    {
        Workspace.ApplyPageSettings(_pageSettings.BackgroundColorKey);
    }

    /// <summary>Clears the tracked set when the diagram's collections are reset.</summary>
    private void OnElementsReset(object? sender, EventArgs e)
    {
        _propertyService.Track([], []);
    }

    /// <summary>Document loaded by service → push to live SfDiagram (template load).</summary>
    private void OnDocumentLoaded(object? sender, Models.DiagramDocument document)
    {
        Workspace.LoadTemplate(document);
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

    private View BuildPropertyPanelTrailingButton()
    {
        var button = new Button
        {
            Text = Constants.MaterialIcons.ChevronRight,
            FontFamily = "MaterialIcons",
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "property_panel_close",
        };
        button.Clicked += (_, _) => _viewModel.TogglePropertiesCommand.Execute(null);
        return button;
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
        // TemplateLoaded is likewise constructor-scoped (see the ctor) — a template
        // load must reach PropertyService.Track even while the modal is up.
        _viewModel.Zoom.PropertyChanged -= _zoomSubscription;
    }
}
