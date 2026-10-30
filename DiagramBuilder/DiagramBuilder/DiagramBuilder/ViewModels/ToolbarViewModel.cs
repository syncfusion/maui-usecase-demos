using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Toolbar state: 8 groups from <see cref="ToolbarCatalog"/>, active tool tracking
/// (pan/selection/connector/shape/text), and selection-only group-6 visibility.
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

    /// <summary>True when exactly one node is selected (drives group-6 visibility).</summary>
    [ObservableProperty]
    private bool hasSingleSelection;

    internal void RaiseSelectionChanged()
    {
        HasSingleSelection = _owner.SelectedNodeId is not null;
        OnPropertyChanged(nameof(HasSingleSelection));
    }

    /// <summary>Executed by ToolbarControl with the item's id (tool or command).</summary>
    [RelayCommand]
    private Task Tool(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return Task.CompletedTask;
        }

        // Tool ids toggle the active tool; command ids dispatch.
        if (id.StartsWith("tool.", StringComparison.Ordinal))
        {
            ActiveToolId = id;
            return Task.CompletedTask;
        }

        return _owner.ExecuteCommand(id);
    }
}
