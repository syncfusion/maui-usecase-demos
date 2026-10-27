using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Designer orchestrator: owns the child VMs, the central command dispatch table
/// (every menu/toolbar/keyboard id from spec.md §6–7), and document lifecycle.
/// Receives selection/geometry events from DiagramSurfaceBehavior via commands.
/// </summary>
public partial class DiagramDesignerViewModel : ObservableObject
{
    private readonly IDiagramService _diagramService;
    private readonly ITemplateService _templateService;
    private readonly IDialogService _dialogService;

    // Child VMs (delegate to stay under the 500-line limit).
    public MenuBarViewModel MenuBar { get; }
    public ToolbarViewModel Toolbar { get; }
    public StencilViewModel Stencil { get; }
    public PropertyPanelViewModel PropertyPanel { get; }
    public ZoomViewModel Zoom { get; }
    public ArrangeViewModel Arrange { get; }
    public ExportViewModel Export { get; }

    [ObservableProperty]
    private string documentTitle = AppStrings.UntitledDocument;

    [ObservableProperty]
    private string status = AppStrings.StatusReady;

    public DiagramDesignerViewModel(
        IDiagramService diagramService,
        ITemplateService templateService,
        IDialogService dialogService,
        ISelectionService selectionService,
        IPropertyService propertyService,
        IExportService exportService)
    {
        _diagramService = diagramService;
        _templateService = templateService;
        _dialogService = dialogService;
        _diagramService.StatusChanged += (_, message) => Status = message;

        Zoom = new ZoomViewModel();
        Stencil = new StencilViewModel();
        PropertyPanel = new PropertyPanelViewModel(propertyService);
        MenuBar = new MenuBarViewModel(this);
        Toolbar = new ToolbarViewModel(this);
        Arrange = new ArrangeViewModel(this);
        Export = new ExportViewModel(exportService, () => DocumentTitle);

        Selection = selectionService;
        _selectionService = selectionService;
        _selectionService.SelectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SelectionSummary));
            Toolbar.RaiseSelectionChanged();
            PropertyPanel.Refresh(SelectedNodeId);
        };

        SyncTitle();
    }

    public ISelectionService Selection { get; }

    private readonly ISelectionService _selectionService;

    public string SelectionSummary => Selection.SelectionSummary;

    /// <summary>Id of the single selected node, or null (drives group-6 + property panel).</summary>
    public string? SelectedNodeId =>
        Selection.SelectedNodeIds.Count == 1 ? Selection.SelectedNodeIds[0] : null;

    // ---------- Event surface used by DiagramSurfaceBehavior (Task 6) ----------
    // The behavior extracts ids from control event args and passes plain strings here —
    // ViewModels never reference Syncfusion control types.

    /// <summary>Receives the selected node/connector ids extracted by the behavior.</summary>
    public void ApplySelection(IReadOnlyList<string> nodeIds, IReadOnlyList<string> connectorIds)
    {
        _selectionService.ApplySelection(nodeIds, connectorIds);
    }

    [RelayCommand]
    private void OnGeometryChanged(object? parameter)
    {
        if (SelectedNodeId is not null)
        {
            PropertyPanel.Refresh(SelectedNodeId);
        }
    }

    [RelayCommand]
    private void OnAnnotationEdited(object? parameter)
    {
        _diagramService.SetDirty();
    }

    [RelayCommand]
    private void OnCollectionChanged(object? parameter)
    {
        _diagramService.SetDirty();
    }

    // ---------- Document lifecycle ----------

    [RelayCommand]
    private async Task NewDocumentAsync()
    {
        await Shell.Current.GoToAsync($"//{AppConstants.RouteCreateDiagram}");
    }

    [RelayCommand]
    private async Task OpenDocumentAsync()
    {
        var recents = await _diagramService.GetRecentDocumentsAsync();
        var message = recents.Count > 0
            ? string.Join(Environment.NewLine, recents)
            : AppStrings.OpenNotAvailableMessage;
        await _dialogService.ShowAlertAsync(AppStrings.OpenNotAvailableTitle, message);
    }

    [RelayCommand]
    private async Task SaveDocumentAsync()
    {
        await _diagramService.SaveAsync();
        SyncTitle();
    }

    [RelayCommand]
    private async Task SaveAsDocumentAsync()
    {
        var newTitle = DocumentTitle + " (2)";
        await _diagramService.SaveAsAsync(newTitle);
        SyncTitle();
    }

    [RelayCommand]
    private async Task PrintDocumentAsync() => await Task.CompletedTask;

    [RelayCommand]
    private Task ShowExportDialogAsync() => Task.CompletedTask;

    // ---------- Central dispatch table (spec.md §6–7) ----------

    /// <summary>
    /// Executes a command id from the menu/toolbar/keyboard. ViewModels and controls
    /// route here; never duplicate dispatch logic.
    /// </summary>
    public async Task ExecuteCommand(string commandId)
    {
        switch (commandId)
        {
            // FILE
            case AppConstants.CmdNew: await NewDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdOpen: await OpenDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdSave: await SaveDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdSaveAs: await SaveAsDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdExport: await ShowExportDialogCommand.ExecuteAsync(null); break;
            case AppConstants.CmdPrint: await PrintDocumentCommand.ExecuteAsync(null); break;

            // EDIT (undo/redo handled by the behavior; clipboard ops are control-supported,
            // the rest dispatch with status messages via the mock services).
            case AppConstants.CmdUndo:
            case AppConstants.CmdRedo:
            case AppConstants.CmdCut:
            case AppConstants.CmdCopy:
            case AppConstants.CmdPaste:
            case AppConstants.CmdDelete:
            case AppConstants.CmdDuplicate:
            case AppConstants.CmdSelectAll:
                Status = string.Format(AppStrings.StatusCommandExecuted, commandId);
                break;

            // VIEW
            case AppConstants.CmdZoomIn: Zoom.ZoomInCommand.Execute(null); break;
            case AppConstants.CmdZoomOut: Zoom.ZoomOutCommand.Execute(null); break;
            case AppConstants.CmdFitToScreen: Zoom.FitToScreenCommand.Execute(null); break;
            case AppConstants.CmdShowGrid: Zoom.ToggleShowGridCommand.Execute(null); break;
            case AppConstants.CmdSnapToGrid: Zoom.ToggleSnapToGridCommand.Execute(null); break;

            // ARRANGE
            case AppConstants.CmdSendToBack:
            case AppConstants.CmdBringToFront:
            case AppConstants.CmdSendBackward:
            case AppConstants.CmdBringForward:
            case AppConstants.CmdAlignLeft:
            case AppConstants.CmdAlignRight:
            case AppConstants.CmdAlignCenter:
            case AppConstants.CmdAlignTop:
            case AppConstants.CmdAlignBottom:
            case AppConstants.CmdAlignMiddle:
            case AppConstants.CmdDistributeHorizontally:
            case AppConstants.CmdDistributeVertically:
            case AppConstants.CmdSameWidth:
            case AppConstants.CmdSameHeight:
            case AppConstants.CmdSameWidthAndHeight:
            case AppConstants.CmdGroup:
            case AppConstants.CmdUngroup:
                Status = string.Format(AppStrings.StatusCommandExecuted, commandId);
                break;

            // WINDOW
            case AppConstants.CmdShowToolbar: ToggleToolbarCommand.Execute(null); break;
            case AppConstants.CmdShowStencil: ToggleStencilCommand.Execute(null); break;
            case AppConstants.CmdShowProperties: TogglePropertiesCommand.Execute(null); break;

            default:
                throw new NotSupportedException($"Unknown command id: {commandId}");
        }
    }

    // ---------- Window panel toggles ----------

    [ObservableProperty]
    private bool showToolbar = true;

    [ObservableProperty]
    private bool showStencil = true;

    [ObservableProperty]
    private bool showProperties = true;

    [RelayCommand]
    private void ToggleToolbar() => ShowToolbar = !ShowToolbar;

    [RelayCommand]
    private void ToggleStencil() => ShowStencil = !ShowStencil;

    [RelayCommand]
    private void ToggleProperties() => ShowProperties = !ShowProperties;

    private void SyncTitle()
    {
        DocumentTitle = _diagramService.CurrentDocument?.Title ?? AppStrings.UntitledDocument;
    }
}
