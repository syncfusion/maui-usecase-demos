# Diagram Builder — Product Specification (spec.md)

## 1. Product Summary

**Diagram Builder** is a production-quality .NET MAUI use case sample that showcases the Syncfusion .NET MAUI **Diagram** component (`SfDiagram` + `SymbolPalette`) through a professional desktop diagram designer. Users create, edit, arrange, and export three diagram categories:

| Category | Blank diagram | Predefined template |
|---|---|---|
| Flow Chart | Blank Flow Chart | Credit Card Processing |
| Mind Map | Blank Mind Map | Product Planning |
| Organization Chart | Blank Org Chart | Company Hierarchy |

## 2. Scope

### In scope
- Diagram creation dialog (category → blank or predefined template).
- Full designer workspace: stencil panel (left), diagram canvas (center), property inspector (right).
- Top menu bar (FILE / EDIT / VIEW / ARRANGE / WINDOW) and 8-group top toolbar.
- Zoom / pan / fit-to-screen; grid & snap toggles.
- Stencil library with search, collapsible categories, and native palette drag-and-drop.
- Property inspector: Dimensions, Insert, Background, Fill, Border, Text.
- Arrange operations: z-order, align, distribute, match size, group/ungroup.
- Export UI (PNG / JPEG / PDF / SVG) backed by mock services.
- Light theme, desktop-first (Windows primary, MacCatalyst secondary), collapsible panels.

### Out of scope
- Backend, database, web services, authentication, AI services.
- Real filesystem persistence ("Save"/"Open" operate on the in-memory mock document store).
- Cloud / telemetry features.

## 3. User Journeys

### J1 — First launch
1. App starts → Create Diagram dialog appears (Shell modal route `create-diagram`).
2. User picks a category card (Flow Chart / Mind Map / Organization Chart).
3. Category reveals its template list (Blank + predefined) as cards with thumbnail, title, description, node count.
4. User taps **Create** → app navigates to the Diagram Designer workspace with the chosen template loaded.

### J2 — Editing a diagram
1. Drag a shape from the stencil palette onto the canvas (native same-document drag-and-drop).
2. Select a node → property panel populates (position, size, rotation, fill, border, text).
3. Edit values in the property panel → live canvas update via the control's coalesced flush.
4. Double-click a node → inline annotation edit.
5. Draw connectors interactively or via toolbar connector menu.
6. Undo/redo, cut/copy/paste/delete/duplicate/select-all via Edit menu and keyboard shortcuts.

### J3 — Arranging
Z-order (send to back/front, backward/forward), align (6 axes), distribute (H/V), match size (3 modes), group/ungroup — via the ARRANGE menu and the selection-only toolbar group.

### J4 — Viewing
Zoom in/out/fit, preset zoom chips (50/100/150/200 %), Show Grid and Snap To Grid toggles, panel show/hide toggles (WINDOW menu).

### J5 — Exporting
FILE ▸ Export → Export dialog: format (PNG/JPEG/PDF/SVG), region (Content/Page), file name → mock export returns a success status; PNG/JPEG/SVG additionally demo a real `ExportDiagramAsync` data URI.

## 4. Functional Requirements

| ID | Requirement |
|---|---|
| FR-1 | App launch shows the Create Diagram dialog; no other flow can bypass it for the first document. |
| FR-2 | Exactly 3 categories × 2 templates (blank + predefined) = 6 template cards available at first run, zero network. |
| FR-3 | Designer workspace = menu bar + toolbar + stencil panel + canvas + property panel + status bar. |
| FR-4 | All menu commands in §"Menu specification" exist, are reachable, and execute without crash (mocked where the platform lacks support). |
| FR-5 | Toolbar groups 1–8 exactly as specified; group 6 visible only when exactly one node is selected. |
| FR-6 | Stencil: FLOW SHAPES (Process, Decision, Terminator, Document, Data Preparation), BASIC SHAPES (Rectangle, Circle, Triangle, Hexagon, Diamond, Pentagon, Star), CONNECTORS (Straight, Orthogonal, Bezier); search filters shapes; categories collapsible. |
| FR-7 | Canvas supports add/move/resize/rotate/connect/delete, single + rubber-band multi-select, grid show/hide, snap to grid. |
| FR-8 | Property panel visible only for a selected object; six sections per spec; empty state when nothing selected. |
| FR-9 | Keyboard shortcuts: Ctrl+C, Ctrl+X, Ctrl+V, Delete, Ctrl+Z, Ctrl+Y, Ctrl+A. |
| FR-10 | Zoom presets 50/100/150/200 % plus incremental zoom and Fit To Screen. |
| FR-11 | Export dialog with PNG/JPEG/PDF/SVG; mock service returns status; PDF is mock-only. |
| FR-12 | Panels collapsible via Window menu and header chevrons. |

## 5. Non-functional Requirements

- **Architecture**: strict MVVM, `CommunityToolkit.Mvvm` only (`[ObservableProperty]`, `[RelayCommand]`), no ViewModel > 500 lines, no business logic in code-behind (code-behind only relays control events → commands).
- **SOLID**: all behavior behind interfaces (`IDiagramService`, `ITemplateService`, `IExportService`, `IPropertyService`, `ISelectionService`, `IDialogService`) with mock implementations registered in DI.
- **No hardcoded strings**: every user-visible string from `AppConstants`/`AppStrings`; colors only via `ResourceDictionary` keys.
- **Centralized constants**: `AppConstants`, `AppStrings`, `ResourceKeys`.
- **Reusable controls**: MenuBar, Toolbar, StencilPanel, Workspace, PropertyPanel, ColorPickerButton, PanelHeaderControl, ZoomIndicator.
- **No TODO comments**; no placeholder classes without implementation.
- **Builds and runs immediately** from a clean clone with zero missing references.
- **Sample data on every screen**: templates preloaded at startup with C# fallback seeds; stencil/menu/toolbar catalogs are in-code mock catalogs.

## 6. Menu Specification

- **FILE**: New, Open (mock recents), Save, Save As, — , Export, Print.
- **EDIT**: Undo, Redo, — , Cut, Copy, Paste, Delete, Duplicate, — , Select All.
- **VIEW**: Zoom In, Zoom Out, Fit To Screen, — , Show Grid ✓, Snap To Grid ✓.
- **ARRANGE**: Send To Back, Bring To Front, Send Backward, Bring Forward, — ,
  Align Objects ▸ (Left/Right/Center/Top/Bottom/Middle), Distribute Objects ▸ (Horizontally/Vertically),
  Match Size ▸ (Same Width / Same Height / Same Width & Height), — , Group, Ungroup.
- **WINDOW**: Show Toolbar ✓, Show Stencil ✓, Show Properties ✓.

## 7. Toolbar Specification (groups 1–8)

1. Undo, Redo
2. Pan tool (hand) / Selection tool (arrow) — mutually exclusive toggles
3. Basic shapes flyout: Rectangle, Ellipse, Polygon
4. Connector flyout: Straight, Orthogonal, Bezier
5. Text tool
6. *(selection-only)* Fill color, Border color, Send To Back, Bring To Front, Send Backward, Bring Forward, Lock, Delete
7. Zoom In, Zoom Out, Fit To Screen, zoom chips 50/100/150/200 %
8. Toggle property panel

## 8. Success Criteria

1. `dotnet build` succeeds for at least one target framework with zero errors.
2. All 6 templates render at first run; all designer regions have initialized state.
3. Every command executes without crash.
4. Line-count, MVVM, string, and theme constraints verified (see validation.md).
5. Visual output matches `design-images/*.png` mockups (menus, toolbar, property panel, layout).
