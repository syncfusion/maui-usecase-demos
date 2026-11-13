namespace VisitorPass.Models;

/// <summary>
/// One row of the "Visitor Status Distribution" donut chart
/// (Dashboard Section 2). One record per
/// <see cref="VisitorStatus"/>. <see cref="Count"/> comes from the
/// store; <see cref="BrushKey"/> resolves the legend swatch via the
/// Syncfusion palette overrides in
/// <c>Resources/Styles/VisitorPassTheme.xaml</c>.
///
/// The XAML binds these to <c>SfCircularChart.DoughnutSeries.ItemsSource</c>
/// with <c>XBindingPath="Label"</c> and <c>YBindingPath="Count"</c>.
/// </summary>
public sealed record StatusBreakdownPoint(
    VisitorStatus Status,
    string Label,
    int Count)
{
    /// <summary>Palette key (resolved by the donut's PaletteBrushes).</summary>
    public string BrushKey => Status.BackgroundBrushKey();
}
