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
│   IPropertyService, ISelectionService, IDialogService,         │
│   DiagramModelAdapter                                          │
├───────────────────────────────────────────────────────────────┤
│ Models + Constants + Resources (theme)                         │  domain / shared
└───────────────────────────────────────────────────────────────┘
```

Dependency direction is strictly downward. ViewModels depend only on service interfaces and
models — never on views or controls. Controls depend only on models/converters/constants.

## 2. Dependency Injection (MauiProgram.cs)

```csharp
builder.Services.AddSingleton<IDiagramService, DiagramService>();      // in-memory document store
builder.Services.AddSingleton<ITemplateService, TemplateService>();    // JSON template provider
builder.Services.AddSingleton<IExportService, MockExportService>();    // export command surface
builder.Services.AddSingleton<IPropertyService, PropertyService>();    // selection → property bridge
builder.Services.AddSingleton<ISelectionService, SelectionService>();
builder.Services.AddSingleton<IDialogService, DialogService>();        // view-agnostic dialogs
builder.Services.AddSingleton<MainViewModel>();
builder.Services.AddSingleton<CreateDiagramViewModel>();
builder.Services.AddSingleton<DiagramDesignerViewModel>();
builder.Services.AddTransient<CreateDiagramPage>();
builder.Services.AddTransient<DiagramDesignerPage>();
```

ViewModels receive services through constructors only (no service locator).
`ConfigureSyncfusionCore()` is called before DI registration.

## 3. SfDiagram integration pattern (critical design decision)

`SfDiagram` is a HybridWebView-based control exposing `Nodes`/`Connectors`
ObservableCollections, async methods, and events. MVVM purity is preserved with two components:

### 3.1 `DiagramSurfaceBehavior` (Behaviors/)
- Attaches to an `SfDiagram`.
- Subscribes to native events (`DiagramReady`, `SelectionChanged`, `PositionChanged`,
  `SizeChanged`, `RotationChanged`, `CollectionChanged`, `AnnotationEdited`) and republishes
  them through bindable `ICommand` properties — the ViewModel never references the control.
- Exposes VM→control relays (`FitToPageCommand`, `UndoCommand`, `RedoCommand`, zoom/state
  updates) implemented against the control's async methods.

### 3.2 `DiagramModelAdapter` (Services/)
Pure mapper between domain models (`ShapeItem`, `ConnectorItem`) and Syncfusion
`Node` / `Connector` / annotation objects. Used by `ITemplateService` to materialize JSON
templates and by `IPropertyService` to write property edits (setting properties on tracked
elements triggers the control's dirty-flush — no re-add required).

### 3.3 SymbolPalette constraint
`SymbolPalette` must be placed as a **direct layout sibling** of `SfDiagram` in the same
`Grid` — sibling discovery walks immediate children only; nesting inside a Border breaks
palette configuration (per `diagram-control-specs.md` §1.5/§10.6). The palette renders
nothing itself; its tile size comes from the shared page CSS.

## 4. Selection & property data flow

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

## 5. Command routing

| Source | Target |
|---|---|
| MenuBarControl items (`MenuCommandItem`) | `MenuBarViewModel` → dispatch table in `DiagramDesignerViewModel` |
| Toolbar buttons (`ToolbarItem`) | `ToolbarViewModel` `[RelayCommand]`s → `DiagramDesignerViewModel` |
| Keyboard shortcuts (Ctrl+C/V/X/Z/Y/Delete/Ctrl+A) | `DiagramKeyboardBehavior` → same dispatch table |
| Export menu | `ExportViewModel` → `IExportService` (mock) |
| Arrange operations | `ArrangeViewModel` → dispatch table (mock where control lacks native API) |

All commands are `[RelayCommand]`; `CanExecute` derives from observable selection/undo state.

## 6. Navigation (Shell)

- `AppShell` routes: `create-diagram` (modal page), `designer` (workspace).
- `MainViewModel` opens `CreateDiagramPage` at launch.
- `CreateDiagramViewModel.CreateDiagramCommand(template)` →
  `ITemplateService.GetTemplateAsync(id)` → `IDiagramService.CreateDocumentAsync(template)`
  → `Shell.Current.GoToAsync("//designer")`.
- FILE ▸ New returns to `create-diagram` (modal) over the designer.

## 7. State & persistence (mock)

- `DiagramService` holds the current `DiagramDocument` in memory with a dirty flag.
- Save/Save As update the in-memory record and raise status messages.
- Open shows a mock recent-document list (`RecentDocumentCatalog`).
- `SfDiagram.GetDiagramState()` / `LoadDiagramState(json)` used for the round-trip snapshot.

## 8. Responsiveness

Desktop-first: fixed panel widths (stencil 280 px, properties 320 px) on Windows;
`OnIdiom`/`OnPlatform` adjustments for MacCatalyst. Panels collapse to zero width (animated)
driven by `DiagramDesignerViewModel.ShowStencil/ShowProperties/ShowToolbar` observables,
toggled from the WINDOW menu or panel header chevrons.

## 9. Known control constraints honored (from diagram-control-specs.md)

- Wait for `DiagramReady` (or rely on the startup snapshot) before populating templates.
- `Node.IsSelected` is one-way; selection tracked via `SelectionChanged` events.
- Annotation ids are control-managed; never hand-craft annotation ids.
- `PrintAsync` reliability is Windows-best; Print command surfaces status feedback on other platforms.
- `AddInfo` is client-only (never serialized to JS).
- Bezier connector rendering follows control defaults; UI command is a routed placeholder.
