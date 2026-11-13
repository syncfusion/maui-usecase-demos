namespace DiagramBuilder.Models;

public sealed record PrintOptions(
    string Region,
    string PaperSize,
    string Orientation,
    bool ScaleToFit,
    double PageWidth,
    double PageHeight);
