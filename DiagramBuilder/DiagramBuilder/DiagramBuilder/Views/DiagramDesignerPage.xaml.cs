using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    private readonly DiagramDesignerViewModel _viewModel;
    private readonly IDiagramService _diagramService;
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
        Workspace.GeometryChangedCommand = viewModel.OnGeometryChangedCommand;
        Workspace.AnnotationEditedCommand = viewModel.OnAnnotationEditedCommand;
        Workspace.CollectionChangedCommand = viewModel.OnCollectionChangedCommand;

        // Wire panel-header chevrons to the show/hide relay commands.
        StencilPanel.Header.ToggleCommand = new Command(() => viewModel.ShowStencil = !viewModel.ShowStencil);
        PropertyPanel.Header.ToggleCommand = new Command(() => viewModel.ShowProperties = !viewModel.ShowProperties);

        viewModel.ExportDialogRequested += OnExportDialogRequested;
        _diagramService.DocumentLoaded += OnDocumentLoaded;
        Workspace.TemplateLoaded += (_, payload) => propertyService.Track(payload.Nodes, payload.Connectors);
        Appearing += OnAppearing;
        Disappearing += OnDisappearing;
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

    private async void OnAppearing(object? sender, EventArgs e)
    {
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
        _diagramService.DocumentLoaded -= OnDocumentLoaded;
    }
}
