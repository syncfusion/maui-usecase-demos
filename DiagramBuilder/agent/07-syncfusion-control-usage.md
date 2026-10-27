# Diagram Builder — Syncfusion Control Usage

| Surface | Control / package | Notes |
|---|---|---|
| Canvas | `SfDiagram` (`Syncfusion.Maui.Diagram`) | Nodes/Connectors collections, events, FitToPage/Undo/Redo/Export/Print, AutoLayout for org chart |
| Stencil | `SymbolPalette` (same package) | Direct **layout sibling** of SfDiagram in the same Grid (sibling discovery requirement); `DiagramPaletteGroup`s from stencil catalog; `EnableSearch` handled by our own search box bound to `StencilViewModel` |
| Stencil panel chrome | `SfAccordion` (`Syncfusion.Maui.Expander`) | Collapsible categories, multiple expand mode |
| Color pickers | `SfColorPicker` (`Syncfusion.Maui.Inputs`) | Fill/border/text/background swatch popups; Palette mode with alpha slider |
| Numeric fields | `SfNumericEntry` (`Syncfusion.Maui.Inputs`) | Position/size/rotation/thickness/opacity/font-size |
| Popups / flyouts | `SfPopup` (`Syncfusion.Maui.Toolkit`) | Menu flyouts, toolbar shape/connector menus, export dialog |
| Buttons | `SfButton` (`Syncfusion.Maui.Buttons`) | primary actions, zoom chips |
| Chips | `SfChip`/`SfChipGroup` (`Syncfusion.Maui.Buttons`) | zoom level chips 50/100/150/200 % |
| Inputs | `SfTextInputLayout` (`Syncfusion.Maui.TextInputLayout`) | property panel labeled fields |
| Core hosting | `Syncfusion.Maui.Core` + `ConfigureSyncfusionCore()` | handler registration |

## SfDiagram API mapping (per diagram-control-specs.md)

| Feature | API |
|---|---|
| Add shape | `Diagram.Nodes.Add(new Node { … Shape = NodeShapeType.X, Style = … })` |
| Rename | `ShapeAnnotation.Content` (dirty-flush auto-syncs) |
| Connect | `Connector { SourceID, TargetID, Type = ConnectorSegmentType.Straight/Orthogonal }` (Bezier rendered via control defaults; UI command placeholder) |
| Grid/snap | control page settings via init config / `ZoomViewModel` state |
| Fit | `FitToPage()` |
| Undo/redo | `Undo()/Redo()/CanUndoAsync()/CanRedoAsync()` |
| Z-order | command placeholders routed through `DiagramSurfaceBehavior` (EJ2 commands; mocked status when unsupported) |
| Export | `ExportDiagram()` → data URI (Png/Jpg/Svg); PDF = mock |
| Print | `Print()` (Windows primary) |
| Org layout | `AutoLayout = new DiagramLayout { Type = OrganizationalChart, Orientation = TopToBottom }` |
| Save/Open | `SaveDiagram()` / `LoadDiagram(json)` round-trip |

## Constraints respected
- `SymbolPalette` must not be nested inside another layout child of the diagram's parent Grid.
- Wait for `Created` (JS instance ready) (or rely on startup snapshot) before template population — behavior
  gates template load on the ready event.
- `Node.IsSelected` is read-only from the control; selection state tracked by
  `ISelectionService` from `SelectionChanged` events.
- Annotation ids are control-managed; do not hand-craft ids for annotations.
