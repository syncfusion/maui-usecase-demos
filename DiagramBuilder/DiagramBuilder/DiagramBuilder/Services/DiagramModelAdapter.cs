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
        ShapeType.Diamond => new FlowShape(NodeFlowShapes.Decision),
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

        var annotation = new ShapeAnnotation
        {
            Content = shape.Text,
            HorizontalAlignment = ToHorizontalAlignment(shape.HorizontalAlignment),
            VerticalAlignment = ToVerticalAlignment(shape.VerticalAlignment),
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
        connector.Style = new ShapeStyle
        {
            StrokeColor = ResourceLookup.Color(connectorItem.StrokeColorKey),
            StrokeWidth = connectorItem.StrokeThickness,
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
}
