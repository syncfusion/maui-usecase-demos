namespace VisitorPass.Models;

/// <summary>
/// One bar of the "Visits by Purpose" column chart
/// (Dashboard Section 3). One record per
/// <see cref="VisitPurpose"/>. The XAML binds these to
/// <c>SfCartesianChart.ColumnSeries.ItemsSource</c> with
/// <c>XBindingPath="Label"</c> and <c>YBindingPath="Count"</c>.
/// </summary>
public sealed record PurposeBreakdownPoint(
    VisitPurpose Purpose,
    string Label,
    int Count);
