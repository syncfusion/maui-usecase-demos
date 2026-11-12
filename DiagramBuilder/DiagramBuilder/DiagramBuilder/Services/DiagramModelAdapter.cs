using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using Syncfusion.Maui.Diagram;
using HorizontalAlignment = Syncfusion.Maui.Diagram.HorizontalAlignment;
using VerticalAlignment = Syncfusion.Maui.Diagram.VerticalAlignment;

namespace DiagramBuilder.Services;

/// <summary>
/// Pure mapper between app domain models (<see cref="ShapeItem"/>, <see cref="ConnectorItem"/>)
/// and Syncfusion.Diagram model types (Node/Connector/annotations/styles).
/// References model types only — never the SfDiagram control.
/// </summary>
public static class DiagramModelAdapter
{
    private const string StencilShapeAnnotationPrefix = "stencil-shape-";

    /// <summary>
    /// Maps a domain <see cref="ShapeItem"/> to a Syncfusion <see cref="Node"/>,
    /// setting an explicit <c>Id</c> so connector <c>SourceID</c>/<c>TargetID</c> resolve.
    /// </summary>
    public static Node ToNode(ShapeItem shape)
    {
        var node = new Node
        {
            Id = shape.Id,
            OffsetX = shape.PositionX,
            OffsetY = shape.PositionY,
            Width = shape.Width,
            Height = shape.Height,
            RotationAngle = shape.Rotation,
            Shape = ToShape(shape.ShapeType),
        };

        ApplyStyle(node, shape);
        ApplyAnnotation(node, shape);
        return node;
    }

    /// <summary>
    /// Maps a domain <see cref="ConnectorItem"/> to a Syncfusion <see cref="Connector"/>.
    /// </summary>
    public static Connector ToConnector(ConnectorItem connector)
    {
        var result = new Connector
        {
            Id = connector.Id,
            SourceID = connector.SourceId,
            TargetID = connector.TargetId,
            Type = connector.ConnectorKind switch
            {
                ConnectorKind.Orthogonal => ConnectorSegmentType.Orthogonal,
                // Straight and Bezier both render via control defaults for straight segments.
                _ => ConnectorSegmentType.Straight,
            },
        };

        if (string.IsNullOrEmpty(connector.SourceId))
        {
            result.SourcePoint = new Microsoft.Maui.Graphics.Point(connector.SourceX, connector.SourceY);
        }

        if (string.IsNullOrEmpty(connector.TargetId))
        {
            result.TargetPoint = new Microsoft.Maui.Graphics.Point(connector.TargetX, connector.TargetY);
        }

        ApplyConnectorStyle(result, connector);
        return result;
    }

    public static ShapeItem ToShapeItem(Node node)
    {
        var style = node.Style as ShapeStyle;
        var annotation = node.Annotations.OfType<ShapeAnnotation>().FirstOrDefault();
        var textStyle = annotation?.Style;

        return new ShapeItem
        {
            Id = node.Id,
            Text = annotation?.Content ?? string.Empty,
            ShapeType = FromShape(node.Shape),
            PositionX = node.OffsetX,
            PositionY = node.OffsetY,
            Width = node.Width,
            Height = node.Height,
            Rotation = node.RotationAngle,
            FillColorKey = KeyForColor(style?.Fill),
            FillOpacity = style?.Opacity ?? 1,
            BorderColorKey = KeyForColor(style?.StrokeColor),
            BorderThickness = style?.StrokeWidth ?? 0,
            BorderStyle = style?.StrokeDashArray switch
            {
                "4 2" => "dashed",
                "1 2" => "dotted",
                _ => string.Empty,
            },
            FontFamily = textStyle?.FontFamily ?? string.Empty,
            FontSize = textStyle?.FontSize ?? 0,
            TextBold = textStyle?.Bold ?? false,
            TextItalic = textStyle?.Italic ?? false,
            TextUnderline = false,
            TextColorKey = KeyForColor(textStyle?.Color),
            HorizontalAlignment = annotation?.HorizontalAlignment.ToString() ?? string.Empty,
            VerticalAlignment = annotation?.VerticalAlignment.ToString() ?? string.Empty,
        };
    }

    public static ConnectorItem ToConnectorItem(Connector connector)
    {
        var annotation = connector.Annotations.OfType<PathAnnotation>().FirstOrDefault();
        var style = connector.Style as ShapeStyle;
        return new ConnectorItem
        {
            Id = connector.Id,
            Text = annotation?.Content ?? string.Empty,
            SourceId = connector.SourceID ?? string.Empty,
            TargetId = connector.TargetID ?? string.Empty,
            SourceX = connector.SourcePoint?.X ?? 0,
            SourceY = connector.SourcePoint?.Y ?? 0,
            TargetX = connector.TargetPoint?.X ?? 0,
            TargetY = connector.TargetPoint?.Y ?? 0,
            ConnectorKind = connector.Type == ConnectorSegmentType.Orthogonal
                ? ConnectorKind.Orthogonal
                : ConnectorKind.Straight,
            StrokeColorKey = KeyForColor(style?.StrokeColor),
            StrokeThickness = style?.StrokeWidth ?? 0,
        };
    }

    /// <summary>
    /// Maps a domain <see cref="ShapeType"/> to the Syncfusion <see cref="Shape"/> object:
    /// flow shapes via <see cref="FlowShape"/>, basic shapes via <see cref="BasicShape"/>
    /// (Polygon reaches the JS bridge as a basic shape with Sides).
    /// </summary>
    public static Shape ToShape(ShapeType shapeType) => shapeType switch
    {
        ShapeType.Process => new FlowShape(NodeFlowShapes.Process),
        ShapeType.Decision => new FlowShape(NodeFlowShapes.Decision),
        ShapeType.Terminator => new FlowShape(NodeFlowShapes.Terminator),
        ShapeType.Document => new FlowShape(NodeFlowShapes.Document),
        ShapeType.DataPreparation => new FlowShape(NodeFlowShapes.Preparation),

        ShapeType.Rectangle => new BasicShape(NodeBasicShapes.Rectangle, 0),
        ShapeType.Circle => new BasicShape(NodeBasicShapes.Ellipse, 0),
        ShapeType.Triangle => new BasicShape(NodeBasicShapes.Triangle, 0),
        ShapeType.Hexagon => new BasicShape(NodeBasicShapes.Hexagon, 0),
        ShapeType.Diamond => new BasicShape(NodeBasicShapes.Diamond, 0),
        ShapeType.Pentagon => new BasicShape(NodeBasicShapes.Pentagon, 0),
        ShapeType.Star => new BasicShape(NodeBasicShapes.Star, 0),

        _ => new BasicShape(NodeBasicShapes.Rectangle, 0),
    };

    public static void RestoreStencilShape(Node node)
    {
        if ((TryGetStencilShapeType(node, out var shapeType)
                || TryGetStencilShapeType(node.Id, out shapeType))
            && FromShape(node.Shape) != shapeType)
        {
            node.Shape = ToShape(shapeType);
        }
    }

    private static bool TryGetStencilShapeType(Node node, out ShapeType shapeType)
    {
        var annotationId = node.Annotations
            .OfType<ShapeAnnotation>()
            .Select(annotation => annotation.Id)
            .FirstOrDefault(id => id.StartsWith(StencilShapeAnnotationPrefix, StringComparison.Ordinal));

        if (annotationId is not null
            && Enum.TryParse(
                annotationId[StencilShapeAnnotationPrefix.Length..],
                ignoreCase: true,
                out shapeType)
            && Enum.IsDefined(shapeType))
        {
            return true;
        }

        shapeType = default;
        return false;
    }

    private static bool TryGetStencilShapeType(string symbolId, out ShapeType shapeType)
    {
        foreach (var item in StencilCatalog.All.SelectMany(group => group.Items))
        {
            if (item.ShapeType is { } candidate
                && (symbolId.Equals(item.Id, StringComparison.Ordinal)
                    || symbolId.StartsWith(item.Id + "_", StringComparison.Ordinal)))
            {
                shapeType = candidate;
                return true;
            }
        }

        shapeType = default;
        return false;
    }

    private static ShapeType FromShape(Shape? shape) => shape switch
    {
        FlowShape flow => flow.Shape switch
        {
            NodeFlowShapes.Process => ShapeType.Process,
            NodeFlowShapes.Decision => ShapeType.Decision,
            NodeFlowShapes.Terminator => ShapeType.Terminator,
            NodeFlowShapes.Document => ShapeType.Document,
            NodeFlowShapes.Preparation => ShapeType.DataPreparation,
            _ => ShapeType.Process,
        },
        BasicShape basic => basic.Shape switch
        {
            NodeBasicShapes.Rectangle => ShapeType.Rectangle,
            NodeBasicShapes.Ellipse => ShapeType.Circle,
            NodeBasicShapes.Triangle => ShapeType.Triangle,
            NodeBasicShapes.Hexagon => ShapeType.Hexagon,
            NodeBasicShapes.Diamond => ShapeType.Diamond,
            NodeBasicShapes.Pentagon => ShapeType.Pentagon,
            NodeBasicShapes.Star => ShapeType.Star,
            _ => ShapeType.Rectangle,
        },
        _ => ShapeType.Rectangle,
    };

    private static string KeyForColor(Color? color)
    {
        if (color is null)
        {
            return string.Empty;
        }

        var closestKey = string.Empty;
        var closestDistance = double.MaxValue;
        foreach (var (key, candidate) in ResourceLookup.PaletteSnapshot())
        {
            var distance = ColorDistance(color, candidate);
            if (distance < closestDistance)
            {
                closestKey = key;
                closestDistance = distance;
            }
        }

        return closestKey;
    }

    private static double ColorDistance(Color? first, Color second) =>
        first is null
            ? double.MaxValue
            : Math.Abs(first.Red - second.Red)
                + Math.Abs(first.Green - second.Green)
                + Math.Abs(first.Blue - second.Blue);

    private static void ApplyStyle(Node node, ShapeItem shape)
    {
        node.Style = new ShapeStyle
        {
            Fill = ResourceLookup.Color(shape.FillColorKey),
            StrokeColor = ResourceLookup.Color(shape.BorderColorKey),
            StrokeWidth = shape.BorderThickness,
            Opacity = shape.FillOpacity,
        };

        if (string.Equals(shape.BorderStyle, "dashed", StringComparison.Ordinal))
        {
            node.Style.StrokeDashArray = "4 2";
        }
        else if (string.Equals(shape.BorderStyle, "dotted", StringComparison.Ordinal))
        {
            node.Style.StrokeDashArray = "1 2";
        }
    }

    private static void ApplyAnnotation(Node node, ShapeItem shape)
    {
        // Always attach a label, even when the template has no text: the property panel's
        // text options and in-canvas text entry both need a native annotation to target.
        var horizontal = ToHorizontalAlignment(shape.HorizontalAlignment);
        var vertical = ToVerticalAlignment(shape.VerticalAlignment);

        // The offset must match the alignment: Syncfusion's MAUI wrapper writes
        // the annotation offset as an {x, y} object while the underlying EJ2
        // diagram expects a single 0-1 fraction, and derives the label position
        // from it. Writing the matching fraction keeps the label on the side the
        // user asked for (see PropertyService.ApplyTextAlignment).
        var annotation = new ShapeAnnotation
        {
            Content = shape.Text ?? string.Empty,
            HorizontalAlignment = horizontal,
            VerticalAlignment = vertical,
            Offset = new Syncfusion.Maui.Diagram.DiagramPoint(
                HorizontalFraction(horizontal),
                VerticalFraction(vertical)),
        };

        if (double.IsFinite(shape.FontSize) && shape.FontSize > 0)
        {
            annotation.Style = new TextStyle
            {
                FontFamily = shape.FontFamily,
                FontSize = shape.FontSize,
                Bold = shape.TextBold,
                Italic = shape.TextItalic,
                Color = ResourceLookup.Color(shape.TextColorKey),
                Fill = ResourceLookup.Color(shape.FillColorKey),
            };
        }

        node.Annotations.Add(annotation);
    }

    private static void ApplyConnectorStyle(Connector connector, ConnectorItem connectorItem)
    {
        var stroke = new ShapeStyle
        {
            StrokeColor = ResourceLookup.Color(connectorItem.StrokeColorKey),
            StrokeWidth = connectorItem.StrokeThickness,
        };
        
        connector.Style = stroke;

        // Add arrow decorator to the target end
        connector.TargetDecorator = new DecoratorSettings
        {
            Shape = DecoratorShape.Arrow,
            Style = stroke,
        };

        connector.Annotations.Add(new PathAnnotation { Content = connectorItem.Text ?? string.Empty });
    }

    /// <summary>
    /// Maps a domain <see cref="StencilGroup"/> (with its <see cref="StencilItem"/>s) to a
    /// Syncfusion palette <see cref="Palette"/> so the shared <c>SymbolPalette</c> can
    /// offer drag-and-drop symbols (spec: shapes fed to the palette from the stencil catalog).
    /// </summary>
    public static Palette ToPalette(StencilGroup group)
    {
        var palette = new Palette
        {
            Id = group.Id,
            Title = group.Title,
            IsExpanded = true,
        };

        foreach (var item in group.Items)
        {
            if (item.ShapeType is { } shapeType)
            {
                palette.Symbols.Add(ToSymbolNode(item, ToShape(shapeType)));
            }
            else if (item.ConnectorKind is { } connectorKind)
            {
                palette.Symbols.Add(ToSymbolConnector(item, connectorKind));
            }
        }

        return palette;
    }

    /// <summary>Palette symbol node for a stencil shape (fixed preview size, label below).</summary>
    private static Node ToSymbolNode(StencilItem item, Shape shape)
    {
        var node = new Node
        {
            Id = item.Id,
            Shape = shape,
            Width = 64,
            Height = 64,
            Style = new ShapeStyle { Fill = ResourceLookup.Color(ResourceKeys.CanvasBackground) },
        };

        // The annotation ID carries the stencil type through nodeAdded even if
        // the palette changes the dropped node's ID. It remains invisible in the canvas.
        node.Annotations.Add(new ShapeAnnotation
        {
            Id = StencilShapeAnnotationPrefix + item.ShapeType!.Value,
        });
        return node;
    }

    /// <summary>
    /// Palette symbol for a connector stencil entry. Each kind gets a **distinct** source/target
    /// geometry within the 64×64 symbol cell so the previews are visually different:
    /// <list type="bullet">
    ///   <item>Straight → horizontal line with arrowhead</item>
    ///   <item>Orthogonal → L-shaped path (Type=Orthogonal forces right-angle routing)</item>
    ///   <item>Bezier → diagonal line (Type=Straight, since the SDK only supports Straight/Orthogonal;
    ///     diagonal direction visually suggests a curved connector)</item>
    /// </list>
    /// </summary>
    private static Connector ToSymbolConnector(StencilItem item, ConnectorKind connectorKind)
    {
        Microsoft.Maui.Graphics.Point source, target;
        ConnectorSegmentType segment;

        switch (connectorKind)
        {
            case ConnectorKind.Orthogonal:
                // L-shaped: the Orthogonal segment type routes this diagonal as right angles.
                source = new Microsoft.Maui.Graphics.Point(20, 14);
                target = new Microsoft.Maui.Graphics.Point(44, 50);
                segment = ConnectorSegmentType.Orthogonal;
                break;
            case ConnectorKind.Bezier:
                // The SDK wire format only has Straight/Orthogonal; a diagonal reads as the
                // curved-connector preview distinct from the two above.
                source = new Microsoft.Maui.Graphics.Point(14, 50);
                target = new Microsoft.Maui.Graphics.Point(50, 14);
                segment = ConnectorSegmentType.Straight;
                break;
            case ConnectorKind.Straight:
            default:
                source = new Microsoft.Maui.Graphics.Point(8, 32);
                target = new Microsoft.Maui.Graphics.Point(56, 32);
                segment = ConnectorSegmentType.Straight;
                break;
        }

        var stroke = new ShapeStyle
        {
            StrokeColor = ResourceLookup.Color(ResourceKeys.TextPrimary),
            StrokeWidth = 2,
        };

        return new Connector
        {
            Id = item.Id,
            Type = segment,
            SourcePoint = source,
            TargetPoint = target,
            Style = stroke,
            SourceDecorator = new DecoratorSettings { Shape = DecoratorShape.None },
            TargetDecorator = new DecoratorSettings { Shape = DecoratorShape.Arrow, Style = stroke },
        };
    }



    private static Syncfusion.Maui.Diagram.HorizontalAlignment ToHorizontalAlignment(string value) => value switch
    {
        "Left" => HorizontalAlignment.Left,
        "Right" => HorizontalAlignment.Right,
        "Stretch" => HorizontalAlignment.Stretch,
        _ => HorizontalAlignment.Center,
    };

    private static Syncfusion.Maui.Diagram.VerticalAlignment ToVerticalAlignment(string value) => value switch
    {
        "Top" => VerticalAlignment.Top,
        "Bottom" => VerticalAlignment.Bottom,
        "Stretch" => VerticalAlignment.Stretch,
        _ => VerticalAlignment.Center,
    };

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
}
