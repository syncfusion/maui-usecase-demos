# Diagram Builder — Task Breakdown (tasks.md)

Execution rules for the implementing agent:
- Execute tasks sequentially; verify each task's acceptance criteria before starting the next.
- Each task is self-contained: it lists objective, implementation requirements, impacted files,
  acceptance criteria, and validation steps.
- Never commit to the protected base branch; work on the assigned feature branch.
- No TODO comments, no hardcoded user-visible strings, no literal colors in XAML
  (use `ResourceKeys` + `DynamicResource`) from Task 1 onward.
- Requirements trace to FR-n in `spec.md` and to `ui-spec.md` sections; do not reduce scope
  without recording the decision in the task notes.

---

## Task 1 — Project foundation: packages, DI, theme shell, constants

**Objective** — Establish a buildable foundation: NuGet packages, Syncfusion initialization,
DI registrations, theme ResourceDictionaries, centralized constants, and Shell routes.

**Implementation requirements**
1. Add packages to `DiagramBuilder.csproj` with **pinned versions** (no floating `Version="*"`):
   `CommunityToolkit.Mvvm`, `Syncfusion.Maui.Diagram`, `Syncfusion.Maui.Core`,
   `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Inputs`, `Syncfusion.Maui.Expander`,
   `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.TextInputLayout` (versions compatible with
   the project TFM).
2. `MauiProgram.cs`: call `ConfigureSyncfusionCore()` before DI; register all services and
   ViewModels and pages per architecture.md §3 (register interfaces now even if
   implementations arrive in Task 3 — until then register only what exists so the build
   stays green).
3. Create `Resources/Styles/Colors.xaml` with the full theme palette from ui-spec.md §9
   (every key named in `ResourceKeys`, incl. `ThumbnailNodeFill/Stroke/WireColor`,
   `ModalScrim`, `CategoryRailBackground` used by the create dialog).
4. Extend `Resources/Styles/Styles.xaml` with base control styles: `ToolbarIconButtonStyle`,
   `PropertyEntryStyle`, `PropertySectionHeaderStyle`, `ZoomChipStyle`/`ZoomChipActiveStyle`,
   `MenuFlyoutItemStyle` (incl. hover), `FlyoutPopupStyle` (shadowed white popup), panel
   chrome styles.
5. Create `Constants/AppConstants.cs` (dimensions: panel widths, toolbar height 48, menu bar
   height 40, menu item 32, zoom levels 50/100/150/200, grid size 20, corner radius 12,
   route names), `Constants/AppStrings.cs` (every user-visible string, incl. all
   menu/toolbar/stencil/property labels and the empty-state message), `Constants/ResourceKeys.cs`
   (color/style keys). Only declare constants that Tasks 5–9 will consume.
6. `App.xaml` merges `Colors.xaml` and `Styles.xaml` (two dictionaries only). `AppShell.xaml`
   registers routes: `create-diagram` (modal) and `designer`; set shell background/title
   from constants. Route strings come from `AppConstants` — no magic strings.
7. If SourceGen XAML inflation cannot resolve App-level cross-dictionary resources, apply
   the documented per-file `Runtime` inflation opt-out to affected files only, with a csproj
   comment explaining why, and keep the list minimal.

**Impacted files** — `DiagramBuilder.csproj`, `MauiProgram.cs`, `App.xaml(.cs)`,
`AppShell.xaml(.cs)`, `Constants/*`, `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`.

**Acceptance criteria**
- Project builds with zero errors/warnings-as-errors for at least one TFM.
- All theme keys exist and no hex literals appear outside `Colors.xaml`.
- DI registrations compile and resolve for every registered type.
- All package versions pinned.

**Validation steps**
- `dotnet build` (or `dotnet build -f <available Tfm>`); confirm exit 0.
- Grep XAML for `#`-prefixed colors → only `Colors.xaml` may match.
- Launch app shell (or at minimum verify XAML compiles and routes resolve).

---

## Task 2 — Domain models & enums

**Objective** — Define the complete domain model layer with no UI dependencies.

**Implementation requirements**
1. Enums: `DiagramCategory` (FlowChart, MindMap, OrganizationChart), `ShapeType` (Process,
   Decision, Terminator, Document, DataPreparation, Rectangle, Circle, Triangle, Hexagon,
   Diamond, Pentagon, Star), `ConnectorKind` (Straight, Orthogonal, Bezier),
   `PropertyGroup` (Dimensions, Insert, Background, Fill, Border, Text),
   `ExportFormat` (Png, Jpeg, Pdf, Svg).
2. `DiagramItem` base (Id, Text, Position X/Y, Width, Height, Rotation, LockAspectRatio) →
   `ShapeItem` (ShapeType, fill/border/text style fields) and `ConnectorItem`
   (source/target ids or points, ConnectorKind, style fields).
3. `DiagramDocument` (Id, Title, Category, Items, Connectors, IsDirty, CreatedUtc).
4. Template DTOs: `DiagramTemplate` (Id, Category, Title, Description, IsBlank, AccentColorKey,
   ThumbnailKey, Nodes, Connectors), `TemplateNode`, `TemplateConnector` (JSON-mappable).
5. UI metadata models: `StencilGroup`, `StencilItem` (ShapeType/ConnectorKind, DisplayName,
   SearchTags), `PropertyModel` (group, key, label key, value, min/max), `MenuCommandItem`
   (Id, TextKey, GestureKey, Children, IsSeparator, IsCheckable, IsChecked), `ToolbarItem`
   (Id, IconKey, TooltipKey, Group, IsToggle, IsChecked, IsEnabled), `ZoomLevel`
   (Value, LabelKey).
6. All collections `ObservableCollection<T>`; models are plain POCOs (INPC not required in
   models — ViewModels own change notification).

**Impacted files** — `Models/*.cs` (all files listed above).

**Acceptance criteria** — All models compile; JSON DTOs round-trip a sample payload with
`System.Text.Json`; no references to Views/ViewModels/controls from Models.

**Validation steps** — Build; grep `Models/` for `Syncfusion`, `ViewModels`, `Views` → no matches.

---

## Task 3 — Service interfaces & mock implementations

**Objective** — Implement the application service layer: interfaces plus fully working
mock implementations (in-memory, no backend).

**Implementation requirements**
1. `IDiagramService`/`DiagramService`: holds the current `DiagramDocument`; methods
   `CreateDocumentAsync(DiagramTemplate)`, `SaveAsync()`, `SaveAsAsync(title)`,
   `GetRecentDocumentsAsync()` (mock list), `SetDirty()`, `CurrentDocument` property,
   status `event EventHandler<string> StatusChanged`.
2. `ITemplateService`/`TemplateService`: `PreloadAsync()` loads the three JSON template files
   from `Resources/Raw/Templates` via `FileSystem.OpenAppPackageFileAsync`; builds the six
   `DiagramTemplate`s (3 blank + 3 predefined); **C# fallback seed** built in code used when
   JSON parsing fails so the UI is never empty; `GetAllAsync()`, `GetByIdAsync(id)`.
3. `IExportService`/`MockExportService`: `ExportAsync(ExportFormat, region, fileName)` —
   simulates processing (small delay), returns a result record (success, file name, data-URI
   or null for PDF).
4. `IPropertyService`/`PropertyService`: bridge between selection and live diagram models —
   `Apply(elementId, PropertyModel)` writes to the tracked Syncfusion `Node`/`Connector`
   objects via `DiagramModelAdapter`; `Read(elementId)` snapshot for the panel (a **node
   branch and a connector branch**); `Track(node)/Track(connector)` registration so every
   canvas element (template-loaded **and** palette-dropped) is editable. Applied values use
   `double` for all numeric styling (opacity, thickness); the UI layer converts.
5. `ISelectionService`/`SelectionService`: current selection sets (nodes/connectors),
   `ApplySelection(...)`, `SelectionSummary` text, change event.
6. `IDialogService`/`DialogService`: view-agnostic alert/confirm wrappers used by VMs
   (incl. an unsaved-changes confirm used before New/Open).
7. `DiagramModelAdapter`: pure mapper domain ↔ `Syncfusion.Maui.Diagram.Node`/`Connector`/
   `ShapeAnnotation`/`PathAnnotation`/styles/ports; must set explicit `Node.Id` strings for
   template nodes so connector `SourceID`/`TargetID` resolve; maps `ShapeType` →
   node shapes (Polygon via `Sides`, Decision→Diamond, Terminator→rounded, etc.) per
   architecture.md §7.3; also maps `StencilGroup`s to `SymbolPalette` palettes (connectors
   group included or explicitly omitted — never an empty palette).
8. `StencilCatalog`, `MenuCatalog`, `ToolbarCatalog`, `RecentDocumentCatalog` static
   in-code catalogs (mock data) built from constants only.

**Impacted files** — `Services/*.cs`, plus `Services/Catalogs.cs` (or per-catalog files).

**Acceptance criteria** — All services compile; no service references Views/Controls except
`PropertyService`/`DiagramModelAdapter` which reference only `Syncfusion.Maui.Diagram` model
types (never `SfDiagram` the control); catalogs contain the exact stencil/menu/toolbar data
from spec.md §6–7 and FR-6.

**Validation steps** — Build; write a scratch check (or review) that
`DiagramModelAdapter.Map(StencilCatalog.All)` yields valid Syncfusion `Node`s; grep services
for `SfDiagram` type usage → none.

---

## Task 4 — Template mock data (JSON + seeds)

**Objective** — Author the three predefined template payloads and blank-template seeds.

**Implementation requirements**
1. `Resources/Raw/Templates/flow-credit-card-processing.json`: 11-node payment flow (mirrors
   the control's GettingStarted sample): Start → Enter Card → Validate (decision with Yes/No
   ports) → declined path → Process Payment → Store → Receipt → End; orthogonal connectors,
   labeled branches, explicit node ids.
2. `mindmap-product-planning.json`: central "Product Planning" ellipse + 5 branches
   (Research, Design, Development, Marketing, Launch), each 2–3 children, balanced manual
   positions, curved connectors, per-branch accent colors.
3. `orgchart-company-hierarchy.json`: CEO → 3 directors → managers/teams; rounded rectangles;
   manager nodes PrimaryColor fill; layout hint `OrganizationalChart`/`TopToBottom`.
4. Blank-template seeds emitted in code (single starter node "Double-click to rename" +
   category default connector tool).
5. JSON schema matches `TemplateNode`/`TemplateConnector` DTOs exactly; add JSON files to
   the csproj as `MauiAsset`. Fallback C# seeds must describe the same diagrams as the JSON
   payloads (same node counts/structure) so behavior does not differ by packaging success.

**Impacted files** — `Resources/Raw/Templates/*.json`, `DiagramBuilder.csproj`,
`Services/TemplateService.cs` (fallback seeds reference).

**Acceptance criteria** — All three JSON files parse against the DTOs; every connector
`SourceID`/`TargetID` references an existing node id; every template yields ≥1 node.

**Validation steps** — Deserialize each file in a scratch console check or unit-style review;
validate id references programmatically; confirm files are packaged (build output contains them).

---

## Task 5 — ViewModels (all, CommunityToolkit.Mvvm)

**Objective** — Implement the full ViewModel layer with strict MVVM, all ≤ 500 lines.

**Implementation requirements**
1. `MainViewModel`: app title, launch flow (`StartCommand` → open `create-diagram`), status
   message relay from `IDiagramService.StatusChanged`.
2. `CreateDiagramViewModel`: `Categories` (3 cards), `SelectedCategory`, `Templates` for the
   category (from `ITemplateService`), `CreateDiagramCommand(DiagramTemplate)` → create
   document → navigate `//designer`.
3. `DiagramDesignerViewModel` (orchestrator, ≤ 500 lines — delegate): owns
   `MenuBarViewModel`, `ToolbarViewModel`, `StencilViewModel`, `PropertyPanelViewModel`,
   `ZoomViewModel`, `ArrangeViewModel`, `ExportViewModel` child VMs; central command
   dispatch table (`ExecuteCommand(commandId)`) handling every menu/toolbar/keyboard id from
   spec.md §6–7; document lifecycle (New/Open/Save/SaveAs/Export/Print); receives selection/
   geometry events from the behavior. The dispatch table must:
   - call the real control APIs through the behavior relays for natively supported
     operations: undo/redo (+ `CanUndo/CanRedo` → command `CanExecute`), delete,
     cut/copy/paste, select all, zoom in/out/fit (via `ZoomViewModel` state), and update
     `ZoomViewModel.CurrentZoom` from the behavior's reported zoom factor;
   - set the document dirty flag on geometry/collection/annotation/property changes;
   - emit an explicit status message only for commands the control genuinely cannot
     perform (mock commands);
   - never throw for unknown ids — log/degrade to a status message.
4. `MenuBarViewModel`: builds the five `MenuCommandItem` roots from `MenuCatalog`; exposes
   `MenuCommand` executed by MenuBarControl; **checkable item state is live** (binds to
   `ZoomViewModel.ShowGrid/SnapToGrid` and the designer VM panel observables), not a
   build-time snapshot.
5. `ToolbarViewModel`: 8 groups of `ToolbarItem`; active-tool state (pan/selection/connector/
   shape/text); group 6 visibility bound to selection state; fill/border color commands that
   **apply the picked color to the selection** via the property service, and swatch state
   reflecting the current selection's colors.
6. `StencilViewModel`: groups + `FilterText` search (filters by DisplayName/SearchTags),
   collapse state, no-results flag; connector selection sets an active connector tool that
   the canvas/toolbar flow consumes.
7. `PropertyPanelViewModel`: six `PropertyGroup` sections built via `IPropertyService.Read`;
   `Apply` writes via `IPropertyService` (sets dirty); empty-state flag; auto-show on
   selection; works for single-node and single-connector selections.
8. `ZoomViewModel`: `ZoomLevel` collection, `CurrentZoom` (the single source of truth that
   the behavior relay binds to), In/Out/Fit commands (Fit routes to `RequestFitToPage`),
   ShowGrid and SnapToGrid toggles (consumed by the workspace).
9. `ArrangeViewModel`: align/distribute/match-size/z-order/group commands routed to the
   dispatch table (compute on selected nodes where feasible; mock status where the control
   has no native API).
10. `ExportViewModel`: format selection, region, file name, `ExportCommand` →
    `IExportService`; result state for the dialog; dialog close/reset semantics.
11. Every command `[RelayCommand]` (with `CanExecute` where state-dependent); every bound
    field `[ObservableProperty]`; constructor injection only.

**Impacted files** — `ViewModels/*.cs` (all listed above).

**Acceptance criteria** — Build passes; every ViewModel ≤ 500 lines; no code-behind coupling;
all user-visible strings sourced from `AppStrings`; dispatch table routes undo/redo/delete/
clipboard/select-all/zoom to real control relays (no status-only stubs for natively
supported operations).

**Validation steps** — `wc -l ViewModels/*.cs` (all ≤ 500); build; grep for `TODO` → none;
grep XAML/VM for hardcoded menu labels → none; review dispatch table against spec.md §6–7
id-by-id.

---

## Task 6 — Converters & behaviors (control↔VM bridge)

**Objective** — Create the reusable converters and the two behaviors that keep MVVM intact
around `SfDiagram`.

**Implementation requirements**
1. Converters: `BoolToObjectConverter`, `ZoomToPercentConverter`, `ColorToBrushConverter`,
   `EnumToDisplayConverter`, `NullToBoolConverter`, `CountToBoolConverter`,
   `SelectionKindToVisibilityConverter`. All parameterized, stateless, registered in XAML
   resources, and **consumed by the views** (do not register converters that no XAML uses).
2. `DiagramSurfaceBehavior` (`Behaviors/`): attaches to `SfDiagram`; bindable `ICommand`s:
   `ReadyCommand`, `SelectionChangedCommand`, `GeometryChangedCommand`,
   `AnnotationEditedCommand`, `CollectionChangedCommand`; bindable command relays VM→control:
   `RequestFitToPage`, `RequestUndo`, `RequestRedo`, plus **zoom/grid/snap state passthrough**
   (bindable `ZoomFactor`, `ShowGrid`, `SnapToGrid` properties that call the control APIs on
   change and report the control's actual zoom factor back through a bindable event/command);
   one-way VM→control via awaiting the control's async methods; subscribes/unsubscribes on
   attach/detach. Expose `CanUndoAsync/CanRedoAsync` state for command `CanExecute`.
3. `DiagramKeyboardBehavior`: attaches to the designer page content; handles
   Ctrl+C/V/X/Z/Y/Delete/Ctrl+A and routes each to a bound `ICommand` with a gesture id.
   Platform key events require the Windows platform hook (architecture.md §7.2); other
   platforms get the no-op default partial (commands remain reachable via menus/toolbar).

**Impacted files** — `Converters/*.cs`, `Behaviors/DiagramSurfaceBehavior.cs`,
`Behaviors/DiagramKeyboardBehavior.cs`.

**Acceptance criteria** — Behaviors compile and contain **zero business logic** (only
event→command relaying and control API calls); converters unit-reviewable pure functions;
the behavior's zoom/undo/redo relays are bindable and ready for Task 9 wiring.

**Validation steps** — Build; review behavior files for any direct ViewModel type references
(must use `ICommand` bindables only).

---

## Task 7 — Reusable controls & data templates

**Objective** — Build the reusable designer controls and shared DataTemplates.

**Implementation requirements**
1. `MenuBarControl`: ItemsSource-driven menu bar; flyout rendering of `MenuCommandItem`
   trees (separators, check glyphs, shortcut text, **cascading submenus**); `MenuCommand`
   bindable; flyout chrome per ui-spec.md §2 (mixed-case roots, hover highlight, shadowed
   popup, transparent overlay). Check glyphs reflect live state at every open.
2. `ToolbarControl`: 8 groups with separators; shape/connector flyouts as **48 px glyph tile
   grids** with labels; color swatch buttons opening `SfColorPicker` popups that apply the
   picked color; zoom chips with active state; group-6 visibility binding; state changes
   (active tool, selection, zoom) update the rendered toolbar reactively.
3. `StencilPanelControl`: header (title, chevron, search entry), `SfAccordion`/`SfExpander`
   sections with 64×64 borderless glyph tiles + names, hover highlight, no-results state;
   search box bound to `StencilViewModel.FilterText`; hosts/feeds the shared `SymbolPalette`
   **as a direct Grid sibling of the workspace's `SfDiagram`** (architecture.md §4.3 —
   coordinate with Task 8's workspace layout; exactly one visible palette surface).
4. `DiagramWorkspaceControl`: hosts `SfDiagram` + sibling `SymbolPalette` in one Grid;
   attaches `DiagramSurfaceBehavior`; grid/snap/zoom state bindings (the behavior relays
   from Task 6); status bar strip.
5. `PropertyPanelControl`: six sections rendered via the `PropertyEditorTemplates`
   DataTemplates per `PropertyGroup` (not imperative code-behind row construction);
   `SfNumericEntry` fields (numeric rows only — Lock Aspect Ratio is a checkbox),
   `ColorPickerButton` swatches (all wired to `ColorChanged`), segmented line-style buttons
   and alignment selectors (wired), text toggles (wired), opacity sliders (typed to match
   the service layer); empty-state view with ghost icon.
6. `ColorPickerButton` (swatch → `SfColorPicker` popup, returns Color via a bindable
   `ColorChanged` event/command), `PanelHeaderControl` (title + collapse chevron),
   `ZoomIndicatorControl` (zoom % + snap/grid indicator chips, pill-shaped).
7. `Templates/`: `StencilItemTemplate.xaml`, `PropertyEditorTemplates.xaml`,
   `TemplateCardTemplate.xaml` DataTemplates — used by all list surfaces (App.xaml merges
   them and the consuming controls select them).
8. Control code-behind contains only event→command relays; all styling via theme keys;
   controls subscribe to VM notifications symmetrically (unsubscribe old VM before new).

**Impacted files** — `Controls/*.xaml(.cs)`, `Templates/*.xaml`.

**Acceptance criteria** — Each control binds exclusively to bindable properties/commands;
no hardcoded strings/colors; palette sibling rule respected in the workspace layout; every
rendered interactive element is functional (no decorative-only buttons/segments/sliders);
control code-behind files ≤ 500 lines.

**Validation steps** — Build; XAML review against ui-spec.md §2–6; grep controls for hex
colors and literal labels → none.

---

## Task 8 — Designer page & create-diagram page assembly

**Objective** — Assemble the two pages: the 3-column designer workspace and the create
dialog, wired to their ViewModels.

**Implementation requirements**
1. `DiagramDesignerPage.xaml`: Grid rows (title, menu, toolbar, content, status); columns
   280 / * / 320 (from `AppConstants`); hosts MenuBarControl, ToolbarControl,
   StencilPanelControl, DiagramWorkspaceControl, PropertyPanelControl; collapsible panels
   driven by designer VM observables; `DiagramKeyboardBehavior` attached; minimal code-behind
   (binding-context resolution + event→command relays only).
2. **Bind the behavior relays**: `Workspace.SurfaceBehaviorZoomFactor` ↔
   `Zoom.CurrentZoom`, `RequestFitToPage`/`RequestUndo`/`RequestRedo` ↔ designer VM commands,
   `ShowGrid`/`SnapToGrid` ↔ `ZoomViewModel` toggles. Prefer XAML bindings; keep any
   code-behind wiring symmetric (subscribe on `OnAppearing`, unsubscribe on `OnDisappearing`
   — and resubscribe on return).
3. `CreateDiagramPage.xaml`: category cards + template cards (using `TemplateCardTemplate`),
   Create button; modal styling per ui-spec.md §8 (selected-category state, thumbnail
   wireframe theme keys). Pick **one** navigation mechanism for the create dialog (Shell
   route or `PushModalAsync`) and keep open/close symmetric — FILE ▸ New must round-trip.
4. `Views/Dialogs/ExportDialog.xaml(.cs)`: export options popup (`SfPopup`-based) bound to
   `ExportViewModel` — format/region selected states, "Export" primary action that runs the
   export and closes, Cancel discards.
5. Register pages in DI; ensure navigation strings come from `AppConstants` route constants.

**Impacted files** — `Views/*.xaml(.cs)`, `Views/Dialogs/*`, `MauiProgram.cs`, `AppShell.xaml`.

**Acceptance criteria** — Both pages render with all regions visible and populated from
mock data; panel collapse toggles work; navigation launch → create → designer works
end-to-end with any of the 6 templates, and FILE ▸ New → create → Create round-trips;
zoom toolbar buttons visibly zoom the canvas.

**Validation steps** — Run the app (Windows TFM when available, otherwise review + build);
walk J1 flow; verify group 6 appears only with a node selected; verify empty state of
property panel; verify zoom in/out/fit change the canvas.

---

## Task 9 — Command wiring & interaction pass

**Objective** — Make every command functional end-to-end (real where the control supports
it, mock status elsewhere).

**Implementation requirements**
1. Wire dispatch table in `DiagramDesignerViewModel`: FILE (New→create dialog with
   unsaved-changes confirm, Open→mock recents, Save/SaveAs→document store, Export→dialog,
   Print→`Print` w/ status), EDIT (Undo/Redo→`Undo/Redo` relays + CanUndo/CanRedo state;
   Delete→real removal; Cut/Copy/Paste→control clipboard APIs; Duplicate/Select All→real
   where supported, mock status otherwise), VIEW (zoom commands drive the canvas, grid/snap
   toggles drive the diagram), ARRANGE (z-order/align/distribute/match-size/group —
   compute on selected nodes where feasible, mock status where the control lacks a native
   API), WINDOW (panel toggles).
2. Toolbar commands mirror the same dispatch table (no duplicate logic); color swatches
   apply the picked color to the selection; zoom chips apply zoom; tool toggles set the
   active tool.
3. Property panel edits flow through `IPropertyService` to live nodes/connectors (verify
   live canvas update via dirty-flush); newly dropped palette elements are tracked and
   editable; edits set the dirty flag.
4. Connector tool flow: selecting a connector entry (stencil or toolbar flyout) arms the
   connector draw mode; clicking/tapping two nodes creates the connector (control's native
   interactive connect, or a manual flow via `CollectionChanged`-tracked nodes).
5. Export dialog demo: PNG/JPEG/SVG call `ExportDiagram` for a data URI preview; PDF
   mock-only.
6. Template load on `Created` (JS instance ready) (behavior gates population).

**Impacted files** — `ViewModels/DiagramDesignerViewModel.cs` + child VMs,
`Behaviors/DiagramSurfaceBehavior.cs`, `Services/PropertyService.cs`, `Views/DiagramDesignerPage.xaml(.cs)`.

**Acceptance criteria** — Every menu item, toolbar button, and keyboard shortcut executes
without crash and produces the specified effect or an explicit status message; zoom,
undo/redo, delete, select-all, and clipboard verifiably act on the canvas; property edits
update the canvas live for template-loaded and palette-dropped elements.

**Validation steps** — Interactive walkthrough of FR-4…FR-12; confirm undo/redo round-trip,
selection-driven panel updates, zoom chips update indicator **and** canvas, palette-dropped
node is property-editable.

---

## Task 10 — Final verification & polish

**Objective** — Verify the harness against every constraint and the design mockups.

**Implementation requirements**
1. Full build for every available TFM; fix all warnings-as-errors.
2. Constraint audit: ViewModel and code-behind line counts ≤ 500; no TODO comments; no
   business logic in code-behind; no hardcoded strings/colors outside constants/theme;
   AutomationIds present on all interactive elements; no dead code (unused converters,
   orphaned constants, unreferenced DataTemplates, unbound behaviors APIs).
3. Visual comparison against `design-images/*.png` (menus, toolbar flyouts, property panel,
   full layouts); adjust spacing/colors where deviating — key checkpoints: menu root
   typography, cascading ARRANGE submenu, 48 px flyout glyph tiles, active zoom chip, dot
   grid, property panel section detail, thumbnail wireframes, category selected state.
4. Confirm sample data on every screen (templates, stencil, mock recents, status bar).
5. Record final completion status in the task notes of this file.

**Impacted files** — Any file failing the audit; `agent/tasks.md` (status notes).

**Acceptance criteria** — All checks in validation.md pass; build clean; visual parity with
mockups for all major surfaces; dead-code sweep complete.

**Validation steps** — Execute validation.md checklist item by item; record results.
