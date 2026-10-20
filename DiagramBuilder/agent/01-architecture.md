# Diagram Builder — Architecture

## 1. Layering (clean architecture, single project harness)

```
┌───────────────────────────────────────────────────────────────┐
│ Views (XAML) + Controls + Behaviors                           │  presentation
│   bind only to ViewModels; zero business logic                │
├───────────────────────────────────────────────────────────────┤
│ ViewModels (CommunityToolkit.Mvvm)                            │  presentation logic
│   MainViewModel, DiagramDesignerViewModel (+ child VMs)       │
├───────────────────────────────────────────────────────────────┤
│ Services (interfaces + mock implementations)                  │  application
│   IDiagramService, ITemplateService, IExportService,          │
│   IPropertyService, IDialogService, ISelectionService         │
├───────────────────────────────────────────────────────────────┤
│ Models + Constants                                            │  domain / shared
└───────────────────────────────────────────────────────────────┘
```

Dependency direction is strictly downward. ViewModels depend only on service interfaces and
models — never on views or controls. Controls depend only on models/converters.

## 2. Dependency injection (MauiProgram)

```csharp
builder.Services.AddSingleton<IDiagramService, DiagramService>();      // in-memory document store
builder.Services.AddSingleton<ITemplateService, TemplateService>();    // JSON template provider
builder.Services.AddSingleton<IExportService, MockExportService>();    // export command surface
builder.Services.AddSingleton<IPropertyService, PropertyService>();    // selection → property bridge
builder.Services.AddSingleton<IDialogService, DialogService>();        // view-agnostic dialog shell
builder.Services.AddSingleton<MainViewModel>();
builder.Services.AddSingleton<DiagramDesignerViewModel>();
builder.Services.AddSingleton<CreateDiagramViewModel>();
builder.Services.AddTransient<CreateDiagramPage>();
builder.Services.AddTransient<DiagramDesignerPage>();
```

ViewModels receive services through constructors (no service locator).

## 3. Diagram integration pattern (critical design decision)

`SfDiagram` is a HybridWebView-based control whose public surface is
`Nodes`/`Connectors` ObservableCollections plus events and methods. Because it is a *view*
control, MVVM purity is preserved through two reusable components:

1. **`DiagramSurfaceBehavior`** (`Behaviors/`)
   - Attaches to an `SfDiagram`.
   - Subscribes to native events (`DiagramReady`, `SelectionChanged`, `PositionChanged`,
     `SizeChanged`, `RotationChanged`, `CollectionChanged`, `AnnotationEdited`).
   - Publishes them to the bound `DiagramDesignerViewModel` through `Command`/`ICommand`
     bindable properties — the ViewModel never touches the control.
   - Exposes `FitToPageCommand`, `UndoCommand`, `RedoCommand` relays that call the control's
     async methods in response to ViewModel commands (one-way: VM → behavior → control).

2. **`DiagramModelAdapter`** (`Services/`)
   - Pure mapper between app domain models (`ShapeItem`, `ConnectorItem`) and
     `Syncfusion.Maui.Diagram.Node` / `Connector` / annotation objects.
   - Used by `ITemplateService` to materialize JSON templates and by the property panel
     writes (set `Node.Style.Fill`, etc. — the control's dirty-flush coalescing pushes the
     update to the JS surface automatically; no re-add required).

The `SymbolPalette` is placed as a **direct layout sibling** of `SfDiagram` in the same `Grid`
(mandatory: sibling discovery walks immediate children only — nesting it inside a Border
breaks palette configuration per the control spec).

## 4. Selection & property flow

```
User selects node on canvas
  → SfDiagram.SelectionChanged
  → DiagramSurfaceBehavior → SelectionChangedCommand
  → DiagramDesignerViewModel.SelectionService.ApplySelection(nodes, connectors)
  → PropertyPanelViewModel exposes editable PropertyModel fields
  → user edits value → PropertyPanelViewModel.ApplyCommand
  → IPropertyService writes to the live Syncfusion Node/Connector objects
  → SfDiagram dirty-flush → canvas updates
```

## 5. Command routing

| Source | Target |
|---|---|
| MenuBarControl items (`MenuCommandItem`) | `ToolbarViewModel.ExecuteMenuCommand` → dispatch table in `DiagramDesignerViewModel` |
| Toolbar buttons (`ToolbarItem`) | `ToolbarViewModel` `[RelayCommand]`s delegating to `DiagramDesignerViewModel` |
| Keyboard shortcuts (Ctrl+C/V/X/Z/Y/Delete) | `DiagramKeyboardBehavior` → same dispatch table |
| Export menu | `ExportViewModel` → `IExportService` (mock produces a status toast) |

All commands are `[RelayCommand]`; command availability (`CanExecute`) is derived from
observable selection/undo state.

## 6. Navigation

- `AppShell` registers routes: `create-diagram` (modal), `designer` (workspace).
- `MainViewModel.StartCommand` opens `CreateDiagramPage`.
- `CreateDiagramViewModel.CreateDiagramCommand(category, templateId)` →
  `IDiagramService.CreateFromTemplateAsync` → `Shell.Current.GoToAsync("//designer")`.

## 7. Responsiveness

`OnIdiom` + `OnPlatform` defaults (desktop-first), plus VisualState trigger to collapse
left/right panels to overlay flyouts when window width < 1000. Panel visibility is bound to
`WindowViewModel.ShowStencil / ShowProperties / ShowToolbar` observables (Window menu).
