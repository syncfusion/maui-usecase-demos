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

    /// <summary>Which editor control the panel builds for this row.</summary>
    public PropertyEditorKind Editor { get; set; } = PropertyEditorKind.Text;

    /// <summary>
    /// Optional inline caption rendered above the editor. The design places
    /// "Border / Color / Thickness" captions over a single control row.
    /// </summary>
    public string? CaptionKey { get; set; }

    /// <summary>Pre-computed options for <see cref="PropertyEditorKind.Combo"/> editors.</summary>
    public IReadOnlyList<string> Options { get; set; } = [];

    /// <summary>True when the row should span the full panel width (no fixed label column).</summary>
    public bool IsFullWidth { get; set; }

    /// <summary>
    /// Short prefix rendered inside the editor (e.g. "X", "Y", "W", "H" in the
    /// dimensions grid). Empty when the row has no inline caption.
    /// </summary>
    public string PrefixKey { get; set; } = string.Empty;

    /// <summary>
    /// Zero-based column of this row inside a multi-editor row. A value of 0 with
    /// <see cref="PairCount"/> of 1 means the row spans the full panel width.
    /// </summary>
    public int PairIndex { get; set; }

    /// <summary>Number of editors sharing this row (1 = full width).</summary>
    public int PairCount { get; set; } = 1;

    /// <summary>
    /// Factor applied to the editor value before it reaches the diagram model.
    /// The opacity slider is displayed on a 0-100 scale but ShapeStyle.Opacity is
    /// a 0-1 value, so the panel scales it by 0.01 on the way out and back in.
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// True when the value is a whole number, so the numeric editor shows it
    /// without decimals (rotation, font size, border thickness).
    /// </summary>
    public bool IsInteger { get; set; }
}

/// <summary>
/// Editor control requested for a <see cref="PropertyModel"/> row.
/// </summary>
public enum PropertyEditorKind
{
    /// <summary>Plain text entry.</summary>
    Text,

    /// <summary>Whole-number / decimal entry (SfNumericEntry).</summary>
    Number,

    /// <summary>Drop-down of fixed options (SfComboBox).</summary>
    Combo,

    /// <summary>Swatch that opens a colour picker popup.</summary>
    Color,

    /// <summary>Continuous value track (SfSlider).</summary>
    Slider,

    /// <summary>Mutually exclusive option strip (SfSegmentedControl).</summary>
    Segmented,

    /// <summary>On/off switch (SfSwitch).</summary>
    Toggle,

    /// <summary>Read-only caption (no editor).</summary>
    Caption,
}
