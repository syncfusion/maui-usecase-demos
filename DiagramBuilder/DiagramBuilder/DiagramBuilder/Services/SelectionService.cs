using DiagramBuilder.Constants;

namespace DiagramBuilder.Services;

/// <summary>
/// <see cref="ISelectionService"/> implementation: current selection sets,
/// summary text and change event.
/// </summary>
public class SelectionService : ISelectionService
{
    private List<string> _nodeIds = [];
    private List<string> _connectorIds = [];

    public IReadOnlyList<string> SelectedNodeIds => _nodeIds;

    public IReadOnlyList<string> SelectedConnectorIds => _connectorIds;

    public string SelectionSummary
    {
        get
        {
            var count = _nodeIds.Count + _connectorIds.Count;
            return count == 0
                ? AppStrings.StatusNothingSelected
                : string.Format(AppStrings.StatusSelectionFormat, count);
        }
    }

    public event EventHandler? SelectionChanged;

    public void ApplySelection(IEnumerable<string> nodeIds, IEnumerable<string> connectorIds)
    {
        _nodeIds = [.. nodeIds];
        _connectorIds = [.. connectorIds];
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _nodeIds = [];
        _connectorIds = [];
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
