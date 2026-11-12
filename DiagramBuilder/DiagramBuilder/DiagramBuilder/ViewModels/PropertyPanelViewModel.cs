using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Property inspector state. The panel has two modes:
/// <list type="bullet">
///   <item><see cref="PropertyTarget.Page"/> — nothing is selected, so the
///   drawing-area properties (format, orientation, background) are shown.</item>
///   <item><see cref="PropertyTarget.Shape"/> — a shape or connector is
///   selected, so its own properties are shown.</item>
/// </list>
/// Rows come from <see cref="IPropertyService.Read"/> (shape) or
/// <see cref="IPropertyService.ReadPage"/> (page); edits are applied via
/// <see cref="IPropertyService.Apply"/>.
/// </summary>
public partial class PropertyPanelViewModel : ObservableObject
{
    private readonly IPropertyService _propertyService;
    private string? _selectedElementId;

    public PropertyPanelViewModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>True when nothing is selected — the panel shows the page settings.</summary>
    [ObservableProperty]
    private bool isPageMode = true;

    /// <summary>What the panel is currently describing.</summary>
    public PropertyTarget Target => IsPageMode ? PropertyTarget.Page : PropertyTarget.Shape;

    /// <summary>Title of the panel (from constants).</summary>
    public string Title => IsPageMode ? AppStrings.PropertyPageSettingsTitle : AppStrings.PropertyPanelTitle;

    /// <summary>
    /// Sections shown in the panel, in display order. The INSERT group is
    /// intentionally omitted — its Insert Link / Insert Image actions are not
    /// implemented yet.
    ///
    /// Page mode exposes only the BACKGROUND section: the MAUI Diagram control
    /// has no page format/orientation API (page settings exist solely on
    /// <c>DiagramExportOptions</c> for export/print), so those unsupported
    /// options were removed from the UI.
    /// </summary>
    public IReadOnlyList<PropertyGroup> SectionGroups =>
        IsPageMode ? PageSectionGroups : ShapeSectionGroups;

    /// <summary>Section headers (from constants), keyed by <see cref="SectionGroups"/> order.</summary>
    public IReadOnlyList<string> SectionTitles =>
        IsPageMode ? PageSectionTitles : ShapeSectionTitles;

    private static readonly IReadOnlyList<PropertyGroup> PageSectionGroups =
    [
        PropertyGroup.Background,
    ];

    private static readonly IReadOnlyList<string> PageSectionTitles =
    [
        string.Empty,
    ];

    private static readonly IReadOnlyList<PropertyGroup> ShapeSectionGroups =
    [
        PropertyGroup.Dimensions,
        PropertyGroup.Fill,
        PropertyGroup.Border,
        PropertyGroup.Text,
    ];

    private static readonly IReadOnlyList<string> ShapeSectionTitles =
    [
        AppStrings.PropertySectionDimensions,
        AppStrings.PropertySectionFill,
        AppStrings.PropertySectionBorder,
        AppStrings.PropertySectionText,
    ];

    /// <summary>Property rows per group, keyed by section index.</summary>
    public IReadOnlyDictionary<PropertyGroup, IReadOnlyList<PropertyModel>> Sections =>
        _sections;

    private Dictionary<PropertyGroup, IReadOnlyList<PropertyModel>> _sections = [];

    /// <summary>
    /// Rebuilds the panel. With a selected element the shape's properties are
    /// shown; without one the drawing-area (page) properties are shown.
    /// </summary>
    public void Refresh(string? elementId)
    {
        _selectedElementId = elementId;
        IsPageMode = elementId is null;

        var rows = elementId is null
            ? _propertyService.ReadPage()
            : _propertyService.Read(elementId);

        _sections = Enum.GetValues<PropertyGroup>()
            .ToDictionary(g => g, g => (IReadOnlyList<PropertyModel>)rows.Where(p => p.Group == g).ToList());

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
        else
        {
            // Page properties apply to the drawing area.
            _propertyService.Apply(string.Empty, property);
        }
    }

    partial void OnIsPageModeChanged(bool value)
    {
        OnPropertyChanged(nameof(Target));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(SectionGroups));
        OnPropertyChanged(nameof(SectionTitles));
    }
}
