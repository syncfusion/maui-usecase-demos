using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    private readonly CreateDiagramPage _createDiagramPage;
    private bool _dialogShown;

    public DiagramDesignerPage(DiagramDesignerViewModel viewModel, CreateDiagramPage createDiagramPage)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _createDiagramPage = createDiagramPage;

        // Binding-context resolution + data mapping only (no business logic):
        // the palette needs Syncfusion Palette objects, the VM exposes domain groups.
        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;
        Workspace.PaletteGroups = viewModel.Stencil.AllGroups
            .Select(DiagramModelAdapter.ToPalette)
            .ToList();

        Appearing += OnAppearing;
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
