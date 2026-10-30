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

    public PropertyService(ISelectionService selectionService)
    {
        _selectionService = selectionService;
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

    public void Apply(string elementId, PropertyModel property)
    {
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
            case "Fill":
                ApplyStyle(element, fill: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "FillColorKey":
                ApplyStyle(element, fill: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "Border":
                ApplyStyle(element, stroke: ResourceLookup.Color(property.Value?.ToString()));
                break;
            case "BorderThickness":
                if (element.Style is ShapeStyle s1 && property.Value is double t) s1.StrokeWidth = t;
                break;
            case "FillOpacity":
                if (element.Style is ShapeStyle s2 && property.Value is double o) s2.Opacity = o;
                break;
            case "Text":
                SetAnnotationContent(element, property.Value?.ToString() ?? string.Empty);
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
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "PositionX", LabelKey = AppStrings.PropertyPositionX, Value = node.OffsetX, Min = 0 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "PositionY", LabelKey = AppStrings.PropertyPositionY, Value = node.OffsetY, Min = 0 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Width", LabelKey = AppStrings.PropertyWidth, Value = node.Width, Min = 10, Max = 2000 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Height", LabelKey = AppStrings.PropertyHeight, Value = node.Height, Min = 10, Max = 2000 });
            result.Add(new PropertyModel { Group = PropertyGroup.Dimensions, Key = "Rotation", LabelKey = AppStrings.PropertyRotation, Value = node.RotationAngle, Min = 0, Max = 360 });

            var style = node.Style as ShapeStyle;
            result.Add(new PropertyModel { Group = PropertyGroup.Fill, Key = "Fill", LabelKey = AppStrings.PropertyFillColor, Value = ColorKeyFor(style?.Fill) });
            result.Add(new PropertyModel { Group = PropertyGroup.Fill, Key = "FillOpacity", LabelKey = AppStrings.PropertyOpacity, Value = style?.Opacity ?? 1.0, Min = 0, Max = 1 });

            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderStyle", LabelKey = AppStrings.PropertyLineStyle, Value = BorderStyleFor(style?.StrokeDashArray) });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "Border", LabelKey = AppStrings.PropertyBorderColor, Value = ColorKeyFor(style?.StrokeColor) });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderThickness", LabelKey = AppStrings.PropertyThickness, Value = style?.StrokeWidth ?? 1.0, Min = 0, Max = 64 });
            result.Add(new PropertyModel { Group = PropertyGroup.Border, Key = "BorderOpacity", LabelKey = AppStrings.PropertyOpacity, Value = style?.Opacity ?? 1.0, Min = 0, Max = 1 });

            var annotation = node.Annotations.OfType<ShapeAnnotation>().FirstOrDefault();
            var textStyle = annotation?.Style;
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "Text", LabelKey = AppStrings.PropertyTextContent, Value = annotation?.Content });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "FontSize", LabelKey = AppStrings.PropertyFontSize, Value = textStyle?.FontSize ?? 12.0, Min = 6, Max = 96 });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextBold", LabelKey = AppStrings.PropertyBold, Value = textStyle?.Bold ?? false });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextItalic", LabelKey = AppStrings.PropertyItalic, Value = textStyle?.Italic ?? false });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "TextColor", LabelKey = AppStrings.PropertyTextColor, Value = ColorKeyFor(textStyle?.Color) });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "HorizontalAlignment", LabelKey = AppStrings.PropertyHorizontalAlign, Value = HorizontalKeyFor(annotation?.HorizontalAlignment) });
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "VerticalAlignment", LabelKey = AppStrings.PropertyVerticalAlign, Value = VerticalKeyFor(annotation?.VerticalAlignment) });
        }

        return result;
    }

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

    private static void ApplyTextStyle(NodeBase element, double? fontSize = null, bool? bold = null, bool? italic = null)
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
    }

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
