using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Services;

/// <summary>
/// In-code stencil catalog: three groups (flow shapes, basic shapes, connectors)
/// per spec.md §7 / FR-6. Built from constants only.
/// </summary>
public static class StencilCatalog
{
    public static IReadOnlyList<StencilGroup> All { get; } =
    [
        new StencilGroup
        {
            Id = "stencil.flow",
            Title = AppStrings.StencilGroupFlowShapes,
            Items =
            {
                Shape(ShapeType.Process, AppStrings.StencilProcess, "process"),
                Shape(ShapeType.Decision, AppStrings.StencilDecision, "decision,diamond"),
                Shape(ShapeType.Terminator, AppStrings.StencilTerminator, "terminator,start,end"),
                Shape(ShapeType.Document, AppStrings.StencilDocument, "document"),
                Shape(ShapeType.DataPreparation, AppStrings.StencilDataPreparation, "data,preparation"),
            },
        },
        new StencilGroup
        {
            Id = "stencil.basic",
            Title = AppStrings.StencilGroupBasicShapes,
            Items =
            {
                Shape(ShapeType.Rectangle, AppStrings.StencilRectangle, "rectangle,square"),
                Shape(ShapeType.Circle, AppStrings.StencilCircle, "circle,ellipse"),
                Shape(ShapeType.Triangle, AppStrings.StencilTriangle, "triangle"),
                Shape(ShapeType.Hexagon, AppStrings.StencilHexagon, "hexagon"),
                Shape(ShapeType.Diamond, AppStrings.StencilDiamond, "diamond"),
                Shape(ShapeType.Pentagon, AppStrings.StencilPentagon, "pentagon"),
                Shape(ShapeType.Star, AppStrings.StencilStar, "star"),
            },
        },
        new StencilGroup
        {
            Id = "stencil.connectors",
            Title = AppStrings.StencilGroupConnectors,
            IsConnectorGroup = true,
            Items =
            {
                Connector(ConnectorKind.Straight, AppStrings.StencilStraight, "straight,line"),
                Connector(ConnectorKind.Orthogonal, AppStrings.StencilOrthogonal, "orthogonal,right,angle"),
                Connector(ConnectorKind.Bezier, AppStrings.StencilBezier, "bezier,curve"),
            },
        },
    ];

    private static StencilItem Shape(ShapeType type, string display, string tags) => new()
    {
        Id = "shape." + type.ToString().ToLowerInvariant(),
        ShapeType = type,
        DisplayName = display,
        SearchTags = tags.Split(','),
    };

    private static StencilItem Connector(ConnectorKind kind, string display, string tags) => new()
    {
        Id = "connector." + kind.ToString().ToLowerInvariant(),
        ConnectorKind = kind,
        DisplayName = display,
        SearchTags = tags.Split(','),
    };
}

/// <summary>
/// In-code menu bar catalog: FILE/EDIT/VIEW/ARRANGE/WINDOW trees per spec.md §6.
/// </summary>
public static class MenuCatalog
{
    public static IReadOnlyList<MenuCommandItem> All { get; } =
    [
        new MenuCommandItem
        {
            Id = "menu.file",
            TextKey = AppStrings.MenuFile,
            Children =
            {
                Item(AppConstants.CmdNew, AppStrings.MenuNew, "Ctrl+N"),
                Item(AppConstants.CmdOpen, AppStrings.MenuOpen),
                Item(AppConstants.CmdSave, AppStrings.MenuSave, "Ctrl+S"),
                Item(AppConstants.CmdSaveAs, AppStrings.MenuSaveAs),
                Separator(),
                Item(AppConstants.CmdExport, AppStrings.MenuExport),
                Item(AppConstants.CmdPrint, AppStrings.MenuPrint, "Ctrl+P"),
            },
        },
        new MenuCommandItem
        {
            Id = "menu.edit",
            TextKey = AppStrings.MenuEdit,
            Children =
            {
                Item(AppConstants.CmdUndo, AppStrings.MenuUndo, "Ctrl+Z"),
                Item(AppConstants.CmdRedo, AppStrings.MenuRedo, "Ctrl+Y"),
                Separator(),
                Item(AppConstants.CmdCut, AppStrings.MenuCut, "Ctrl+X"),
                Item(AppConstants.CmdCopy, AppStrings.MenuCopy, "Ctrl+C"),
                Item(AppConstants.CmdPaste, AppStrings.MenuPaste, "Ctrl+V"),
                Item(AppConstants.CmdDelete, AppStrings.MenuDelete, "Del"),
                Item(AppConstants.CmdDuplicate, AppStrings.MenuDuplicate, "Ctrl+D"),
                Separator(),
                Item(AppConstants.CmdSelectAll, AppStrings.MenuSelectAll, "Ctrl+A"),
            },
        },
        new MenuCommandItem
        {
            Id = "menu.view",
            TextKey = AppStrings.MenuView,
            Children =
            {
                Item(AppConstants.CmdZoomIn, AppStrings.MenuZoomIn, "Ctrl+="),
                Item(AppConstants.CmdZoomOut, AppStrings.MenuZoomOut, "Ctrl+-"),
                Item(AppConstants.CmdFitToScreen, AppStrings.MenuFitToScreen, "Shift+F1"),
                Separator(),
                Check(AppConstants.CmdShowGrid, AppStrings.MenuShowGrid, isChecked: true),
                Check(AppConstants.CmdSnapToGrid, AppStrings.MenuSnapToGrid, isChecked: true),
            },
        },
        new MenuCommandItem
        {
            Id = "menu.arrange",
            TextKey = AppStrings.MenuArrange,
            Children =
            {
                Item(AppConstants.CmdSendToBack, AppStrings.MenuSendToBack),
                Item(AppConstants.CmdBringToFront, AppStrings.MenuBringToFront),
                Item(AppConstants.CmdSendBackward, AppStrings.MenuSendBackward),
                Item(AppConstants.CmdBringForward, AppStrings.MenuBringForward),
                Separator(),
                Submenu("arrange.align", AppStrings.MenuAlignObjects,
                [
                    Item(AppConstants.CmdAlignLeft, AppStrings.MenuAlignLeft),
                    Item(AppConstants.CmdAlignRight, AppStrings.MenuAlignRight),
                    Item(AppConstants.CmdAlignCenter, AppStrings.MenuAlignCenter),
                    Item(AppConstants.CmdAlignTop, AppStrings.MenuAlignTop),
                    Item(AppConstants.CmdAlignBottom, AppStrings.MenuAlignBottom),
                    Item(AppConstants.CmdAlignMiddle, AppStrings.MenuAlignMiddle),
                ]),
                Submenu("arrange.distribute", AppStrings.MenuDistributeObjects,
                [
                    Item(AppConstants.CmdDistributeHorizontally, AppStrings.MenuDistributeHorizontally),
                    Item(AppConstants.CmdDistributeVertically, AppStrings.MenuDistributeVertically),
                ]),
                Submenu("arrange.matchsize", AppStrings.MenuMatchSize,
                [
                    Item(AppConstants.CmdSameWidth, AppStrings.MenuSameWidth),
                    Item(AppConstants.CmdSameHeight, AppStrings.MenuSameHeight),
                    Item(AppConstants.CmdSameWidthAndHeight, AppStrings.MenuSameWidthAndHeight),
                ]),
                Separator(),
                Item(AppConstants.CmdGroup, AppStrings.MenuGroup, "Ctrl+G"),
                Item(AppConstants.CmdUngroup, AppStrings.MenuUngroup, "Ctrl+Shift+G"),
            },
        },
        new MenuCommandItem
        {
            Id = "menu.window",
            TextKey = AppStrings.MenuWindow,
            Children =
            {
                Check(AppConstants.CmdShowToolbar, AppStrings.MenuShowToolbar, isChecked: true),
                Check(AppConstants.CmdShowStencil, AppStrings.MenuShowStencil, isChecked: true),
                Check(AppConstants.CmdShowProperties, AppStrings.MenuShowProperties, isChecked: true),
            },
        },
    ];

    private static MenuCommandItem Item(string id, string textKey, string? gesture = null) => new()
    {
        Id = id,
        TextKey = textKey,
        GestureKey = gesture ?? string.Empty,
    };

    private static MenuCommandItem Check(string id, string textKey, bool isChecked) => new()
    {
        Id = id,
        TextKey = textKey,
        IsCheckable = true,
        IsChecked = isChecked,
    };

    private static MenuCommandItem Separator() => new() { IsSeparator = true };

    private static MenuCommandItem Submenu(string id, string textKey, IEnumerable<MenuCommandItem> children)
    {
        var result = new MenuCommandItem { Id = id, TextKey = textKey };
        foreach (var child in children)
        {
            result.Children.Add(child);
        }

        return result;
    }
}

/// <summary>
/// In-code toolbar catalog: 8 groups per spec.md §7 / F2.2.
/// </summary>
public static class ToolbarCatalog
{
    public static IReadOnlyList<Models.ToolbarItem> All { get; } =
    [
        // Group 1 — undo/redo
        Tool(AppConstants.CmdUndo, AppStrings.ToolbarUndo, group: 1),
        Tool(AppConstants.CmdRedo, AppStrings.ToolbarRedo, group: 1),

        // Group 2 — pan/select mutually exclusive toggles
        Tool(AppConstants.ToolPan, AppStrings.ToolbarPanTool, group: 2, isToggle: true),
        Tool(AppConstants.ToolSelect, AppStrings.ToolbarSelectionTool, group: 2, isToggle: true, isChecked: true),

        // Group 3 — basic shapes flyout
        Tool("tool.shape.rectangle", AppStrings.ToolbarRectangle, group: 3),
        Tool("tool.shape.ellipse", AppStrings.ToolbarEllipse, group: 3),
        Tool("tool.shape.polygon", AppStrings.ToolbarPolygon, group: 3),

        // Group 4 — connector flyout
        Tool(AppConstants.ToolConnectorStraight, AppStrings.ToolbarStraightConnector, group: 4),
        Tool(AppConstants.ToolConnectorOrthogonal, AppStrings.ToolbarOrthogonalConnector, group: 4),
        Tool(AppConstants.ToolConnectorBezier, AppStrings.ToolbarBezierConnector, group: 4),

        // Group 5 — text tool
        Tool(AppConstants.ToolText, AppStrings.ToolbarTextTool, group: 5),

        // Group 6 — selection-only: colors, z-order, lock, delete
        Tool("tool.fill.color", AppStrings.ToolbarFillColor, group: 6),
        Tool("tool.border.color", AppStrings.ToolbarBorderColor, group: 6),
        Tool(AppConstants.CmdSendToBack, AppStrings.ToolbarArrangeObject, group: 6),
        Tool(AppConstants.CmdBringToFront, AppStrings.ToolbarArrangeObject, group: 6),
        Tool(AppConstants.CmdSendBackward, AppStrings.ToolbarArrangeObject, group: 6),
        Tool(AppConstants.CmdBringForward, AppStrings.ToolbarArrangeObject, group: 6),
        Tool("tool.lock", AppStrings.ToolbarLockObject, group: 6),
        Tool(AppConstants.CmdDelete, AppStrings.ToolbarDeleteObject, group: 6),

        // Group 7 — zoom
        Tool(AppConstants.CmdZoomIn, AppStrings.ToolbarZoomIn, group: 7),
        Tool(AppConstants.CmdZoomOut, AppStrings.ToolbarZoomOut, group: 7),
        Tool(AppConstants.CmdFitToScreen, AppStrings.ToolbarFitToScreen, group: 7),

        // Group 8 — toggle property panel
        Tool(AppConstants.CmdShowProperties, AppStrings.ToolbarToggleProperties, group: 8, isToggle: true, isChecked: true),
    ];

    private static Models.ToolbarItem Tool(string id, string tooltipKey, int group, bool isToggle = false, bool isChecked = false) => new()
    {
        Id = id,
        TooltipKey = tooltipKey,
        Group = group,
        IsToggle = isToggle,
        IsChecked = isChecked,
    };
}

/// <summary>
/// Mock recent-document list for FILE ▸ Open (in-memory only, no backend).
/// </summary>
public static class RecentDocumentCatalog
{
    public static IReadOnlyList<string> All { get; } =
    [
        AppStrings.TemplateCreditCardProcessing,
        AppStrings.TemplateProductPlanning,
        AppStrings.TemplateCompanyHierarchy,
    ];
}
