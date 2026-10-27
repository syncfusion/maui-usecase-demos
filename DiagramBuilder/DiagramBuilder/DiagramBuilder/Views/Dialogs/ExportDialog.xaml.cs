namespace DiagramBuilder.Views.Dialogs;

/// <summary>
/// Export options popup content (ui-spec.md §7), bound to
/// <see cref="DiagramBuilder.ViewModels.ExportViewModel"/> via the host page.
/// Code-behind is empty by design — everything is binding-driven.
/// </summary>
public partial class ExportDialog : ContentView
{
    public ExportDialog()
    {
        InitializeComponent();
    }
}