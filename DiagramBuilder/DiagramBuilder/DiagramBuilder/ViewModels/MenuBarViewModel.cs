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

    /// <summary>
    /// Whether a menu command is applicable to the current selection. Z-order needs a
    /// selection; align / match size need two or more shapes; distribute needs three
    /// (the diagram spaces the shapes between the outer two); group needs two or more
    /// elements and ungroup at least one shape.
    /// </summary>
    public bool IsEnabled(string id)
    {
        var nodes = _owner.Selection.SelectedNodeIds.Count;
        var total = nodes + _owner.Selection.SelectedConnectorIds.Count;
        return id switch
        {
            Constants.AppConstants.CmdSendToBack
                or Constants.AppConstants.CmdBringToFront
                or Constants.AppConstants.CmdSendBackward
                or Constants.AppConstants.CmdBringForward => total >= 1,
            Constants.AppConstants.CmdAlignLeft
                or Constants.AppConstants.CmdAlignRight
                or Constants.AppConstants.CmdAlignCenter
                or Constants.AppConstants.CmdAlignTop
                or Constants.AppConstants.CmdAlignBottom
                or Constants.AppConstants.CmdAlignMiddle
                or Constants.AppConstants.CmdSameWidth
                or Constants.AppConstants.CmdSameHeight
                or Constants.AppConstants.CmdSameWidthAndHeight => nodes >= 2,
            Constants.AppConstants.CmdDistributeHorizontally
                or Constants.AppConstants.CmdDistributeVertically => nodes >= 3,
            Constants.AppConstants.CmdGroup => total >= 2,
            Constants.AppConstants.CmdUngroup => nodes >= 1,
            _ => true,
        };
    }
}
