# Diagram Builder — Functional Specification

## F1. Diagram creation experience (`CreateDiagramPage`)

- Displayed at launch as a modal route (`create-diagram`). App shell title: "Diagram Builder".
- Three category cards (Flow Chart, Mind Map, Organization Chart) with accent color, icon and
  description; selecting a category reveals its template list.
- Template list per category:
  - Flow Chart → *Blank Flow Chart*, *Credit Card Processing*
  - Mind Map → *Blank Mind Map*, *Product Planning*
  - Organization Chart → *Blank Org Chart*, *Company Hierarchy*
- Each card: thumbnail swatch, title, description, node count. "Create" button per card.
- Command: `CreateDiagramViewModel.CreateDiagramCommand(DiagramTemplate)` →
  `ITemplateService.GetTemplateAsync(id)` → `IDiagramService.CreateDocumentAsync(template)` →
  navigate to `//designer`.

## F2. Designer workspace (`DiagramDesignerPage`)

Three-column desktop layout:

| Region | Control | Collapsible |
|---|---|---|
| Top | MenuBarControl + ToolbarControl | Toolbar only (Window ▸ Show Toolbar) |
| Left | StencilPanelControl (accordion + palette + search) | Yes |
| Center | DiagramWorkspaceControl (SfDiagram) | — |
| Right | PropertyPanelControl (accordion sections) | Yes (auto-shows on selection) |

### F2.1 Menu commands
- **FILE** — New (opens create dialog), Open (mock recent documents), Save, Save As, Export
  (opens ExportDialog), Print.
- **EDIT** — Undo, Redo, Cut, Copy, Paste, Delete, Duplicate, Select All.
- **VIEW** — Zoom In, Zoom Out, Fit To Screen, Show Grid (toggle), Snap To Grid (toggle).
- **ARRANGE** — Send To Back, Bring To Front, Send Backward, Bring Forward;
  Align (Left/Right/Center/Top/Bottom/Middle); Distribute Horizontally/Vertically;
  Match Size (Same Width / Same Height / Same Width & Height); Group, Ungroup.
- **WINDOW** — Show Toolbar, Show Stencil, Show Properties (all toggles with check state).

Group/submenu items are rendered with separators exactly as in `design-images/*_section.png`.

### F2.2 Toolbar groups (top to bottom order per mockup)
1. Undo, Redo
2. Pan tool (hand), Selection tool (arrow) — mutually exclusive toggles
3. Basic shapes flyout: Rectangle, Ellipse, Polygon (click-then-place on canvas)
4. Connector flyout: Straight, Orthogonal, Bezier
5. Text tool
6. *Selection-only group* — Fill color, Border color (swatch buttons with popup pickers);
   arrange: Send To Back / Bring To Front / Send Backward / Bring Forward; Lock; Delete
7. Zoom in, Zoom out, Fit to screen; zoom value chips 50 % / 100 % / 150 % / 200 %
8. Toggle property panel

Group 6 is visible only when exactly one node is selected (`SelectionKindToVisibilityConverter`).

### F2.3 Stencil panel (left)
- `SfAccordion`-style collapsible categories: **FLOW SHAPES** (Process, Decision, Terminator,
  Document, Data Preparation), **BASIC SHAPES** (Rectangle, Circle, Triangle, Hexagon, Diamond,
  Pentagon, Star), **CONNECTORS** (Straight, Orthogonal, Bezier).
- Search box filters shapes by name/tag (`StencilViewModel.FilterText`).
- Shapes are provided to the shared `SymbolPalette` as `DiagramPaletteGroup`s so drag-and-drop
  to the canvas works natively (same-document palette mechanism).
- Connectors group entries set the active connector tool on click (not draggable).

### F2.4 Canvas (center)
- `SfDiagram` fills the workspace; grid & snap follow `ZoomViewModel.ShowGrid/SnapToGrid`.
- Operations supported by the control: add (palette drop / tool placement), move, resize,
  rotate, connect (interactive connector draw), delete; rubber-band multi-select; undo/redo.
- Double-click a node → inline annotation edit (native `AnnotationEdited` event).

### F2.5 Property panel (right) — sections
- **DIMENSIONS**: Position X/Y, Width, Height (SfNumericEntry), Rotation Angle,
  Lock Aspect Ratio (checkbox).
- **INSERT**: Insert Link, Insert Image (mock commands).
- **BACKGROUND**: Solid / Gradient type chooser + color.
- **FILL**: color picker + opacity slider.
- **BORDER**: line style (Solid/Dashed/Dotted), color, thickness, opacity.
- **TEXT**: content, font family (Segoe UI default), size, Bold/Italic/Underline,
  text color, horizontal + vertical alignment.
- Panel shows an empty-state view ("Select an object to edit") when nothing is selected.

### F2.6 Keyboard shortcuts
Ctrl+C / Ctrl+X / Ctrl+V / Delete / Ctrl+Z / Ctrl+Y / Ctrl+A handled by
`DiagramKeyboardBehavior` → dispatch into `DiagramDesignerViewModel` command map.

### F2.7 Zoom
Zoom levels 50 %, 100 %, 150 %, 200 %, plus incremental in/out and Fit To Screen
(`SfDiagram.FitToPage`). Current zoom shown in ZoomIndicatorControl.

## F3. Export (`ExportDialog`)

- Format chooser: PNG, JPEG, PDF, SVG. Region: Content / Page settings. File name field.
- `MockExportService.ExportAsync` simulates processing and returns success status with the
  would-be file name; PNG/JPEG/SVG additionally demo a real `ExportDiagram` data URI when
  the diagram is ready (displayed in dialog as confirmation). PDF is mock-only.

## F4. Acceptance criteria
- Builds and runs from a clean clone with zero missing references.
- Every screen has sample data at first run (templates load immediately).
- All commands execute without crash (mocked where the platform lacks support).
- All ViewModels < 500 lines; no business logic in code-behind.
