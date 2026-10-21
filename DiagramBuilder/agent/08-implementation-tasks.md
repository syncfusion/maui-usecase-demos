# Diagram Builder — Implementation Tasks

## Task 1 — Foundation
- [ ] Add NuGet packages: `CommunityToolkit.Mvvm`, `Syncfusion.Maui.Diagram`,
      `Syncfusion.Maui.Core`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Inputs`,
      `Syncfusion.Maui.Expander`, `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.TextInputLayout`.
- [ ] `MauiProgram`: `ConfigureSyncfusionCore()`, DI registrations (services + VMs + pages).
- [ ] `App.xaml`: merge `Colors.xaml`, `Styles.xaml`.
- [ ] `AppShell`: register routes `create-diagram` (modal), `designer`.
- [ ] Constants: `AppConstants`, `AppStrings`, `ResourceKeys`. *(Files exist from a previous
      partial pass — verify contents against spec.md/tasks.md Task 1 before completion.)*

## Task 2 — Models & services
- [ ] Domain models (DiagramDocument, DiagramItem, ShapeItem, ConnectorItem, DiagramTemplate,
      StencilGroup/Item, PropertyModel/Group, MenuCommandItem, ToolbarItem, ZoomLevel,
      ExportFormat, enums).
- [ ] Service interfaces + mock implementations (DiagramService, TemplateService,
      MockExportService, PropertyService, SelectionService, DialogService).
- [ ] `DiagramModelAdapter` (domain ↔ Syncfusion Node/Connector/Annotation mapping).
- [ ] Template JSON payloads (3 predefined templates) + C# fallback seeds.

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
