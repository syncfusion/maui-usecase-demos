using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Property inspector state: six <see cref="PropertyGroup"/> sections built via
/// <see cref="IPropertyService.Read"/>, edits applied via <see cref="IPropertyService.Apply"/>,
/// empty-state flag, auto-show on selection.
/// </summary>
public partial class PropertyPanelViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;
    private string? _selectedElementId;

    public PropertyPanelViewModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>True when nothing is selected — drives the empty-state view.</summary>
    [ObservableProperty]
    private bool isEmptyState = true;

    /// <summary>Title of the panel (from constants).</summary>
    public string Title => AppStrings.PropertyPanelTitle;

    /// <summary>Empty-state message (from constants).</summary>
    public string EmptyStateText => AppStrings.PropertyEmptyState;

    /// <summary>Section headers (from constants).</summary>
    public IReadOnlyList<string> SectionTitles { get; } =
    [
        AppStrings.PropertySectionDimensions,
        AppStrings.PropertySectionInsert,
        AppStrings.PropertySectionBackground,
        AppStrings.PropertySectionFill,
        AppStrings.PropertySectionBorder,
        AppStrings.PropertySectionText,
    ];

    /// <summary>Property rows per group, keyed by section index.</summary>
    public IReadOnlyDictionary<PropertyGroup, IReadOnlyList<PropertyModel>> Sections =>
        _sections;

    private Dictionary<PropertyGroup, IReadOnlyList<PropertyModel>> _sections = [];

    /// <summary>
    /// Rebuilds the panel for the selected element (or clears to empty state).
    /// </summary>
    public void Refresh(string? elementId)
    {
        _selectedElementId = elementId;
        IsEmptyState = elementId is null;
        _sections = elementId is null
            ? []
            : Enum.GetValues<PropertyGroup>()
                .ToDictionary(g => g, g => (IReadOnlyList<PropertyModel>)_propertyService.Read(elementId).Where(p => p.Group == g).ToList());
        OnPropertyChanged(nameof(Sections));
    }

    /// <summary>Applies a single edited property to the live diagram element.</summary>
    [RelayCommand]
    private void Apply(PropertyModel property)
    {
        if (_selectedElementId is not null)
        {
            _propertyService.Apply(_selectedElementId, property);
        }
    }

    partial void OnIsEmptyStateChanged(bool value) => OnPropertyChanged(nameof(Sections));
}
