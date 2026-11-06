using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using Syncfusion.Maui.Diagram;

namespace DiagramBuilder.Services;

/// <summary>
/// <see cref="IPropertyService"/> implementation: bridges property-panel edits to
/// the live Syncfusion Node/Connector model objects (never the SfDiagram control).
/// Setting properties on tracked elements triggers the control's dirty-flush.
/// </summary>
public class PropertyService : IPropertyService
{
    private readonly Dictionary<string, NodeBase> _tracked = [];
    private readonly ISelectionService _selectionService;
    private readonly IPageSettingsService _pageSettings;

    public PropertyService(ISelectionService selectionService, IPageSettingsService pageSettings)
    {
        _selectionService = selectionService;
        _pageSettings = pageSettings;
    }

    public void Track(IEnumerable<object> nodes, IEnumerable<object> connectors)
    {
        _tracked.Clear();
        foreach (var node in nodes.OfType<Node>())
        {
            _tracked[node.Id] = node;
        }

        foreach (var connector in connectors.OfType<Connector>())
        {
            _tracked[connector.Id] = connector;
        }
    }

    public void TrackAdded(object element)
    {
        switch (element)
        {
            case Node node when !string.IsNullOrEmpty(node.Id):
                _tracked[node.Id] = node;
                break;
            case Connector connector when !string.IsNullOrEmpty(connector.Id):
                _tracked[connector.Id] = connector;
                break;
        }
    }

    public void Untrack(string elementId) => _tracked.Remove(elementId);

    public void Apply(string elementId, PropertyModel property)
    {
        // Page-level properties apply to the drawing area, not to an element.
        if (ApplyPage(property))
        {
            return;
        }

        if (!_tracked.TryGetValue(elementId, out var element))
        {
            return;
        }

        switch (property.Key)
        {
            case "PositionX":
                if (element is Node n1 && property.Value is double x) n1.OffsetX = x;
                break;
            case "PositionY":
                if (element is Node n2 && property.Value is double y) n2.OffsetY = y;
                break;
            case "Width":
                if (element is Node n3 && property.Value is double w) n3.Width = w;
                break;
            case "Height":
                if (element is Node n4 && property.Value is double h) n4.Height = h;
                break;
            case "Rotation":
                if (element is Node n5 && property.Value is double r) n5.RotationAngle = r;
                break;
            case "BackgroundColor":
            case "Fill":
            case "FillColorKey":
            case "FillColor":
                // "FillColor"/"BorderColor" are sent by the toolbar colour swatches;
                // "Fill" comes from the property panel. Both must be handled or the
                // toolbar pickers silently do nothing.
                ApplyStyle(element, fill: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "Border":
            case "BorderColor":
                ApplyStyle(element, stroke: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "BorderThickness":
                if (element.Style is ShapeStyle s1 && property.Value is double t) s1.StrokeWidth = t;
                break;
            case "BorderStyle":
                ApplyBorderStyle(element, property.Value?.ToString());
                break;
            case "Opacity":
            case "BorderOpacity":
            case "FillOpacity":
                // ShapeStyle.Opacity is the single element-wide opacity the control exposes.
                if (element.Style is ShapeStyle s2 && property.Value is double o) s2.Opacity = o;
                break;
            case "Text":
                SetAnnotationContent(element, property.Value?.ToString() ?? string.Empty);
                break;
            case "FontFamily":
                ApplyTextStyle(element, fontFamily: property.Value?.ToString());
                break;
            case "FontSize":
                ApplyTextStyle(element, fontSize: property.Value as double?);
                break;
            case "TextBold":
                ApplyTextStyle(element, bold: property.Value as bool?);
                break;
            case "TextItalic":
                ApplyTextStyle(element, italic: property.Value as bool?);
                break;
            case "TextColor":
                ApplyTextStyle(element, color: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "HorizontalAlignment":
                ApplyTextAlignment(element, horizontal: HorizontalAlignmentFor(property.Value?.ToString()));
                break;
            case "VerticalAlignment":
                ApplyTextAlignment(element, vertical: VerticalAlignmentFor(property.Value?.ToString()));
                break;
        }
    }

    public IReadOnlyList<PropertyModel> Read(string elementId)
    {
        if (!_tracked.TryGetValue(elementId, out var element))
        {
            return [];
        }

        var result = new List<PropertyModel>();

        if (element is Node node)
        {
            // DIMENSIONS — the design pairs the fields two-per-row (X | Y, W | H)
            // with a single-letter prefix rendered inside each editor.
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "PositionX", LabelKey = AppStrings.PropertyPositionX, PrefixKey = "X", Value = node.OffsetX, Min = 0, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "PositionY", LabelKey = AppStrings.PropertyPositionY, PrefixKey = "Y", Value = node.OffsetY, Min = 0, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Width", LabelKey = AppStrings.PropertyWidth, PrefixKey = "W", Value = node.Width, Min = 10, Max = 2000, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Height", LabelKey = AppStrings.PropertyHeight, PrefixKey = "H", Value = node.Height, Min = 10, Max = 2000, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Rotation", LabelKey = AppStrings.PropertyRotation, PrefixKey = "R", Value = node.RotationAngle, Min = 0, Max = 360, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 0, PairCount = 2 });

            var style = node.Style as ShapeStyle;

            // FILL — the MAUI Diagram's ShapeStyle exposes a single flat Fill colour
            // (no gradient model), so one colour picker covers it. A separate
            // BACKGROUND section is not offered for shapes: it would duplicate
            // the same ShapeStyle.Fill value.
            result.Add(new PropertyModel { Group = PropertyGroup.Fill, Key = "Fill", LabelKey = AppStrings.PropertyFillColor, Value = ColorKeyFor(style?.Fill), Editor = PropertyEditorKind.Color, IsFullWidth = true });

            // BORDER — the design puts the style / colour / thickness editors on
            // one row, each under its own caption.
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderStyle", LabelKey = AppStrings.PropertyLineStyle, CaptionKey = AppStrings.PropertyBorderCaption, Value = BorderStyleFor(style?.StrokeDashArray), Editor = PropertyEditorKind.Combo, Options = BorderStyleOptions, PairIndex = 0, PairCount = 3 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "Border", LabelKey = AppStrings.PropertyBorderColor, CaptionKey = AppStrings.PropertyColorCaption, Value = ColorKeyFor(style?.StrokeColor), Editor = PropertyEditorKind.Color, PairIndex = 1, PairCount = 3 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderThickness", LabelKey = AppStrings.PropertyThickness, CaptionKey = AppStrings.PropertyThicknessCaption, Value = style?.StrokeWidth ?? 1.0, Min = 0, Max = 64, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 2, PairCount = 3 });

            // OPACITY — ShapeStyle.Opacity is the only opacity the control exposes and it
            // applies to the whole element (fill + stroke together), so a single row is shown.
            // The design shows a 0-100 scale, so the value is scaled on the way in and out.
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "Opacity", LabelKey = AppStrings.PropertyOpacity, Value = (style?.Opacity ?? 1.0) * 100.0, Min = 0, Max = 100, Editor = PropertyEditorKind.Slider, Scale = 0.01 });

            var annotation = node.Annotations.OfType<ShapeAnnotation>().FirstOrDefault();
            var textStyle = annotation?.Style;

            // TEXT — the design lays out font family / size and the alignment
            // dropdowns two-per-row, then a bold/italic toggle row.
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "FontFamily", LabelKey = AppStrings.PropertyFontFamily, Value = textStyle?.FontFamily ?? "Arial", Editor = PropertyEditorKind.Combo, Options = FontFamilyOptions, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "FontSize", LabelKey = AppStrings.PropertyFontSize, Value = textStyle?.FontSize ?? 12.0, Min = 6, Max = 96, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "HorizontalAlignment", LabelKey = AppStrings.PropertyHorizontalAlign, Value = HorizontalKeyFor(annotation?.HorizontalAlignment), Editor = PropertyEditorKind.Combo, Options = HorizontalAlignOptions, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "VerticalAlignment", LabelKey = AppStrings.PropertyVerticalAlign, Value = VerticalKeyFor(annotation?.VerticalAlignment), Editor = PropertyEditorKind.Combo, Options = VerticalAlignOptions, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextBold", LabelKey = AppStrings.PropertyBold, Value = textStyle?.Bold ?? false, Editor = PropertyEditorKind.Toggle, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextItalic", LabelKey = AppStrings.PropertyItalic, Value = textStyle?.Italic ?? false, Editor = PropertyEditorKind.Toggle, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextColor", LabelKey = AppStrings.PropertyTextColor, Value = ColorKeyFor(textStyle?.Color), Editor = PropertyEditorKind.Color, PairIndex = 0, PairCount = 2 });
        }
        else if (element is Connector connector)
        {
            // CONNECTOR — no position/size (its geometry is derived from the two
            // endpoints), so the panel shows line style, colour, thickness and the
            // label's text properties.
            var style = connector.Style as ShapeStyle;

            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderStyle", LabelKey = AppStrings.PropertyLineStyle, CaptionKey = AppStrings.PropertyBorderCaption, Value = BorderStyleFor(style?.StrokeDashArray), Editor = PropertyEditorKind.Combo, Options = BorderStyleOptions, PairIndex = 0, PairCount = 3 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "Border", LabelKey = AppStrings.PropertyBorderColor, CaptionKey = AppStrings.PropertyColorCaption, Value = ColorKeyFor(style?.StrokeColor), Editor = PropertyEditorKind.Color, PairIndex = 1, PairCount = 3 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderThickness", LabelKey = AppStrings.PropertyThickness, CaptionKey = AppStrings.PropertyThicknessCaption, Value = style?.StrokeWidth ?? 1.0, Min = 0, Max = 64, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 2, PairCount = 3 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "Opacity", LabelKey = AppStrings.PropertyOpacity, Value = (style?.Opacity ?? 1.0) * 100.0, Min = 0, Max = 100, Editor = PropertyEditorKind.Slider, Scale = 0.01 });

            var annotation = connector.Annotations.OfType<PathAnnotation>().FirstOrDefault();
            var textStyle = annotation?.Style;
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "Text", LabelKey = AppStrings.PropertyTextContent, Value = annotation?.Content, Editor = PropertyEditorKind.Text, IsFullWidth = true });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "FontFamily", LabelKey = AppStrings.PropertyFontFamily, Value = textStyle?.FontFamily ?? "Arial", Editor = PropertyEditorKind.Combo, Options = FontFamilyOptions, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "FontSize", LabelKey = AppStrings.PropertyFontSize, Value = textStyle?.FontSize ?? 12.0, Min = 6, Max = 96, Editor = PropertyEditorKind.Number, IsInteger = true, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextBold", LabelKey = AppStrings.PropertyBold, Value = textStyle?.Bold ?? false, Editor = PropertyEditorKind.Toggle, PairIndex = 0, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextItalic", LabelKey = AppStrings.PropertyItalic, Value = textStyle?.Italic ?? false, Editor = PropertyEditorKind.Toggle, PairIndex = 1, PairCount = 2 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextColor", LabelKey = AppStrings.PropertyTextColor, Value = ColorKeyFor(textStyle?.Color), Editor = PropertyEditorKind.Color, PairIndex = 0, PairCount = 2 });
        }

        return result;
    }

    /// <summary>
    /// Applies a drawing-area property. Returns true when the property was a
    /// page property and has been handled.
    ///
    /// Page settings support in the MAUI Diagram control (verified against the
    /// Syncfusion.Maui.Diagram assembly): the control exposes NO page-settings
    /// API — paper format and orientation exist only on
    /// <c>DiagramExportOptions</c> (consumed by ExportDiagram/Print), and even
    /// there EJ2 falls back to its own defaults unless every option is set.
    /// Only the canvas background is genuinely controllable (via the MAUI host
    /// views behind the transparent EJ2 canvas), so that is the only option
    /// this panel exposes. Format/orientation options were removed rather than
    /// kept as non-functional placeholders.
    /// </summary>
    private bool ApplyPage(PropertyModel property)
    {
        switch (property.Key)
        {
            case "PageBackground":
                _pageSettings.SetBackgroundColor(property.Value?.ToString() ?? string.Empty);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Drawing-area properties, shown when no shape is selected: only the
    /// canvas background. The MAUI Diagram control has no page
    /// format/orientation API (page settings exist solely on
    /// <c>DiagramExportOptions</c> for export/print), so those unsupported
    /// options are not exposed in the UI.
    /// </summary>
    public IReadOnlyList<PropertyModel> ReadPage()
    {
        return
        [
            new PropertyModel
            {
                Group = PropertyGroup.Background,
                Key = "PageBackground",
                LabelKey = AppStrings.PropertyPageBackground,
                Value = _pageSettings.BackgroundColorKey,
                Editor = PropertyEditorKind.Color,
                IsFullWidth = true,
            },
        ];
    }

    /// <summary>Dash-style options offered by the border style dropdown.</summary>
    private static readonly string[] BorderStyleOptions =
    [
        AppStrings.PropertyBorderSolid,
        AppStrings.PropertyBorderDashed,
        AppStrings.PropertyBorderDotted,
    ];

    /// <summary>Font families offered by the text font dropdown.</summary>
    private static readonly string[] FontFamilyOptions =
    [
        "Arial",
        "Segoe UI",
        "Calibri",
        "Verdana",
        "Tahoma",
        "Times New Roman",
        "Courier New",
    ];

    /// <summary>Horizontal text alignment options.</summary>
    private static readonly string[] HorizontalAlignOptions =
    [
        AppStrings.PropertyAlignLeft,
        AppStrings.PropertyAlignCenter,
        AppStrings.PropertyAlignRight,
    ];

    /// <summary>Vertical text alignment options.</summary>
    private static readonly string[] VerticalAlignOptions =
    [
        AppStrings.PropertyAlignTop,
        AppStrings.PropertyAlignMiddle,
        AppStrings.PropertyAlignBottom,
    ];

    private static string? ColorKeyFor(Color? color)
    {
        if (color is null)
        {
            return null;
        }

        // Map the live color back to the nearest theme key so the picker shows it.
        var palette = ResourceLookup.PaletteSnapshot();
        var (bestKey, bestDistance) = (string.Empty, double.MaxValue);
        foreach (var (key, candidate) in palette)
        {
            var d = Math.Abs(color.Red - candidate.Red)
                  + Math.Abs(color.Green - candidate.Green)
                  + Math.Abs(color.Blue - candidate.Blue);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestKey = key;
            }
        }

        return bestKey;
    }

    private static string BorderStyleFor(string? dashArray)
    {
        if (string.IsNullOrEmpty(dashArray))
        {
            return AppStrings.PropertyBorderSolid;
        }

        return dashArray switch
        {
            "4 2" => AppStrings.PropertyBorderDashed,
            "1 2" => AppStrings.PropertyBorderDotted,
            _ => AppStrings.PropertyBorderSolid,
        };
    }

    private static string HorizontalKeyFor(Syncfusion.Maui.Diagram.HorizontalAlignment? alignment)
    {
        return alignment switch
        {
            Syncfusion.Maui.Diagram.HorizontalAlignment.Center => AppStrings.PropertyAlignCenter,
            Syncfusion.Maui.Diagram.HorizontalAlignment.Right => AppStrings.PropertyAlignRight,
            _ => AppStrings.PropertyAlignLeft,
        };
    }

    private static string VerticalKeyFor(Syncfusion.Maui.Diagram.VerticalAlignment? alignment)
    {
        return alignment switch
        {
            Syncfusion.Maui.Diagram.VerticalAlignment.Center => AppStrings.PropertyAlignMiddle,
            Syncfusion.Maui.Diagram.VerticalAlignment.Bottom => AppStrings.PropertyAlignBottom,
            _ => AppStrings.PropertyAlignTop,
        };
    }

    private static void ApplyStyle(NodeBase element, Color? fill = null, Color? stroke = null)
    {
        if (element.Style is not ShapeStyle style)
        {
            return;
        }

        if (fill is not null)
        {
            style.Fill = fill;
        }

        if (stroke is not null)
        {
            style.StrokeColor = stroke;
        }
    }

    private static void ApplyBorderStyle(NodeBase element, string? value)
    {
        if (element.Style is not ShapeStyle style)
        {
            return;
        }

        style.StrokeDashArray = value switch
        {
            AppStrings.PropertyBorderDashed => "4 2",
            AppStrings.PropertyBorderDotted => "1 2",
            _ => string.Empty,
        };
    }

    private static void ApplyTextStyle(NodeBase element, double? fontSize = null, bool? bold = null, bool? italic = null, Color? color = null, string? fontFamily = null)
    {
        TextStyle? textStyle = element switch
        {
            Node n => n.Annotations.OfType<ShapeAnnotation>().FirstOrDefault()?.Style,
            Connector c => c.Annotations.OfType<PathAnnotation>().FirstOrDefault()?.Style,
            _ => null,
        };

        if (textStyle is null)
        {
            return;
        }

        if (fontSize.HasValue)
        {
            textStyle.FontSize = fontSize.Value;
        }

        if (bold.HasValue)
        {
            textStyle.Bold = bold.Value;
        }

        if (italic.HasValue)
        {
            textStyle.Italic = italic.Value;
        }

        if (color is not null)
        {
            textStyle.Color = color;
        }

        if (!string.IsNullOrWhiteSpace(fontFamily))
        {
            textStyle.FontFamily = fontFamily;
        }
    }

    /// <summary>
    /// Applies the horizontal/vertical label alignment.
    ///
    /// The alignment alone is not enough: Syncfusion's MAUI wrapper writes the
    /// annotation offset as an <c>{x, y}</c> object while the underlying EJ2
    /// diagram expects a single 0-1 fraction. EJ2 derives the label's position
    /// from that offset, so with the wrapper's object shape the horizontal
    /// alignment ends up driving the vertical axis and the label appears
    /// mirrored (choosing "Left" moves the text up, "Right" moves it down).
    ///
    /// Writing the matching offset alongside the alignment makes the placement
    /// explicit: Left -&gt; x 0, Center -&gt; x 0.5, Right -&gt; x 1 and
    /// Top -&gt; y 0, Middle -&gt; y 0.5, Bottom -&gt; y 1.
    /// </summary>
    private static void ApplyTextAlignment(NodeBase element, Syncfusion.Maui.Diagram.HorizontalAlignment? horizontal = null, Syncfusion.Maui.Diagram.VerticalAlignment? vertical = null)
    {
        switch (element)
        {
            case Node node:
                foreach (var annotation in node.Annotations.OfType<ShapeAnnotation>())
                {
                    Align(annotation, horizontal, vertical);
                }

                break;

            case Connector connector:
                foreach (var annotation in connector.Annotations.OfType<PathAnnotation>())
                {
                    // A connector label is a path annotation: only the horizontal
                    // alignment is meaningful (the MAUI API maps Left/Right to
                    // EJ2's Before/After along the path).
                    if (horizontal.HasValue)
                    {
                        annotation.HorizontalAlignment = horizontal.Value;
                    }

                    if (vertical.HasValue)
                    {
                        annotation.VerticalAlignment = vertical.Value;
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Sets a node label's alignment together with the offset fraction that
    /// matches it, so the label lands where the user picked.
    /// </summary>
    private static void Align(ShapeAnnotation annotation, Syncfusion.Maui.Diagram.HorizontalAlignment? horizontal, Syncfusion.Maui.Diagram.VerticalAlignment? vertical)
    {
        // EJ2 anchors a label at its offset and places it relative to that anchor
        // (Left = label starts at the anchor, Right = label ends at it). The anchor
        // must therefore move together with the alignment. A *new* DiagramPoint is
        // assigned: mutating the existing one and re-assigning the same reference
        // raises no change notification, so the anchor stayed at 0.5 and the
        // labels came out mirrored (Left pushed text right, Right pushed it left).
        var x = annotation.Offset.X;
        var y = annotation.Offset.Y;

        if (horizontal.HasValue)
        {
            x = HorizontalFraction(horizontal.Value);
        }

        if (vertical.HasValue)
        {
            y = VerticalFraction(vertical.Value);
        }

        annotation.Offset = new Syncfusion.Maui.Diagram.DiagramPoint(x, y);

        if (horizontal.HasValue)
        {
            annotation.HorizontalAlignment = horizontal.Value;
        }

        if (vertical.HasValue)
        {
            annotation.VerticalAlignment = vertical.Value;
        }
    }

    /// <summary>Horizontal offset fraction that matches an alignment.</summary>
    private static double HorizontalFraction(Syncfusion.Maui.Diagram.HorizontalAlignment alignment) => alignment switch
    {
        Syncfusion.Maui.Diagram.HorizontalAlignment.Left => 0.0,
        Syncfusion.Maui.Diagram.HorizontalAlignment.Right => 1.0,
        _ => 0.5,
    };

    /// <summary>Vertical offset fraction that matches an alignment.</summary>
    private static double VerticalFraction(Syncfusion.Maui.Diagram.VerticalAlignment alignment) => alignment switch
    {
        Syncfusion.Maui.Diagram.VerticalAlignment.Top => 0.0,
        Syncfusion.Maui.Diagram.VerticalAlignment.Bottom => 1.0,
        _ => 0.5,
    };

    private static Syncfusion.Maui.Diagram.HorizontalAlignment? HorizontalAlignmentFor(string? value) => value switch
    {
        AppStrings.PropertyAlignCenter => Syncfusion.Maui.Diagram.HorizontalAlignment.Center,
        AppStrings.PropertyAlignRight => Syncfusion.Maui.Diagram.HorizontalAlignment.Right,
        _ => Syncfusion.Maui.Diagram.HorizontalAlignment.Left,
    };

    private static Syncfusion.Maui.Diagram.VerticalAlignment? VerticalAlignmentFor(string? value) => value switch
    {
        AppStrings.PropertyAlignMiddle => Syncfusion.Maui.Diagram.VerticalAlignment.Center,
        AppStrings.PropertyAlignBottom => Syncfusion.Maui.Diagram.VerticalAlignment.Bottom,
        _ => Syncfusion.Maui.Diagram.VerticalAlignment.Top,
    };

    private static void SetAnnotationContent(NodeBase element, string content)
    {
        switch (element)
        {
            case Node n:
                foreach (var annotation in n.Annotations.OfType<ShapeAnnotation>())
                {
                    annotation.Content = content;
                }

                break;

            case Connector c:
                foreach (var annotation in c.Annotations.OfType<PathAnnotation>())
                {
                    annotation.Content = content;
                }

                break;
        }
    }
}
