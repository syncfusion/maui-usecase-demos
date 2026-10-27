namespace DiagramBuilder.Controls;

/// <summary>
/// Stencil panel: panel header + search entry + accordion of categories
/// (ui-spec.md §4). The BindingContext must be a <see cref="DiagramBuilder.ViewModels.StencilViewModel"/>.
/// Search text is two-way bound to <see cref="DiagramBuilder.ViewModels.StencilViewModel.FilterText"/>
/// via the Entry's <c>Text</c> binding the host page supplies.
/// The shared <see cref="Syncfusion.Maui.Diagram.SymbolPalette"/> lives as a Grid
/// SIBLING of the diagram in <see cref="DiagramWorkspaceControl"/> per architecture.md §3.3.
/// </summary>
public partial class StencilPanelControl : ContentView
{
    public StencilPanelControl()
    {
        InitializeComponent();
    }
}