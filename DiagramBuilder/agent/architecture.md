# Diagram Builder — Architecture Specification (architecture.md)

## 1. Layering (clean architecture, single project)

```
┌───────────────────────────────────────────────────────────────┐
│ Views (XAML) + Controls + Behaviors + Templates + Converters   │  presentation
│   bind only to ViewModels; zero business logic                 │
├───────────────────────────────────────────────────────────────┤
│ ViewModels (CommunityToolkit.Mvvm)                             │  presentation logic
│   MainViewModel, CreateDiagramViewModel,                       │
│   DiagramDesignerViewModel (+ MenuBar/Toolbar/Stencil/         │
│   PropertyPanel/Zoom/Arrange/Export child VMs)                 │
├───────────────────────────────────────────────────────────────┤
│ Services (interfaces + mock implementations)                   │  application
│   IDiagramService, ITemplateService, IExportService,           │
│   IPropertyService, ISelectionService, IDialogService,        │
│   DiagramModelAdapter                                          │
├───────────────────────────────────────────────────────────────┤
│ Models + Constants + Resources (theme)                         │  domain / shared
└───────────────────────────────────────────────────────────────┘
```

Dependency direction is strictly downward. ViewModels depend only on service interfaces and
models — never on views or controls. Controls depend only on models/converters/constants.

## 2. MVVM rules

- `CommunityToolkit.Mvvm` only: `[ObservableProperty]` for every bound field, `[RelayCommand]`
  for every command (with `CanExecute` where state-dependent), `ObservableObject` base.
- ViewModels receive services through constructors only (no service locator).
- No business logic in code-behind; code-behind resolves binding context and relays control
  events → `ICommand` where binding is impossible.
- Every file ≤ 500 lines — including controls' code-behind. List/panel content is rendered
  through DataTemplates + bindings, not imperative UI construction in code-behind.
- Views must never write ViewModel state directly (bind or invoke commands only).
- Behaviors contain zero business logic: event→command relaying and control API calls only.
- ViewModels never reference `SfDiagram` or other controls; all control interaction goes
  through bindable `ICommand` properties on `DiagramSurfaceBehavior`.

## 3. Dependency Injection (MauiProgram.cs)

```csharp
builder.Services.AddSingleton<IDiagramService, DiagramService>();      // in-memory document store
builder.Services.AddSingleton<ITemplateService, TemplateService>();    // JSON template provider
builder.Services.AddSingleton<IExportService, MockExportService>();    // export command surface
builder.Services.AddSingleton<IPropertyService, PropertyService>();    // selection → property bridge
builder.Services.AddSingleton<ISelectionService, SelectionService>();
builder.Services.AddSingleton<IDialogService, DialogService>();        // view-agnostic dialogs
builder.Services.AddSingleton<CreateDiagramViewModel>();
builder.Services.AddSingleton<DiagramDesignerViewModel>();
builder.Services.AddTransient<CreateDiagramPage>();
builder.Services.AddTransient<DiagramDesignerPage>();
```

`ConfigureSyncfusionCore()` (namespace `Syncfusion.Maui.Core.Hosting`) is called before DI
registration.

**Singleton VM + transient page caution:** ViewModels are singletons while pages are
transient. Any page/control that subscribes to a singleton VM's `PropertyChanged` or events
must unsubscribe when the page is removed (or bind through XAML instead), and any page-level
event wiring must re-subscribe symmetrically on `OnAppearing`/`OnDisappearing` so a
navigate-away/back cycle does not silently break the flow.

## 4. SfDiagram integration pattern (critical design decision)

`SfDiagram` is a HybridWebView-based control exposing `Nodes`/`Connectors`
ObservableCollections, async methods, and events. MVVM purity is preserved with two components:

### 4.1 `DiagramSurfaceBehavior` (Behaviors/)
- Attaches to an `SfDiagram`; subscribes/unsubscribes native events on attach/detach:
  `Created` (JS instance ready), `SelectionChanged`, `PositionChanged`, `SizeChanged`,
  `RotationChanged`, `CollectionChanged`, `AnnotationEdited`/`TextEdited` — republished as
  bindable `ICommand` properties (`ReadyCommand`, `SelectionChangedCommand`,
  `GeometryChangedCommand`, `AnnotationEditedCommand`, `CollectionChangedCommand`), so the
  ViewModel never references the control.
- Exposes VM→control relays implemented against the control's async methods:
  `RequestFitToPage`, `RequestUndo`, `RequestRedo`, and zoom/grid/snap state passthrough
  (e.g. a bindable `ZoomFactor` that calls `SfDiagram.Zoom()` on change, plus
  `UndoAsync()/RedoAsync()/CanUndoAsync()/CanRedoAsync()/FitToPageAsync()` helpers).
  **These relays must actually be bound** by the designer page so toolbar/menu/keyboard
  commands reach the canvas.

### 4.2 `DiagramModelAdapter` (Services/)
Pure mapper between domain models (`ShapeItem`, `ConnectorItem`) and Syncfusion
`Node` / `Connector` / annotation objects. Used by `ITemplateService` to materialize JSON
templates and by `IPropertyService` to write property edits (setting properties on tracked
elements triggers the control's dirty-flush — no re-add required). Must set explicit
`Node.Id` strings for template nodes so connector `SourceID`/`TargetID` resolve. Also maps
stencil catalog items to `SymbolPalette` palettes (symbols configured as `Node`s).

### 4.3 SymbolPalette constraint
`SymbolPalette` must be placed as a **direct layout sibling** of `SfDiagram` in the same
`Grid` — sibling discovery walks immediate children only; nesting inside a Border breaks
palette configuration (per `diagram-control-specs.md` §1.5/§10.6). The palette renders
nothing itself; its tile size comes from the shared page CSS. If the SymbolPalette is
hosted inside the workspace control, the left stencil panel must not duplicate it — one
palette instance feeds one drag source.

## 5. Selection & property data flow

```
User selects node on canvas
  → SfDiagram.SelectionChanged
  → DiagramSurfaceBehavior.SelectionChangedCommand
  → DiagramDesignerViewModel → ISelectionService.ApplySelection(nodes, connectors)
  → PropertyPanelViewModel exposes editable PropertyModel fields
  → user edits value → PropertyPanelViewModel Apply
  → IPropertyService writes to live Syncfusion Node/Connector objects
  → SfDiagram dirty-flush coalescing → canvas updates
```

Selection state lives in `ISelectionService` (control `IsSelected` is read-only from JS).
Property editing must support both single-node and single-connector selections, and the
`IPropertyService` must **track** every node/connector added to the canvas (template load
and palette drops via `CollectionChanged`) so the property panel works for all elements.
Property edits set the document dirty flag.

## 6. Command routing

| Source | Target |
|---|---|
| MenuBarControl items (`MenuCommandItem`) | `MenuBarViewModel` → central dispatch table in `DiagramDesignerViewModel` |
| Toolbar buttons (`ToolbarItem`) | `ToolbarViewModel` `[RelayCommand]`s → same dispatch table (no duplicated logic) |
| Keyboard shortcuts (Ctrl+C/V/X/Z/Y/Delete/Ctrl+A) | `DiagramKeyboardBehavior` → same dispatch table |
| Export dialog | `ExportViewModel` → `IExportService` (mock) |
| Arrange operations | `ArrangeViewModel` → dispatch table (real where possible, mock status where the control lacks a native API) |

All commands are `[RelayCommand]`; `CanExecute` derives from observable selection/undo state.
The dispatch table handles every menu/toolbar/keyboard command id from spec.md §6–7. Where
the control supports the operation natively (undo, redo, delete, select all, zoom, fit,
copy/cut/paste), the dispatch must call the control relay — status-only handling is not
acceptable. Unknown command ids must degrade to a logged status message, never throw.

## 7. Verified SfDiagram API notes (source of truth: `Syncfusion.Maui.Diagram` 35.1.41)

Verified by reflection probe of the shipped DLL + help.syncfusion.com UG docs
(`diagram-sdk/maui/*`). Use this as the source of truth when writing control-facing code;
where it conflicts with `diagram-control-specs.md` generalities, this section wins.

### 7.1 SfDiagram surface
- **Properties:** `Nodes` (ObservableCollection\<Node\>), `Connectors` (ObservableCollection\<Connector\>),
  `Width`/`Height` (double, -1 = auto), `Id`, `AutoLayout`, `SymbolPaletteSibling`.
- **Methods:** `Add(Node|Connector)`, `Remove(Node|Connector)`, `Undo()`, `Redo()`,
  `CanUndoAsync()`, `CanRedoAsync()`, `Copy()` → Task\<string\> (JSON clipboard), `Cut()`,
  `Paste()`, `SelectAll()`, `FitToPage()`, `Zoom()`, `Pan()`, `RotateAsync()`,
  `ScaleAsync(Node|Connector)`, `ExportDiagram()` → Task\<string\> (data URI / SVG markup),
  `Print()`, `SaveDiagram()` → string (JSON state), `LoadDiagram()` (void, from JSON),
  `DiscoverSymbolPalette()`, `TrackExistingNodes/Connectors`, `Drag`, `RaiseCollectionChanged`.
- **Events:** `CollectionChanged`, `ConnectionChanged`, `Created` (JS instance ready — this
  is the "DiagramReady" gate), `JavaScriptError`, `PositionChanged`, `RotationChanged`,
  `SelectionChanged`, `SizeChanged`, `SourcePointChanged`, `TargetPointChanged`, `TextEdited`.

### 7.2 Gotchas
- **Key events are not surfaced by the MAUI layer** in this package version (no
  `View.KeyPress`/`KeyEventArgs`); `DiagramKeyboardBehavior` must use a platform hook
  (Windows `UIElement.KeyDown` in `Platforms/Windows/`) with a no-op default partial so
  every TFM compiles.
- `SfDiagram` name collisions: `ToolbarItem`, `HorizontalAlignment`, `VerticalAlignment`
  clash with `Microsoft.Maui.Controls`/`Microsoft.Maui.Graphics` types — use aliases or
  qualified names.
- `Page.DisplayAlert` is obsolete → use `DisplayAlertAsync`.
- No `IAnnotation` public interface — pattern-match `ShapeAnnotation`/`PathAnnotation`.
- The control is HybridWebView-based: gate template population on `Created`.
- SourceGen XAML inflator cannot resolve cross-dictionary `StaticResource` references in
  App-level resources; affected XAML files may opt out per-file with `Runtime` inflation
  (documented csproj mechanism) — keep the list minimal and documented.

### 7.3 Node / Connector / styles
- `Node.Id` (string) **must be set explicitly** for `SourceID`/`TargetID` resolution;
  `OffsetX`/`OffsetY` are the node **center** (not top-left); `Width`, `Height`,
  `RotationAngle` (degrees around center), `Shape` (Shape), `Style` (ShapeStyle),
  `Ports`, `Annotations`; `IsSelected` is read-only from JS — track selection in
  `ISelectionService`.
- `Shape` family: abstract with get-only `.Type`; use constructors
  `new FlowShape(NodeFlowShapes.X)` / `new BasicShape(NodeBasicShapes.X, cornerRadius)`.
  Adapter mapping: Process→Flow.Process, Decision→Flow.Decision, Terminator→Flow.Terminator,
  Document→Flow.Document, DataPreparation→Flow.Preparation, Rectangle→Basic.Rectangle,
  Circle→Basic.Ellipse, Triangle→Basic.Triangle, Hexagon→Basic.Hexagon,
  Diamond→Flow.Decision, Pentagon→Basic.Pentagon, Star→Basic.Star.
- `Connector`: `SourceID`/`TargetID` (node ids), `SourcePortID`/`TargetPortID`,
  `SourcePoint`/`TargetPoint` (**Microsoft.Maui.Graphics.Point?** — `DiagramPoint` is not
  implemented for connector endpoints in this release), `Type`
  (`ConnectorSegmentType.Straight|Orthogonal` **only — no Bezier segment type**; Bezier
  falls back to Straight in the adapter), `Style`, `Annotations` (`PathAnnotation`),
  `SourceDecorator`/`TargetDecorator`.
- `ShapeStyle`: `Fill`, `StrokeColor` (Microsoft.Maui.Graphics.Color), `StrokeWidth`,
  `Opacity` (double 0–1), `StrokeDashArray` (string, e.g. `"4 2"` dashed, `"1 2"` dotted).
- `TextStyle`: `Color`, `Fill`, `FontSize`, `FontFamily`, `Bold`, `Italic`.
  **Not implemented yet:** `TextAlign`, `TextDecoration` (underline) — do not use.
- Annotations: `ShapeAnnotation` (nodes) with control-managed `Id`, `Content`,
  `Offset` (DiagramPoint here), `HorizontalAlignment`/`VerticalAlignment` (Syncfusion
  enums), `Style`; `PathAnnotation` (connectors) same members but `Offset` is **double**.
  Never hand-craft annotation ids.

### 7.4 SymbolPalette
`EnableSearch`, `SymbolHeight`, `SymbolWidth`, `Palettes` (Palette collection), `ExpandMode`
(DiagramPaletteExpandMode, default Multiple). `Width`/`Height` not implemented — use
`WidthRequest`/`HeightRequest`. `Palette`: `Id`, `Title`, `Symbols` (collection of Node
configured as symbols), `IsExpanded`.

## 8. Navigation (Shell)

- `AppShell` routes: `create-diagram` (modal page), `designer` (workspace); route strings
  come from `AppConstants` (no magic strings). The designer workspace is a single
  registered route; the create-diagram experience opens as a modal so the workspace can
  preload beneath it.
- `CreateDiagramViewModel.CreateDiagramCommand(template)` →
  `ITemplateService.GetTemplateAsync(id)` → `IDiagramService.CreateDocumentAsync(template)`
  → `Shell.Current.GoToAsync("//designer")`.
- FILE ▸ New returns to the create dialog (modal) over the designer — pick **one**
  navigation mechanism (Shell route or `Navigation.PushModalAsync`) and keep the open/close
  paths symmetric so the flow round-trips.

## 9. State & persistence (mock)

- `DiagramService` holds the current `DiagramDocument` in memory with a dirty flag
  (set on text edits, geometry changes, collection changes, property edits).
- Save/Save As update the in-memory record and raise status messages.
- Open shows a mock recent-document list (`RecentDocumentCatalog`).
- `SfDiagram.SaveDiagram()` / `LoadDiagram(json)` used for the round-trip snapshot.

## 10. Responsiveness

Desktop-first: fixed panel widths (stencil 280 px, properties 320 px) on Windows;
`OnIdiom`/`OnPlatform` adjustments for MacCatalyst. Panels collapse to zero width (animated)
driven by `DiagramDesignerViewModel.ShowStencil/ShowProperties/ShowToolbar` observables,
toggled from the WINDOW menu or panel header chevrons. Layout dimensions come from
`AppConstants` (menu bar 40, toolbar 48, menu item 32, corner radius 12) — no hardcoded
dimension literals in XAML.

## 11. Known control constraints honored (from diagram-control-specs.md)

- Wait for `Created` (JS instance ready) before populating templates.
- `Node.IsSelected` is one-way; selection tracked via `SelectionChanged` events.
- Annotation ids are control-managed; never hand-craft annotation ids.
- `Print` reliability is Windows-best; Print command surfaces status feedback on other platforms.
- `AddInfo` is client-only (never serialized to JS).
- Bezier connector rendering follows control defaults; UI command is a routed placeholder.
- Org chart template uses `AutoLayout = new DiagramLayout { Type = OrganizationalChart, Orientation = TopToBottom }` where applicable.

## 12. Project structure

```
DiagramBuilder/
├── agent/                              ← harness documentation (this folder)
├── design-images/                      ← reference UI mockups
└── DiagramBuilder/
    ├── DiagramBuilder.slnx
    └── DiagramBuilder/
        ├── DiagramBuilder.csproj       ← pinned Syncfusion + CommunityToolkit.Mvvm packages
        ├── App.xaml / App.xaml.cs      ← merged theme dictionaries
        ├── AppShell.xaml(.cs)          ← routes: create-diagram (modal), designer
        ├── MauiProgram.cs              ← ConfigureSyncfusionCore + DI registrations
        ├── Constants/
        │   ├── AppConstants.cs         ← dimensions, zoom levels, routes, limits
        │   ├── AppStrings.cs           ← every user-visible string
        │   └── ResourceKeys.cs         ← ResourceDictionary color/style keys
        ├── Models/                     ← enums, DiagramItem/ShapeItem/ConnectorItem,
        │                                  DiagramDocument, template DTOs, UI metadata
        │                                  (StencilGroup/Item, PropertyModel/Group,
        │                                  MenuCommandItem, ToolbarItem, ZoomLevel)
        ├── Services/                   ← 6 interfaces + mocks, DiagramModelAdapter, Catalogs
        ├── ViewModels/                 ← Main, CreateDiagram, DiagramDesigner (orchestrator)
        │                                  + MenuBar/Toolbar/Stencil/PropertyPanel/
        │                                  Zoom/Arrange/Export child VMs
        ├── Views/
        │   ├── CreateDiagramPage.xaml(.cs)
        │   ├── DiagramDesignerPage.xaml(.cs)
        │   └── Dialogs/ExportDialog.xaml(.cs)
        ├── Controls/                   ← MenuBar, Toolbar, StencilPanel, DiagramWorkspace,
        │                                  PropertyPanel, ColorPickerButton, PanelHeader,
        │                                  ZoomIndicator
        ├── Templates/                   ← StencilItemTemplate, PropertyEditorTemplates,
        │                                  TemplateCardTemplate DataTemplates
        ├── Behaviors/                  ← DiagramSurfaceBehavior, DiagramKeyboardBehavior
        ├── Converters/                 ← 7 parameterized stateless converters
        ├── Resources/
        │   ├── Styles/Colors.xaml, Styles.xaml
        │   ├── Fonts/ (OpenSans fallback for Segoe UI)
        │   └── Raw/Templates/*.json    ← 3 embedded template payloads
        └── Platforms/ (Android, iOS, MacCatalyst, Windows incl. keyboard hook)
```

## 13. Syncfusion control usage map

| Surface | Control / package | Notes |
|---|---|---|
| Canvas | `SfDiagram` (`Syncfusion.Maui.Diagram`) | Nodes/Connectors collections, events, FitToPage/Undo/Redo/Export/Print, AutoLayout for org chart |
| Stencil | `SymbolPalette` (same package) | Direct **layout sibling** of SfDiagram in the same Grid; palettes built from stencil catalog; `EnableSearch` superseded by our own search box bound to `StencilViewModel.FilterText` |
| Stencil panel chrome | `SfAccordion` / `SfExpander` (`Syncfusion.Maui.Expander`) | Collapsible categories, multiple expand mode |
| Color pickers | `SfColorPicker` (`Syncfusion.Maui.Inputs`) | Fill/border/text/background swatch popups; spectrum mode |
| Numeric fields | `SfNumericEntry` (`Syncfusion.Maui.Inputs`) | Position/size/rotation/thickness/opacity/font-size (never for boolean fields) |
| Popups / flyouts | `SfPopup` (`Syncfusion.Maui.Toolkit`) | Menu flyouts, toolbar shape/connector menus, export dialog — transparent overlay for flyouts, shadowed white popup for dialogs |
| Buttons | `SfButton` (`Syncfusion.Maui.Buttons`) | Primary actions, zoom chips |
| Chips | `SfChip`/`SfChipGroup` (`Syncfusion.Maui.Buttons`) | Zoom level chips 50/100/150/200 % with active-state styling |
| Inputs | `SfTextInputLayout` (`Syncfusion.Maui.TextInputLayout`) | Property panel labeled fields |
| Core hosting | `Syncfusion.Maui.Core` + `ConfigureSyncfusionCore()` | Handler registration |
