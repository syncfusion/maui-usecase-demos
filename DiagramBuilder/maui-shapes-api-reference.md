# MAUI Shapes API — Research Reference

**Scope.** `Microsoft.Maui.Controls.Shapes` — the built-in .NET MAUI shape primitives. This is the
API the Diagram Builder stencil panel uses to render shape previews (`StencilPanelControl.xaml.cs`),
and the API available for any custom vector/geometry rendering in the sample.

**Method.** Every claim below was verified by reflection + runtime execution against the **shipped
assemblies**, not from memory or docs:

| MAUI version | Package | Source of truth |
|---|---|---|
| 8.0.100 | `microsoft.maui.controls.core` | `lib/net8.0/Microsoft.Maui.Controls.dll` |
| 9.0.120 | `microsoft.maui.controls.core` | `lib/net9.0/Microsoft.Maui.Controls.dll` |
| 10.0.120 | `microsoft.maui.controls.core` | `lib/net10.0/Microsoft.Maui.Controls.dll` |

The `Microsoft.Maui.Controls.Shapes` namespace contains **exactly 49 public types**, and the type set
plus the `Shape` member surface are **byte-for-byte identical across MAUI 8 / 9 / 10**. There is no
version-gated Shapes API. `dotnet/maui` source was also read for the parsing/measure logic quoted below.

---

## 1. Type map

```
View
└── Shape (abstract) ......................... Fill, Stroke, StrokeThickness, StrokeDashArray,
    │                                          StrokeDashOffset, StrokeDashPattern, StrokeLineCap,
    │                                          StrokeLineJoin, StrokeMiterLimit, Aspect
    ├── Ellipse            (sealed) ......... aspect-preserving ellipse/circle
    ├── Rectangle          (sealed) ......... rectangle w/ RadiusX + RadiusY corner rounding
    ├── RoundRectangle     (sealed) ......... per-corner CornerRadius (TL, TR, BL, BR)
    ├── Line               (sealed) ......... X1,Y1,X2,Y2 segment
    ├── Path               (sealed) ......... Data (Geometry) + RenderTransform (Transform)
    ├── Polygon            (sealed) ......... Points (PointCollection), FillRule — closed
    └── Polyline           (sealed) ......... Points (PointCollection), FillRule — open
```

Supporting types:

| Group | Types |
|---|---|
| Geometry | `Geometry` (abstract, `BindableObject`), `EllipseGeometry`, `RectangleGeometry`, `RoundRectangleGeometry` (derives `GeometryGroup`), `LineGeometry`, `PathGeometry`, `GeometryGroup`, `GeometryCollection` |
| Path building | `PathFigure`, `PathFigureCollection`, `PathSegment` (abstract), `LineSegment`, `ArcSegment`, `BezierSegment`, `QuadraticBezierSegment`, `PolyBezierSegment`, `PolyQuadraticBezierSegment`, `PolyLineSegment`, `PathSegmentCollection` |
| Transforms | `Transform` (abstract), `CompositeTransform`, `MatrixTransform`, `RotateTransform`, `ScaleTransform`, `SkewTransform`, `TranslateTransform`, `TransformGroup`, `TransformCollection`, `Matrix` (struct), `MatrixExtensions` |
| Enums | `FillRule` (EvenOdd, Nonzero), `PenLineCap` (Flat, Square, Round), `PenLineJoin` (Miter, Bevel, Round) |
| Converters | `PathGeometryConverter`, `PathFigureCollectionConverter`, `PointCollectionConverter`, `StrokeShapeTypeConverter`, `MatrixTypeConverter`, `TransformTypeConverter` |
| Interfaces | `IGeometry`, `GeometryHelper` (static flattening helpers) |

---

## 2. `Shape` base class

`Shape : View` — so every shape participates in MAUI layout, supports `WidthRequest`/`HeightRequest`,
`HorizontalOptions`/`VerticalOptions`, `Margin`, `Opacity`, `Rotation`, `InputTransparent`, gestures, etc.

| Property | Type | Default | Notes |
|---|---|---|---|
| `Fill` | `Brush` | `null` | Any `Brush`: `SolidColorBrush`, `LinearGradientBrush`, `Brush.Red`, … |
| `Stroke` | `Brush` | `null` | |
| `StrokeThickness` | `double` | `1` | |
| `StrokeDashArray` | `DoubleCollection` | `null` | Dash/gap pattern |
| `StrokeDashOffset` | `double` | `0` | |
| `StrokeDashPattern` | `float[]` | *(computed)* | **Read-only.** Derived from `StrokeDashArray`; odd-length arrays are duplicated (see §6) |
| `StrokeLineCap` | `PenLineCap` | `Flat` | |
| `StrokeLineJoin` | `PenLineJoin` | `Miter` | |
| `StrokeMiterLimit` | `double` | `10` | |
| `Aspect` | `Stretch` | `None` on base | Overridden to `Stretch.Fill` by `Ellipse`/`Rectangle`/`RoundRectangle` ctors |

`Shape` implements `IShapeView` (`Shape`, `Aspect`, `Fill`) and `IShape`. The platform handler is
`IShapeViewHandler` (`VirtualView`, `PlatformView`).

### `Aspect` defaults — verified at runtime

| Shape | `Aspect` default |
|---|---|
| `Ellipse`, `Rectangle`, `RoundRectangle` | `Stretch.Fill` (set in the constructor) |
| `Line`, `Path`, `Polygon`, `Polyline` | `Stretch.None` (base default) |

`Stretch` → renderer `PathAspect` mapping (from `Shape.cs`): `None`→`None`, `Fill`→`Stretch`,
`Uniform`→`AspectFit`, `UniformToFill`→`AspectFill`.

### `Shape.GetPath()` — the geometry contract

`public abstract PathF GetPath()` returns a `Microsoft.Maui.Graphics.PathF`. **Critical caveat:
`GetPath()` is *not* a pure function of the shape's own properties.** It uses the *layout-assigned*
`Width`/`Height`:

```csharp
internal virtual double WidthForPathComputation
{
    get
    {
        var width = Width;                         // layout-assigned; -1 before layout
        return width == -1 ? _fallbackWidth : width;   // fallback set by the platform handler
    }
}
```

Consequences, all verified by execution:

- Before a layout pass, `new Ellipse { WidthRequest = 100, HeightRequest = 60 }.GetPath()` returns a
  **1×1 unit path** (bounds `-0.5,-0.5 1×1`), *not* a 100×60 path. `WidthRequest` is a *measure*
  input, not a path input.
- `Line`, `Polygon`, `Polyline`, `Path` are **absolute-coordinate** shapes: their `GetPath()` ignores
  the view's size entirely and returns the points as authored.
- `RoundRectangle` is the exception: it uses `WidthForPathComputation`/`HeightForPathComputation`
  but **does not** subtract `StrokeThickness` (unlike `Ellipse`/`Rectangle`, which inset by
  `StrokeThickness / 2`).

> **Practical rule:** to preview a shape off-screen (unit test, stencil tile, generated icon),
> call `GetPath()` only after the shape has been laid out, or author the shape in absolute
> coordinates (`Line` / `Polygon` / `Polyline` / `Path`), which are size-independent.

---

## 3. Per-shape specifics

### `Ellipse`
`Aspect` forced to `Stretch.Fill`. Path = `AppendEllipse(stroke/2, stroke/2, w - stroke, h - stroke)`,
i.e. a 4-arc cubic approximation (13 points observed).

### `Rectangle`
`RadiusX` (double, default `0`), `RadiusY` (double, default `0`). Path = `AppendRectangle` when
`max(RadiusX, RadiusY) == 0`, else `AppendRoundedRectangle` with **`cornerRadius = max(RadiusX, RadiusY)`**
— i.e. `RadiusX`/`RadiusY` are **not** independent ellipse radii; the larger one wins for all four
corners. (`RoundRectangle` is the type that supports genuinely per-corner radii.)

### `RoundRectangle`
`CornerRadius` (`Microsoft.Maui.CornerRadius`, default `new CornerRadius()`). Order of values is
**top-left, top-right, bottom-left, bottom-right**. `CornerRadius` ctors: `(double)` (all four equal)
and `(double tl, double tr, double bl, double br)`. The path is *not* inset by `StrokeThickness`; a
separate internal `GetInnerPath(strokeThickness)` exists for the round-rect-specific measure path.

### `Line`
`X1`, `Y1`, `X2`, `Y2` (all double, default `0`). Ctor overload `Line(double x1, double y1, double x2, double y2)`.
Path = `MoveTo(X1,Y1) → LineTo(X2,Y2)`; `Closed == false`. Absolute coordinates — the view's size is irrelevant.

### `Polygon` / `Polyline`
`Points` (`PointCollection`, default = empty collection, **not null**), `FillRule` (default `EvenOdd`).
Ctors accept a `PointCollection`. `Polygon.GetPath()` = `MoveTo(p0) → LineTo(p1..pn) → Close()`;
`Polyline` is identical **without** `Close()` (verified: `Polygon.Closed == true`, `Polyline.Closed == false`).
`PointCollection` has an implicit conversion from `Point[]`.

### `Path`
`Data` (`Geometry`, `null` by default) — decorated with `[TypeConverter(typeof(PathGeometryConverter))]`,
so **XAML can assign a path string directly**:

```xml
<shapes:Path Data="M0,0 L100,0 L50,80 Z" Fill="Red" Stroke="Black" StrokeThickness="2" />
```

`RenderTransform` (`Transform`, `null` by default) — applies a transform to the geometry.
`Path` also has a `Path(Geometry data)` ctor.
`Path` changes to `Data` are observed live: `Path` subscribes to the geometry's `PropertyChanged`
and re-raises `OnPropertyChanged(nameof(Data))`.

> Note: `Path.RenderTransform` is **not** the same as the `VisualElement.RenderTransform`; it is a
> `Microsoft.Maui.Controls.Shapes.Transform` and only affects the path geometry.

---

## 4. Geometry hierarchy

`Geometry : BindableObject`, with `public abstract void AppendPath(PathF path)`.

| Geometry | Members | `AppendPath` behaviour |
|---|---|---|
| `EllipseGeometry` | `Center` (`Point`), `RadiusX`, `RadiusY` | 13-point ellipse |
| `RectangleGeometry` | `Rect` (`Microsoft.Maui.Graphics.Rect`) | 4 corners |
| `RoundRectangleGeometry` | `Rect`, `CornerRadius` (derives `GeometryGroup`) | 16 points, per-corner radii |
| `LineGeometry` | `StartPoint`, `EndPoint` | ⚠ **appends only the END point** — see below |
| `PathGeometry` | `Figures` (`PathFigureCollection`), `FillRule` | full figure/segment walk |
| `GeometryGroup` | `Children` (`GeometryCollection`), `FillRule` | concatenates children |

> ⚠ **`LineGeometry` quirk (verified).** `new LineGeometry { StartPoint = (0,0), EndPoint = (100,50) }.AppendPath(path)`
> yields a path containing only `(100,50)` — the start point is dropped. `LineGeometry` is effectively
> broken for use as `Path.Data` (`Path.Data = LineGeometry` gives a zero-size path). **Use the `Line`
> *shape* instead**, which is correct.
>
> `GeometryGroup` with `FillRule = Nonzero` produces the same points as `EvenOdd` — the fill rule is
> applied by the platform renderer, not by `AppendPath`.

### `PathFigure` and segments

`PathFigure : BindableObject` (sealed) — `StartPoint` (`Point`, default `(0,0)`), `Segments`
(`PathSegmentCollection`, non-null), `IsClosed` (default `false`), `IsFilled` (default `true`),
plus `BatchBegin()` / `BatchCommit()` to suppress per-segment change notifications.

`PathSegment : BindableObject` (abstract) — also has `BatchBegin()` / `BatchCommit()`.

| Segment | Members |
|---|---|
| `LineSegment` | `Point` |
| `ArcSegment` | `Point`, `Size`, `RotationAngle`, `SweepDirection` (`SweepDirection`, default `CounterClockwise`), `IsLargeArc` |
| `BezierSegment` | `Point1`, `Point2`, `Point3` |
| `QuadraticBezierSegment` | `Point1`, `Point2` |
| `PolyBezierSegment` | `Points` (`PointCollection`) |
| `PolyQuadraticBezierSegment` | `Points` (`PointCollection`) |
| `PolyLineSegment` | `Points` (`PointCollection`) |

---

## 5. Path mini-language (SVG path syntax)

`PathGeometryConverter` parses an SVG-style path string into a `PathGeometry`.

```csharp
var geo = (Geometry)new PathGeometryConverter()
    .ConvertFrom(null, CultureInfo.InvariantCulture, "M0,0 L100,0 L50,80 Z");
```

**Supported commands (all verified by parsing real strings):**

| Command | Support | Segment produced |
|---|---|---|
| `M` / `m` | ✅ | `PathFigure.StartPoint`; extra coordinate pairs after `M` become `LineSegment`s |
| `L` / `l` | ✅ | `LineSegment` |
| `H` / `h` | ✅ | `LineSegment` (horizontal) |
| `V` / `v` | ✅ | `LineSegment` (vertical) |
| `Z` / `z` | ✅ | closes the figure (`IsClosed = true`) |
| `C` / `c` | ✅ | `BezierSegment` |
| `S` / `s` | ✅ | `BezierSegment` (smooth cubic — control point reflected) |
| `Q` / `q` | ✅ | `QuadraticBezierSegment` |
| `T` / `t` | ✅ | `QuadraticBezierSegment` (smooth quadratic — control reflected) |
| `A` / `a` | ✅ | `ArcSegment` |

**Parsing rules confirmed by execution:**

- The string **must start with `M` or `m`** — anything else throws `FormatException` ("IllegalToken"/bad token).
- Separators: commas, spaces, or none are all accepted (`M0,0L10,10` ≡ `M 0 0 L 10 10`).
- Relative (lowercase) commands are supported and computed against the current point.
- A second `M` starts a **new figure** — `"M0,0 L10,10 M50,50 L60,60"` → **2 figures**.
- `Z` sets `PathFigure.IsClosed = true`; it does **not** add a segment.
- The optional `F0` / `F1` **prefix** is parsed but **ignored** — the resulting `PathGeometry.FillRule`
  stays `EvenOdd` in both cases. Set `FillRule` explicitly in code if you need `Nonzero`.
- An **empty string** is legal and yields 0 figures ("Empty path is allowed in XAML").
- `IsFilled` is always `true` for parsed figures.

---

## 6. Converters

| Converter | Converts | Notes |
|---|---|---|
| `PathGeometryConverter` | `string` → `PathGeometry` | `CanConvertTo` → `false`; `ConvertTo` throws `NotSupportedException`. Public static helpers: **none** (see §8). |
| `PathFigureCollectionConverter` | `string` → `PathFigureCollection` | Exposes `public static void ParseStringToPathFigureCollection(PathFigureCollection, string)` |
| `PointCollectionConverter` | `string` / `Point[]` → `PointCollection` | Splits on `' '` and `','`; pairs of numbers become points. `"0,0 10,10 20,20"` → 3 points. Odd token count → `InvalidOperationException`. `;` is **not** a separator. |
| `StrokeShapeTypeConverter` | `string` → `Shape` | Used by `Border.StrokeShape` in XAML |

### `StrokeShapeTypeConverter` string grammar

Case-insensitive prefix match, then space-separated arguments:

| String | Result |
|---|---|
| `"Ellipse"` | `Ellipse` |
| `"Rectangle"` | `Rectangle` |
| `"RoundRectangle 8"` | `RoundRectangle { CornerRadius = 8 }` |
| `"RoundRectangle 8,4,8,4"` | `RoundRectangle { CornerRadius = (tl,tr,bl,br) }` |
| `"Line 0,0 100,50"` | `Line { X1=0, Y1=0, X2=100, Y2=50 }` |
| `"Path M0,0 L10,10 Z"` | `Path { Data = <parsed> }` |
| `"Polygon 0,0 10,0 5,8"` | `Polygon { Points = … }` |
| `"Polyline 0,0 10,0 5,8"` | `Polyline { Points = … }` |
| `"8"` (a bare double) | `RoundRectangle { CornerRadius = 8 }` — Border CSS compatibility |

Anything else throws `InvalidOperationException("Cannot convert \"…\" into …Shape")`.

### `StrokeDashArray` → `StrokeDashPattern`

`StrokeDashPattern` (the `float[]` actually handed to the platform) is derived from `StrokeDashArray`:

- `StrokeDashArray = [4, 2]` → `StrokeDashPattern = [4, 2]` (length 2)
- `StrokeDashArray = [1, 2, 3]` → `StrokeDashPattern = [1, 2, 3]` (length 3)

Odd-length arrays are expanded (dash/gap alternating pattern) by the platform mapper
(`Shape.MapStrokeDashArray`).

---

## 7. Transforms

`Transform : BindableObject` — base exposes `Value` (`Matrix`), a read-only computed property.

| Transform | Members |
|---|---|
| `CompositeTransform` | `CenterX`, `CenterY`, `Rotation`, `ScaleX` (1), `ScaleY` (1), `SkewX`, `SkewY`, `TranslateX`, `TranslateY` |
| `MatrixTransform` | `Matrix` |
| `RotateTransform` | `Angle`, `CenterX`, `CenterY` |
| `ScaleTransform` | `ScaleX` (1), `ScaleY` (1), `CenterX`, `CenterY` |
| `SkewTransform` | `AngleX`, `AngleY`, `CenterX`, `CenterY` |
| `TranslateTransform` | `X`, `Y` |
| `TransformGroup` | `Children` (`TransformCollection`) |

`Matrix` (struct): `M11`, `M12`, `M21`, `M22`, `OffsetX`, `OffsetY`; `Determinant`, `HasInverse`,
`IsIdentity`; mutators `Translate`, `Scale`, `Rotate`, `Skew`, `Invert`, `Append`, `Prepend`,
`SetIdentity`; statics `Multiply`.

`MatrixExtensions` bridges to `System.Numerics.Matrix3x2`: `ToMatrix(Matrix3x2)` / `ToMatrix3X2(Matrix)`.

> **Note:** `Path.RenderTransform` does **not** change `Path.GetPath()` output — the transform is
> applied by the platform renderer, not by `GetPath()`. Verified: a `Path` with a `RotateTransform(45)`
> still reports `Bounds = 0,0 10×10` from `GetPath()`.

---

## 8. ⚠ `PathGeometryConverter.FromString` does not exist

The Diagram Builder stencil panel calls:

```csharp
// DiagramBuilder/DiagramBuilder/DiagramBuilder/Controls/StencilPanelControl.xaml.cs
//   lines 212, 219, 259, 268
Data = Microsoft.Maui.Controls.Shapes.PathGeometryConverter.FromString("M2,2 L34,2 …"),
```

**There is no static `FromString` on `PathGeometryConverter`** — in MAUI 8.0.100, 9.0.120 **or**
10.0.120. The type's only public members are `CanConvertFrom`, `CanConvertTo`, `ConvertFrom`,
`ConvertTo`. A repository-wide scan of every `Microsoft.Maui*.dll` found exactly two public static
`FromString` methods in the whole MAUI stack (`Microsoft.Maui.FontFile.FromString` and
`Microsoft.Maui.Controls.StyleSheets.StyleSheet.FromString`) — neither is a `TypeConverter` helper.

**This is a compile error (CS0117), not a runtime issue.** Confirmed with an isolated repro against
the shipped assembly:

```
error CS0117: 'PathGeometryConverter' does not contain a definition for 'FromString'
```

It was introduced in commit `c142d41` ("Complete Diagram Builder sample…"). Because the sandbox
lacks the `maui-android` workload the project could not be compiled here, so it went unnoticed.

**✅ Fixed in this repo.** `Controls/StencilPanelControl.xaml.cs` now uses a `ParsePathData` helper
built on `PathFigureCollectionConverter.ParseStringToPathFigureCollection`; all four call sites
(lines 212, 219, 259, 268) were updated and the helper was verified to compile and produce the
expected geometry against the real MAUI 10 assemblies.

### Working replacements (all verified to compile and run)

```csharp
// Option A — instance ConvertFrom (most direct 1:1 swap)
var conv = new PathGeometryConverter();
var geo = (Geometry)conv.ConvertFrom(
    null, System.Globalization.CultureInfo.InvariantCulture, "M2,2 L34,2 L46,14 L46,30 L2,30 Z");
new Path { Data = geo };

// Option B — parse straight into a figure collection
var figures = new PathFigureCollection();
PathFigureCollectionConverter.ParseStringToPathFigureCollection(figures, "M2,2 L34,2 …");
new Path { Data = new PathGeometry { Figures = figures } };

// Option C — build the Path in one step
var p = new Path((Geometry)new PathGeometryConverter()
    .ConvertFrom(null, CultureInfo.InvariantCulture, "M2,2 L34,2 …"));
```

Option B is the best fit for the stencil panel because it avoids the `ConvertFrom` boxing/casting
and makes the "parse into figures" intent explicit.

---

## 9. Layout / measurement behaviour

`Shape.MeasureOverride` computes the shape's desired size from the **flattened path bounds**:

```csharp
pathBounds = this.GetPath().GetBoundsByFlattening(1);   // RoundRectangle uses InnerPath() instead
result.Height = pathBounds.Height;
result.Width  = pathBounds.Width;
```

then applies `Aspect`, and finally adds `StrokeThickness` to both dimensions.

**Important:** the flattened-path branch only runs when the base measure returned nothing useful.
The full source is:

```csharp
protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
{
    var result = base.MeasureOverride(widthConstraint, heightConstraint);   // View: W+stroke, H+stroke
    RectF pathBounds;

    if (result.Width != 0 && result.Height != 0 &&
        result.Width > Margin.HorizontalThickness && result.Height > Margin.VerticalThickness)
    {
        return result;                     // <-- EARLY RETURN: requests honoured, path ignored
    }

    pathBounds = (this is IRoundRectangle rr) ? rr.InnerPath().GetBoundsByFlattening(1)
                                             : this.GetPath().GetBoundsByFlattening(1);
    ...
}
```

So there are **two distinct regimes**:

- **Sized** (`WidthRequest`/`HeightRequest` set): the shape measures to the requested size **plus
  `StrokeThickness`** on each axis. The path geometry is irrelevant to the measured size.
- **Unsized**: the shape measures to its **flattened path bounds** plus `StrokeThickness`.

Verified measurements (`MeasureOverride` invoked directly, infinite constraint):

| Shape | Measured size |
|---|---|
| `Ellipse` (no requests, stroke 1) | **2 × 2** |
| `Ellipse` (no requests, stroke 5) | **10 × 10** |
| `Ellipse` (100×60 requested, stroke 1) | **101 × 61** |
| `Ellipse` (100×60 requested, stroke 5) | **105 × 65** |
| `Ellipse` (100×60 requested, stroke 0) | **100 × 60** |
| `Rectangle` (100×60 requested) | 101 × 61 |
| `RoundRectangle` (100×60 requested) | 101 × 61 |
| `Polygon` 3 points spanning 100×80 | 101 × 81 |
| `Polyline` 3 points spanning 100×80 | 101 × 81 |
| `Path` triangle spanning 100×80 | 101 × 81 |
| `Line` from (0,0) to (100,50) | 101 × 51 |

Notes:

- The `+1` per axis **is** the `StrokeThickness` (default `1`) — confirmed by varying it to 5 and 0.
  It is added by the base `View` measure when the shape is sized, and by `Shape.MeasureOverride`
  when it is not.
- `Line`, `Path` and `Polyline` additionally get `Margin.HorizontalThickness` /
  `Margin.VerticalThickness` added to the measured size.
- `Stretch.Fill` honours `WidthRequest`/`HeightRequest` when set; `Uniform`/`UniformToFill` scale the
  flattened bounds to fit the constraint.
- `RoundRectangle` measures against `InnerPath()` (stroke already subtracted) rather than `GetPath()`.

### ⚠ `PathF.Bounds` is the control-point hull, not the visual bounds

For shapes built from Bézier segments (`Ellipse`, `RoundRectangle`, any arc), `PathF.Bounds` is the
**axis-aligned hull of the control points**, which overshoots the real outline. Measured for an
`Ellipse` laid out at 100×60 with stroke 1:

| Metric | Value | Truth |
|---|---|---|
| `PathF.Bounds` | 103.85 × 61.45 | true ellipse is 99 × 59 |
| `GetBoundsByFlattening(0.01f)` | 103.85 × 61.45 | same — flattening does not tighten it |

Use `GetBoundsByFlattening` only for straight-line shapes, where it is exact:
a `Rectangle` laid out at 100×60 with stroke 1 reports exactly `99 × 59`
(inset by `stroke / 2` on each side).

---

## 10. How the Diagram Builder sample uses Shapes today

`Controls/StencilPanelControl.xaml.cs` builds stencil-tile previews (48×36 canvas) from:

| `ShapeType` | Preview implementation |
|---|---|
| `Rectangle`, `Process` | `Shapes.Rectangle` (36×20 / 36×16) |
| `Circle` | `Shapes.Ellipse` (28×28) |
| `Terminator` | `Shapes.Ellipse` (36×16) |
| `Triangle`, `Hexagon`, `Diamond`, `Pentagon`, `Star`, `Decision` | `Shapes.Polygon` built from a space-separated `"x,y x,y"` string parsed in `BuildPolygonShape` |
| `Document`, `DataPreparation` | `Shapes.Path` + `ParsePathData(...)` ✅ (fixed — see §8) |
| `Straight` connector | `Shapes.Line` (X1=2,Y1=18 → X2=46,Y2=18) |
| `Orthogonal` connector | `Shapes.Path` + `ParsePathData("M2,4 L24,4 L24,32 L46,32")` ✅ |
| `Bezier` connector | `Shapes.Path` + `ParsePathData("M2,30 C12,4 36,4 46,30")` ✅ |

Other Shapes usages in the sample:

- `Controls/StencilPanelControl.xaml.cs:129` — `Border.StrokeShape = new RoundRectangle { CornerRadius = 6 }`
- `Controls/MenuBarControl.xaml.cs:109` — `Border.StrokeShape = new RoundRectangle { CornerRadius = 8 }`
- `Templates/StencilItemTemplate.xaml`, `Templates/TemplateCardTemplate.xaml`,
  `Controls/StencilPanelControl.xaml`, `Views/CreateDiagramPage.xaml`,
  `Views/Dialogs/ExportDialog.xaml`, `Controls/ColorPickerButton.xaml`,
  `Resources/Styles/Styles.xaml` — all declare
  `xmlns:shapes="clr-namespace:Microsoft.Maui.Controls.Shapes;assembly=Microsoft.Maui.Controls"`

`Services/DiagramModelAdapter.cs` maps the same `ShapeType` enum to the **Syncfusion**
`SfDiagram` node shapes (`BasicShape` / `FlowShape`) — that is a *different* API surface
(`Syncfusion.Maui.Diagram`), not MAUI Shapes.

> ⚠ **Naming collision to watch.** `Microsoft.Maui.Controls.Shapes.Path` clashes with `System.IO.Path`
> in any file with `using System.IO;` + `using Microsoft.Maui.Controls.Shapes;`. The sample avoids it by
> fully qualifying (`Microsoft.Maui.Controls.Shapes.Path`). Prefer a `using` alias:
> `using MPath = Microsoft.Maui.Controls.Shapes.Path;`.

---

## 11. Practical recipes

**Stencil tile preview (absolute coordinates — size-independent, safe pre-layout):**

```csharp
new Polygon
{
    Points = new PointCollection
    {
        new Point(24, 2), new Point(46, 16), new Point(24, 30), new Point(2, 16)
    },
    Fill = new SolidColorBrush(fillColor),
    Stroke = new SolidColorBrush(strokeColor),
    StrokeThickness = 1.5,
    WidthRequest = 48,
    HeightRequest = 36,
}
```

**Arrow head / custom flow shape from a path string:**

```csharp
// Reusable helper (this is what the repo now uses in StencilPanelControl.xaml.cs)
static Geometry ParsePathData(string pathData)
{
    var figures = new PathFigureCollection();
    PathFigureCollectionConverter.ParseStringToPathFigureCollection(figures, pathData);
    return new PathGeometry { Figures = figures };
}

new Path
{
    Data = ParsePathData("M0,0 L10,5 L0,10 Z"),
    Fill = brush,
    Stroke = stroke,
    StrokeThickness = 1,
    WidthRequest = 12,
    HeightRequest = 12,
}
```

**Dashed outline:**

```csharp
new Ellipse
{
    Stroke = Brush.Black,
    StrokeThickness = 2,
    StrokeDashArray = new DoubleCollection { 4, 2 },   // -> StrokeDashPattern [4,2]
    StrokeLineCap = PenLineCap.Round,
}
```

**XAML (the `Data` string is converted by `PathGeometryConverter` automatically):**

```xml
<shapes:Path Data="M0,0 L100,0 L50,80 Z"
             Fill="{StaticResource CanvasBackground}"
             Stroke="{StaticResource TextPrimary}"
             StrokeThickness="1.5"
             WidthRequest="48" HeightRequest="36" />
```

---

## 12. Gotcha summary

| # | Gotcha |
|---|---|
| 1 | `GetPath()` uses **layout-assigned** `Width`/`Height`, not `WidthRequest`. Before layout it returns a 1×1 unit path. Absolute-coordinate shapes (`Line`, `Polygon`, `Polyline`, `Path`) are unaffected. |
| 2 | `PathGeometryConverter.FromString(...)` **does not exist** → CS0117. Use `ConvertFrom` or `PathFigureCollectionConverter.ParseStringToPathFigureCollection`. *(Already fixed in this repo.)* |
| 3 | `LineGeometry.AppendPath` **drops the start point** — use the `Line` shape. |
| 4 | `Rectangle.RadiusX`/`RadiusY` are **not** independent radii; `max(RadiusX, RadiusY)` is applied to all four corners. Use `RoundRectangle` for per-corner radii. |
| 5 | `RoundRectangle` corners are ordered **TL, TR, BL, BR** — not TL, BL, TR, BR. |
| 6 | `RoundRectangle.GetPath()` does **not** inset by `StrokeThickness` (Ellipse/Rectangle do). |
| 7 | The `F0`/`F1` path prefix is parsed but **ignored** — `FillRule` stays `EvenOdd`. |
| 8 | A path string must start with `M`/`m`, else `FormatException`. |
| 9 | `Path.RenderTransform` does **not** affect `GetPath()`; it is applied by the platform renderer. |
| 10 | `Polygon` closes the path, `Polyline` does not — same `Points`, different result. |
| 11 | `Shape` is a `View`, so `StrokeThickness` inflates the measured size by 1 per axis (default stroke = 1). Sized shapes measure to *request + stroke*; unsized shapes measure to *flattened path bounds + stroke*. |
| 11b | `PathF.Bounds` on a Bézier shape (`Ellipse`, `RoundRectangle`) is the **control-point hull** and overshoots the true outline (103.85×61.45 for a 99×59 ellipse). `GetBoundsByFlattening` does not tighten it. Use `Line`/`Polygon`/`Path` with straight segments when exact bounds matter. |
| 12 | `Microsoft.Maui.Controls.Shapes.Path` vs `System.IO.Path` — alias it. |
| 13 | `StrokeShapeTypeConverter` is the `Border.StrokeShape` XAML grammar (`"Path M0,0 …"`, `"RoundRectangle 8"`), and it is the only place path strings and shapes mix. |

---

## Appendix — Reproducing this research

```bash
# 1. Pull the real assemblies
for p in microsoft.maui.controls.core microsoft.maui.graphics microsoft.maui.core microsoft.maui.essentials; do
  curl -sS -o $p.nupkg "https://api.nuget.org/v3-flatcontainer/$p/10.0.120/$p.10.0.120.nupkg"
  unzip -o -q $p.nupkg -d ext
done

# 2. Reflect over the namespace (the probe used for this document)
#    - asm.GetExportedTypes().Where(t => t.Namespace == "Microsoft.Maui.Controls.Shapes")
#    - BindableProperty static fields + DefaultValue
#    - instantiate each shape and call GetPath()
#    - parse path strings via new PathGeometryConverter().ConvertFrom(null, CultureInfo.InvariantCulture, s)
```

The `Path` mini-language parser lives in
`src/Controls/src/Core/Shapes/PathFigureCollectionConverter.cs`
(`ParseStringToPathFigureCollection`) in the `dotnet/maui` repository.
