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

    partial void OnSelectedCategoryChanged(DiagramCategory value) => _ = LoadTemplatesAsync(value);

    /// <summary>Selects a category card (parameter is the enum name, from a bound CommandParameter).</summary>
    [RelayCommand]
    private void SelectCategory(string? categoryName)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            return;
        }

        SelectedCategory = Enum.TryParse<DiagramCategory>(categoryName, ignoreCase: true, out var parsed)
            ? parsed
            : SelectedCategory;
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
        await Shell.Current.GoToAsync($"//{AppConstants.RouteDesigner}");
    }
}
