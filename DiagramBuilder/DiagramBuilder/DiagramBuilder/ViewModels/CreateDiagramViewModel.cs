using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Create-diagram dialog view model.
/// </summary>
public partial class CreateDiagramViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = AppStrings.CreateDialogTitle;

    [ObservableProperty]
    private string subtitle = AppStrings.CreateDialogSubtitle;
}
