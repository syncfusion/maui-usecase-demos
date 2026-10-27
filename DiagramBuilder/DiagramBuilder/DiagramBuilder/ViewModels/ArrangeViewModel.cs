using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Arrange operations (z-order, align, distribute, match size, group/ungroup) routed
/// into the designer dispatch table; status feedback is mock where the control has
/// no native API.
/// </summary>
public partial class ArrangeViewModel : ObservableObject
{
    private readonly DiagramDesignerViewModel _owner;

    public ArrangeViewModel(DiagramDesignerViewModel owner)
    {
        _owner = owner;
    }

    [RelayCommand]
    private Task SendToBack() => _owner.ExecuteCommand(AppConstants.CmdSendToBack);

    [RelayCommand]
    private Task BringToFront() => _owner.ExecuteCommand(AppConstants.CmdBringToFront);

    [RelayCommand]
    private Task SendBackward() => _owner.ExecuteCommand(AppConstants.CmdSendBackward);

    [RelayCommand]
    private Task BringForward() => _owner.ExecuteCommand(AppConstants.CmdBringForward);

    [RelayCommand]
    private Task AlignLeft() => _owner.ExecuteCommand(AppConstants.CmdAlignLeft);

    [RelayCommand]
    private Task AlignRight() => _owner.ExecuteCommand(AppConstants.CmdAlignRight);

    [RelayCommand]
    private Task AlignCenter() => _owner.ExecuteCommand(AppConstants.CmdAlignCenter);

    [RelayCommand]
    private Task AlignTop() => _owner.ExecuteCommand(AppConstants.CmdAlignTop);

    [RelayCommand]
    private Task AlignBottom() => _owner.ExecuteCommand(AppConstants.CmdAlignBottom);

    [RelayCommand]
    private Task AlignMiddle() => _owner.ExecuteCommand(AppConstants.CmdAlignMiddle);

    [RelayCommand]
    private Task DistributeHorizontally() => _owner.ExecuteCommand(AppConstants.CmdDistributeHorizontally);

    [RelayCommand]
    private Task DistributeVertically() => _owner.ExecuteCommand(AppConstants.CmdDistributeVertically);

    [RelayCommand]
    private Task MatchWidth() => _owner.ExecuteCommand(AppConstants.CmdSameWidth);

    [RelayCommand]
    private Task MatchHeight() => _owner.ExecuteCommand(AppConstants.CmdSameHeight);

    [RelayCommand]
    private Task MatchBoth() => _owner.ExecuteCommand(AppConstants.CmdSameWidthAndHeight);

    [RelayCommand]
    private Task Group() => _owner.ExecuteCommand(AppConstants.CmdGroup);

    [RelayCommand]
    private Task Ungroup() => _owner.ExecuteCommand(AppConstants.CmdUngroup);
}
