# Diagram Builder — Harness Overview

## 1. Purpose

**Diagram Builder** is a production-quality .NET MAUI use case sample that showcases the
Syncfusion .NET MAUI **Diagram** control (`SfDiagram` + `SymbolPalette`) by building a
professional desktop diagram designer.

Users can create, edit, arrange and export three diagram categories:

| Category | Blank diagram | Predefined template |
|---|---|---|
| Flow Chart | Blank Flow Chart | Credit Card Processing |
| Mind Map | Blank Mind Map | Product Planning |
| Organization Chart | Blank Org Chart | Company Hierarchy |

There is **no backend, database, web service, authentication, or AI service**. All data is
local mock data / in-memory collections.

## 2. Scope

### In scope
- Diagram creation dialog (choose category → blank or template)
- Full designer workspace: stencil panel (left), diagram canvas (center), property panel (right)
- Top menu bar (File / Edit / View / Arrange / Window) and top toolbar command groups
- Zoom / pan / fit-to-screen, grid & snap toggles
- Shape stencil library with search and collapsible categories
- Property inspector (position, size, rotation, fill, border, text, background)
- Arrange operations (z-order, align, distribute, match size)
- Export UI commands (PNG / JPEG / PDF / SVG) backed by mock services
- Light theme only, desktop-first (Windows primary, MacCatalyst secondary)

### Out of scope
- File-system persistence beyond in-memory session state ("Save"/"Open" update session state and
  are command placeholders implemented against the mock `IDiagramService` document store)
- Cloud/AI features, authentication, telemetry

## 3. Application flow

```
App Launch
   └── CreateDiagramDialog (Shell route: "create-diagram")
         ├── user picks category (Flow Chart / Mind Map / Organization Chart)
         ├── user picks Blank or a predefined template
         └── Navigate → DiagramDesignerPage (Shell route: "designer")
               ├── Left panel  : StencilPanelControl   (SymbolPalette)
               ├── Center      : DiagramWorkspaceControl (SfDiagram)
               ├── Right panel : PropertyPanelControl
               └── Top         : MenuBarControl + ToolbarControl
```

Shell is used with a custom layout: the designer workspace is a single registered route;
the create-diagram experience opens as a modal page so the workspace can preload beneath it.

## 4. Deliverables map

| Area | Location |
|---|---|
| Harness documents | `agent/*.md` (this folder) |
| Models | `DiagramBuilder/Models` |
| Services (interfaces + mock impls) | `DiagramBuilder/Services` |
| ViewModels (CommunityToolkit.Mvvm) | `DiagramBuilder/ViewModels` |
| Views (pages + dialogs) | `DiagramBuilder/Views` |
| Reusable controls | `DiagramBuilder/Controls` |
| Data templates / item templates | `DiagramBuilder/Templates` |
| Converters | `DiagramBuilder/Converters` |
| Behaviors | `DiagramBuilder/Behaviors` |
| Theme / colors / styles | `DiagramBuilder/Resources/Styles` |
| Centralized constants | `DiagramBuilder/Constants` |
| Mock template JSON | `DiagramBuilder/Resources/Raw/Templates` (also embedded C# seed) |

## 5. Key constraints honored

- MVVM strictly; **no business logic in code-behind** (code-behind only wires control events →
  ViewModel commands where binding is impossible, e.g. `SfDiagram` native events relayed through
  a thin behavior).
- Every ViewModel ≤ 500 lines (DiagramDesignerViewModel delegates to child VMs).
- `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]` everywhere.
- No hardcoded strings in views — all user-visible text via `AppResources`-style constants
  (`AppConstants.Strings`) and theme colors via `ResourceDictionary`.
- Centralized constants (`AppConstants`, `ResourceKeys`).
- SOLID: services behind interfaces, single-responsibility VMs, controls reusable.
- Syncfusion sample browser standards: consistent header, AdaptiveTrigger-responsive panels,
  `AutomationId`s, no TODO comments.
