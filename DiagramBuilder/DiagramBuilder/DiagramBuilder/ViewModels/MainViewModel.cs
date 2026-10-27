using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Launch-page view model: app title, start-command (opens create-diagram),
/// status relay from <see cref="IDiagramService.StatusChanged"/>.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IDiagramService _diagramService;

    public MainViewModel(IDiagramService diagramService)
    {
        _diagramService = diagramService;
        _diagramService.StatusChanged += (_, message) => Status = message;
    }

    [ObservableProperty]
    private string title = AppStrings.AppTitle;

    [ObservableProperty]
    private string status = AppStrings.StatusReady;

    /// <summary>Navigates to the create-diagram dialog.</summary>
    [RelayCommand]
    private async Task Start() => await Shell.Current.GoToAsync(AppConstants.RouteCreateDiagram);
}
