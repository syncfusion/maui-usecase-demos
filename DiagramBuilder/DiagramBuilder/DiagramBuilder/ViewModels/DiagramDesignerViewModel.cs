using CommunityToolkit.Mvvm.ComponentModel;
using DiagramBuilder.Constants;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Designer workspace orchestrator.
/// </summary>
public partial class DiagramDesignerViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = AppStrings.UntitledDocument;

    [ObservableProperty]
    private string status = AppStrings.StatusReady;
}
