using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Zoom/grid/snap state: preset levels 50/100/150/200 %, current zoom,
/// incremental in/out, fit-to-screen, grid and snap toggles.
/// </summary>
public partial class ZoomViewModel : ObservableObject
{
    [ObservableProperty]
    private double currentZoom = 1.0;

    [ObservableProperty]
    private bool showGrid = true;

    [ObservableProperty]
    private bool snapToGrid = true;

    public IReadOnlyList<ZoomLevel> Levels { get; } =
        AppConstants.ZoomLevels.Select(z => new ZoomLevel { Value = z, LabelKey = $"{z * 100:0}%" }).ToList();

    /// <summary>Display string for the zoom indicator.</summary>
    public string CurrentZoomText => $"{CurrentZoom * 100:0}%";

    partial void OnCurrentZoomChanged(double value) => OnPropertyChanged(nameof(CurrentZoomText));

    [RelayCommand]
    private void ZoomIn() => SetZoom(CurrentZoom + AppConstants.ZoomStep);

    [RelayCommand]
    private void ZoomOut() => SetZoom(CurrentZoom - AppConstants.ZoomStep);

    [RelayCommand]
    private void SetZoomLevel(ZoomLevel level) => SetZoom(level.Value);

    private void SetZoom(double value)
    {
        if (value < AppConstants.ZoomMinimum)
        {
            CurrentZoom = AppConstants.ZoomMinimum;
        }
        else if (value > AppConstants.ZoomMaximum)
        {
            CurrentZoom = AppConstants.ZoomMaximum;
        }
        else
        {
            CurrentZoom = Math.Round(value, 2);
        }
    }

    // Fit is a control operation (SfDiagram.FitToPage) routed through the behavior:
    // the VM raises the request; the host page binds a relay that executes it.

    /// <summary>
    /// Set by the host page to a relay that executes fit-to-page on the live
    /// SfDiagram (control operation — the VM never references the control).
    /// </summary>
    public System.Windows.Input.ICommand? FitToPageRequested { get; set; }

    [RelayCommand]
    private void FitToScreen() => FitToPageRequested?.Execute(null);

    [RelayCommand]
    private void ToggleShowGrid() => ShowGrid = !ShowGrid;

    [RelayCommand]
    private void ToggleSnapToGrid() => SnapToGrid = !SnapToGrid;
}
