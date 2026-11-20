
namespace SmartExpense;

/// <summary>
/// ViewModel representing a single insight card displayed on the dashboard.
/// Designed to be immutable and lightweight for safe UI binding.
/// </summary>
public sealed class InsightCardViewModel
{
    /// <summary>
    /// Display title of the insight (for example, Balance or Income).
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Numeric value associated with the insight.
    /// </summary>
    public decimal Value { get; init; } = 0;

    /// <summary>
    /// Icon glyph used to visually represent the insight.
    /// </summary>
    public string IconGlyph { get; init; } = string.Empty;

    /// <summary>
    /// Foreground color of the icon.
    /// </summary>
    public Color IconColor { get; init; }

    /// <summary>
    /// Background color of the icon container.
    /// </summary>
    public Color IconBackground { get; init; }

    /// <summary>
    /// Indicates whether the icon should be displayed.
    /// </summary>
    public bool ShowIcon { get; init; } = true;
}
