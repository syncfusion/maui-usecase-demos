# Diagram Builder — Project Structure

```
DiagramBuilder/
├── agent/                              ← harness documentation (this folder)
│   ├── 00-harness-overview.md
│   ├── 01-architecture.md
│   ├── 02-project-structure.md
│   ├── 03-functional-specification.md
│   ├── 04-ui-requirements.md
│   ├── 05-design-guidelines.md
│   ├── 06-mock-data-strategy.md
│   ├── 07-syncfusion-control-usage.md
│   └── 08-implementation-tasks.md
├── design-images/                      ← reference UI mockups (file/edit/view/arrange/window
│                                          menus, toolbar flyouts, property panel, full screens)
└── DiagramBuilder/
    ├── DiagramBuilder.slnx
    └── DiagramBuilder/
        ├── DiagramBuilder.csproj       ← + Syncfusion.Maui.Diagram, .Core, .Inputs,
        │                                  .Toolkit (popup/accordion), CommunityToolkit.Mvvm
        ├── App.xaml / App.xaml.cs      ← merged theme dictionaries
        ├── AppShell.xaml(.cs)          ← routes: create-diagram (modal), designer
        ├── MauiProgram.cs              ← ConfigureSyncfusionCore + DI registrations
        ├── Constants/
        │   ├── AppConstants.cs         ← dimensions, zoom levels, limits, keyboard hints
        │   ├── AppStrings.cs           ← every user-visible string (no hardcoded strings)
        │   └── ResourceKeys.cs         ← ResourceDictionary color/style keys
        ├── Models/
        │   ├── DiagramCategory.cs      ← enum FlowChart | MindMap | OrganizationChart
        │   ├── DiagramDocument.cs      ← in-memory document (items, metadata, dirty flag)
        │   ├── DiagramItem.cs          ← base: Id, Text, Position, Size, Rotation, Style ids
        │   ├── ShapeItem.cs            ← DiagramItem + ShapeType, fill, border, text style
        │   ├── ConnectorItem.cs        ← DiagramItem + source/target, connector kind, style
        │   ├── ShapeType.cs            ← enum: Process, Decision, Terminator, Document,
        │   │                              DataPreparation, Rectangle, Circle, Triangle,
        │   │                              Hexagon, Diamond, Pentagon, Star…
        │   ├── ConnectorKind.cs        ← Straight | Orthogonal | Bezier
        │   ├── DiagramTemplate.cs      ← Id, Category, Title, Description, IsBlank,
        │   │                              Nodes[], Connectors[], AccentColor, ThumbnailKey
        │   ├── TemplateNode.cs / TemplateConnector.cs  ← JSON DTOs
        │   ├── StencilGroup.cs         ← Title, IconKey, Shapes[StencilItem]
        │   ├── StencilItem.cs          ← ShapeType, DisplayName, SearchTags
        │   ├── PropertyModel.cs        ← generic property descriptor (group, key, label, value)
        │   ├── PropertyGroup.cs        ← Dimensions | Insert | Background | Fill | Border | Text
        │   ├── MenuCommandItem.cs      ← menu model (Id, Text, Gesture, IconKey, IsSeparator)
        │   ├── ToolbarItem.cs          ← toolbar model (Id, IconKey, Tooltip, Group, IsToggle,
        │   │                              IsEnabled — state-driven)
        │   ├── ZoomLevel.cs            ← Value + display label
        │   └── ExportFormat.cs         ← Png | Jpeg | Pdf | Svg
        ├── Services/
        │   ├── IDiagramService.cs / DiagramService.cs          (mock document store)
        │   ├── ITemplateService.cs / TemplateService.cs        (JSON → DiagramTemplate)
        │   ├── IExportService.cs / MockExportService.cs        (mock export pipeline)
        │   ├── IPropertyService.cs / PropertyService.cs        (selection ↔ property bridge,
        │   │                                                    writes to live SfDiagram models)
        │   ├── ISelectionService.cs / SelectionService.cs
        │   ├── IDialogService.cs / DialogService.cs
        │   └── DiagramModelAdapter.cs   (domain ↔ Syncfusion.Diagram mapper)
        ├── ViewModels/
        │   ├── MainViewModel.cs                (launch, shell title, diagnostics status)
        │   ├── CreateDiagramViewModel.cs       (category/template picker — under 500 lines)
        │   ├── DiagramDesignerViewModel.cs     (orchestrator: selection, commands, docs)
        │   ├── MenuBarViewModel.cs             (menu model + dispatch)
        │   ├── ToolbarViewModel.cs             (toolbar groups, active tool state)
        │   ├── StencilViewModel.cs             (groups, search filter, collapse state)
        │   ├── PropertyPanelViewModel.cs       (six groups; applies edits via IPropertyService)
        │   ├── ZoomViewModel.cs                (zoom levels, in/out/fit, snap/grid toggles)
        │   ├── ArrangeViewModel.cs             (align/distribute/match-size/z-order)
        │   └── ExportViewModel.cs              (format picker + mock export)
        ├── Views/
        │   ├── CreateDiagramPage.xaml(.cs)     (category + template chooser)
        │   ├── DiagramDesignerPage.xaml(.cs)   (3-column shell; minimal code-behind)
        │   └── Dialogs/ExportDialog.xaml(.cs)  (mock export options popup)
        ├── Controls/
        │   ├── MenuBarControl.xaml(.cs)        (top menu bar, ItemsSource-driven)
        │   ├── ToolbarControl.xaml(.cs)        (8 command groups incl. flyout menus)
        │   ├── StencilPanelControl.xaml(.cs)   (SfAccordion + SfSymbolPalette host + search)
        │   ├── DiagramWorkspaceControl.xaml(.cs)(SfDiagram + palette sibling + grid overlay)
        │   ├── PropertyPanelControl.xaml(.cs)  (SfAccordion sections; SfNumericEntry, color pick)
        │   ├── ColorPickerButton.xaml(.cs)     (reusable swatch → SfColorPicker popup)
        │   ├── PanelHeaderControl.xaml(.cs)    (collapsible panel chrome, reused L/R)
        │   └── ZoomIndicatorControl.xaml(.cs)
        ├── Templates/
        │   ├── StencilItemTemplate.xaml        (DataTemplates for stencil tiles)
        │   ├── PropertyEditorTemplates.xaml    (DataTemplates per PropertyGroup)
        │   └── TemplateCardTemplate.xaml       (create-dialog template cards)
        ├── Behaviors/
        │   ├── DiagramSurfaceBehavior.cs       (control ↔ VM bridge)
        │   └── DiagramKeyboardBehavior.cs      (Ctrl+C/V/X/Z/Y, Delete, Select All)
        ├── Converters/
        │   ├── BoolToObjectConverter.cs, ZoomToPercentConverter.cs,
        │   ├── ColorToBrushConverter.cs, EnumToDisplayConverter.cs,
        │   ├── NullToBoolConverter.cs, CountToBoolConverter.cs,
        │   └── SelectionKindToVisibilityConverter.cs
        ├── Resources/
        │   ├── Styles/
        │   │   ├── Colors.xaml                 (theme ResourceDictionary)
        │   │   └── Styles.xaml                 (control styles)
        │   ├── Fonts/ (Segoe UI fallback: OpenSans shipped; FontFamily constant = "Segoe UI")
        │   └── Raw/Templates/
        │       ├── flow-credit-card-processing.json
        │       ├── mindmap-product-planning.json
        │       └── orgchart-company-hierarchy.json
        └── Platforms/ (Android, iOS, MacCatalyst, Windows — untouched template scaffolding)
```
