# Diagram Builder — Verified MAUI Diagram API Notes

Verified against Syncfusion.Maui.Diagram **35.1.41** (reflection probe of the shipped
DLL + help.syncfusion.com UG docs, `diagram-sdk/maui/*`). Use this as the source of
truth when writing control-facing code in Tasks 5–10.

## SfDiagram (control) — public surface

**Properties:** `Nodes` (ObservableCollection\<Node\>), `Connectors`
(ObservableCollection\<Connector\>), `Width`/`Height` (double, -1 = auto), `Id`,
`AutoLayout`, `SymbolPaletteSibling`.

**Methods (all verified present):** `Add(Node|Connector)` (places on canvas without
touching the collection), `Remove(Node|Connector)`, `Undo()`, `Redo()`,
`CanUndoAsync()`, `CanRedoAsync()`, `Copy()` → Task\<string\> (JSON clipboard),
`Cut()`, `Paste()`, `SelectAll()`, `FitToPage()`, `Zoom()`, `Pan()`,
`RotateAsync()`, `ScaleAsync(Node|Connector)`, `ExportDiagram()` → Task\<string\>
(data URI / SVG markup), `Print()`, `SaveDiagram()` → string (JSON state),
`LoadDiagram()` (void, from JSON), `DiscoverSymbolPalette()`,
`TrackExistingNodes/Connectors`, `Drag`, `RaiseCollectionChanged`.

**Events:** `CollectionChanged`, `ConnectionChanged`, `Created` (JS instance ready —
this is the "DiagramReady" gate), `JavaScriptError`, `PositionChanged`,
`RotationChanged`, `SelectionChanged`, `SizeChanged`, `SourcePointChanged`,
`TargetPointChanged`, `TextEdited`.

## Node

`Id` (string, **must be set explicitly** for `SourceID`/`TargetID` resolution),
`OffsetX`/`OffsetY` (node **center**), `Width`, `Height`, `RotationAngle`
(degrees around center), `Shape` (Shape), `Style` (ShapeStyle), `Ports`
(PointPort), `Annotations` (ShapeAnnotation), plus `NodeBase`: `Id`, `Style`,
`AddInfo`, `IsSelected` (writable per UG but read-only from JS — track selection
in `ISelectionService` instead).

**Shape family:** `Shape` is abstract with `.Type` (NodeShapes.Basic|Flow, get-only).
Use constructors: `new FlowShape(NodeFlowShapes.X)` / `new BasicShape(NodeBasicShapes.X,
cornerRadius)`. Setting `.Shape` derives `.Type` automatically.
Flow: Process, Decision, Terminator, Document, Preparation (…plus 20 more).
Basic: Rectangle, Ellipse, Triangle, Hexagon, Diamond, Pentagon, Star, Polygon
(…plus more). Mapping used by `DiagramModelAdapter`: Process→Flow.Process,
Decision→Flow.Decision, Terminator→Flow.Terminator, Document→Flow.Document,
DataPreparation→Flow.Preparation, Rectangle→Basic.Rectangle, Circle→Basic.Ellipse,
Triangle→Basic.Triangle, Hexagon→Basic.Hexagon, Diamond→Flow.Decision,
Pentagon→Basic.Pentagon, Star→Basic.Star.

## Connector

`Id`, `SourceID`/`TargetID` (node ids), `SourcePortID`/`TargetPortID`, `SourcePoint`/
`TargetPoint` (**Microsoft.Maui.Graphics.Point?** — `DiagramPoint` is NOT implemented
for connector endpoints in the current release, per UG note), `Type`
(ConnectorSegmentType.**Straight|Orthogonal only** — no Bezier segment type; Bezier
falls back to Straight in the adapter), `Style` (ShapeStyle), `Annotations`
(PathAnnotation), `SourceDecorator`/`TargetDecorator` (DecoratorSettings).

## ShapeStyle / TextStyle

`ShapeStyle`: `Fill` (**Microsoft.Maui.Graphics.Color**), `StrokeColor` (Color),
`StrokeWidth` (double), `Opacity` (double 0–1), `StrokeDashArray` (string, e.g. "4 2"
dashed, "1 2" dotted).
`TextStyle`: `Color`, `Fill` (Color), `FontSize`, `FontFamily`, `Bold`, `Italic`.
**Not implemented yet:** `TextAlign`, `TextDecoration` (underline) — scheduled for
a future release; do not use in code.

## Annotations

`ShapeAnnotation` (nodes): `Id`, `Content`, `Offset` (**DiagramPoint** here —
implemented for annotations), `HorizontalAlignment`/`VerticalAlignment`
(Syncfusion enums: Left/Center/Right/Stretch and Top/Center/Bottom/Stretch + Auto),
`Style` (TextStyle). Multiple annotations per node supported.
`PathAnnotation` (connectors): same members but `Offset` is **double**.
Annotation `Id` is control-managed — do not hand-craft ids (except content labels
where the UG shows explicit ids are allowed).

## SymbolPalette

`EnableSearch`, `SymbolHeight`, `SymbolWidth`, `Palettes` (Palette collection),
`ExpandMode` (DiagramPaletteExpandMode, default Multiple). **Not implemented:**
`Width`/`Height` — use `WidthRequest`/`HeightRequest` from the View base class.
`Palette`: `Id`, `Title`, `Symbols` (collection of Node configured as symbols),
`IsExpanded`, `IconCss` (Height not implemented).

## Gotchas discovered during Task 3

- `SfDiagram` name collision: `ToolbarItem`, `HorizontalAlignment`,
  `VerticalAlignment` clash with `Microsoft.Maui.Controls`/`Microsoft.Maui.Graphics`
  types — use aliases or qualified names.
- `Page.DisplayAlert` is obsolete → use `DisplayAlertAsync`.
- No `IAnnotation` public interface — pattern-match `ShapeAnnotation`/`PathAnnotation`.
- Node positions are **center-based** (`OffsetX/Y`), not top-left.
- The control is HybridWebView-based: use the `Created` event to gate population.
