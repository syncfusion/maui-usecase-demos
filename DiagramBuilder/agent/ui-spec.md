# Diagram Builder — UI Specification (ui-spec.md)

Derived from `design-images/` mockups: `image_1..8.png` = full designer states;
`file/edit/view/arrange/window_section.png` = menu flyouts; `toolbar_shape.png`,
`toolbar_sline.png`, `toolbar_fill_color_picker.png` = toolbar flyouts; `property_panel.png`
= right panel.

## 1. Overall frame

- Desktop chrome, light theme, 8 px outer padding, 1 px `PanelBorder` separators.
- Title row: "Diagram Builder" + document name (left), status text (right).
- Row order: Title → Menu bar → Toolbar → 3-column content → status bar.
- Content columns: Stencil 280 px | Canvas (fill) | Property 320 px.
- Panels collapse to 0 width with animation; toggled by WINDOW menu or header chevrons.

## 2. Menu bar (`MenuBarControl`)

Horizontal items: FILE · EDIT · VIEW · ARRANGE · WINDOW. Click opens a flyout whose items
match `*_section.png` exactly, including separators:

| Menu | Items (— = separator) |
|---|---|
| FILE | New, Open, Save, Save As, —, Export, Print |
| EDIT | Undo, Redo, —, Cut, Copy, Paste, Delete, Duplicate, —, Select All |
| VIEW | Zoom In, Zoom Out, Fit To Screen, —, Show Grid ✓, Snap To Grid ✓ |
| ARRANGE | Send To Back, Bring To Front, Send Backward, Bring Forward, —, Align Objects ▸ (Left Right Center Top Bottom Middle), Distribute Objects ▸ (Horizontally Vertically), Match Size ▸ (Same Width Same Height Same Width & Height), —, Group, Ungroup |
| WINDOW | Show Toolbar ✓, Show Stencil ✓, Show Properties ✓ |

Checkable items show a check glyph; shortcuts right-aligned (e.g. "Ctrl+Z").

## 3. Toolbar (`ToolbarControl`)

Icon buttons 36×36, 4 px spacing, 1 px separators between groups 1–8:

1. Undo, Redo
2. Pan (hand) / Selection (arrow) — mutually exclusive; active tool highlighted with `SelectionBlue`
3. Basic shapes flyout (`toolbar_shape.png`): Rectangle, Ellipse, Polygon — large 48 px glyphs + labels; click-then-place on canvas
4. Connector flyout (`toolbar_sline.png`): Straight, Orthogonal, Bezier
5. Text tool
6. *(visible only when exactly one node is selected)* Fill color swatch, Border color swatch
   (`toolbar_fill_color_picker.png` → `SfColorPicker` popup), Send To Back, Bring To Front,
   Send Backward, Bring Forward, Lock, Delete
7. Zoom In, Zoom Out, Fit To Screen; zoom chips 50 % / 100 % / 150 % / 200 %
8. Toggle property panel

## 4. Stencil panel (left, `StencilPanelControl`)

- Header "Shapes" with collapse chevron and search entry (placeholder "Search shapes").
- Collapsible categories (accordion, multiple expanded):
  - **FLOW SHAPES**: Process, Decision, Terminator, Document, Data Preparation
  - **BASIC SHAPES**: Rectangle, Circle, Triangle, Hexagon, Diamond, Pentagon, Star
  - **CONNECTORS**: Straight, Orthogonal, Bezier
- Shape tiles: 64×64 glyph preview + name beneath; hover highlight; search filters by name/tag.
- Connector entries set the active connector tool on click (not draggable).
- Shapes are fed to the shared `SymbolPalette` as `DiagramPaletteGroup`s so drag-and-drop
  onto the canvas is native (same-document mechanism).

## 5. Canvas (center, `DiagramWorkspaceControl`)

- White canvas over `WindowBackground` workspace; dot grid at 20 px when Show Grid enabled.
- SfDiagram fills the region; supports add/move/resize/rotate/connect/delete,
  single-select, rubber-band multi-select, undo/redo, inline annotation edit on double-click.
- Status bar (bottom): selection summary, zoom %, snap/grid indicators.

## 6. Property panel (right, `PropertyPanelControl` — matches `property_panel.png`)

Section headers uppercase, 11 px letter-spaced; two-column label/field rows.

| Section | Fields |
|---|---|
| DIMENSIONS | Position X, Position Y, Width, Height (`SfNumericEntry`), Rotation Angle, Lock Aspect Ratio (checkbox) |
| INSERT | Insert Link, Insert Image (mock commands) |
| BACKGROUND | Solid / Gradient chooser, color swatch |
| FILL | color picker, opacity slider |
| BORDER | line style segmented (Solid/Dashed/Dotted), color, thickness, opacity |
| TEXT | content entry, font family (Segoe UI), size, Bold/Italic/Underline toggles, text color, horizontal + vertical alignment |

Empty state: centered ghost icon + "Select an object to edit its properties".
Panel auto-shows when a node is selected (unless manually hidden via WINDOW).

## 7. Export dialog (`ExportDialog`)

- Format chooser: PNG, JPEG, PDF, SVG; Region: Content / Page Settings; File name entry
  pre-filled from document title.
- Mock export returns success status with would-be file name; PNG/JPEG/SVG demo a real
  `ExportDiagram` data URI when the diagram is ready. PDF is mock-only.

## 8. Create Diagram dialog (`CreateDiagramPage`)

- Three category cards (accent color, icon, description): Flow Chart `#0D6EFD`,
  Mind Map `#7C3AED`, Organization Chart `#0F9D58`.
- Selecting a category reveals its two template cards: thumbnail swatch, title, description,
  node count, Create button.

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

Font: Segoe UI (OpenSans fallback on MacCatalyst). Type scale: Title 20 Sb / Section 14 Sb /
Body 13 / Caption 11. Spacing: 8 px base grid; panel padding 12; menu item height 32;
menu bar 40; toolbar 48; zoom chips pill 28 px.

## 10. Polish & accessibility

- `AutomationId` on every interactive element (`toolbar_undo`, `stencil_search`, …).
- Tooltips on all toolbar/menu items; minimum hit target 36 px; focus visuals on inputs.
- 12 px corner radius, 1 px borders, no drop shadows except popups (white, 8 px radius,
  1 px #DADADA border).
- Hover background #EFEFEF; disabled items 40 % opacity.
