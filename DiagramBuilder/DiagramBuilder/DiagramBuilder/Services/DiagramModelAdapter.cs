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
        if (string.IsNullOrEmpty(shape.Text))
        {
            return;
        }

        var horizontal = ToHorizontalAlignment(shape.HorizontalAlignment);
        var vertical = ToVerticalAlignment(shape.VerticalAlignment);

        // The offset must match the alignment: Syncfusion's MAUI wrapper writes
        // the annotation offset as an {x, y} object while the underlying EJ2
        // diagram expects a single 0-1 fraction, and derives the label position
        // from it. Writing the matching fraction keeps the label on the side the
        // user asked for (see PropertyService.ApplyTextAlignment).
        var annotation = new ShapeAnnotation
        {
            Content = shape.Text,
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

        if (!string.IsNullOrEmpty(connectorItem.Text))
        {
            connector.Annotations.Add(new PathAnnotation { Content = connectorItem.Text });
        }
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
    private static Node ToSymbolNode(StencilItem item, Shape shape) => new()
    {
        Id = item.Id,
        Shape = shape,
        Width = 64,
        Height = 64,
        Style = new ShapeStyle { Fill = ResourceLookup.Color(ResourceKeys.CanvasBackground) },
    };

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
