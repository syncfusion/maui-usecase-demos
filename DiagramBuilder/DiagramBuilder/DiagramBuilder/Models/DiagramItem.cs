namespace DiagramBuilder.Models;

/// <summary>
/// Base class for everything placed on the diagram canvas.
/// Models are plain POCOs — change notification is owned by the presentation layer.
/// </summary>
public abstract class DiagramItem
{
    public string Id { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double Rotation { get; set; }

    public bool LockAspectRatio { get; set; }
}

/// <summary>
/// A shape node on the canvas.
/// </summary>
public class ShapeItem : DiagramItem
{
    public ShapeType ShapeType { get; set; }

    // Fill
    public string FillColorKey { get; set; } = string.Empty;

    public double FillOpacity { get; set; } = 1.0;

    // Border
    public string BorderColorKey { get; set; } = string.Empty;

    public string BorderStyle { get; set; } = string.Empty;

    public double BorderThickness { get; set; }

    public double BorderOpacity { get; set; } = 1.0;

    // Text
    public string FontFamily { get; set; } = string.Empty;

    public double FontSize { get; set; }

    public bool TextBold { get; set; }

    public bool TextItalic { get; set; }

    public bool TextUnderline { get; set; }

    public string TextColorKey { get; set; } = string.Empty;

    public string HorizontalAlignment { get; set; } = string.Empty;

    public string VerticalAlignment { get; set; } = string.Empty;
}

/// <summary>
/// A connector between two nodes (by id) or two free points.
/// </summary>
public class ConnectorItem : DiagramItem
{
    public string SourceId { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public double SourceX { get; set; }

    public double SourceY { get; set; }

    public double TargetX { get; set; }

    public double TargetY { get; set; }

    public ConnectorKind ConnectorKind { get; set; }

    public string StrokeColorKey { get; set; } = string.Empty;

    public double StrokeThickness { get; set; }

    public double StrokeOpacity { get; set; } = 1.0;
}