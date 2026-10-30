using CommunityToolkit.Mvvm.ComponentModel;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Stencil panel state: groups from <see cref="StencilCatalog"/>, search filtering
/// (by DisplayName/SearchTags), collapse state, and active connector tool selection.
/// </summary>
public partial class StencilViewModel : ObservableObject
{
    public StencilViewModel()
    {
        // Eagerly materialize the symbol-palette groups once so the diagram's
        // SymbolPalette has data to host (architecture.md §3.3 requires SfDiagram
        // and SymbolPalette as direct Grid siblings in DiagramWorkspaceControl).
        foreach (var palette in AllGroups.Select(Services.DiagramModelAdapter.ToPalette))
        {
            PaletteGroups.Add(palette);
        }
    }
    [ObservableProperty]
    private string filterText = string.Empty;

    /// <summary>Active connector tool id set by clicking a CONNECTORS entry.</summary>
    [ObservableProperty]
    private string activeConnectorToolId = string.Empty;

    /// <summary>Group ids currently expanded (accordion multiple mode).</summary>
    public HashSet<string> ExpandedGroupIds { get; } = ["stencil.flow"];

    public IReadOnlyList<StencilGroup> AllGroups { get; } = StencilCatalog.All.ToList();

    /// <summary>
    /// Backing collection for the diagram SymbolPalette. Updated only when the
    /// host page first initializes; changes are pushed through the bindable
    /// property on the workspace control.
    /// </summary>
    public System.Collections.ObjectModel.ObservableCollection<object> PaletteGroups { get; } = [];

    /// <summary>Groups filtered by the current search text.</summary>
    public IEnumerable<StencilGroup> VisibleGroups
    {
        get
        {
            var query = FilterText?.Trim() ?? string.Empty;
            if (query.Length == 0)
            {
                return AllGroups;
            }

            var filtered = new List<StencilGroup>();
            foreach (var g in AllGroups.Where(g => g.Items.Any(Matches)))
            {
                var copy = new StencilGroup
                {
                    Id = g.Id,
                    Title = g.Title,
                    IconKey = g.IconKey,
                    IsConnectorGroup = g.IsConnectorGroup,
                    IsExpanded = g.IsExpanded,
                };
                foreach (var item in g.Items.Where(Matches))
                {
                    copy.Items.Add(item);
                }
                filtered.Add(copy);
            }

            return filtered;

            bool Matches(StencilItem item) =>
                item.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || item.SearchTags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase));
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        OnPropertyChanged(nameof(VisibleGroups));
        OnPropertyChanged(nameof(HasResults));
    }

    partial void OnActiveConnectorToolIdChanged(string value) =>
        OnPropertyChanged(nameof(ActiveConnectorToolId));

    /// <summary>Search placeholder from constants (no hardcoded strings in XAML).</summary>
    public string SearchPlaceholder => AppStrings.StencilSearchPlaceholder;

    public bool HasResults => VisibleGroups.Any();
}
