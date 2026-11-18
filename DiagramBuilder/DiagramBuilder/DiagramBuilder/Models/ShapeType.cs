namespace DiagramBuilder.Models;

/// <summary>
/// Shape kinds available in the stencil and toolbar.
/// Flow shapes (Process…DataPreparation) plus basic geometric shapes.
/// </summary>
public enum ShapeType
{
    Process,
    Decision,
    Terminator,
    Document,
    DataPreparation,

    Rectangle,
    Circle,
    Triangle,
    Hexagon,
    Diamond,
    Pentagon,
    Star,
}