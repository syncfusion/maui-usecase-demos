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

    /// <summary>Set to true when the page should pop (raised after a successful create).</summary>
    [ObservableProperty]
    private bool closeRequested;

    public string SelectedCategoryName => SelectedCategory.ToString();

    public DiagramTemplate? EmptyDiagramTemplate => Templates.FirstOrDefault(t => t.Category == SelectedCategory && t.IsBlank);

    public IReadOnlyList<DiagramTemplate> PredefinedTemplates => Templates
        .Where(t => t.Category == SelectedCategory && !t.IsBlank)
        .ToList();

    partial void OnSelectedCategoryChanged(DiagramCategory value)
    {
        OnPropertyChanged(nameof(SelectedCategoryName));
        OnPropertyChanged(nameof(EmptyDiagramTemplate));
        OnPropertyChanged(nameof(PredefinedTemplates));
        _ = LoadTemplatesAsync(value);
    }

    /// <summary>Selects a category card. Parameter is the display label (e.g. "Flow Chart"),
    /// which doesn't match the <see cref="DiagramCategory"/> enum member names ("FlowChart").
    /// Map the label back to the enum explicitly so the click always changes the selection.</summary>
    [RelayCommand]
    private void SelectCategory(string? categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            return;
        }

        SelectedCategory = categoryName switch
        {
            AppStrings.CategoryFlowChart => DiagramCategory.FlowChart,
            AppStrings.CategoryMindMap => DiagramCategory.MindMap,
            AppStrings.CategoryOrganizationChart => DiagramCategory.OrganizationChart,
            _ => SelectedCategory,
        };
    }

    /// <summary>Preloads templates; called on page appearing.</summary>
    public async Task InitializeAsync()
    {
        await _templateService.PreloadAsync();
        await LoadTemplatesAsync(SelectedCategory);
    }

    private async Task LoadTemplatesAsync(DiagramCategory category)
    {
        var all = await _templateService.GetAllAsync();
        var forCategory = all.Where(t => t.Category == category).ToList();
        Templates = forCategory;
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
