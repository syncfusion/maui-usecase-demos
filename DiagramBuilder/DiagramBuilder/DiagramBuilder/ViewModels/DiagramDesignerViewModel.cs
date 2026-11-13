using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;
using Microsoft.Maui.ApplicationModel;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Designer orchestrator: owns the child VMs, the central command dispatch table
/// (every menu/toolbar/keyboard id from spec.md §6–7), and document lifecycle.
/// Receives selection/geometry events from DiagramSurfaceBehavior via commands.
/// </summary>
public partial class DiagramDesignerViewModel : ObservableObject
{
    private readonly IDiagramService _diagramService;
    private readonly IDiagramFileService _fileService;
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
    public PrintSettingsViewModel PrintSettings { get; }

    [ObservableProperty]
    private string documentTitle = AppStrings.UntitledDocument;

    private string _saveAsFileName = AppStrings.UntitledDocument;

    public string SaveAsFileName
    {
        get => _saveAsFileName;
        set => SetProperty(ref _saveAsFileName, value);
    }

    [ObservableProperty]
    private string status = AppStrings.StatusReady;

    public DiagramDesignerViewModel(
        IDiagramService diagramService,
        IDiagramFileService fileService,
        ITemplateService templateService,
        IDialogService dialogService,
        ISelectionService selectionService,
        IPropertyService propertyService)
    {
        _diagramService = diagramService;
        _fileService = fileService;
        _templateService = templateService;
        _dialogService = dialogService;
        _diagramService.StatusChanged += (_, message) => Status = message;

        Zoom = new ZoomViewModel();
        Stencil = new StencilViewModel();
        PropertyPanel = new PropertyPanelViewModel(propertyService);
        MenuBar = new MenuBarViewModel(this);
        Toolbar = new ToolbarViewModel(this);
        Arrange = new ArrangeViewModel(this);
        Export = new ExportViewModel(fileService, dialogService, () => DocumentTitle);
        PrintSettings = new PrintSettingsViewModel(dialogService);

        Selection = selectionService;
        _selectionService = selectionService;
        _selectionService.SelectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(SelectionSummary));
            OnPropertyChanged(nameof(SelectedNodeId));
            OnPropertyChanged(nameof(SelectedConnectorId));
            OnPropertyChanged(nameof(SelectedElementId));
            Toolbar.RaiseSelectionChanged();
            PropertyPanel.Refresh(SelectedElementId);
            // Mirror the selection summary into the status strip (single status bar).
            Status = Selection.SelectionSummary;
        };

        SyncTitle();
    }

    public ISelectionService Selection { get; }

    private readonly ISelectionService _selectionService;

    public string SelectionSummary => Selection.SelectionSummary;

    /// <summary>Id of the single selected node, or null (drives group-6 + property panel).</summary>
    public string? SelectedNodeId =>
        Selection.SelectedNodeIds.Count == 1 ? Selection.SelectedNodeIds[0] : null;

    public string? SelectedConnectorId =>
        Selection.SelectedConnectorIds.Count == 1 ? Selection.SelectedConnectorIds[0] : null;

    public string? SelectedElementId =>
        Selection.SelectedNodeIds.Count == 1 && Selection.SelectedConnectorIds.Count == 0
            ? Selection.SelectedNodeIds[0]
            : Selection.SelectedConnectorIds.Count == 1 && Selection.SelectedNodeIds.Count == 0
                ? Selection.SelectedConnectorIds[0]
                : null;

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
        if (SelectedElementId is not null)
        {
            PropertyPanel.Refresh(SelectedElementId);
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

    /// <summary>Raised when the create-diagram dialog should be shown (host page pushes it modally).</summary>
    public event EventHandler? CreateDialogRequested;

    [RelayCommand]
    private void NewDocument()
    {
        // Spec: FILE ▸ New returns to the create dialog (modal over the designer).
        // The host page pushes the same CreateDiagramPage instance used at launch so
        // the modal push/pop flow stays symmetric.
        CreateDialogRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task OpenDocumentAsync()
    {
        try
        {
            var document = await _fileService.PickDiagramAsync();
            if (document is not null)
            {
                await _diagramService.LoadDocumentAsync(document);
                _selectionService.Clear();
                SyncTitle();
            }
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or FeatureNotSupportedException)
        {
            await _dialogService.ShowAlertAsync(AppStrings.FileOpenFailedTitle, exception.Message);
        }
    }

    [RelayCommand]
    private async Task SaveDocumentAsync()
    {
        await SaveSnapshotAsync(AppStrings.UntitledDocument, saveAs: false);
    }

    [RelayCommand]
    private void SaveAsDocument()
    {
        SaveAsFileName = DocumentTitle;
        SaveAsDialogRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task ConfirmSaveAsAsync()
    {
        if (string.IsNullOrWhiteSpace(SaveAsFileName))
        {
            await _dialogService.ShowAlertAsync(AppStrings.FileSaveFailedTitle, AppStrings.FileNameInvalidMessage);
            return;
        }

        if (await SaveSnapshotAsync(SaveAsFileName, saveAs: true))
        {
            SaveAsCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    private void CancelSaveAs() => SaveAsCancelled?.Invoke(this, EventArgs.Empty);

    private async Task<bool> SaveSnapshotAsync(string fileName, bool saveAs)
    {
        var source = _diagramService.CurrentDocument;
        var snapshot = DiagramSnapshotRequested?.Invoke() ?? source;
        if (snapshot is null)
        {
            await _dialogService.ShowAlertAsync(AppStrings.FileSaveFailedTitle, AppStrings.SaveDialogFailedMessage);
            return false;
        }

        try
        {
            var path = saveAs
                ? await _fileService.SaveDiagramAsAsync(snapshot, fileName)
                : await _fileService.SaveDiagramAsync(snapshot, fileName);
            var title = Path.GetFileNameWithoutExtension(path);
            if (saveAs)
            {
                await _diagramService.SaveAsAsync(snapshot, title);
            }
            else
            {
                await _diagramService.SaveAsync(snapshot, title);
            }

            SetDocumentTitle(title);
            SaveAsFileName = title;
            Status = string.Format(AppStrings.FileSavedPathFormat, path);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            await _dialogService.ShowAlertAsync(AppStrings.FileSaveFailedTitle, exception.Message);
            return false;
        }
    }

    [RelayCommand]
    private void PrintDocument() => PrintDialogRequested?.Invoke(this, EventArgs.Empty);

    public event EventHandler? SaveAsDialogRequested;
    public event EventHandler? SaveAsCompleted;
    public event EventHandler? SaveAsCancelled;
    public event EventHandler? PrintDialogRequested;

    public Func<DiagramDocument?>? DiagramSnapshotRequested { get; set; }

    /// <summary>Raised when the export dialog should be shown.</summary>
    public event EventHandler? ExportDialogRequested;

    // ---------- Surface request relays (set by the host page; VM stays control-agnostic) ----------
    // The page assigns each delegate to the matching DiagramSurfaceBehavior or
    // workspace method (architecture.md §12: one-way VM→control by awaiting the
    // control's async methods). A null delegate degrades to an explicit status
    // message so every command still executes without crash (Task 9 acceptance).

    /// <summary>Executes undo on the live diagram, or null when unbound.</summary>
    public Func<Task>? UndoRequested { get; set; }

    /// <summary>Executes redo on the live diagram, or null when unbound.</summary>
    public Func<Task>? RedoRequested { get; set; }

    /// <summary>Cuts the selection on the live diagram, or null when unbound.</summary>
    public Func<Task>? CutRequested { get; set; }

    /// <summary>Copies the selection on the live diagram, or null when unbound.</summary>
    public Func<Task>? CopyRequested { get; set; }

    /// <summary>Pastes on the live diagram, or null when unbound.</summary>
    public Func<Task>? PasteRequested { get; set; }

    /// <summary>Deletes the selection on the live diagram, or null when unbound.</summary>
    public Func<Task>? DeleteRequested { get; set; }

    /// <summary>Changes the live diagram's tool mode or starts a drawing interaction.</summary>
    public Func<string, Task>? ToolRequested { get; set; }

    /// <summary>Brings the selection to the front, or null when unbound.</summary>
    public Func<Task>? BringToFrontRequested { get; set; }

    /// <summary>Sends the selection to the back, or null when unbound.</summary>
    public Func<Task>? SendToBackRequested { get; set; }

    /// <summary>Moves the selection one step forward, or null when unbound.</summary>
    public Func<Task>? BringForwardRequested { get; set; }

    /// <summary>Moves the selection one step backward, or null when unbound.</summary>
    public Func<Task>? SendBackwardRequested { get; set; }

    /// <summary>Runs an align / distribute / match-size / group command on the live diagram.</summary>
    public Func<string, Task>? ArrangeRequested { get; set; }

    private async Task RunSurfaceRequestAsync(Func<Task>? request, string commandId)
    {
        if (request is null)
        {
            Status = string.Format(AppStrings.StatusCommandExecuted, commandId);
            return;
        }

        await request();
    }

    private async Task RunToolRequestAsync(string toolId)
    {
        if (ToolRequested is null)
        {
            Status = string.Format(AppStrings.StatusCommandExecuted, toolId);
            return;
        }

        await ToolRequested(toolId);
    }

    [RelayCommand]
    private void ShowExportDialog() => ExportDialogRequested?.Invoke(this, EventArgs.Empty);

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
            case AppConstants.CmdNew: NewDocumentCommand.Execute(null); break;
            case AppConstants.CmdOpen: await OpenDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdSave: await SaveDocumentCommand.ExecuteAsync(null); break;
            case AppConstants.CmdSaveAs: SaveAsDocumentCommand.Execute(null); break;
            case AppConstants.CmdExport: ShowExportDialogCommand.Execute(null); break;
            case AppConstants.CmdPrint: PrintDocumentCommand.Execute(null); break;

            // EDIT: control-supported operations route to the behavior relays
            // (architecture.md §12). Duplicate and Select All have no SfDiagram
            // API in this package version (verified 35.1.42) — status messages.
            case AppConstants.CmdUndo: await RunSurfaceRequestAsync(UndoRequested, commandId); break;
            case AppConstants.CmdRedo: await RunSurfaceRequestAsync(RedoRequested, commandId); break;
            case AppConstants.CmdCut: await RunSurfaceRequestAsync(CutRequested, commandId); break;
            case AppConstants.CmdCopy: await RunSurfaceRequestAsync(CopyRequested, commandId); break;
            case AppConstants.CmdPaste: await RunSurfaceRequestAsync(PasteRequested, commandId); break;
            case AppConstants.CmdDelete: await RunSurfaceRequestAsync(DeleteRequested, commandId); break;
            // Select All / Duplicate run on the live EJ2 diagram (selectAll, copy+paste).
            case AppConstants.CmdDuplicate:
            case AppConstants.CmdSelectAll:
                if (ArrangeRequested is null)
                {
                    Status = string.Format(AppStrings.StatusCommandExecuted, commandId);
                }
                else
                {
                    await ArrangeRequested(commandId);
                }

                break;

            // VIEW
            case AppConstants.CmdZoomIn: Zoom.ZoomInCommand.Execute(null); break;
            case AppConstants.CmdZoomOut: Zoom.ZoomOutCommand.Execute(null); break;
            case AppConstants.CmdFitToScreen: Zoom.FitToScreenCommand.Execute(null); break;
            case AppConstants.CmdShowGrid: Zoom.ToggleShowGridCommand.Execute(null); break;
            case AppConstants.CmdSnapToGrid: Zoom.ToggleSnapToGridCommand.Execute(null); break;

            // ARRANGE
            case AppConstants.CmdSendToBack: await RunSurfaceRequestAsync(SendToBackRequested, commandId); break;
            case AppConstants.CmdBringToFront: await RunSurfaceRequestAsync(BringToFrontRequested, commandId); break;
            case AppConstants.CmdSendBackward: await RunSurfaceRequestAsync(SendBackwardRequested, commandId); break;
            case AppConstants.CmdBringForward: await RunSurfaceRequestAsync(BringForwardRequested, commandId); break;
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
                if (ArrangeRequested is null)
                {
                    Status = string.Format(AppStrings.StatusCommandExecuted, commandId);
                }
                else
                {
                    await ArrangeRequested(commandId);
                }

                break;

            // WINDOW
            case AppConstants.CmdShowToolbar: ToggleToolbarCommand.Execute(null); break;
            case AppConstants.CmdShowStencil: ToggleStencilCommand.Execute(null); break;
            case AppConstants.CmdShowProperties: TogglePropertiesCommand.Execute(null); break;

            default:
                throw new NotSupportedException($"Unknown command id: {commandId}");
        }
    }

    /// <summary>Executes a toolbar tool id against the live diagram.</summary>
    public Task ExecuteToolAsync(string toolId) => RunToolRequestAsync(toolId);

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

    /// <summary>Used by the host page after DocumentLoaded to sync the title bar.</summary>
    public void SetDocumentTitle(string title) => DocumentTitle = title;

    /// <summary>Used by the host page to push a one-off status message into the status bar.</summary>
    public void SetStatus(string message) => Status = message;
}
