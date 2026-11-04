using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Create-diagram dialog: three category cards, per-category template list
/// (from <see cref="ITemplateService"/>), Create command → document → designer.
/// </summary>
public partial class CreateDiagramViewModel : ObservableObject
{
    private readonly ITemplateService _templateService;
    private readonly IDiagramService _diagramService;

    public CreateDiagramViewModel(ITemplateService templateService, IDiagramService diagramService)
    {
        _templateService = templateService;
        _diagramService = diagramService;
    }

    public string Title => AppStrings.CreateDialogTitle;

    public string Subtitle => AppStrings.CreateDialogSubtitle;

    /// <summary>Category card labels in display order.</summary>
    public IReadOnlyList<string> Categories { get; } =
    [
        AppStrings.CategoryFlowChart,
        AppStrings.CategoryMindMap,
        AppStrings.CategoryOrganizationChart,
    ];

    public string DescriptionFor(string category) => category switch
    {
        var c when c == AppStrings.CategoryFlowChart => AppStrings.CategoryFlowChartDescription,
        var c when c == AppStrings.CategoryMindMap => AppStrings.CategoryMindMapDescription,
        var c when c == AppStrings.CategoryOrganizationChart => AppStrings.CategoryOrganizationChartDescription,
        _ => string.Empty,
    };

    [ObservableProperty]
    private DiagramCategory selectedCategory = DiagramCategory.FlowChart;

    [ObservableProperty]
    private IReadOnlyList<DiagramTemplate> templates = [];

    private IReadOnlyList<DiagramTemplate> _allTemplates = [];

    /// <summary>Set to true when the page should pop (raised after a successful create).</summary>
    [ObservableProperty]
    private bool closeRequested;

    public string SelectedCategoryName => SelectedCategory.ToString();

    /// <summary>
    /// Backing property for the SfListView SelectedItem two-way binding on the category rail.
    /// Kept in sync with <see cref="SelectedCategory"/> via <see cref="OnSelectedCategoryChanged"/>;
    /// a selection made in the UI flows back through <see cref="OnSelectedCategoryLabelChanged"/>.
    /// </summary>
    [ObservableProperty]
    private string selectedCategoryLabel = AppStrings.CategoryFlowChart;

    public DiagramTemplate? EmptyDiagramTemplate => Templates.FirstOrDefault(t => t.Category == SelectedCategory && t.IsBlank);

    public IReadOnlyList<DiagramTemplate> PredefinedTemplates => Templates
        .Where(t => t.Category == SelectedCategory && !t.IsBlank)
        .ToList();

    partial void OnSelectedCategoryChanged(DiagramCategory value)
    {
        var label = value switch
        {
            DiagramCategory.FlowChart => AppStrings.CategoryFlowChart,
            DiagramCategory.MindMap => AppStrings.CategoryMindMap,
            DiagramCategory.OrganizationChart => AppStrings.CategoryOrganizationChart,
            _ => AppStrings.CategoryFlowChart,
        };
        // Set the label without firing the OnSelectedCategoryLabelChanged partial so the
        // SfListView's two-way binding sees the value matching its SelectedItem and skips
        // re-selecting (avoids a selection-change loop).
        if (!string.Equals(SelectedCategoryLabel, label, StringComparison.Ordinal))
        {
            selectedCategoryLabel = label;
            OnPropertyChanged(nameof(SelectedCategoryLabel));
        }
        OnPropertyChanged(nameof(SelectedCategoryName));
        UpdateTemplatesForCategory(value);
    }

    /// <summary>Called when the SfListView two-way binding pushes a new label up.</summary>
    partial void OnSelectedCategoryLabelChanged(string value)
    {
        var mapped = value switch
        {
            AppStrings.CategoryFlowChart => DiagramCategory.FlowChart,
            AppStrings.CategoryMindMap => DiagramCategory.MindMap,
            AppStrings.CategoryOrganizationChart => DiagramCategory.OrganizationChart,
            _ => SelectedCategory,
        };
        if (mapped != SelectedCategory)
        {
            SelectedCategory = mapped;
        }
    }

    /// <summary>Preloads templates; called on page appearing.</summary>
    public async Task InitializeAsync()
    {
        await _templateService.PreloadAsync();
        _allTemplates = await _templateService.GetAllAsync();
        UpdateTemplatesForCategory(SelectedCategory);
    }

    private void UpdateTemplatesForCategory(DiagramCategory category)
    {
        Templates = _allTemplates.Where(t => t.Category == category).ToList();
        OnPropertyChanged(nameof(EmptyDiagramTemplate));
        OnPropertyChanged(nameof(PredefinedTemplates));
    }

    /// <summary>Creates the document from the template and navigates to the designer.</summary>
    [RelayCommand]
    private async Task CreateDiagram(DiagramTemplate? template)
    {
        if (template is null)
        {
            return;
        }

        await _diagramService.CreateDocumentAsync(template);
        // Reset-then-set so the change always notifies even if the flag is
        // already true from a previous create (singleton VM, reused dialog).
        CloseRequested = false;
        CloseRequested = true;
    }
}
