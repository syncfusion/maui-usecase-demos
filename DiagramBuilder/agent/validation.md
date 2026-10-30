# Diagram Builder — Validation Checklist (validation.md)

Run after each task (relevant subset) and fully after Task 10.

## 1. Build & references
- [ ] `dotnet build` succeeds with zero errors for at least one TFM (all available TFMs after Task 10).
- [ ] No missing NuGet references: CommunityToolkit.Mvvm, Syncfusion.Maui.Diagram, .Core, .Buttons, .Inputs, .Expander, .Toolkit, .TextInputLayout resolve; **all package versions are pinned** (no floating `Version="*"`).
- [ ] No TODO/FIXME/placeholder comments anywhere in the project.
- [ ] All XAML namespaces resolve; no missing `clr-namespace` references; Runtime-inflation opt-outs (if any) are minimal and documented in the csproj.

## 2. MVVM compliance
- [ ] Every ViewModel uses `CommunityToolkit.Mvvm` only (`[ObservableProperty]`, `[RelayCommand]`).
- [ ] `wc -l` on all `.cs` files — every file ≤ 500 lines (esp. `DiagramDesignerViewModel` and control code-behind).
- [ ] No business logic in any `.xaml.cs`; code-behind only resolves binding context and relays control events → `ICommand`.
- [ ] ViewModels never reference `SfDiagram`/controls directly — only `ICommand` bindables via `DiagramSurfaceBehavior`.
- [ ] Views never write ViewModel state directly; all interaction goes through bindings/commands.
- [ ] Constructor injection only; no service locator in ViewModels.
- [ ] No dead MVVM artifacts: every declared converter is used in XAML; every merged DataTemplate is applied by a consuming surface; behavior relays (`RequestFitToPage/RequestUndo/RequestRedo`, zoom passthrough) are bound, not orphaned.
- [ ] Event subscriptions are symmetric: transient pages/subscribing controls unsubscribe on removal; singleton-VM subscriptions do not accumulate across page instances; navigate-away/back cycles re-establish wiring (`OnAppearing`/`OnDisappearing` pairs).

## 3. SOLID & services
- [ ] All services behind interfaces (`IDiagramService`, `ITemplateService`, `IExportService`, `IPropertyService`, `ISelectionService`, `IDialogService`) with mock implementations registered in DI.
- [ ] Services contain no UI code; `DiagramModelAdapter` references only Syncfusion model types (never the `SfDiagram` control).
- [ ] Each service has a single responsibility; no cross-service concrete dependencies.
- [ ] `IPropertyService` tracks every canvas element (template-loaded and palette-dropped) and reads properties for both nodes and connectors.

## 4. Constants, strings & theme
- [ ] Zero hardcoded user-visible strings in XAML/controls — all from `AppStrings`/`AppConstants` (incl. section labels and StringFormats).
- [ ] Zero hex colors outside `Resources/Styles/Colors.xaml` (grep `#[0-9A-Fa-f]{6}` across XAML **and** C# — C# color code resolves keys from the merged dictionary, not re-declared hex).
- [ ] All colors referenced via `{DynamicResource <ResourceKeys.X>}`.
- [ ] Theme values match spec: Primary #0D6EFD, Background #F5F6F8, Canvas #FFFFFF, Panel #FAFAFA, PanelBorder #DADADA, SelectionBlue #2196F3, Segoe UI (with OpenSans fallback mechanism on non-Windows).
- [ ] No orphaned constants: every `AppConstants`/`AppStrings`/`ResourceKeys` member is consumed; layout dimensions in XAML come from `AppConstants`, not literals.

## 5. Models & mock data
- [ ] All required models exist: DiagramItem, ShapeItem, ConnectorItem, DiagramTemplate, PropertyModel, StencilItem, MenuCommandItem, ToolbarItem (+ enums, DTOs, catalogs).
- [ ] Three predefined JSON templates parse; every connector id references an existing node id; blank seeds present; fallback C# seeds match the JSON payloads' structure.
- [ ] Templates preloaded at startup with C# fallback — Create page always shows 6 cards.
- [ ] Every designer region has initialized sample state on first run (stencil expanded, zoom 100 %, grid on, snap on, property empty-state).

## 6. Navigation & flow
- [ ] Launch → `create-diagram` modal shows 3 categories; each shows Blank + predefined template.
- [ ] Create → navigates to `//designer` with chosen template rendered on canvas.
- [ ] FILE ▸ New returns to the create dialog and round-trips (one navigation mechanism, symmetric open/close); unsaved-changes confirmation shown when dirty.
- [ ] Routes are constants (no magic strings).

## 7. Feature completeness (functional)
- [ ] Menu bar: all items from FILE/EDIT/VIEW/ARRANGE/WINDOW spec, with separators, live check glyphs, cascading submenus, shortcuts — matching `*_section.png`; menu roots mixed-case regular weight.
- [ ] Toolbar: groups 1–8 as specified; group 6 visible only when exactly one node is selected; shape/connector flyouts are 48 px glyph tile grids matching `toolbar_*.png`; color popups apply the picked color; zoom chips show active state and zoom the canvas.
- [ ] Stencil: 3 accordion categories with all listed shapes; search filters (with no-results state); connector entries set the active tool and the tool is consumable; drag-and-drop from palette adds nodes.
- [ ] Canvas: add/move/resize/rotate/connect/delete; rubber-band multi-select; grid toggle renders/removes the dot grid; snap toggle affects dragging; double-click inline text edit; undo/redo act on the canvas.
- [ ] Property panel: six sections with all fields wired; edits update the canvas live (incl. opacity sliders — type-compatible values); works for nodes and connectors; empty state shown without selection; edits set dirty flag.
- [ ] Keyboard: Ctrl+C/X/V/Z/Y, Delete, Ctrl+A all route to the dispatch table and act on the canvas (Windows platform hook; menu/toolbar parity on other platforms).
- [ ] Zoom: In/Out/Fit + 50/100/150/200 % chips all drive the actual canvas zoom; indicator updates.
- [ ] Export dialog: PNG/JPEG/PDF/SVG + region + filename; selected states visible; Export action runs and closes; PNG/JPEG/SVG produce a real data URI when ready; PDF mock status.
- [ ] Arrange: z-order/align/distribute/match-size either compute on the selection or emit explicit mock status; never silently no-op.
- [ ] Every command executes without crash (mock status where unsupported); unknown command ids degrade to status, never throw.

## 8. Reusable controls
- [ ] MenuBarControl, ToolbarControl, StencilPanelControl, DiagramWorkspaceControl, PropertyPanelControl, ColorPickerButton, PanelHeaderControl, ZoomIndicatorControl exist and are property/command-driven.
- [ ] `SymbolPalette` is a direct Grid sibling of `SfDiagram` (not nested in a Border/ContentView); exactly one visible palette surface (no duplicate shape lists).
- [ ] DataTemplates (`StencilItemTemplate`, `PropertyEditorTemplates`, `TemplateCardTemplate`) used by all list surfaces (no imperative code-behind row/tile construction).
- [ ] Every rendered interactive element is wired (no decorative-only buttons, segments, sliders, or pickers).

## 9. UI polish & accessibility
- [ ] AutomationId on every interactive element; tooltips on all toolbar/menu items.
- [ ] Hit targets ≥ 36 px; 12 px corner radii; 1 px borders; shadows only on popups (with transparent overlay for flyouts).
- [ ] Hover states (#EFEFEF), disabled 40 % opacity, active-tool SelectionBlue highlight.
- [ ] Panels collapse/expand smoothly from WINDOW menu and header chevrons.
- [ ] Visual comparison vs `design-images/*.png` passes for: full designer, each menu flyout, shape/connector flyouts, fill/border popups, property panel, create dialog (thumbnail wireframes, selected-category state).

## 10. Syncfusion control usage
- [ ] SfDiagram, SymbolPalette, SfAccordion/SfExpander, SfColorPicker, SfNumericEntry (numeric fields only), SfPopup, SfButton, SfChip/SfChipGroup, SfTextInputLayout used per architecture.md §13.
- [ ] Template population gated on `Created` (JS instance ready).
- [ ] `Node.IsSelected` never set programmatically; selection via `SelectionChanged` events.
- [ ] Org chart template uses `AutoLayout` (`OrganizationalChart`, TopToBottom) where applicable.
- [ ] Control-facing code follows architecture.md §7 (verified API notes): explicit `Node.Id`, center-based offsets, Straight/Orthogonal connector types only, no `TextAlign`/`TextDecoration` usage, annotation ids control-managed.

## 11. Final release gate (after Task 10)
- [ ] All boxes above checked.
- [ ] Completion status recorded in `agent/tasks.md` task notes.
- [ ] Clean-clone build & run verified; no manual steps required.
- [ ] Dead-code sweep complete: unused converters, orphaned constants/strings, unreferenced DataTemplates, and unbound behavior APIs removed.
