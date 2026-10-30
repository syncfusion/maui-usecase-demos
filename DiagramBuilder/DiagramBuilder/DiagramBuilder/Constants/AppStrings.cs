namespace DiagramBuilder.Constants;

/// <summary>
/// Centralized user-visible strings. Views must never hardcode text.
/// </summary>
public static class AppStrings
{
    // Application
    public const string AppTitle = "Diagram Builder";
    public const string UntitledDocument = "Untitled Diagram";

    // Categories
    public const string CategoryFlowChart = "Flow Chart";
    public const string CategoryMindMap = "Mind Map";
    public const string CategoryOrganizationChart = "Organization Chart";
    public const string CategoryFlowChartDescription = "Process flows, decision trees and workflows.";
    public const string CategoryMindMapDescription = "Radial idea maps for brainstorming and planning.";
    public const string CategoryOrganizationChartDescription = "Company structures, teams and reporting lines.";

    // Create dialog
    public const string CreateDialogTitle = "Create Diagram";
    public const string CreateDialogSubtitle = "Choose a diagram category and start from a blank canvas or a predefined template.";
    public const string CreateAction = "Create";
    public const string BlankSuffix = "Blank";
    public const string NodesCountFormat = "{0} shapes";
    public const string BlankStarterText = "Double-click to rename";

    // Templates
    public const string TemplateBlankFlowChart = "Blank Flow Chart";
    public const string TemplateCreditCardProcessing = "Credit Card Processing";
    public const string TemplateBlankMindMap = "Blank Mind Map";
    public const string TemplateProductPlanning = "Product Planning";
    public const string TemplateBlankOrgChart = "Blank Org Chart";
    public const string TemplateCompanyHierarchy = "Company Hierarchy";
    public const string TemplateBlankFlowChartDescription = "Start with an empty flow chart canvas.";
    public const string TemplateCreditCardProcessingDescription = "End-to-end card payment approval flow.";
    public const string TemplateBlankMindMapDescription = "Start with an empty mind map canvas.";
    public const string TemplateProductPlanningDescription = "Product planning branches for a new release.";
    public const string TemplateBlankOrgChartDescription = "Start with an empty organization chart.";
    public const string TemplateCompanyHierarchyDescription = "Executive, director and team reporting tree.";

    // Menus
    public const string MenuFile = "FILE";
    public const string MenuEdit = "EDIT";
    public const string MenuView = "VIEW";
    public const string MenuArrange = "ARRANGE";
    public const string MenuWindow = "WINDOW";

    public const string MenuNew = "New";
    public const string MenuOpen = "Open";
    public const string MenuSave = "Save";
    public const string MenuSaveAs = "Save As";
    public const string MenuExport = "Export";
    public const string MenuPrint = "Print";

    public const string MenuUndo = "Undo";
    public const string MenuRedo = "Redo";
    public const string MenuCut = "Cut";
    public const string MenuCopy = "Copy";
    public const string MenuPaste = "Paste";
    public const string MenuDelete = "Delete";
    public const string MenuDuplicate = "Duplicate";
    public const string MenuSelectAll = "Select All";

    public const string MenuZoomIn = "Zoom In";
    public const string MenuZoomOut = "Zoom Out";
    public const string MenuFitToScreen = "Fit To Screen";
    public const string MenuShowGrid = "Show Grid";
    public const string MenuSnapToGrid = "Snap To Grid";

    public const string MenuSendToBack = "Send To Back";
    public const string MenuBringToFront = "Bring To Front";
    public const string MenuSendBackward = "Send Backward";
    public const string MenuBringForward = "Bring Forward";
    public const string MenuAlignObjects = "Align Objects";
    public const string MenuDistributeObjects = "Distribute Objects";
    public const string MenuMatchSize = "Match Size";
    public const string MenuGroup = "Group";
    public const string MenuUngroup = "Ungroup";

    public const string MenuAlignLeft = "Left";
    public const string MenuAlignRight = "Right";
    public const string MenuAlignCenter = "Center";
    public const string MenuAlignTop = "Top";
    public const string MenuAlignBottom = "Bottom";
    public const string MenuAlignMiddle = "Middle";
    public const string MenuDistributeHorizontally = "Horizontally";
    public const string MenuDistributeVertically = "Vertically";
    public const string MenuSameWidth = "Same Width";
    public const string MenuSameHeight = "Same Height";
    public const string MenuSameWidthAndHeight = "Same Width & Height";

    public const string MenuShowToolbar = "Show Toolbar";
    public const string MenuShowStencil = "Show Stencil";
    public const string MenuShowProperties = "Show Properties";

    // Toolbar
    public const string ToolbarUndo = "Undo";
    public const string ToolbarRedo = "Redo";
    public const string ToolbarPanTool = "Pan Tool";
    public const string ToolbarSelectionTool = "Selection Tool";
    public const string ToolbarBasicShapes = "Basic Shapes";
    public const string ToolbarRectangle = "Rectangle";
    public const string ToolbarEllipse = "Ellipse";
    public const string ToolbarPolygon = "Polygon";
    public const string ToolbarConnectors = "Connectors";
    public const string ToolbarStraightConnector = "Straight Connector";
    public const string ToolbarOrthogonalConnector = "Orthogonal Connector";
    public const string ToolbarBezierConnector = "Bezier Connector";
    public const string ToolbarTextTool = "Text Tool";
    public const string ToolbarFillColor = "Fill Color";
    public const string ToolbarBorderColor = "Border Color";
    public const string ToolbarArrangeObject = "Arrange Object";
    public const string ToolbarLockObject = "Lock Object";
    public const string ToolbarDeleteObject = "Delete Object";
    public const string ToolbarZoomIn = "Zoom In";
    public const string ToolbarZoomOut = "Zoom Out";
    public const string ToolbarFitToScreen = "Fit To Screen";
    public const string ToolbarToggleProperties = "Toggle Property Panel";

    // Stencil
    public const string StencilPanelTitle = "Shapes";
    public const string StencilSearchPlaceholder = "Search shapes";
    public const string StencilGroupFlowShapes = "FLOW SHAPES";
    public const string StencilGroupBasicShapes = "BASIC SHAPES";
    public const string StencilGroupConnectors = "CONNECTORS";
    public const string StencilProcess = "Process";
    public const string StencilDecision = "Decision";
    public const string StencilTerminator = "Terminator";
    public const string StencilDocument = "Document";
    public const string StencilDataPreparation = "Data Preparation";
    public const string StencilRectangle = "Rectangle";
    public const string StencilCircle = "Circle";
    public const string StencilTriangle = "Triangle";
    public const string StencilHexagon = "Hexagon";
    public const string StencilDiamond = "Diamond";
    public const string StencilPentagon = "Pentagon";
    public const string StencilStar = "Star";
    public const string StencilStraight = "Straight";
    public const string StencilOrthogonal = "Orthogonal";
    public const string StencilBezier = "Bezier";
    public const string StencilNoResults = "No shapes match your search.";

    // Property panel
    public const string PropertyPanelTitle = "Properties";
    public const string PropertyEmptyState = "Select an object to edit its properties";
    public const string PropertySectionDimensions = "DIMENSIONS";
    public const string PropertySectionInsert = "INSERT";
    public const string PropertySectionBackground = "BACKGROUND";
    public const string PropertySectionFill = "FILL";
    public const string PropertySectionBorder = "BORDER";
    public const string PropertySectionText = "TEXT";

    public const string PropertyPositionX = "Position X";
    public const string PropertyPositionY = "Position Y";
    public const string PropertyWidth = "Width";
    public const string PropertyHeight = "Height";
    public const string PropertyRotation = "Rotation Angle";
    public const string PropertyLockAspectRatio = "Lock Aspect Ratio";
    public const string PropertyInsertLink = "Insert Link";
    public const string PropertyInsertImage = "Insert Image";
    public const string PropertyBackgroundType = "Type";
    public const string PropertyBackgroundSolid = "Solid";
    public const string PropertyBackgroundGradient = "Gradient";
    public const string PropertyFillColor = "Color";
    public const string PropertyOpacity = "Opacity";
    public const string PropertyLineStyle = "Line Style";
    public const string PropertyLineSolid = "Solid";
    public const string PropertyLineDashed = "Dashed";
    public const string PropertyLineDotted = "Dotted";
    public const string PropertyBorderColor = "Border Color";
    public const string PropertyThickness = "Thickness";
    public const string PropertyTextContent = "Text Content";
    public const string PropertyFontFamily = "Font Family";
    public const string PropertyFontSize = "Font Size";
    public const string PropertyBold = "Bold";
    public const string PropertyItalic = "Italic";
    public const string PropertyUnderline = "Underline";
    public const string PropertyTextColor = "Text Color";
    public const string PropertyHorizontalAlign = "Horizontal Alignment";
    public const string PropertyVerticalAlign = "Vertical Alignment";

    // Border segment keys (used by PropertyPanelControl segmented control).
    public const string PropertyBorderSolid = "Solid";
    public const string PropertyBorderDashed = "Dashed";
    public const string PropertyBorderDotted = "Dotted";

    // Text alignment segment keys.
    public const string PropertyAlignLeft = "Left";
    public const string PropertyAlignCenter = "Center";
    public const string PropertyAlignRight = "Right";
    public const string PropertyAlignTop = "Top";
    public const string PropertyAlignMiddle = "Middle";
    public const string PropertyAlignBottom = "Bottom";

    // Export
    public const string ExportDialogTitle = "Export Diagram";
    public const string ExportFormatPng = "PNG";
    public const string ExportFormatJpeg = "JPEG";
    public const string ExportFormatPdf = "PDF";
    public const string ExportFormatSvg = "SVG";
    public const string ExportFormatLabel = "Format";
    public const string ExportFileNameLabel = "File name";
    public const string ExportRegionLabel = "Region";
    public const string ExportRegionContent = "Content";
    public const string ExportRegionPageSettings = "Page Settings";
    public const string ExportAction = "Export";
    public const string ExportCancel = "Cancel";
    public const string ExportSuccessFormat = "Diagram exported as {0}.";
    public const string ExportMockNotice = "Demo build: export output is simulated.";

    // Status / misc
    public const string StatusReady = "Ready";
    public const string StatusSavedFormat = "{0} saved.";
    public const string StatusExportedFormat = "{0} exported.";
    public const string StatusOpenedFormat = "{0} opened.";
    public const string StatusSelectionFormat = "{0} selected";
    public const string StatusNothingSelected = "Nothing selected";
    public const string StatusCommandExecuted = "{0} executed.";
    public const string SaveNotAvailableTitle = "Save Diagram";
    public const string SaveNotAvailableMessage = "Diagrams are kept in memory for this demo session.";
    public const string OpenNotAvailableTitle = "Open Diagram";
    public const string OpenNotAvailableMessage = "This demo lists recent diagrams in memory only.";
    public const string OkAction = "OK";
    public const string ZoomFitLabel = "Fit";
    public const string GridOnLabel = "Grid on";
    public const string GridOffLabel = "Grid off";
    public const string SnapOnLabel = "Snap on";
    public const string SnapOffLabel = "Snap off";
}
