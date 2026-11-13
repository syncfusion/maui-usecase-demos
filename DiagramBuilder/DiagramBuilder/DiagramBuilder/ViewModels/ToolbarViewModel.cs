using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Toolbar state: 8 groups from <see cref="ToolbarCatalog"/>, active tool tracking
/// (pan/selection/connector/shape/lock), and selection-gated group-6 visibility.
/// </summary>
public partial class ToolbarViewModel : ObservableObject
{
    private readonly DiagramDesignerViewModel _owner;

    public ToolbarViewModel(DiagramDesignerViewModel owner)
    {
        _owner = owner;
        Items = ToolbarCatalog.All.ToList();
    }

    public IReadOnlyList<Models.ToolbarItem> Items { get; }

    /// <summary>Active tool id (pan/select/connector/shape/text) for highlight state.</summary>
    [ObservableProperty]
    private string activeToolId = Constants.AppConstants.ToolSelect;

    /// <summary>True when exactly one node is selected (drives color tools + arrange).</summary>
    [ObservableProperty]
    private bool hasSingleSelection;

    /// <summary>True when any element is selected (drives Delete and arrange/lock actions).</summary>
    [ObservableProperty]
    private bool hasAnySelection;

    internal void RaiseSelectionChanged()
    {
        HasSingleSelection = _owner.SelectedNodeId is not null;
        HasAnySelection = _owner.SelectedElementId is not null
            || _owner.Selection.SelectedNodeIds.Count > 0
            || _owner.Selection.SelectedConnectorIds.Count > 0;
    }

    /// <summary>Executed by ToolbarControl with the item's id (tool or command).</summary>
    [RelayCommand]
    private Task Tool(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Task.CompletedTask;
        }

        if (id == Constants.AppConstants.ToolLock)
        {
            return _owner.ExecuteToolAsync(id);
        }

        if (id.StartsWith("tool.", StringComparison.Ordinal))
        {
            ActiveToolId = id;
            return _owner.ExecuteToolAsync(id);
        }

        return _owner.ExecuteCommand(id);
    }
}
