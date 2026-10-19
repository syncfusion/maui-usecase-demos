namespace StockChart.model;

/// <summary>
/// Represents a range with start and end values.
/// </summary>
public class RangeModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeModel"/> class.
    /// </summary>
    /// <param name="start">The start value.</param>
    /// <param name="end">The end value.</param>
    public RangeModel(double start, double end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the start value.
    /// </summary>
    public double Start { get; }

    /// <summary>
    /// Gets the end value.
    /// </summary>
    public double End { get; }
}