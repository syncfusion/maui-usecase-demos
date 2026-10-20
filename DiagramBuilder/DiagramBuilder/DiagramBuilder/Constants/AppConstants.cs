namespace DiagramBuilder.Constants;

/// <summary>
/// Centralized non-string constants: metrics, limits, ids and resource locations.
/// </summary>
public static class AppConstants
{
    public const string FontFamily = "Segoe UI";

    // Layout metrics
    public const double StencilPanelWidth = 280;
    public const double PropertyPanelWidth = 320;
    public const double MenuBarHeight = 40;
    public const double ToolbarHeight = 48;
    public const double ToolbarButtonSize = 36;
    public const double StatusbarHeight = 26;
    public const double PanelCornerRadius = 8;
    public const double ResponsiveCollapseWidth = 1000;

    // Diagram defaults
    public const double DefaultNodeWidth = 120;
    public const double DefaultNodeHeight = 60;
    public const double DefaultConnectorStrokeWidth = 1.5;
    public const double GridSize = 20;
    public const int AnnotationFontSize = 12;

    // Zoom
    public static readonly double[] ZoomLevels = { 0.5, 1.0, 1.5, 2.0 };
    public const double ZoomStep = 0.25;
    public const double ZoomMinimum = 0.25;
    public const double ZoomMaximum = 4.0;

    // Export
    public const string DefaultExportFileName = "diagram";

    // Template asset paths
    public const string TemplatePathFlowCreditCard = "Templates/flow-credit-card-processing.json";
    public const string TemplatePathMindMapProductPlanning = "Templates/mindmap-product-planning.json";
    public const string TemplatePathOrgChartCompanyHierarchy = "Templates/orgchart-company-hierarchy.json";

    // Well-known template ids
    public const string TemplateIdBlankFlowChart = "flow-blank";
    public const string TemplateIdCreditCardProcessing = "flow-credit-card-processing";
    public const string TemplateIdBlankMindMap = "mindmap-blank";
    public const string TemplateIdProductPlanning = "mindmap-product-planning";
    public const string TemplateIdBlankOrgChart = "orgchart-blank";
    public const string TemplateIdCompanyHierarchy = "orgchart-company-hierarchy";

    // Well-known command ids
    public const string CmdNew = "file.new";
    public const string CmdOpen = "file.open";
    public const string CmdSave = "file.save";
    public const string CmdSaveAs = "file.saveas";
    public const string CmdExport = "file.export";
    public const string CmdPrint = "file.print";
    public const string CmdUndo = "edit.undo";
    public const string CmdRedo = "edit.redo";
    public const string CmdCut = "edit.cut";
    public const string CmdCopy = "edit.copy";
    public const string CmdPaste = "edit.paste";
    public const string CmdDelete = "edit.delete";
    public const string CmdDuplicate = "edit.duplicate";
    public const string CmdSelectAll = "edit.selectall";
    public const string CmdZoomIn = "view.zoomin";
    public const string CmdZoomOut = "view.zoomout";
    public const string CmdFitToScreen = "view.fittoscreen";
    public const string CmdShowGrid = "view.showgrid";
    public const string CmdSnapToGrid = "view.snaptogrid";
    public const string CmdSendToBack = "arrange.sendtoback";
    public const string CmdBringToFront = "arrange.bringtofront";
    public const string CmdSendBackward = "arrange.sendbackward";
    public const string CmdBringForward = "arrange.bringforward";
    public const string CmdAlignLeft = "arrange.align.left";
    public const string CmdAlignRight = "arrange.align.right";
    public const string CmdAlignCenter = "arrange.align.center";
    public const string CmdAlignTop = "arrange.align.top";
    public const string CmdAlignBottom = "arrange.align.bottom";
    public const string CmdAlignMiddle = "arrange.align.middle";
    public const string CmdDistributeHorizontally = "arrange.distribute.horizontally";
    public const string CmdDistributeVertically = "arrange.distribute.vertically";
    public const string CmdSameWidth = "arrange.matchsize.width";
    public const string CmdSameHeight = "arrange.matchsize.height";
    public const string CmdSameWidthAndHeight = "arrange.matchsize.both";
    public const string CmdGroup = "arrange.group";
    public const string CmdUngroup = "arrange.ungroup";
    public const string CmdShowToolbar = "window.showtoolbar";
    public const string CmdShowStencil = "window.showstencil";
    public const string CmdShowProperties = "window.showproperties";

    // Toolbar tool ids
    public const string ToolPan = "tool.pan";
    public const string ToolSelect = "tool.select";
    public const string ToolConnectorStraight = "tool.connector.straight";
    public const string ToolConnectorOrthogonal = "tool.connector.orthogonal";
    public const string ToolConnectorBezier = "tool.connector.bezier";
    public const string ToolText = "tool.text";
}
