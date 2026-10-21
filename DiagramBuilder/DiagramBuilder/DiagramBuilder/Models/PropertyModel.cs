namespace DiagramBuilder.Models;

/// <summary>
/// Generic property-inspector field descriptor: one editable row in a
/// <see cref="PropertyGroup"/> section (label is a key into AppStrings).
/// </summary>
public class PropertyModel
{
    public string Id { get; set; } = string.Empty;

    public PropertyGroup Group { get; set; }

    /// <summary>Key used to look up the localized label.</summary>
    public string LabelKey { get; set; } = string.Empty;

    /// <summary>Property key applied to the selected item (e.g. "FillOpacity").</summary>
    public string Key { get; set; } = string.Empty;

    public object? Value { get; set; }

    public double? Min { get; set; }

    public double? Max { get; set; }

    public bool IsReadOnly { get; set; }
}