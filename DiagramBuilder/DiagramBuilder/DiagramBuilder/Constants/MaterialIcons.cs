namespace DiagramBuilder.Constants;

/// <summary>
/// Google Material Icons (regular) codepoints for the sample's command/tool ids.
/// Loaded from the "MaterialIcons" font registered in MauiProgram
/// (Resources/Fonts/MaterialIcons-Regular.ttf — the official Google
/// material-design-icons font).
///
/// Text is the raw UTF-32 codepoint because MAUI does not ship an icon-font
/// markup extension: a XAML literal cannot express a supplementary-plane
/// character, so each icon is produced via <c>char.ConvertFromUtf32</c>. This
/// replaces the previous hand-rolled Unicode glyphs ("↶", "✋", "🗑", …) —
/// using ad-hoc text characters as icons is not a proper icon implementation
/// and does not align with MAUI UI standards.
/// </summary>
public static class MaterialIcons
{
    // Codepoints from MaterialIcons-Regular.codepoints (Google).
    private const int Undo = 0xE166;
    private const int Redo = 0xE15A;
    private const int PanTool = 0xE925;          // pan tool
    private const int NearMe = 0xE569;           // select/navigate tool
    private const int Category = 0xE574;         // shapes flyout
    private const int AccountTree = 0xE97A;     // connectors flyout
    private const int Delete = 0xE872;
    private const int FormatColorFill = 0xE23A;  // fill colour
    private const int BorderColor = 0xE22B;      // stroke colour
    private const int Menu = 0xE5D2;             // properties (panel) toggle
    private const int ZoomIn = 0xE8FF;
    private const int ZoomOut = 0xE900;
    private const int FitScreen = 0xEA10;        // fit to screen
    private const int ExpandMore = 0xE5CF;       // dropdown caret / expanded
    private const int ExpandLess = 0xE5CE;      // collapsed caret
    private const int Apps = 0xE5C3;
    private const int Lock = 0xE897;
    private const int Check = 0xE5CA;           // menu checkmark

    // File menu
    private const int NoteAdd = 0xE89C;
    private const int FolderOpen = 0xE2C8;
    private const int Save = 0xE161;
    private const int UploadFile = 0xE9FC;      // export
    private const int Print = 0xE8AD;

    // Edit menu
    private const int ContentCut = 0xE14E;
    private const int ContentCopy = 0xE14D;
    private const int ContentPaste = 0xE14F;
    private const int FileCopy = 0xE173;        // duplicate
    private const int SelectAllIcon = 0xE162;

    // View menu
    private const int GridOn = 0xE3EC;
    private const int FilterCenterFocus = 0xE3DC; // snap to grid

    // Arrange menu
    private const int FlipToFront = 0xE883;
    private const int FlipToBack = 0xE882;
    private const int ArrowUpward = 0xE5D8;
    private const int ArrowDownward = 0xE5DB;
    private const int AlignHorizontalLeft = 0xE00D;
    private const int AlignHorizontalRight = 0xE010;
    private const int AlignHorizontalCenter = 0xE00F;
    private const int AlignVerticalTop = 0xE00C;
    private const int AlignVerticalBottom = 0xE015;
    private const int AlignVerticalCenter = 0xE011;
    private const int SwapHoriz = 0xE8D4;
    private const int SwapVert = 0xE8D5;
    private const int CropDin = 0xE3C1;         // match size

    // Window menu
    private const int ViewModule = 0xE8F0;
    private const int ViewQuilt = 0xE8F1;
    private const int ViewSidebar = 0xF114;

    /// <summary>Material icon text for a command/tool id, or null for unknown ids.</summary>
    public static string? TextFor(string id) => id switch
    {
        // File
        AppConstants.CmdNew => Glyph(NoteAdd),
        AppConstants.CmdOpen => Glyph(FolderOpen),
        AppConstants.CmdSave => Glyph(Save),
        AppConstants.CmdSaveAs => Glyph(Save),
        AppConstants.CmdExport => Glyph(UploadFile),
        AppConstants.CmdPrint => Glyph(Print),

        // Edit
        AppConstants.CmdUndo => Glyph(Undo),
        AppConstants.CmdRedo => Glyph(Redo),
        AppConstants.CmdCut => Glyph(ContentCut),
        AppConstants.CmdCopy => Glyph(ContentCopy),
        AppConstants.CmdPaste => Glyph(ContentPaste),
        AppConstants.CmdDelete => Glyph(Delete),
        AppConstants.CmdDuplicate => Glyph(FileCopy),
        AppConstants.CmdSelectAll => Glyph(SelectAllIcon),

        // View
        AppConstants.CmdZoomIn => Glyph(ZoomIn),
        AppConstants.CmdZoomOut => Glyph(ZoomOut),
        AppConstants.CmdFitToScreen => Glyph(FitScreen),
        AppConstants.CmdShowGrid => Glyph(GridOn),
        AppConstants.CmdSnapToGrid => Glyph(FilterCenterFocus),

        // Arrange
        AppConstants.CmdSendToBack => Glyph(FlipToBack),
        AppConstants.CmdBringToFront => Glyph(FlipToFront),
        AppConstants.CmdSendBackward => Glyph(ArrowDownward),
        AppConstants.CmdBringForward => Glyph(ArrowUpward),
        AppConstants.CmdAlignLeft => Glyph(AlignHorizontalLeft),
        AppConstants.CmdAlignRight => Glyph(AlignHorizontalRight),
        AppConstants.CmdAlignCenter => Glyph(AlignHorizontalCenter),
        AppConstants.CmdAlignTop => Glyph(AlignVerticalTop),
        AppConstants.CmdAlignBottom => Glyph(AlignVerticalBottom),
        AppConstants.CmdAlignMiddle => Glyph(AlignVerticalCenter),
        AppConstants.CmdDistributeHorizontally => Glyph(SwapHoriz),
        AppConstants.CmdDistributeVertically => Glyph(SwapVert),
        AppConstants.CmdSameWidth => Glyph(CropDin),
        AppConstants.CmdSameHeight => Glyph(CropDin),
        AppConstants.CmdSameWidthAndHeight => Glyph(CropDin),
        AppConstants.CmdGroup => Glyph(FlipToFront),
        AppConstants.CmdUngroup => Glyph(FlipToBack),

        // Window
        AppConstants.CmdShowToolbar => Glyph(ViewModule),
        AppConstants.CmdShowStencil => Glyph(ViewSidebar),
        AppConstants.CmdShowProperties => Glyph(ViewQuilt),

        // Toolbar-only ids
        AppConstants.ToolLock => Glyph(Lock),
        AppConstants.ToolPan => Glyph(PanTool),
        AppConstants.ToolSelect => Glyph(NearMe),
        "tool.shape" => Glyph(Category),
        "tool.connector" => Glyph(AccountTree),
        "tool.stroke.color" => Glyph(BorderColor),
        "tool.fill.color" => Glyph(FormatColorFill),
        "arrange" => Glyph(Apps),

        // Submenu headers
        "arrange.align" => Glyph(Menu),
        "arrange.distribute" => Glyph(SwapHoriz),
        "arrange.matchsize" => Glyph(CropDin),

        _ => null,
    };

    /// <summary>Dropdown caret icon (rendered after a flyout label).</summary>
    public static string DropdownCaret => Glyph(ExpandMore);

    /// <summary>Collapse caret icon (expand-less, pointing up).</summary>
    public static string CollapseCaret => Glyph(ExpandLess);

    /// <summary>Checkmark icon (checked menu items).</summary>
    public static string CheckGlyph => Glyph(Check);

    private static string Glyph(int codepoint) => char.ConvertFromUtf32(codepoint);
}