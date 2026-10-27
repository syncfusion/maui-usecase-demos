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

            var annotation = node.Annotations.OfType<ShapeAnnotation>().FirstOrDefault();
            result.Add(new PropertyModel { Group = PropertyGroup.Text, Key = "Text", LabelKey = AppStrings.PropertyTextContent, Value = annotation?.Content });
        }

        return result;
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
