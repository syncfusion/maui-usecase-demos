namespace DiagramBuilder.Controls;

/// <summary>
/// Property inspector panel (ui-spec.md §6). BindingContext must be
/// <see cref="DiagramBuilder.ViewModels.PropertyPanelViewModel"/>; sections are
/// accessed via the VM's <c>Sections</c> dictionary. The empty-state view shows
/// when <see cref="DiagramBuilder.ViewModels.PropertyPanelViewModel.IsEmptyState"/>
/// is true. Section editors are loaded from <c>Templates/PropertyEditorTemplates.xaml</c>
/// via a <see cref="DataTemplateSelector"/>.
/// </summary>
public partial class PropertyPanelControl : ContentView
{
    public PropertyPanelControl()
    {
        InitializeComponent();
    }
}