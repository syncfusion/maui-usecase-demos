# Diagram Builder — Implementation Tasks

## Task 1 — Foundation
- [x] Add NuGet packages: `CommunityToolkit.Mvvm`, `Syncfusion.Maui.Diagram`,
      `Syncfusion.Maui.Core`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Inputs`,
      `Syncfusion.Maui.Expander`, `Syncfusion.Maui.Toolkit` *(TextInputLayout not yet added —
      pull it in with the property-panel work if `SfTextInputLayout` is used).*
- [x] `MauiProgram`: `ConfigureSyncfusionCore()` (namespace `Syncfusion.Maui.Core.Hosting`),
      DI registrations (services + VMs + pages).
- [x] `App.xaml`: merge `Colors.xaml`, `Styles.xaml`.
- [x] `AppShell`: routes registered; `create-diagram` is the startup ShellContent (direct,
      no startup `GoToAsync` redirect — avoids the pending-navigation crash).
- [x] Constants: `AppConstants`, `AppStrings`, `ResourceKeys` (verified; styles compile).

## Task 2 — Models & services
- [x] Domain models (DiagramDocument, DiagramItem, ShapeItem, ConnectorItem, DiagramTemplate,
      StencilGroup/Item, PropertyModel/Group, MenuCommandItem, ToolbarItem, ZoomLevel,
      ExportFormat, enums). *(tasks.md Task 2 — complete.)*
- [x] Service interfaces + mock implementations (DiagramService, TemplateService,
      MockExportService, PropertyService, SelectionService, DialogService). *(tasks.md Task 3 —
      complete; catalogs in Services/Catalogs.cs.)*
- [x] `DiagramModelAdapter` (domain ↔ Syncfusion Node/Connector/Annotation mapping) — verified
      against the shipped package 35.1.41, see `09-verified-diagram-api-notes.md`.
- [ ] Template JSON payloads (3 predefined templates) + C# fallback seeds. *(C# fallback seeds
      complete and verified; the JSON payload files under Resources/Raw/Templates are still
      to be authored — tasks.md Task 4.)*

## Task 3 — ViewModels
- [ ] MainViewModel (launch flow, status).
- [ ] CreateDiagramViewModel (categories, templates, create command).
- [ ] DiagramDesignerViewModel (orchestrator, command dispatch, document lifecycle) —
      delegates to child VMs to stay under 500 lines.
- [ ] MenuBarViewModel, ToolbarViewModel, StencilViewModel, PropertyPanelViewModel,
      ZoomViewModel, ArrangeViewModel, ExportViewModel.

## Task 4 — Views & controls
- [ ] CreateDiagramPage (category + template cards, SfButton/Chips).
- [ ] DiagramDesignerPage (3-column adaptive layout).
- [ ] MenuBarControl (menu items + flyouts per mockups).
- [ ] ToolbarControl (8 groups, flyouts, swatches, selection-only group visibility).
- [ ] StencilPanelControl (search + SfAccordion + SymbolPalette host).
- [ ] DiagramWorkspaceControl (SfDiagram + palette sibling + status bar).
- [ ] PropertyPanelControl (6 sections, numeric entries, color pickers, segmented styles).
- [ ] ExportDialog (format/region/name, mock export result).
- [ ] Reusable: PanelHeaderControl, ColorPickerButton, ZoomIndicatorControl.

## Task 5 — Behaviors & converters
- [ ] DiagramSurfaceBehavior (events → commands; VM commands → control methods).
- [ ] DiagramKeyboardBehavior (Ctrl+C/V/X/Z/Y, Delete, Ctrl+A).
- [ ] Converters: BoolToObject, ZoomToPercent, ColorToBrush, EnumToDisplay, NullToBool,
      CountToBool, SelectionKindToVisibility.

## Task 6 — Theme & polish
- [ ] Colors.xaml theme dictionary (exact spec palette), Styles.xaml.
- [ ] DataTemplates for stencil tiles, property rows, template cards.
- [ ] AutomationIds, tooltips, hover states, empty states, collapsible panels.

## Task 7 — Verification
- [ ] All ViewModels ≤ 500 lines (checked).
- [ ] No TODO comments; no hardcoded user-visible strings in XAML (constants only).
- [ ] Code-behind contains only control-event → command relays.
- [ ] Build with `dotnet build -f net10.0-android` (Linux sandbox has only android TFM
      available; Windows TFM is the primary runtime target but cannot compile on Linux).
- [ ] Manual review against design-images (menus, toolbar, property panel, layout).
