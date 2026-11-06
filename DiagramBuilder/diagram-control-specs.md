# SfDiagram (Syncfusion.Maui.Diagram) — Technical Specification

**Scope.** This document reverse-engineers the *current* implementation of the `Syncfusion.Maui.Diagram`
assembly from its source under `maui/src/diagram/`. It is not a proposal or design doc — every claim below
is grounded in the code actually read (class names, member names, and behavior as written), with explicit
notes wherever the source itself flags something as incomplete, a workaround, or out of scope. Namespace for
all types unless otherwise noted: `Syncfusion.Maui.Diagram`.

---

## 1. Overview & Architecture

### 1.1 What SfDiagram is

`SfDiagram` (`Controls/SfDiagram.cs`) is a .NET MAUI control (`public partial class SfDiagram : SfView,
IDisposable`) that hosts the Syncfusion **EJ2 Diagram** JavaScript library inside a `HybridWebView`, and
exposes a curated subset of that library's API through ordinary MAUI bindable properties and
`ObservableCollection<T>`s: `Nodes`, `Connectors`, and (via those) `Annotation`s and `Port`s. The class-level
XML doc explicitly labels this a "Phase 1" subset, with "Phase 2" work (two-way interaction sync, ports,
auto-layout) partially folded in already (see §10).

The architecture is described in the source as a sibling design to `Syncfusion.Maui.RichTextEditor.SfRichTextEditor`:
a WebView adapter, a JavaScript bridge, a manager facade, and .NET model objects that mirror the underlying
JS library's JSON schema.

### 1.2 Layered architecture

```
Application code (XAML/C#)
        │  (Nodes / Connectors / AutoLayout — bindable properties & ObservableCollections)
        ▼
   SfDiagram (Controls/SfDiagram.cs, .Nodes.cs, .Connectors.cs, .WebAdapters.cs)
        │
        ▼
   DiagramManager (Managers/DiagramManager.cs)      ── orchestration: tracking, dirty-flush coalescing,
        │                                              JS→native structural sync, selection resolution
        ▼
   JavascriptBridge (Managers/JavascriptBridge.cs)  ── per-platform message channel + script execution
        │
        ▼
   HybridWebView (IWebDiagramView / HybridWebViewAdapter, Controls/SfDiagram.WebAdapters.cs)
        │  loads Resources/Raw/wwwroot/index.html (not read for this spec — vendor bundle)
        ▼
   window.SfDiagram JS wrapper  →  ej.diagrams.Diagram / ej.diagrams.SymbolPalette (EJ2 Diagram bundle, ej2.min.js)
```

Two control classes participate: `SfDiagram` (the diagram surface) and `SymbolPalette` (a companion,
non-rendering control — see §1.5).

### 1.3 Native → JavaScript direction

`SfDiagram` never calls into JavaScript directly. It owns a `DiagramManager` (`internal sealed class
DiagramManager : IDisposable`), constructed synchronously inside `SfDiagram.InitializeControl()` — **before**
the `HybridWebView` is even added to the visual tree. The constructor comment explains why: each platform's
native message channel (WebView2 `WebMessageReceived`, Android `JavaScriptInterface`, iOS/macCatalyst
`WKScriptMessageHandler`) is itself wired up via the WebView's own `Loaded`/`HandlerChanged` events, so
building the manager eagerly avoids a race between that registration and the page's first messages.

`DiagramManager` wraps a `JavascriptBridge` (`internal sealed class JavascriptBridge`), which exposes one
method per JS-side operation, each implemented as a literal JavaScript string executed via
`HybridWebView.EvaluateJavaScriptAsync` / `WebView.EvaluateJavaScriptAsync` (`JavascriptBridge.ExecuteScriptAsync`).
Representative calls (all against a global `window.SfDiagram` wrapper defined in `index.html`, not read for
this spec):

| Bridge method | JS call | Purpose |
|---|---|---|
| `InitAsync(configJson)` | `window.SfDiagram.init(configJson)` | Creates the `ej.diagrams.Diagram` instance with the full startup snapshot |
| `AddNodeAsync(nodeJson)` | `window.SfDiagram.addNode(...)` | Adds one node |
| `UpdateNodeAsync(id, nodeJson)` | `window.SfDiagram.updateNode(id, ...)` | Replaces a node's state in place (full object, not a diff) |
| `RemoveNodeAsync(id)` | `window.SfDiagram.removeNode(id)` | Removes a node |
| `AddConnectorAsync` / `UpdateConnectorAsync` / `RemoveConnectorAsync` | mirrors the node calls | Connector CRUD |
| `FitToPageAsync()` | `window.SfDiagram.fitToPage()` | Zoom/pan to fit content |
| `UpdateLayoutAsync(layoutJson)` | `window.SfDiagram.updateLayout(...)` | Applies a new `DiagramLayout` and re-runs it |
| `UndoAsync` / `RedoAsync` / `CanUndoAsync` / `CanRedoAsync` | `window.SfDiagram.undo/redo/canUndo/canRedo()` | EJ2 `UndoRedo` module (auto-injected) |
| `ExportDiagramAsync(optionsJson)` | `window.SfDiagram.exportDiagram(...)` | EJ2 `PrintAndExport` module; always forces `mode: "Data"` |
| `PrintAsync(optionsJson)` | `window.SfDiagram.print(...)` | Thin passthrough to EJ2's own browser-style `print()` |
| `ConfigurePaletteAsync(configJson)` | `window.SfDiagram.configurePalette(...)` | Creates/updates the shared page's `ej.diagrams.SymbolPalette` |
| `DestroyAsync()` | `window.SfDiagram.destroy()` | Tears down the JS-side diagram (defined, not invoked from the read code paths) |

`DiagramManager` serializes every `Node`/`Connector`/`DiagramLayout`/`DiagramExportOptions` payload with
`System.Text.Json.JsonSerializer` and a shared `DiagramJsonOptions.Default` options instance carrying the
custom converters (§6).

### 1.4 JavaScript → Native direction

`JavascriptBridge` registers one native message handler per platform in `SetupMessageHandlers()`
(conditional-compiled `#if WINDOWS / ANDROID / IOS || MACCATALYST`), all funneling into one JSON envelope
`{ type, payload }` parsed by `ProcessMessage`/`Dispatch`. Recognized `type` values and the C# event each
raises:

| `type` | Bridge event | Meaning |
|---|---|---|
| `diagramReady` | `DiagramReady` | `ej.diagrams.Diagram` instance created |
| `bridgeReady` | `BridgeReady` | The hosted page's `<script>` tags finished executing (used to gate the first init payload — see below) |
| `nodeChanged` | `NodeGeometryChanged` | A node finished being dragged/resized/rotated; carries `changeType` = `"Position"`/`"Size"`/`"Rotation"` |
| `selectionChanged` | `SelectionIdsChanged` | Full sets of currently-selected node/connector ids |
| `annotationChanged` | `AnnotationChanged` | A label edit was committed via double-tap/double-click |
| `connectorPointChanged` | `ConnectorPointChanged` | A connector's floating (unattached) endpoint finished being dragged |
| `connectorConnectionChanged` | `ConnectorConnectionChanged` | A connector's endpoint attached to/detached from a node/port |
| `diagramError` | `JavaScriptError` | A call into `window.SfDiagram` threw in JS |
| `jsLog` | *(no C# event; `Debug.WriteLine` only)* | `console.log/warn/error` inside the WebView, relayed because WebView DevTools access is inconvenient across platforms |
| `nodeAdded` / `connectorAdded` | `NodeAdded` / `ConnectorAdded` | An element was created on the JS side without native asking (palette drop, interactive connector draw, redo) |
| `nodeRemoved` / `connectorRemoved` | `NodeRemoved` / `ConnectorRemoved` | An element was removed on the JS side (delete key, cut, undo of a native add) |

Every dispatched event is marshalled back onto the UI thread via `MainThread.BeginInvokeOnMainThread`.

`DiagramManager` subscribes to all of the above and is the layer that actually mutates `Node`/`Connector`
model objects and raises the public `SfDiagram` events (§5).

### 1.5 Symbol palette: shared-page design

`SymbolPalette` (`Controls/SymbolPalette.cs`) is documented as having gone through **three** designs before
the current one:
1. A plain property on `SfDiagram`.
2. A fully independent control with its own `HybridWebView`/`palette.html` and a hand-built cross-WebView
   drag-and-drop protocol.
3. **(current)** No rendering surface of its own at all — a plain bindable-property carrier.

The reason, per the remarks: EJ2's own `SymbolPalette` drags a symbol onto a diagram by walking up the DOM
from the drop point for an ancestor with the `"e-diagram"` class and reaching into that element's
`ej2_instances[0]` registry directly — which only works when both widgets render into the *same* HTML
document. Two separate `HybridWebView` instances never share a document, and WebView2 itself doesn't
reliably support drag-and-drop across separate WebView instances (cited: dotnet/maui#9983,
microsoft-ui-xaml#10576).

So instead: `SfDiagram` owns a single shared page (`Resources/Raw/wwwroot/index.html`, not read for this
spec) containing both a `#symbolpalette` and a `#diagram` DOM element, and constructs both an
`ej.diagrams.SymbolPalette` and an `ej.diagrams.Diagram` instance there. `SfDiagram` discovers a
`SymbolPalette` placed as its own **layout sibling** (e.g. both in the same `Grid`'s children) —
`FindSiblingSymbolPalette()` walks `Parent` as a `Layout` and looks for a `SymbolPalette` child — with **no**
reference property in either direction. Discovery happens in `OnWebViewLoaded`, right before the first init
payload is built, guaranteeing the visual tree (and therefore any sibling) already exists.

Once discovered, `SfDiagram` subscribes to the palette's internal `Changed` event and forwards its full
current configuration to JS via `DiagramManager.UpdateSymbolPaletteAsync` → `JavascriptBridge.ConfigurePaletteAsync`.
The very first configuration, if a sibling is already present, is instead folded directly into the init
payload (`DiagramManager.BuildInitConfigJson`'s `"palette"` key) rather than needing a second round-trip.

Because `SymbolPalette` renders nothing itself, any `WidthRequest`/`HeightRequest`/Grid row-column sizing set
on it in XAML has **no effect** — the palette panel's actual size comes entirely from the shared page's own
CSS.

Once a symbol is dragged from the palette onto the diagram, EJ2's own built-in same-document drag-and-drop
handles the entire gesture — including creating the node — with nothing round-tripping through either
bridge; the new node instead arrives back into `SfDiagram.Nodes` via the `nodeAdded` JS→native message path
described above (`DiagramManager.OnBridgeNodeAdded`).

### 1.6 Initialization sequence & readiness handshake

1. `SfDiagram()` constructor sets `Nodes`/`Connectors`/`AutoLayout` to fresh instances, then calls
   `InitializeControl()`.
2. `InitializeControl()` builds the `HybridWebViewAdapter`, constructs `DiagramManager` (wiring its message
   channel), calls `TrackExistingNodes()`/`TrackExistingConnectors()` (safe pre-manager-ready — they only
   *track*, not talk to JS, until `DiagramManager.IsReady`), starts `LoadHtmlFileAsync()` (fire-and-forget —
   it's actually synchronous, just returning `Task.CompletedTask` after setting `HybridRoot`/`DefaultFile`),
   adds the WebView to a `Grid`, and subscribes to `_webView.Loaded`.
3. `OnWebViewLoaded` (fires once — guarded by `_isInitialized`) calls `DiscoverSymbolPalette()` then
   `DiagramManager.InitializeAsync(this)`.
4. `InitializeAsync` awaits a `TaskCompletionSource` completed by the `BridgeReady` JS message (with a
   10-second `BridgeReadyTimeout` fallback). The remarks explain this replaced an earlier fixed-delay guess
   that "made the diagram render on one run and stay blank on the next with no code changes at all" — script
   execution time varies by WebView2/WKWebView cache state, antivirus scanning, disk I/O, etc. If the timeout
   is hit, a diagnostic JS probe (`typeof window.SfDiagram`) distinguishes "page loaded but message channel
   never attached" from "page never loaded at all" before sending the init payload anyway.
5. `InitAsync` sends `BuildInitConfigJson` — background color, `AutoLayout`, full node/connector snapshot,
   and (if discovered) the palette config — via `window.SfDiagram.init(...)`.
6. The JS side responds with `diagramReady`, which flips `DiagramManager.IsReady = true`, clears any dirty
   marks accumulated during startup, and raises `DiagramManager.Ready` → `SfDiagram.DiagramReady`.

Nodes/connectors added to `Nodes`/`Connectors` **before** `DiagramReady` fires are not lost — they are
included in the startup snapshot — so waiting for the event is optional (confirmed by the getting-started
sample, which *does* wait for it, and by the doc comment on `DiagramReady` itself).

### 1.7 Coalesced ("dirty-flush") update model

`DiagramManager` tracks every `Node`/`Connector` added to the diagram by subscribing to its (internal)
`DiagramElement.Changed` event. A burst of synchronous property writes within the same call stack marks the
element dirty once; `ScheduleFlush()` schedules a single `MainThread.BeginInvokeOnMainThread` callback (only
runs after the stack unwinds) that flushes one full-state JSON patch per dirty element via `UpdateNodeAsync`/
`UpdateConnectorAsync` — never one call per property. `DiagramElement` (the common base of `Node`,
`Connector`, `Annotation`, `ShapeAnnotation`, `PathAnnotation`, styles, decorators, ports, palette groups —
see §3) bubbles `Changed` up from any nested child object or tracked `ObservableCollection<T>` so the owning
top-level `Node`/`Connector` only needs to be listened to once.

Two guard mechanisms prevent feedback loops:
- **`_applyingIncomingNodeUpdate`/`_applyingIncomingConnectorUpdate`** (per-element `HashSet`s): while an
  incoming JS→native geometry/content update is being applied to a tracked element, that element's own
  `Changed` re-entry is suppressed so the update isn't echoed straight back to JavaScript.
- **`_suppressOutboundCollectionSync`** (one shared `bool`): set for the duration of adding/removing a
  `Node`/`Connector` to/from `SfDiagram.Nodes`/`Connectors` in response to a JS-originated structural change
  (`OnBridgeNodeAdded`, `OnBridgeConnectorAdded`, `OnBridgeNodeRemoved`, `OnBridgeConnectorRemoved`) — since
  the collections' own `CollectionChanged` handlers would otherwise call straight back into `AddNodeAsync`/
  `RemoveNodeAsync` and tell JavaScript about something it already knows.

---

## 2. Public API Surface

### 2.1 `SfDiagram` — Bindable properties

| Property | Type | Default | Notes |
|---|---|---|---|
| `Nodes` | `ObservableCollection<Node>` | new empty collection (constructor) | Adding/removing syncs to JS; also receives JS-originated additions/removals |
| `Connectors` | `ObservableCollection<Connector>` | new empty collection | Same sync behavior |
| `AutoLayout` | `DiagramLayout` | `new DiagramLayout()` (`Type = None`) | Named `AutoLayout`, not `Layout`, to avoid shadowing `VisualElement.Layout(Rect)` (CS0108) |

`SfDiagram` deliberately declares **no** custom `Width`/`Height`/`BackgroundColor` bindable properties — it
relies on the inherited `VisualElement` members (`WidthRequest`/`HeightRequest`, standard `BackgroundColor`);
the hosted page's own CSS and `window.SfDiagram.init()` defaults fill the WebView's arranged size at 100%.

### 2.2 `SfDiagram` — Methods

| Method | Returns | Behavior |
|---|---|---|
| `FitToPageAsync()` | `Task` | Zoom/pan to fit all content. No-op before `DiagramReady`. |
| `UndoAsync()` | `Task` | Undo via EJ2's `UndoRedo` module (auto-injected, no opt-in needed). `Ctrl+Z` also works once the diagram has focus. |
| `RedoAsync()` | `Task` | Redo. `Ctrl+Y`/`Ctrl+Shift+Z` also work in-UI. |
| `CanUndoAsync()` | `Task<bool>` | Always `false` before `DiagramReady`. |
| `CanRedoAsync()` | `Task<bool>` | Always `false` before `DiagramReady`. |
| `ExportDiagramAsync(DiagramExportOptions? options = null)` | `Task<string?>` | Returns a `data:image/...;base64,...` URI (Png/Jpg/Bmp) or raw `<svg>` markup (Svg); `null` before `DiagramReady`. Never writes to disk itself. |
| `PrintAsync(DiagramExportOptions? options = null)` | `Task` | Thin passthrough to EJ2's `print()` — see platform caveat in §9. |
| `GetDiagramState(bool indented = true)` | `string` | Native-side JSON snapshot of `Nodes`/`Connectors` via `DiagramStateSerializer.Serialize` — works even before `DiagramReady`. |
| `LoadDiagramState(string json)` | `void` | Clears then repopulates `Nodes`/`Connectors` from `DiagramStateSerializer.Deserialize`; nodes added before connectors so `SourceID`/`TargetID` resolve. |
| `Dispose()` / `Dispose(bool)` | — | Unsubscribes everything, disposes `DiagramManager` (and transitively `JavascriptBridge`), detaches node/connector collection tracking. |

### 2.3 `SfDiagram` — Events

See §5 for full detail; summary list:

`DiagramReady`, `SelectionChanged`, `JavaScriptError`, `AnnotationEdited`, `PositionChanged`,
`SizeChanged` (hides `VisualElement.SizeChanged` with `new`), `RotationChanged`, `CollectionChanged`.

### 2.4 `SymbolPalette` — Bindable properties

| Property | Type | Default | Notes |
|---|---|---|---|
| `SymbolGroups` | `ObservableCollection<DiagramPaletteGroup>` | new empty collection | Each group's `Symbols` reuses `Node` — EJ2's `PaletteModel.symbols` is just a list of node definitions |
| `EnableSearch` | `bool` | `false` | Shows a search box for filtering symbols by name |
| `ExpandMode` | `DiagramPaletteExpandMode` | `Multiple` | Whether more than one category can be expanded at once |
| `SymbolWidth` | `double?` | `null` | Uniform tile width; `null` leaves each `Node.Width` as-is |
| `SymbolHeight` | `double?` | `null` | Uniform tile height |

`SymbolPalette` holds no reference back to a target `SfDiagram` (discovery is one-directional, from
`SfDiagram`'s side — see §1.5) and declares no custom `Width`/`Height` since it renders nothing of its own.
It exposes an `internal event EventHandler? Changed`, raised on any property change or `SymbolGroups`
content change (including nested `DiagramPaletteGroup` edits, bubbled via `DiagramElement`), which a
discovering `SfDiagram` subscribes to.

---

## 3. Data Model

All model classes below derive from `DiagramElement : BindableObject` (`Models/DiagramElement.cs`) unless
noted, which is what gives them the `Changed`-event bubbling described in §1.7. `DiagramElement.Changed` is
`internal` — application code is expected to use ordinary MAUI data binding / `INotifyPropertyChanged`
instead.

### 3.1 `Node` (`Models/Node.cs`)

Mirrors EJ2's `NodeModel`. Constructor auto-generates `Id = "node-" + Guid.NewGuid():N"` (hyphen, not
underscore — see §9.1 on id-format constraints) and initializes `Style`, `Annotations`, `Ports`.

| Property | Type | Default | Notes |
|---|---|---|---|
| `Id` | `string` | auto-generated | Referenced by `Connector.SourceID`/`TargetID` |
| `OffsetX`, `OffsetY` | `double` | `0` | Center position, diagram units |
| `Width`, `Height` | `double` | `100` | |
| `RotationAngle` | `double` | `0` | Degrees, clockwise |
| `Shape` | `NodeShapeType` | `Rectangle` | |
| `PathData` | `string?` | `null` | SVG path; only used when `Shape == Path` |
| `Sides` | `int` | `6` | Only used when `Shape == Polygon`; must be ≥ 3 |
| `IsLocked` | `bool` | `false` | Locks drag/resize/rotate; selection still permitted |
| `Style` | `DiagramStyle` | `new DiagramStyle()` | Fill/stroke appearance |
| `Annotations` | `ObservableCollection<ShapeAnnotation>` | new empty | Text labels |
| `Ports` | `ObservableCollection<Port>` | new empty | Named connection points |
| `AddInfo` | `object?` | `null` | Arbitrary app data; **not** sent to JS |
| `IsSelected` | `bool` (get-only, `internal set`) | `false` | One-way readout from JS selection sync; setting programmatically has no diagram effect; **not** routed through `Changed` |

### 3.2 `Connector` (`Models/Connector.cs`)

Mirrors `ConnectorModel`. Constructor auto-generates `Id = "connector-" + Guid...`, and defaults
`SourceDecorator = { Shape = None }`, `TargetDecorator = { Shape = Arrow }`.

| Property | Type | Default | Notes |
|---|---|---|---|
| `Id` | `string` | auto-generated | |
| `SourceID` / `TargetID` | `string?` | `null` | `Node.Id` to attach to |
| `SourcePortID` / `TargetPortID` | `string?` | `null` | Ignored unless the matching `SourceID`/`TargetID` is also set |
| `SourcePoint` / `TargetPoint` | `Point?` | `null` | Fixed floating endpoint; ignored when the matching `*ID` is set |
| `Type` | `ConnectorSegmentType` | `Orthogonal` | Routing style |
| `Style` | `DiagramStyle` | `new DiagramStyle()` | |
| `SourceDecorator` | `DecoratorSettings` | `Shape = None` | |
| `TargetDecorator` | `DecoratorSettings` | `Shape = Arrow` | |
| `Annotations` | `ObservableCollection<PathAnnotation>` | new empty | |
| `AddInfo` | `object?` | `null` | Not sent to JS |
| `IsSelected` | `bool` (get-only, `internal set`) | `false` | Same one-way semantics as `Node.IsSelected` |

A connector needs at least one resolved source and one resolved target (either a `*ID` attachment or a fixed
`*Point`).

### 3.3 `Port` (`Models/Port.cs`)

Mirrors `PointPortModel`. Constructor auto-generates `Id = "port-" + Guid...`.

| Property | Type | Default | Notes |
|---|---|---|---|
| `Id` | `string` | auto-generated | Referenced by `Connector.SourcePortID`/`TargetPortID` |
| `OffsetX`, `OffsetY` | `double` | `0.5` each | Fractional position within the owning node's bounds (0/0 top-left, 1/1 bottom-right; `0.5/0.5` = center) |
| `Style` | `DiagramStyle` | `new DiagramStyle()` | Port glyph appearance |
| `Visibility` | `PortVisibility` | `Visible` | Deliberately overrides EJ2's own library default (`Connect`) — see §4 |

### 3.4 Annotations: `Annotation`, `ShapeAnnotation`, `PathAnnotation`

There are **three** distinct annotation classes in the source, all with essentially identical shape:

- **`Annotation`** (`Models/Annotation.cs`) — generic base-looking class; `Id = "annotation-" + Guid...`
  (hyphen). Has both `OffsetX`/`OffsetY` (2-D) properties.
- **`ShapeAnnotation`** (`Models/ShapeAnnotation.cs`) — used by `Node.Annotations`. `Id = "annotation_" +
  Guid...` (**underscore**). Same properties as `Annotation` (`OffsetX`/`OffsetY`, `HorizontalAlignment`,
  `VerticalAlignment`, `Content`, `Style`).
- **`PathAnnotation`** (`Models/PathAnnotation.cs`) — used by `Connector.Annotations`. `Id = "annotation_" +
  Guid...` (**underscore**). Has a single `Offset` (0–1 fraction of distance along the path) instead of
  `OffsetX`/`OffsetY`.

See §9.1 for why the `Annotation` class's own constructor comment explains the hyphen-vs-underscore id
convention is load-bearing for EJ2's id-parsing — yet `ShapeAnnotation`/`PathAnnotation` (the ones actually
used by `Node`/`Connector`) still use the underscore form the comment warns against. This inconsistency is
called out as a known constraint, not resolved here.

Common shape (all three classes):

| Property | Type | Default |
|---|---|---|
| `Id` | `string` | auto-generated |
| `Content` | `string` | `string.Empty` |
| `OffsetX`/`OffsetY` (`Annotation`, `ShapeAnnotation`) or `Offset` (`PathAnnotation`) | `double` | `0.5` |
| `HorizontalAlignment` | `AnnotationHorizontalAlignment` | `Center` |
| `VerticalAlignment` | `AnnotationVerticalAlignment` | `Center` |
| `Style` | `AnnotationStyle` | `new AnnotationStyle()` |

On a connector, `HorizontalAlignment.Left`/`Right` map to EJ2's path-relative `"Before"`/`"After"`; anything
else maps to `"Center"`. `VerticalAlignment` has no connector-label equivalent and is ignored there.

### 3.5 `AnnotationStyle` (`Models/AnnotationStyle.cs`)

| Property | Type | Default |
|---|---|---|
| `TextColor` | `Color` | `Colors.Black` |
| `Fill` | `Color` | `Colors.Transparent` |
| `FontSize` | `double` | `12` |
| `FontFamily` | `string` | `"Arial"` |
| `Bold` | `bool` | `false` |
| `Italic` | `bool` | `false` |

### 3.6 `DiagramStyle` (`Models/DiagramStyle.cs`) — shared by `Node`, `Connector`, `Port`, `DecoratorSettings`

| Property | Type | Default |
|---|---|---|
| `Fill` | `Color` | `Colors.White` |
| `StrokeColor` | `Color` | `Colors.Black` |
| `StrokeWidth` | `double` | `1` |
| `Opacity` | `double` | `1` |
| `StrokeDashArray` | `string?` | `null` (solid line) |

### 3.7 `DecoratorSettings` (`Models/DecoratorSettings.cs`)

| Property | Type | Default |
|---|---|---|
| `Shape` | `DecoratorShapeType` | `None` |
| `Width`, `Height` | `double` | `10` |
| `Style` | `DiagramStyle` | `new DiagramStyle()` |

### 3.8 `DiagramLayout` (`Models/DiagramLayout.cs`)

Mirrors (a subset of) `LayoutModel`. Assigned to `SfDiagram.AutoLayout`; mutating in place or reassigning
both push to JS and re-run `diagram.dataBind()`.

| Property | Type | Default |
|---|---|---|
| `Type` | `DiagramLayoutType` | `None` (manual positioning) |
| `Orientation` | `LayoutOrientation` | `TopToBottom` |
| `HorizontalSpacing` | `double` | `30` |
| `VerticalSpacing` | `double` | `30` |
| `Root` | `string?` | `null` (`Node.Id`; most layout types can infer a root from the connector graph) |

### 3.9 `DiagramExportOptions` (`Models/DiagramExportOptions.cs`) — plain class, **not** a `DiagramElement`

Mirrors a subset of EJ2's `ExportOptions`. Deliberately excludes `mode` — `index.html`'s JS wrapper always
forces `mode: "Data"` so exported content returns through `ExportDiagramAsync` rather than triggering an
in-WebView file download.

| Property | Type | Default |
|---|---|---|
| `Format` | `DiagramExportFormat` | `Png` |
| `FileName` | `string?` | `null` (EJ2 default `"diagram"`) |
| `Region` | `DiagramExportRegion` | `Content` |
| `Margin` | `Thickness?` | `null` (EJ2 default: 25 units all sides) |
| `Bounds` | `Rect?` | `null` (EJ2 computes from `Region`) |
| `MultiplePage` | `bool` | `false` |
| `PageWidth` / `PageHeight` | `double?` | `null` (falls back to diagram's own page size) |
| `PageOrientation` | `DiagramPageOrientation?` | `null` (EJ2 default `Portrait`) |

### 3.10 `DiagramPaletteGroup` (`Models/DiagramPaletteGroup.cs`)

Mirrors `PaletteModel`. One category in a `SymbolPalette`. `Id = "palette-" + Guid...` (hyphen).

| Property | Type | Default |
|---|---|---|
| `Id` | `string` | auto-generated |
| `Title` | `string` | `string.Empty` |
| `IconCss` | `string?` | `null` |
| `IsExpanded` | `bool` | `true` |
| `Symbols` | `ObservableCollection<Node>` | new empty |

### 3.11 Model relationship diagram

```
SfDiagram
 ├── Nodes: ObservableCollection<Node>
 │     └── Node
 │           ├── Style: DiagramStyle
 │           ├── Annotations: ObservableCollection<ShapeAnnotation>
 │           │     └── ShapeAnnotation.Style: AnnotationStyle
 │           └── Ports: ObservableCollection<Port>
 │                 └── Port.Style: DiagramStyle
 ├── Connectors: ObservableCollection<Connector>
 │     └── Connector
 │           ├── SourceID/TargetID/SourcePortID/TargetPortID → Node.Id / Port.Id (by string reference)
 │           ├── Style: DiagramStyle
 │           ├── SourceDecorator / TargetDecorator: DecoratorSettings
 │           │     └── DecoratorSettings.Style: DiagramStyle
 │           └── Annotations: ObservableCollection<PathAnnotation>
 │                 └── PathAnnotation.Style: AnnotationStyle
 └── AutoLayout: DiagramLayout (Root → Node.Id)

SymbolPalette (layout sibling, discovered, not referenced)
 └── SymbolGroups: ObservableCollection<DiagramPaletteGroup>
       └── DiagramPaletteGroup.Symbols: ObservableCollection<Node>  (reuses Node as a template)
```

---

## 4. Enums

| Enum | File | Members (meaning) |
|---|---|---|
| `AnnotationHorizontalAlignment` | `Enums/AnnotationAlignment.cs` | `Auto` (engine default), `Left`, `Center` (default), `Right` |
| `AnnotationVerticalAlignment` | `Enums/AnnotationAlignment.cs` | `Auto`, `Top`, `Center` (default), `Bottom` |
| `ConnectorSegmentType` | `Enums/ConnectorSegmentType.cs` | `Straight` (single line), `Orthogonal` (right-angle segments) |
| `DecoratorShapeType` | `Enums/DecoratorShapeType.cs` | `None` (default), `Arrow` (filled), `OpenArrow`, `Circle`, `Diamond`, `Cross`, `Custom` |
| `DiagramExportFormat` | `Enums/DiagramExportFormat.cs` | `Png` (default; omitted from wire JSON, relies on EJ2's own fallback), `Jpg`, `Bmp`, `Svg` |
| `DiagramExportRegion` | `Enums/DiagramExportRegion.cs` | `Content` (bounding box of current content, default), `PageSettings` (full page area) |
| `DiagramLayoutType` | `Enums/DiagramLayoutType.cs` | `None` (manual, default), `HierarchicalTree`, `OrganizationalChart`, `ComplexHierarchicalTree` (multi-parent), `RadialTree`, `SymmetricalLayout` (force-directed), `MindMap`, `Flowchart` |
| `DiagramPageOrientation` | `Enums/DiagramPageOrientation.cs` | `Portrait` (default), `Landscape` |
| `DiagramPaletteExpandMode` | `Enums/DiagramPaletteExpandMode.cs` | `Multiple` (default), `Single` (expanding one collapses others) |
| `LayoutOrientation` | `Enums/LayoutOrientation.cs` | `TopToBottom` (default), `BottomToTop`, `LeftToRight`, `RightToLeft` |
| `NodeShapeType` | `Enums/NodeShapeType.cs` | `Rectangle` (default), `Ellipse`, `Diamond`, `Parallelogram`, `Hexagon`, `Triangle`, `Plus`, `Minus` (synthesized as a `Path`, no EJ2 basic-shape equivalent), `Polygon` (uses `Node.Sides`, sent as computed point list), `Path` (uses `Node.PathData`) |
| `PortVisibility` | `Enums/PortVisibility.cs` | `Visible` (`Port`'s own default), `Hidden` (connectable but never drawn), `Hover` (drawn on node hover), `Connect` (EJ2's own library default — drawn only during a nearby connector drag) |

---

## 5. Events & EventArgs

All `SfDiagram` public events are one-line relays from the matching internal `DiagramManager` event, which
is itself a relay (sometimes with resolution/enrichment) from the matching `JavascriptBridge` event.

| `SfDiagram` event | EventArgs type | Fires when | Payload |
|---|---|---|---|
| `DiagramReady` | `EventArgs` | JS-side `ej.diagrams.Diagram` created and ready for add/update/remove calls | — |
| `SelectionChanged` | `DiagramSelectionChangedEventArgs` | User finishes changing selection in the WebView UI | `SelectedNodes: IReadOnlyList<Node>`, `SelectedConnectors: IReadOnlyList<Connector>` (both resolved against tracked elements; also updates each element's `IsSelected`) |
| `JavaScriptError` | `DiagramJavaScriptErrorEventArgs` | A call into `window.SfDiagram` throws in JS | `Phase` (function name, e.g. `"init"`), `Message`, `StackTrace?` |
| `AnnotationEdited` | `DiagramAnnotationEditedEventArgs` | A double-tap/double-click label edit is committed in the WebView, **after** the model's `Content` is already updated | `Node?`, `Connector?` (whichever owns it), `ShapeAnnotation?`, `PathAnnotation?`, `Content: string`. Diagnostic hook — confirms an edit reached the native model without a JS debugger. |
| `PositionChanged` | `DiagramNodePositionChangedEventArgs` | A node finishes being dragged | `Node`, `OldOffsetX/Y`, `NewOffsetX/Y` — fired once per completed gesture, not per frame |
| `SizeChanged` (hides `VisualElement.SizeChanged` via `new`) | `DiagramNodeSizeChangedEventArgs` | A node finishes being resized | `Node`, `OldWidth/Height`, `NewWidth/Height` |
| `RotationChanged` | `DiagramNodeRotationChangedEventArgs` | A node finishes being rotated | `Node`, `OldRotationAngle`, `NewRotationAngle` |
| `CollectionChanged` | `DiagramCollectionChangedEventArgs` | A `Node`/`Connector` is added/removed to/from the diagram — **any** cause (native collection edit, palette drop, interactive connector draw, delete/cut in UI, undo/redo) | `Action: NotifyCollectionChangedAction`, `Node?`, `Connector?` (both `null` for a `Reset`, e.g. `Nodes.Clear()`, since bulk clears don't report which items were removed) |

Note: `SizeChanged` is declared `public new event EventHandler<DiagramNodeSizeChangedEventArgs>?` —
deliberately shadowing the inherited `VisualElement.SizeChanged`. Existing code that wants the base
control-resize event must cast to a `VisualElement`-typed reference first.

Internal-only events (not surfaced on `SfDiagram`, used for the JS→native sync pipeline): `DiagramManager`
also handles `NodeAdded`, `ConnectorAdded`, `NodeRemoved`, `ConnectorRemoved`, `ConnectorPointChanged`,
`ConnectorConnectionChanged`, `NodeGeometryChanged` — these feed `SfDiagram.Nodes`/`Connectors`,
`Connector.SourcePoint`/`TargetPoint`, and `Connector.SourceID`/`SourcePortID`/`TargetID`/`TargetPortID`
directly rather than being re-exposed as separate public events.

---

## 6. JSON Interop & Serialization

There are **two entirely separate JSON shapes** used by this library, deliberately kept apart:

### 6.1 Wire format (native → JS): `Json/*JsonConverter.cs` + `DiagramJsonOptions`

Used only for the live bridge calls (`addNode`, `updateNode`, `init`, etc.). Every converter (`NodeJsonConverter`,
`ConnectorJsonConverter`, `PortJsonConverter`, `AnnotationJsonConverter`, `ShapeAnnotationJsonConverter`,
`PathAnnotationJsonConverter`, `AnnotationStyleJsonConverter`, `DecoratorSettingsJsonConverter`,
`DiagramStyleJsonConverter`, `DiagramLayoutJsonConverter`, `DiagramExportOptionsJsonConverter`,
`DiagramPaletteGroupJsonConverter`) is **write-only** — `Read` throws `NotSupportedException` in every one
of them, since nothing reads this format back; it exists purely to mirror the exact EJ2 `*Model` JSON shape
the bundled JavaScript expects. `DiagramJsonOptions.Default` is a shared, static `JsonSerializerOptions`
registering all of these.

Key format decisions (all noted directly in the converters' own remarks, cross-verified by the authors
against `ej2.min.js`):

- **Always full-object, never a diff.** Every `Update*` call re-serializes the element's *entire* current
  state; `DiagramManager`'s coalescing (§1.7) is what keeps this cheap.
- **`NodeJsonConverter.WriteShape`**: most `NodeShapeType` values map to `{ type: 'Basic', shape: <name> }`.
  `Path` → `{ type: 'Path', data: <Node.PathData> }`. `Minus` → also `{ type: 'Path', data:
  "M0,50 L100,50" }` (EJ2 has no built-in "Minus" *basic* shape). `Polygon` → `{ type: 'Basic', shape:
  'Polygon', points: [...] }`, with the point list computed by `WritePolygonPoints` (evenly spaced around a
  circle inscribed in a virtual 0–100×0–100 box, clockwise from the top) — since EJ2 computes a polygon's
  outline from explicit points rather than static path data.
- **`ConnectorJsonConverter`**: writes `sourceID`/`sourcePortID` **or** `sourcePoint` (never both — `ID`
  attachment takes priority when set), same for target. Connector annotations are written in the narrower
  EJ2 `PathAnnotationModel` shape (single `offset` fraction + `"Before"/"Center"/"After"` alignment) — see
  §6.2 for why this differs from the persistence format.
- **`AnnotationStyleJsonConverter`**: writes `TextColor`/`Fill` alpha-**inclusive** (`ToHexWithAlpha`,
  `"#RRGGBBAA"`, CSS Color Module 4 order) because `AnnotationStyle` has no separate `Opacity` channel to
  carry transparency the way `DiagramStyle` does — without this, `AnnotationStyle.Fill`'s default
  (`Colors.Transparent`) would serialize as opaque black.
- **`DiagramStyleJsonConverter`**: writes `Fill`/`StrokeColor` alpha-**dropping** (`ToHex`, `"#RRGGBB"`)
  since `DiagramStyle.Opacity` is the intended transparency channel there.
- **`DiagramExportOptionsJsonConverter`**: every property is written only when it differs from the EJ2
  default, so an unmodified `DiagramExportOptions` serializes to `{}`. Notably, `Format.Png` writes **no**
  `format` property at all (not the literal string `"PNG"`) — `ej2.min.js`'s `exportDiagram` never actually
  checks for that string; omitting the property triggers its own PNG (`canvas.toDataURL()`) fallback.
- **`PortJsonConverter`**: always writes `visibility` explicitly, since `Port.Visibility` defaults to
  `Visible` rather than EJ2's own `Connect` default (§4).
- **`NodeJsonConverter`** also writes a non-EJ2-model field, `isLocked` — consumed by the JS wrapper to
  compute the node's `constraints` bitmask via `ej.diagrams.NodeConstraints` before being discarded (not
  passed through to the EJ2 model itself).

`DiagramJsonHelpers` (`Json/DiagramJsonHelpers.cs`) provides the two color-to-hex extension methods
(`ToHex`, alpha-dropping; `ToHexWithAlpha`, alpha-inclusive) used throughout.

### 6.2 Persistence format (save/load): `Serialization/DiagramStateSerializer.cs` + `DiagramState`

`DiagramStateSerializer` (a `static class`) is a **deliberately separate** JSON shape from §6.1, used only by
`SfDiagram.GetDiagramState(bool)`/`LoadDiagramState(string)`. The remarks explain why reusing the wire format
would be lossy: EJ2's `ConnectorModel` annotation shape (`PathAnnotationModel`) only has a single 0–1
`offset` and a 3-way alignment string, so a connector annotation's `OffsetY` and full
horizontal/vertical alignment pair would be silently narrowed on save. The persistence format instead mirrors
each model class's own properties directly and **identically** for both node and connector annotations, is
fully round-trippable, and is versioned (`"version": 1`, `DiagramStateSerializer.CurrentVersion`).

Top-level shape: `{ "version": 1, "nodes": [...], "connectors": [...] }`. Each node/connector/style/decorator
is written with its full property set (including `rotationAngle`, `sides`, `isLocked`, alpha-inclusive
colors via `ToHexWithAlpha`, etc.) — the field names differ in places from the wire format (e.g.
`rotationAngle` here vs. `rotateAngle` in `NodeJsonConverter`).

**Read behavior is defensive by design**: `Deserialize(string json)` throws `System.Text.Json.JsonException`
only if the JSON itself is malformed or the top-level `"nodes"`/`"connectors"` arrays are missing entirely.
Any narrower problem — a missing/unrecognized field on one node, an unparseable enum, a malformed color
string — silently falls back to that property's normal model default (helpers `TryGetString`, `GetDouble`,
`GetInt`, `GetBool`, `GetEnum<TEnum>`, and `ParseColor` which accepts both `"#RRGGBB"` and `"#RRGGBBAA"` and
falls back rather than throwing) — "a hand-edited or slightly-older save file degrades gracefully rather than
failing the whole load."

**Dual-purpose reader.** `DiagramStateSerializer.ReadNode(JsonElement)`/`ReadConnector(JsonElement)` are
`internal` (not `private`) specifically so `JavascriptBridge` can reuse them for a second purpose: parsing
the `"node"`/`"connector"` payload of a `nodeAdded`/`connectorAdded` JS→native message
(`JavascriptBridge.DispatchNodeAdded`/`DispatchConnectorAdded`). `index.html`'s JS-side `describeNode`
helper (not read for this spec) deliberately builds that payload in this exact persistence shape rather than
inventing a third format, specifically so one reader covers both call sites.

Ids are preserved exactly as saved on load — required for `Connector.SourceID`/`TargetID`/`SourcePortID`/
`TargetPortID` references to resolve correctly against reloaded nodes.

---

## 7. Platform / WebView Adapter Layer

`Controls/SfDiagram.WebAdapters.cs` defines:

- **`IWebDiagramView`** (`internal interface`): abstracts the web-hosting surface, mirroring
  `Syncfusion.Maui.RichTextEditor`'s `IWebEditorView`. Two members: `View PlatformView { get; }` and `Task
  LoadHtmlFileAsync()`. The doc comment notes Phase 1 hosts exclusively through `HybridWebView`; a classic
  `WebView`-based implementation "can be added later behind the same interface without changing `SfDiagram`"
  — i.e. this is an intentional extension point, not currently exercised.
- **`HybridWebViewAdapter`** (`internal sealed class : IWebDiagramView`): the only implementation. Wraps a
  `Microsoft.Maui.Controls.HybridWebView` configured with `FlowDirection = LeftToRight`,
  `VerticalOptions`/`HorizontalOptions = Fill`. `LoadHtmlFileAsync()` sets `HybridRoot = "wwwroot"` and
  `DefaultFile = "index.html"` (the constructor's `defaultFile` parameter defaults to `"index.html"` and is
  never overridden — only `SfDiagram` ever constructs this adapter; `SymbolPalette` has none of its own).

`Managers/JavascriptBridge.cs` carries the actual per-platform message-channel plumbing referenced above
(§1.4), each guarded by `#if`:

- **Windows**: hooks `webView.Loaded`, awaits `EnsureCoreWebView2Async()`, subscribes to
  `CoreWebView2.WebMessageReceived` and `CoreWebView2.ContextMenuRequested`. The latter
  (`OnContextMenuRequested`) suppresses WebView2's native right-click context menu **except** when the
  request target `IsEditable` (i.e. while editing an annotation's label) — this distinction was added after
  an earlier unconditional suppression broke native Cut/Copy/Paste during label editing. The remarks explain
  the underlying motive for suppressing it at all: MAUI's own WebView2 handler still leaves "Reload"
  reachable, which — if triggered — re-navigates to `index.html` from scratch on the JS side while native has
  no way to know, permanently blanking the diagram until app restart.
- **Android**: `AddJavascriptInterface(new AndroidJavaScriptInterface(this), "AndroidBridge")`, with a
  per-native-view tag (`AndroidBridgeTagKey`) guarding against double-attachment across both `HandlerChanged`
  and `Loaded`.
- **iOS/macCatalyst**: `WKWebView.Configuration.UserContentController.AddScriptMessageHandler(new
  IosMessageHandler(this), "jsBridge")`, hooked **only** on `Loaded` (deliberately not also
  `HandlerChanged`, unlike Android) — the remarks describe this as a regression fix: `HandlerChanged` fires
  early enough on iOS to race `HybridWebView`'s own internal WKWebView configuration setup, silently losing
  the handler. Tracked per-native-instance via a `WeakReference<WKWebView>` rather than a plain `bool`, so a
  MAUI-recreated native view still gets re-attached.

All three funnel into `JavascriptBridge.ProcessMessage(string)` → `Dispatch(JsonElement)` (§1.4).
`JavascriptBridge.Dispose()` detaches all platform handlers; `DiagramManager.Dispose()` calls it after
unsubscribing its own event handlers and clearing all tracking state.

---

## 8. Packaging

From `Syncfusion.Maui.Diagram.csproj`:

- **Target frameworks**: `net9.0`, `net9.0-android`, `net9.0-ios`, `net9.0-maccatalyst`, `net10.0`,
  `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, plus (Windows build machines only)
  `net9.0-windows10.0.19041.0` / `net10.0-windows10.0.19041.0`.
- **Minimum OS versions**: iOS/macCatalyst 15.0, Android API 21.0, Windows 10.0.18362.0.
- **`UseMaui = true`, `SingleProject = true`, `Nullable = enable`.**
- **Package identity**: `AssemblyName`/`RootNamespace` = `Syncfusion.Maui.Diagram`, `Version =
  19.3.0.1-pre`, `AssemblyVersion`/`FileVersion = 19.3600.0.1`, `Company`/`Authors = Syncfusion® Inc.`,
  `Title = "Syncfusion Diagram for .NET MAUI"`.
- **Dependencies**: `Microsoft.Maui.Controls` (version pinned to `$(MauiVersion)`) and
  `Syncfusion.Maui.Core` (`Version="*"`).
- **Analyzer settings**: `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `AnalysisLevel = latest`,
  `AnalysisLevelSecurity = latest-recommended`; `Release-Xml`/`Debug` configurations both set
  `TreatWarningsAsErrors = true` with `WarningLevel 5`.
- **Bundled web assets**: `Resources\Raw\wwwroot\**\*` is declared as `MauiAsset` with `LogicalName =
  wwwroot\...` so `HybridWebView` (`HybridRoot="wwwroot"`) resolves it in any consuming app. A parallel
  `None`/`Pack` re-declaration of the same files under `buildTransitive\Resources\...`, driven by
  `Syncfusion.Maui.Diagram.props` (also packed), exists specifically to make the wwwroot lookup work for
  **NuGet-package** consumers (as opposed to `ProjectReference` consumers) — the comment says omitting this
  causes a 404 on `wwwroot/index.html` in that scenario specifically.
- Assembly-level (`AssemblyInfo.cs`): `InternalsVisibleTo("Syncfusion.Maui.Diagram.UnitTest")`, and an
  `XmlnsDefinition` mapping `http://schemas.syncfusion.com/maui` → this namespace (the XAML namespace used in
  the sample apps' `xmlns:diagram=...`).

---

## 9. Usage Patterns (from the sample apps)

All four samples (`GettingStarted`, `Nodes`, `Connectors`, `Annotations`) under
`SampleBrowser.Maui.Diagram/Samples/Diagram/` follow the same shape:

**XAML**: declare `<diagram:SfDiagram x:Name="Diagram" ... DiagramReady="OnDiagramReady" />` inside a
`Grid`, sized via `HeightRequest`/`HorizontalOptions="Fill"`/`VerticalOptions="Fill"` like any other MAUI
view, with `xmlns:diagram="clr-namespace:Syncfusion.Maui.Diagram;assembly=Syncfusion.Maui.Diagram"`.

**Code-behind**: all diagram population happens in the `DiagramReady` handler (not the constructor), even
though §1.6 confirms this is not strictly required — the samples wait anyway. Common pattern:

```csharp
private void OnDiagramReady(object? sender, EventArgs e)
{
    var node = CreateNode(...);               // new Node { Id, OffsetX, OffsetY, Width, Height, Shape }
    node.Style.Fill = Color.FromArgb("#1976D2");
    node.Style.StrokeColor = Color.FromArgb("#0D47A1");
    var annotation = new ShapeAnnotation("Place Order");
    annotation.Style.TextColor = Colors.White;
    node.Annotations.Add(annotation);
    Diagram.Nodes.Add(node);

    var connector = new Connector { SourceID = "a", TargetID = "b", Type = ConnectorSegmentType.Orthogonal };
    connector.Style.StrokeColor = Color.FromArgb("#1565C0");
    Diagram.Connectors.Add(connector);

    _ = Diagram.FitToPageAsync();
}
```

- **`GettingStarted`** builds an 11-node payment-processing flowchart, explicitly assigning `Node.Id`
  strings (rather than relying on auto-generated ids) so `Connector.SourceID`/`TargetID` can reference them
  readably, and adds explicit `Port`s (`OffsetX`/`OffsetY`) to two decision nodes so Yes/No branch connectors
  attach at specific edges via `Connector.SourcePortID`.
- **`Nodes`** builds an SDLC ring diagram and wires UI pickers (shape/fill/text-color/stroke-width/size/lock)
  directly to live `Node`/`Annotation` property mutation on a tracked `List<Node>` — demonstrating that
  simply setting a property on an already-added `Node` is sufficient to update the rendered diagram (the
  coalesced flush, §1.7, handles the rest); no re-add is needed.
- **`Connectors`** demonstrates the connector-specific surface: `ConnectorSegmentType`, per-end
  `DecoratorSettings` (`SourceDecorator`/`TargetDecorator`), and toggling `PathAnnotation.Content` between
  empty and a stored value to simulate show/hide (there is no separate visibility property on annotations).
- **`Annotations`** demonstrates the annotation-specific surface: 9-way alignment buttons driving
  `AnnotationHorizontalAlignment`/`AnnotationVerticalAlignment`, plus `Bold`/`Italic`/font-size/color
  controls applied directly to `AnnotationStyle` on tracked annotation instances.

None of the four samples exercise `SymbolPalette`, `AutoLayout`, `GetDiagramState`/`LoadDiagramState`,
`ExportDiagramAsync`/`PrintAsync`, or `Undo`/`RedoAsync` — those parts of the API have no example usage in
the material reviewed for this spec (not a statement that they don't work, only that no sample demonstrates
them).

---

## 10. Known Constraints & Notes

These are drawn directly from comments, XML doc remarks, and structural inconsistencies found in the code —
not inferred.

1. **Explicit "Phase 1 / Phase 2" scoping, with the codebase currently in between.** The `SfDiagram` class
   doc says Phase 1 covers nodes/connectors/annotations via bindable properties; "Phase 2 adds two-way
   interaction sync (drag/resize/rotate and selection flow back from JavaScript into `Node`/`Connector`),
   `Port`s, and `AutoLayout`-driven automatic arrangement." In the code actually read, ports, two-way
   interaction sync (`PositionChanged`/`SizeChanged`/`RotationChanged`/`SelectionChanged`/`AnnotationEdited`),
   and `AutoLayout` are all **already implemented** — so this remark is somewhat stale relative to the
   current source, or "Phase 2" work has landed incrementally without the doc comment being updated.
   Explicitly still **out of scope** per the same remark: data-bound generation, and connector-**endpoint**
   (as opposed to whole-node) drag sync.

2. **Annotation-id inconsistency (potential latent bug).** `Annotation.cs`'s constructor contains a detailed,
   verified-against-`ej2.min.js` explanation of why annotation ids must use a hyphen
   (`"annotation-<hex>"`), not an underscore: EJ2's `endEdit()` resolves the edited annotation by splitting
   the DOM element's composite id on `"_"` and taking the **last** segment; an id containing its own
   underscore causes that split to silently strip the "annotation" prefix, leaving an id that never matches
   the live model — described as "root cause of 'annotation not updated after editing in runtime' / edits
   reverting to the original text." However, `ShapeAnnotation` and `PathAnnotation` — the classes actually
   used by `Node.Annotations`/`Connector.Annotations` in every code path this spec found — **both still
   generate `Id = "annotation_" + Guid...` with an underscore**, i.e. the exact pattern the sibling
   `Annotation` class's own comment identifies as broken. The unused `Annotation` class (hyphen id, matching
   `Node`/`Connector`/`Port`/`DiagramPaletteGroup`'s hyphen convention) appears to be either a superseded
   predecessor of `ShapeAnnotation`/`PathAnnotation` or a partial fix that was never propagated to the two
   classes actually wired into the model graph. This is flagged as a genuine open question in the current
   implementation, not resolved by any code reviewed.

3. **Extensive temporary diagnostic logging left in shipped code.** `DiagramManager.OnNodeChanged`,
   `DiagramElement.RaiseChangedFromChild`, and several `JavascriptBridge`/`DiagramManager` annotation- and
   geometry-sync paths contain `Debug.WriteLine` calls explicitly marked "TEMPORARY diagnostic... safe to
   remove once confirmed" — tracing an active annotation-edit-sync and node-rotation investigation. These are
   unconditional (always compiled in, not behind a debug flag) and fire on every relevant property change /
   bridge message, which is a runtime cost (and console noise) beyond normal production logging.

4. **`Node.Sides` / `Node.PathData` have no cross-property validation.** `Sides` is described as "must be at
   least 3," but `NodeJsonConverter.WriteShape` only clamps with `Math.Max(3, value.Sides)` at
   serialization time — the model property itself accepts any `int`, including 0 or negative, without
   throwing or clamping.

5. **`PrintAsync` has a documented, unresolved cross-platform reliability gap.** Both `SfDiagram.PrintAsync`
   and `DiagramManager.PrintAsync`'s remarks state this is a thin passthrough to EJ2's own `print()`, which
   opens a `window.open("")` popup and calls that window's `window.print()` — "most likely to produce a real
   print dialog on Windows (WebView2); iOS/Android's embedded WebViews commonly block or ignore
   `window.open`/`window.print()` altogether," verified directly against `ej2.min.js`. The documented
   workaround is to use `ExportDiagramAsync` and the platform's own native print/share APIs instead; no code
   path in this library does that automatically.

6. **`SymbolPalette` sibling discovery is structurally fragile by design.** It only checks `Parent is Layout`
   and scans immediate children — a `SymbolPalette` nested one level deeper (e.g. inside a `Border` or
   `ContentView` that is itself the `Grid` child) will not be discovered, and there is no diagnostic or
   fallback for that case. This is an inherent trade-off of the "no reference property, either direction"
   design (§1.5), not a bug per se, but a real constraint on how a consuming app's XAML must be structured.

7. **`IsSelected` on `Node`/`Connector` is one-way and silently ignored if set programmatically.** Both
   properties' doc comments state that setting `IsSelected` in application code "has no effect on the
   diagram's actual selection" — there is no corresponding "select this node" API method exposed anywhere in
   `SfDiagram`/`DiagramManager`/`JavascriptBridge`.

8. **`AddInfo` on `Node`/`Connector` is never serialized or synced** — explicitly documented as "not sent to
   the JavaScript side," making it a pure client-side data-carrying convenience (e.g. for a view-model),
   consistent with WPF/UWP diagram control conventions but worth calling out since it's easy to assume it
   round-trips through `GetDiagramState`/`LoadDiagramState` (it does not — `DiagramStateSerializer` has no
   `AddInfo` read/write).

9. **`JavascriptBridge.DestroyAsync()` exists but no read call site invokes it** — `Dispose()` on
   `DiagramManager`/`JavascriptBridge` tears down C#-side event subscriptions and platform message channels,
   but does not appear (in the code read for this spec) to call `DestroyAsync()` to tell the JS side to
   release its `ej.diagrams.Diagram` instance before the WebView itself is torn down.

10. **No unit test source was reviewed** — `AssemblyInfo.cs` grants `InternalsVisibleTo` to
    `Syncfusion.Maui.Diagram.UnitTest`, confirming a test project is expected to exist, but it was not part
    of the file set provided for this spec.

---

*Document generated by reading the source under `maui/src/diagram/` and the four listed sample files only.
Anything about `Resources/Raw/wwwroot/index.html` or `ej2.min.js` is second-hand, taken only from comments in
the C# source that state they were "verified directly against ej2.min.js" — the JS/HTML bundle itself was
not read.*
