# Diagram Builder — UI Specification (ui-spec.md)

Derived from `design-images/` mockups: `image_1..8.png` = full designer states;
`file/edit/view/arrange/window_section.png` = menu flyouts; `toolbar_shape.png`,
`toolbar_sline.png`, `toolbar_fill_color_picker.png` = toolbar flyouts; `property_panel.png`
= right panel.

## 1. Overall frame

- Desktop chrome, light theme, 8 px outer padding, 1 px `PanelBorder` separators.
- Title row: "Diagram Builder" + document name (left), status text (right).
- Row order: Title → Menu bar → Toolbar → 3-column content → status bar.
- Content columns: Stencil 280 px | Canvas (fill) | Property 320 px (values from `AppConstants`).
- Panels collapse to 0 width with animation; toggled by WINDOW menu or header chevrons.

## 2. Menu bar (`MenuBarControl`)

Horizontal items, **mixed-case, regular weight**: File · Edit · View · Arrange · Window.
Click opens a flyout whose items match `*_section.png` exactly, including separators:

| Menu | Items (— = separator) |
|---|---|
| FILE | New, Open, Save, Save As, —, Export, Print |
| EDIT | Undo, Redo, —, Cut, Copy, Paste, Delete, Duplicate, —, Select All |
| VIEW | Zoom In, Zoom Out, Fit To Screen, —, Show Grid ✓, Snap To Grid ✓ |
| ARRANGE | Send To Back, Bring To Front, Send Backward, Bring Forward, —, Align Objects ▸ (Left Right Center Top Bottom Middle), Distribute Objects ▸ (Horizontally Vertically), Match Size ▸ (Same Width Same Height Same Width & Height), —, Group, Ungroup |
| WINDOW | Show Toolbar ✓, Show Stencil ✓, Show Properties ✓ |

- Checkable items show a check glyph in accent blue; shortcuts right-aligned (e.g. "Ctrl+Z").
- ARRANGE submenus are **cascading flyouts** (open to the right), not flattened indented rows.
- Flyout rows: height 32, hover highlight `HoverBackground`, white popup with 8 px corner
  radius, 1 px `PanelBorderColor` border, subtle drop shadow, transparent (non-dimming) overlay.
- Checked state (grid/snap/panels) is live — menu must reflect current observable state on
  every open, not a snapshot baked at build time.

## 3. Toolbar (`ToolbarControl`)

Icon buttons 36×36, 4 px spacing, 1 px separators between groups 1–8:

1. Undo, Redo
2. Pan (hand) / Selection (arrow) — mutually exclusive; active tool highlighted with `SelectionBlue`
3. Basic shapes flyout (`toolbar_shape.png`): Rectangle, Ellipse, Polygon — **large 48 px glyph
   tiles + labels** in a grid (not a text list); click-then-place on canvas
4. Connector flyout (`toolbar_sline.png`): Straight, Orthogonal, Bezier — **line-path preview
   tiles + labels**
5. Text tool
6. *(visible only when exactly one node is selected)* Fill color swatch, Border color swatch
   (`toolbar_fill_color_picker.png` → `SfColorPicker` popup anchored to the swatch; the
   picked color is applied to the selected object), Send To Back, Bring To Front,
   Send Backward, Bring Forward, Lock, Delete. Swatches reflect the selected object's
   current fill/border color.
7. Zoom In, Zoom Out, Fit To Screen; zoom chips 50 % / 100 % / 150 % / 200 % — the chip
   matching the current zoom shows the active style (`ZoomChipActiveStyle`)
8. Toggle property panel

- Single-glyph line icons for every button (crisp icon glyphs; no multi-character fallbacks).
- Toolbar state (active tool, group-6 visibility, zoom chip selection) must update reactively
  when the underlying observable changes — not only at initial build.

## 4. Stencil panel (left, `StencilPanelControl`)

- Header "Shapes" with collapse chevron and search entry (placeholder "Search shapes").
- Collapsible categories (accordion, multiple expanded):
  - **FLOW SHAPES**: Process, Decision, Terminator, Document, Data Preparation
  - **BASIC SHAPES**: Rectangle, Circle, Triangle, Hexagon, Diamond, Pentagon, Star
  - **CONNECTORS**: Straight, Orthogonal, Bezier
- Shape tiles: **64×64 glyph preview + name beneath, borderless**, with hover highlight
  (`HoverBackground`); search filters by name/tag and shows a "no results" empty state.
- Connector entries set the active connector tool on click (not draggable); the active
  connector tool is consumed by the canvas/toolbar connector flow.
- Shapes are fed to the shared `SymbolPalette` (hosted as a direct Grid sibling of the
  diagram — see architecture.md §4.3) so drag-and-drop onto the canvas is native.
  Only **one** palette surface is visible; the stencil panel and palette must not render
  duplicate shape lists.

## 5. Canvas (center, `DiagramWorkspaceControl`)

- White canvas over `WindowBackground` workspace; **dot grid at 20 px when Show Grid is
  enabled** (bound to the VIEW menu toggle, affecting the diagram's grid — not just a label).
- SfDiagram fills the region; supports add/move/resize/rotate/connect/delete,
  single-select, rubber-band multi-select, undo/redo, inline annotation edit on double-click.
- Snap To Grid toggle drives the diagram's snap settings.
- Status bar (bottom): selection summary (left), zoom % and snap/grid indicator chips
  (right, pill-shaped with rounded corners).

## 6. Property panel (right, `PropertyPanelControl` — matches `property_panel.png`)

Section headers uppercase, 11 px letter-spaced; two-column label/field rows.

| Section | Fields |
|---|---|
| DIMENSIONS | Position X, Position Y, Width, Height (`SfNumericEntry`), Rotation Angle, Lock Aspect Ratio (**checkbox**) |
| INSERT | Insert Link, Insert Image (mock commands — rendered as actionable rows, not static labels) |
| BACKGROUND | Solid / Gradient chooser (segmented), color swatch |
| FILL | color picker, opacity slider |
| BORDER | line style segmented (Solid/Dashed/Dotted), color, thickness, opacity |
| TEXT | content entry, font family (Segoe UI), size, Bold/Italic/Underline toggles, text color, horizontal + vertical alignment (segmented) |

- Empty state: centered ghost icon + "Select an object to edit its properties".
- Panel auto-shows when a node is selected (unless manually hidden via WINDOW).
- Every rendered field must be wired: numeric changes, color changes, slider changes,
  toggles, and segmented buttons all apply through the property panel ViewModel. Applied
  values must use the type the service layer expects (e.g. double for opacity).
- Property rows are rendered via the `PropertyEditorTemplates` DataTemplates per
  `PropertyGroup` — not imperative code-behind row construction.
- Property editing must work for a selected **connector** as well as a node (stroke,
  thickness, dash style, text), with a sensible per-kind field set.

## 7. Export dialog (`ExportDialog`)

- Format chooser: PNG, JPEG, PDF, SVG; Region: Content / Page Settings; File name entry
  pre-filled from document title.
- Selected format/region show a clear selected state; the primary action is labeled
  "Export" and runs the export then closes; Cancel discards.
- Mock export returns success status with would-be file name; PNG/JPEG/SVG demo a real
  `ExportDiagram` data URI when the diagram is ready. PDF is mock-only.

## 8. Create Diagram dialog (`CreateDiagramPage`)

- Three category cards (accent color, icon, description): Flow Chart `#0D6EFD`,
  Mind Map `#7C3AED`, Organization Chart `#0F9D58`.
- The **selected category shows a selected state** (accent indicator/highlight) in the
  left category rail.
- Selecting a category reveals its two template cards: **light thumbnail swatch with faint
  gray wireframe nodes** (use the `ThumbnailNodeFill`/`ThumbnailNodeStroke`/`ThumbnailWireColor`
  theme keys — not a solid accent block), title, description, node count, Create button
  colored by the category accent.
- All user-visible text (section labels "Blank"/"Predefined Templates", node-count format)
  comes from `AppStrings`.

## 9. Theme (light only — keys in `Colors.xaml`, referenced via `DynamicResource`)

| Key | Value |
|---|---|
| PrimaryColor | #0D6EFD |
| WindowBackground | #F5F6F8 |
| CanvasBackground | #FFFFFF |
| PanelBackground | #FAFAFA |
| PanelBorderColor | #DADADA |
| SelectionBlue | #2196F3 |
| TextPrimary | #1F1F1F |
| TextSecondary | #5F6368 |
| HoverBackground | #EFEFEF |
| AccentFlow / AccentMindMap / AccentOrgChart | #0D6EFD / #7C3AED / #0F9D58 |
| ErrorColor | #D93025 |

Font: Segoe UI (OpenSans fallback on non-Windows platforms, via a font-family mechanism —
not a hardcoded literal per style). Type scale: Title 20 Sb / Section 14 Sb /
Body 13 / Caption 11. Spacing: 8 px base grid; panel padding 12; menu item height 32;
menu bar 40; toolbar 48; zoom chips pill 28 px. All metrics come from `AppConstants`.

## 10. Polish & accessibility

- `AutomationId` on every interactive element (`toolbar_undo`, `stencil_search`, …).
- Tooltips on all toolbar/menu items; minimum hit target 36 px; focus visuals on inputs.
- 12 px corner radius, 1 px borders, no drop shadows except popups (white, 8 px radius,
  1 px #DADADA border, drop shadow).
- Hover background #EFEFEF; disabled items 40 % opacity.
