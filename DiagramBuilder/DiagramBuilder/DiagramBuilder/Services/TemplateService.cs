using System.Text.Json;
using System.Text.Json.Serialization;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// <see cref="ITemplateService"/> implementation: loads the three JSON template
/// payloads from Resources/Raw/Templates via FileSystem.OpenAppPackageFileAsync;
/// falls back to C# seed templates when JSON is missing or fails to parse so the
/// UI is never empty.
/// </summary>
public class TemplateService : ITemplateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private List<DiagramTemplate> _templates = [];

    public async Task PreloadAsync()
    {
        var loaded = new List<DiagramTemplate>();

        // Predefined templates from embedded JSON.
        loaded.Add(await TryLoadJsonAsync(AppConstants.TemplatePathFlowCreditCard, BuildCreditCardSeed())
            ?? BuildCreditCardSeed());
        loaded.Add(await TryLoadJsonAsync(AppConstants.TemplatePathMindMapProductPlanning, BuildProductPlanningSeed())
            ?? BuildProductPlanningSeed());
        loaded.Add(await TryLoadJsonAsync(AppConstants.TemplatePathOrgChartCompanyHierarchy, BuildCompanyHierarchySeed())
            ?? BuildCompanyHierarchySeed());

        // Blank templates (no JSON payload).
        loaded.Add(BuildBlank(AppConstants.TemplateIdBlankFlowChart, DiagramCategory.FlowChart, AppStrings.TemplateBlankFlowChart, AppStrings.TemplateBlankFlowChartDescription));
        loaded.Add(BuildBlank(AppConstants.TemplateIdBlankMindMap, DiagramCategory.MindMap, AppStrings.TemplateBlankMindMap, AppStrings.TemplateBlankMindMapDescription));
        loaded.Add(BuildBlank(AppConstants.TemplateIdBlankOrgChart, DiagramCategory.OrganizationChart, AppStrings.TemplateBlankOrgChart, AppStrings.TemplateBlankOrgChartDescription));

        _templates = loaded;
    }

    public Task<IReadOnlyList<DiagramTemplate>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<DiagramTemplate>>(_templates);

    public Task<DiagramTemplate?> GetByIdAsync(string id) =>
        Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));

    private static async Task<DiagramTemplate?> TryLoadJsonAsync(string path, DiagramTemplate fallback)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var reader = new StreamReader(stream);
            var payload = await reader.ReadToEndAsync();

            var dto = JsonSerializer.Deserialize<TemplatePayloadDto>(payload, JsonOptions);
            if (dto is null)
            {
                return fallback;
            }

            var template = new DiagramTemplate
            {
                Id = dto.Id,
                Category = dto.Category,
                Title = dto.Title,
                Description = dto.Description,
                IsBlank = false,
                AccentColorKey = AccentFor(dto.Category),
                ThumbnailKey = "thumb." + dto.Id,
            };

            foreach (var node in dto.Nodes)
            {
                template.Nodes.Add(node);
            }

            foreach (var connector in dto.Connectors)
            {
                template.Connectors.Add(connector);
            }

            return template;
        }
        catch (Exception)
        {
            // Fallback seed guarantees the UI is never empty.
            return fallback;
        }
    }

    private static string AccentFor(DiagramCategory category) => category switch
    {
        DiagramCategory.FlowChart => ResourceKeys.AccentFlow,
        DiagramCategory.MindMap => ResourceKeys.AccentMindMap,
        DiagramCategory.OrganizationChart => ResourceKeys.AccentOrgChart,
        _ => ResourceKeys.PrimaryColor,
    };

    private static DiagramTemplate BuildBlank(string id, DiagramCategory category, string title, string description) => new()
    {
        Id = id,
        Category = category,
        Title = title,
        Description = description,
        IsBlank = true,
        AccentColorKey = AccentFor(category),
        ThumbnailKey = "thumb." + id,
        Nodes =
        {
            new TemplateNode
            {
                Id = "starter",
                Text = AppStrings.BlankStarterText,
                ShapeType = ShapeType.Process,
                X = 360,
                Y = 200,
                Width = AppConstants.DefaultNodeWidth,
                Height = AppConstants.DefaultNodeHeight,
                FillColorKey = ResourceKeys.AccentFlow,
            },
        },
    };

    /// <summary>
    /// C# fallback seed: 11-node payment flow (mirrors the control's GettingStarted sample).
    /// </summary>
    private static DiagramTemplate BuildCreditCardSeed() => new()
    {
        Id = AppConstants.TemplateIdCreditCardProcessing,
        Category = DiagramCategory.FlowChart,
        Title = AppStrings.TemplateCreditCardProcessing,
        Description = AppStrings.TemplateCreditCardProcessingDescription,
        AccentColorKey = ResourceKeys.AccentFlow,
        ThumbnailKey = "thumb.flow-credit-card-processing",
        Nodes =
        {
            Node("start", ShapeType.Terminator, "Start", 100, 40, 140, 50),
            Node("enter", ShapeType.Process, "Enter Card Details", 90, 130, 160, 50),
            Node("validate", ShapeType.Decision, "Valid?", 100, 220, 140, 70),
            Node("declined", ShapeType.Process, "Card Declined", 330, 220, 150, 50),
            Node("process", ShapeType.Process, "Process Payment", 100, 330, 160, 50),
            Node("store", ShapeType.Document, "Store Transaction", 90, 420, 170, 50),
            Node("receipt", ShapeType.Process, "Send Receipt", 100, 510, 160, 50),
            Node("end", ShapeType.Terminator, "End", 110, 600, 140, 50),
        },
        Connectors =
        {
            Link("c1", "start", "enter", "Yes"),
            Link("c2", "enter", "validate"),
            Link("c3", "validate", "declined", "No"),
            Link("c4", "validate", "process", "Yes"),
            Link("c5", "process", "store"),
            Link("c6", "store", "receipt"),
            Link("c7", "receipt", "end"),
        },
    };

    /// <summary>
    /// C# fallback seed: central "Product Planning" ellipse with five branches.
    /// </summary>
    private static DiagramTemplate BuildProductPlanningSeed() => new()
    {
        Id = AppConstants.TemplateIdProductPlanning,
        Category = DiagramCategory.MindMap,
        Title = AppStrings.TemplateProductPlanning,
        Description = AppStrings.TemplateProductPlanningDescription,
        AccentColorKey = ResourceKeys.AccentMindMap,
        ThumbnailKey = "thumb.mindmap-product-planning",
        Nodes =
        {
            Node("center", ShapeType.Circle, "Product Planning", 360, 300, 160, 90),
            Node("research", ShapeType.Circle, "Research", 120, 120, 130, 60),
            Node("design", ShapeType.Circle, "Design", 600, 120, 130, 60),
            Node("development", ShapeType.Circle, "Development", 120, 300, 130, 60),
            Node("marketing", ShapeType.Circle, "Marketing", 600, 300, 130, 60),
            Node("launch", ShapeType.Circle, "Launch", 360, 500, 130, 60),
        },
        Connectors =
        {
            Link("c1", "center", "research", kind: ConnectorKind.Bezier),
            Link("c2", "center", "design", kind: ConnectorKind.Bezier),
            Link("c3", "center", "development", kind: ConnectorKind.Bezier),
            Link("c4", "center", "marketing", kind: ConnectorKind.Bezier),
            Link("c5", "center", "launch", kind: ConnectorKind.Bezier),
        },
    };

    /// <summary>
    /// C# fallback seed: CEO → directors → teams tree.
    /// </summary>
    private static DiagramTemplate BuildCompanyHierarchySeed() => new()
    {
        Id = AppConstants.TemplateIdCompanyHierarchy,
        Category = DiagramCategory.OrganizationChart,
        Title = AppStrings.TemplateCompanyHierarchy,
        Description = AppStrings.TemplateCompanyHierarchyDescription,
        AccentColorKey = ResourceKeys.AccentOrgChart,
        ThumbnailKey = "thumb.orgchart-company-hierarchy",
        Nodes =
        {
            Node("ceo", ShapeType.Rectangle, "CEO", 380, 60, 140, 50, isManager: true),
            Node("d-ops", ShapeType.Rectangle, "Director, Operations", 120, 180, 170, 50),
            Node("d-eng", ShapeType.Rectangle, "Director, Engineering", 365, 180, 170, 50, isManager: true),
            Node("d-sales", ShapeType.Rectangle, "Director, Sales", 610, 180, 170, 50),
            Node("t-mob", ShapeType.Rectangle, "Mobile Team", 250, 300, 140, 50),
            Node("t-web", ShapeType.Rectangle, "Web Team", 450, 300, 140, 50),
        },
        Connectors =
        {
            Link("c1", "ceo", "d-ops"),
            Link("c2", "ceo", "d-eng"),
            Link("c3", "ceo", "d-sales"),
            Link("c4", "d-eng", "t-mob"),
            Link("c5", "d-eng", "t-web"),
        },
    };

    private static TemplateNode Node(string id, ShapeType type, string text, double x, double y, double w, double h, bool isManager = false) => new()
    {
        Id = id,
        Text = text,
        ShapeType = type,
        X = x,
        Y = y,
        Width = w,
        Height = h,
        FillColorKey = isManager ? ResourceKeys.PrimaryColor : ResourceKeys.PanelBackground,
        IsManager = isManager,
    };

    private static TemplateConnector Link(string id, string source, string target, string? text = null, ConnectorKind kind = ConnectorKind.Orthogonal) => new()
    {
        Id = id,
        SourceId = source,
        TargetId = target,
        Text = text ?? string.Empty,
        Kind = kind,
        StrokeColorKey = ResourceKeys.PanelBorderColor,
    };

    private sealed class TemplatePayloadDto
    {
        public string Id { get; set; } = string.Empty;

        public DiagramCategory Category { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<TemplateNode> Nodes { get; set; } = [];

        public List<TemplateConnector> Connectors { get; set; } = [];
    }
}
