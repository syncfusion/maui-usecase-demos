# Diagram Builder — Validation Checklist (validation.md)

Run after each task (relevant subset) and fully after Task 10.

## 1. Build & references
- [ ] `dotnet build` succeeds with zero errors for at least one TFM (all available TFMs after Task 10).
- [ ] No missing NuGet references: CommunityToolkit.Mvvm, Syncfusion.Maui.Diagram, .Core, .Buttons, .Inputs, .Expander, .Toolkit, .TextInputLayout resolve.
- [ ] No TODO/FIXME/placeholder comments anywhere in the project.
- [ ] All XAML namespaces resolve; no missing `clr-namespace` references.

## 2. MVVM compliance
- [ ] Every ViewModel uses `CommunityToolkit.Mvvm` only (`[ObservableProperty]`, `[RelayCommand]`).
- [ ] `wc -l ViewModels/*.cs` — every file ≤ 500 lines (esp. `DiagramDesignerViewModel`, which delegates to child VMs).
- [ ] No business logic in any `.xaml.cs`; code-behind only resolves binding context and relays control events → `ICommand`.
- [ ] ViewModels never reference `SfDiagram`/controls directly — only `ICommand` bindables via `DiagramSurfaceBehavior`.
- [ ] Constructor injection only; no service locator in ViewModels.

## 3. SOLID & services
- [ ] All services behind interfaces (`IDiagramService`, `ITemplateService`, `IExportService`, `IPropertyService`, `ISelectionService`, `IDialogService`) with mock implementations registered in DI.
- [ ] Services contain no UI code; `DiagramModelAdapter` references only Syncfusion model types (never the `SfDiagram` control).
- [ ] Each service has a single responsibility; no cross-service concrete dependencies.

## 4. Constants, strings & theme
- [ ] Zero hardcoded user-visible strings in XAML/controls — all from `AppStrings`/`AppConstants`.
- [ ] Zero hex colors outside `Resources/Styles/Colors.xaml` (grep `#[0-9A-Fa-f]{6}` across XAML).
- [ ] All colors referenced via `{DynamicResource <ResourceKeys.X>}`.
- [ ] Theme values match spec: Primary #0D6EFD, Background #F5F6F8, Canvas #FFFFFF, Panel #FAFAFA, PanelBorder #DADADA, SelectionBlue #2196F3, Segoe UI.

## 5. Models & mock data
- [ ] All required models exist: DiagramItem, ShapeItem, ConnectorItem, DiagramTemplate, PropertyModel, StencilItem, MenuCommandItem, ToolbarItem (+ enums, DTOs, catalogs).
- [ ] Three predefined JSON templates parse; every connector id references an existing node id; blank seeds present.
- [ ] Templates preloaded at startup with C# fallback — Create page always shows 6 cards.
- [ ] Every designer region has initialized sample state on first run (stencil expanded, zoom 100 %, grid on, snap on, property empty-state).

## 6. Navigation & flow
- [ ] Launch → `create-diagram` modal shows 3 categories; each shows Blank + predefined template.
- [ ] Create → navigates to `//designer` with chosen template rendered on canvas.
- [ ] FILE ▸ New returns to the create dialog; routes are constants (no magic strings).

## 7. Feature completeness (functional)
- [ ] Menu bar: all items from FILE/EDIT/VIEW/ARRANGE/WINDOW spec, with separators, check glyphs, submenus, shortcuts — matching `*_section.png`.
- [ ] Toolbar: groups 1–8 as specified; group 6 visible only when exactly one node is selected; shape/connector flyouts and color popups match `toolbar_*.png`.
- [ ] Stencil: 3 accordion categories with all listed shapes; search filters; connector entries set active tool; drag-and-drop from palette adds nodes.
- [ ] Canvas: add/move/resize/rotate/connect/delete; rubber-band multi-select; grid toggle; snap toggle; double-click inline text edit.
- [ ] Property panel: six sections with all fields; edits update the canvas live; empty state shown without selection.
- [ ] Keyboard: Ctrl+C/X/V/Z/Y, Delete, Ctrl+A all route to the dispatch table.
- [ ] Zoom: In/Out/Fit + 50/100/150/200 % chips; indicator updates.
- [ ] Export dialog: PNG/JPEG/PDF/SVG + region + filename; PNG/JPEG/SVG produce a real data URI when ready; PDF mock status.
- [ ] Every command executes without crash (mock status where unsupported).

## 8. Reusable controls
- [ ] MenuBarControl, ToolbarControl, StencilPanelControl, DiagramWorkspaceControl, PropertyPanelControl, ColorPickerButton, PanelHeaderControl, ZoomIndicatorControl exist and are property/command-driven.
- [ ] `SymbolPalette` is a direct Grid sibling of `SfDiagram` (not nested in a Border/ContentView).
- [ ] DataTemplates (`StencilItemTemplate`, `PropertyEditorTemplates`, `TemplateCardTemplate`) used by all list surfaces.

## 9. UI polish & accessibility
- [ ] AutomationId on every interactive element; tooltips on all toolbar/menu items.
- [ ] Hit targets ≥ 36 px; 12 px corner radii; 1 px borders; shadows only on popups.
- [ ] Hover states (#EFEFEF), disabled 40 % opacity, active-tool SelectionBlue highlight.
- [ ] Panels collapse/expand smoothly from WINDOW menu and header chevrons.
- [ ] Visual comparison vs `design-images/*.png` passes for: full designer, each menu flyout, shape/connector flyouts, fill/border popups, property panel.

## 10. Syncfusion control usage
- [ ] SfDiagram, SymbolPalette, SfAccordion, SfColorPicker, SfNumericEntry, SfPopup, SfButton, SfChipGroup, SfTextInputLayout used per `07-syncfusion-control-usage.md`.
- [ ] Template population gated on `Created` (JS instance ready) (or relies on startup snapshot).
- [ ] `Node.IsSelected` never set programmatically; selection via `SelectionChanged` events.
- [ ] Org chart template uses `AutoLayout` (`OrganizationalChart`, TopToBottom) where applicable.

## 11. Final release gate (after Task 10)
- [ ] All boxes above checked.
- [ ] `agent/08-implementation-tasks.md` updated to reflect completion.
- [ ] Clean-clone build & run verified; no manual steps required.
