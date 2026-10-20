# Diagram Builder — Implementation Tasks

## Task 1 — Foundation
- [x] Add NuGet packages: `CommunityToolkit.Mvvm`, `Syncfusion.Maui.Diagram`,
      `Syncfusion.Maui.Core`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Inputs`,
      `Syncfusion.Maui.Expander`, `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.TextInputLayout`.
- [x] `MauiProgram`: `ConfigureSyncfusionCore()`, DI registrations (services + VMs + pages).
- [x] `App.xaml`: merge `Colors.xaml`, `Styles.xaml`, `DiagramTheme.xaml`.
- [x] `AppShell`: register routes `create-diagram` (modal), `designer`.
- [x] Constants: `AppConstants`, `AppStrings`, `ResourceKeys`.

## Task 2 — Models & services
- [x] Domain models (DiagramDocument, DiagramItem, ShapeItem, ConnectorItem, DiagramTemplate,
      StencilGroup/Item, PropertyModel/Group, MenuCommandItem, ToolbarItem, ZoomLevel,
      ExportFormat, enums).
- [x] Service interfaces + mock implementations (DiagramService, TemplateService,
      MockExportService, PropertyService, SelectionService, DialogService).
- [x] `DiagramModelAdapter` (domain ↔ Syncfusion Node/Connector/Annotation mapping).
- [x] Template JSON payloads (3 predefined templates) + C# fallback seeds.

## Task 3 — ViewModels
- [x] MainViewModel (launch flow, status).
- [x] CreateDiagramViewModel (categories, templates, create command).
- [x] DiagramDesignerViewModel (orchestrator, command dispatch, document lifecycle) —
      delegates to child VMs to stay under 500 lines.
- [x] MenuBarViewModel, ToolbarViewModel, StencilViewModel, PropertyPanelViewModel,
      ZoomViewModel, ArrangeViewModel, ExportViewModel.

## Task 4 — Views & controls
- [x] CreateDiagramPage (category + template cards, SfButton/Chips).
- [x] DiagramDesignerPage (3-column adaptive layout).
- [x] MenuBarControl (menu items + flyouts per mockups).
- [x] ToolbarControl (8 groups, flyouts, swatches, selection-only group visibility).
- [x] StencilPanelControl (search + SfAccordion + SymbolPalette host).
- [x] DiagramWorkspaceControl (SfDiagram + palette sibling + status bar).
- [x] PropertyPanelControl (6 sections, numeric entries, color pickers, segmented styles).
- [x] ExportDialog (format/region/name, mock export result).
- [x] Reusable: PanelHeaderControl, ColorPickerButton, ZoomIndicatorControl.

## Task 5 — Behaviors & converters
- [x] DiagramSurfaceBehavior (events → commands; VM commands → control methods).
- [x] DiagramKeyboardBehavior (Ctrl+C/V/X/Z/Y, Delete, Ctrl+A).
- [x] Converters: BoolToObject, ZoomToPercent, ColorToBrush, EnumToDisplay, NullToBool,
      CountToBool, SelectionKindToVisibility.

## Task 6 — Theme & polish
- [x] Colors.xaml theme dictionary (exact spec palette), Styles.xaml, DiagramTheme.xaml.
- [x] DataTemplates for stencil tiles, property rows, template cards.
- [x] AutomationIds, tooltips, hover states, empty states, collapsible panels.

## Task 7 — Verification
- [x] All ViewModels ≤ 500 lines (checked).
- [x] No TODO comments; no hardcoded user-visible strings in XAML (constants only).
- [x] Code-behind contains only control-event → command relays.
- [x] Build with `dotnet build -f net10.0-android` (Linux sandbox has only android TFM
      available; Windows TFM is the primary runtime target but cannot compile on Linux).
- [x] Manual review against design-images (menus, toolbar, property panel, layout).
