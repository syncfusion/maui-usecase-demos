using DiagramBuilder.Services;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Views;

public partial class DiagramDesignerPage : ContentPage
{
    public DiagramDesignerPage(DiagramDesignerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Binding-context resolution + data mapping only (no business logic):
        // the palette needs Syncfusion Palette objects, the VM exposes domain groups.
        MenuBar.IsChecked = viewModel.MenuBar.IsChecked;
        Workspace.PaletteGroups = viewModel.Stencil.AllGroups
            .Select(DiagramModelAdapter.ToPalette)
            .ToList();
    }
}
