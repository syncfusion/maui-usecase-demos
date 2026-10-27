using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Builds the five menu roots from <see cref="MenuCatalog"/> and routes execution
/// into the designer dispatch table. Checkable items reflect panel/zoom state.
/// </summary>
public partial class MenuBarViewModel : ObservableObject
{
    private readonly DiagramDesignerViewModel _owner;

    public MenuBarViewModel(DiagramDesignerViewModel owner)
    {
        _owner = owner;
        Roots = MenuCatalog.All.ToList();
    }

    public IReadOnlyList<MenuCommandItem> Roots { get; }

    /// <summary>Executed by MenuBarControl with the item's command id.</summary>
    [RelayCommand]
    private Task Menu(string? commandId) =>
        string.IsNullOrEmpty(commandId)
            ? Task.CompletedTask
            : _owner.ExecuteCommand(commandId);

    /// <summary>True when a checkable item's state is on (Show Grid, panels, snap).</summary>
    public bool IsChecked(string id) => id switch
    {
        Constants.AppConstants.CmdShowGrid => _owner.Zoom.ShowGrid,
        Constants.AppConstants.CmdSnapToGrid => _owner.Zoom.SnapToGrid,
        Constants.AppConstants.CmdShowToolbar => _owner.ShowToolbar,
        Constants.AppConstants.CmdShowStencil => _owner.ShowStencil,
        Constants.AppConstants.CmdShowProperties => _owner.ShowProperties,
        _ => false,
    };
}
