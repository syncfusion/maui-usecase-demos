using System.Collections.ObjectModel;

namespace DiagramBuilder.Models;

/// <summary>
/// Menu bar item: top-level root, submenu parent, leaf command, or separator.
/// </summary>
public class MenuCommandItem
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Key used to look up the localized text.</summary>
    public string TextKey { get; set; } = string.Empty;

    /// <summary>Gesture hint shown on the right of a flyout item (e.g. "Ctrl+Z").</summary>
    public string GestureKey { get; set; } = string.Empty;

    public string IconKey { get; set; } = string.Empty;

    public ObservableCollection<MenuCommandItem> Children { get; } = [];

    public bool IsSeparator { get; set; }

    public bool IsCheckable { get; set; }

    public bool IsChecked { get; set; }

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Toolbar button descriptor.
/// </summary>
public class ToolbarItem
{
    public string Id { get; set; } = string.Empty;

    public string IconKey { get; set; } = string.Empty;

    /// <summary>Key used to look up the localized tooltip.</summary>
    public string TooltipKey { get; set; } = string.Empty;

    public int Group { get; set; }

    public bool IsToggle { get; set; }

    public bool IsChecked { get; set; }

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Preset zoom entry (chips + menu).
/// </summary>
public class ZoomLevel
{
    /// <summary>Zoom factor (1.0 = 100 %).</summary>
    public double Value { get; set; }

    /// <summary>Key used to look up the localized chip label (e.g. "50 %").</summary>
    public string LabelKey { get; set; } = string.Empty;
}