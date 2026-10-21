using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Launch-page view model: title, status line and the start-of-session entry command.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = AppStrings.AppTitle;

    [ObservableProperty]
    private string status = AppStrings.StatusReady;

    [RelayCommand]
    private void Start()
    {
    }
}
