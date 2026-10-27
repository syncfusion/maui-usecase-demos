using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    private readonly CreateDiagramPage _createDiagramPage;
    private readonly Views.Dialogs.ExportDialog _exportDialog;
    private readonly ContentPage _exportDialogHost;
    private bool _dialogShown;

    public DiagramDesignerPage(DiagramDesignerViewModel viewModel, CreateDiagramPage createDiagramPage, Views.Dialogs.ExportDialog exportDialog)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _createDiagramPage = createDiagramPage;
        _exportDialog = exportDialog;
        _exportDialog.BindingContext = viewModel.Export;
        _exportDialogHost = new ContentPage
        {
            Title = Constants.AppStrings.ExportDialogTitle,
            Content = _exportDialog
        };

        // Binding-context resolution + data mapping only (no business logic):
        // the palette needs Syncfusion Palette objects, the VM exposes domain groups.
        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;
        Workspace.PaletteGroups = viewModel.Stencil.AllGroups
            .Select(DiagramModelAdapter.ToPalette)
            .ToList();

        viewModel.ExportDialogRequested += OnExportDialogRequested;
        Appearing += OnAppearing;
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
}
