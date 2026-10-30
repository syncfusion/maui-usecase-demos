using System.Globalization;
using HotelManagerSample;
using Syncfusion.Maui.Charts;

namespace HotelManagerSample.Controls;

/// <summary>
/// Custom <see cref="ChartTrackballBehavior"/> that renders the Revenue chart's
/// "SATURDAY, SEP 08 \n $19,800.00 \n Direct: $13.4k • OTA: $6.4k" rich card
/// (matching the dashboard reference design) instead of the two stacked
/// per-series dark bubbles that the default <see cref="ChartTrackballBehavior"/>
/// produces when the chart has two <c>SplineAreaSeries</c>.
///
/// The trick is to override <see cref="LabelsGenerated"/>, which the chart
/// calls once per pointer move to give the behavior a chance to mutate the
/// <see cref="TrackballPointInfo"/> list before the labels are drawn. We:
///
/// <list type="number">
///   <item>Identify the "Direct" series' <see cref="TrackballPointInfo"/>
///         (the primary line) — fallback to whichever point is nearest.</item>
///   <item>Build a single multi-line <see cref="TrackballPointInfo.Label"/>
///         combining the date header, total, and per-series breakdown.</item>
///   <item>Drop the OTA series' <see cref="TrackballPointInfo"/> from the list
///         so Syncfusion only renders one bubble.</item>
/// </list>
/// </summary>
public sealed class RichRevenueTrackballBehavior : ChartTrackballBehavior
{
    /// <summary>
    /// Separator rendered between the Direct and OTA breakdown lines so a
    /// design tweak ("—" for "•" or a wider column) only needs to land in
    /// one place. Matches the upper-bound formatting used by the chart
    /// tooltip everywhere else in the dashboard.
    /// </summary>
    private const string Separator = " • ";

    /// <summary>
    /// Mutates the supplied <paramref name="pointInfos"/> list so the
    /// trackball renders a single consolidated bubble instead of two
    /// stacked per-series bubbles. Picks the Direct-series point as the
    /// anchor, builds a multi-line label combining the date header,
    /// the total and the per-series breakdown, and removes the OTA
    /// point from the list before letting the base class render.
    /// </summary>
    /// <param name="pointInfos">Per-series points the chart supplies
    /// for the current pointer position. Mutated in place — the OTA
    /// entry is removed when present.</param>
    protected override void LabelsGenerated(System.Collections.Generic.List<TrackballPointInfo> pointInfos)
    {
        if (pointInfos is null || pointInfos.Count == 0)
        {
            base.LabelsGenerated(pointInfos!);
            return;
        }

        // Prefer the Direct series' info so we always pick the primary
        // line's revenue source. Fallback to the first point if the chart
        // configuration ever changes.
        var directInfo = pointInfos.FirstOrDefault(IsDirectSeries)
                         ?? pointInfos[0];

        var directPoint = directInfo.DataItem as RevenuePoint;
        if (directPoint is null)
        {
            base.LabelsGenerated(pointInfos);
            return;
        }

        var otaInfo = pointInfos.FirstOrDefault(IsOtaSeries);

        var total  = ToTotalLine(directPoint.DirectRevenue, otaInfo?.DataItem is RevenuePoint otaPoint ? otaPoint.OtaRevenue : 0);
        var breakdown = ToBreakdownLine(directPoint.DirectRevenue,
                                        otaInfo?.DataItem is RevenuePoint ota ? ota.OtaRevenue : directPoint.OtaRevenue);

        // Multi-line label. Syncfusion's default label renderer supports
        // embedded '\n' as long as MultiLineLabel semantics are honoured
        // by the platform; that's enough here to produce three lines.
        directInfo.Label = $"{total}\n{breakdown}";

        // Style the bubble slightly larger so the multi-line text fits.
        if (directInfo.LabelStyle is null)
            directInfo.LabelStyle = new ChartLabelStyle();

        directInfo.LabelStyle.Margin = new Thickness(10, 7);
        directInfo.LabelStyle.FontSize = 11;
        directInfo.LabelStyle.FontFamily = "OpenSansSemibold";

        // Drop the OTA point so only the rich consolidated card renders.
        if (otaInfo is not null && !ReferenceEquals(otaInfo, directInfo))
            pointInfos.Remove(otaInfo);

        base.LabelsGenerated(pointInfos);
    }

    /// <summary>True when the supplied <paramref name="info"/> belongs
    /// to the "Direct" series.</summary>
    /// <param name="info">Trackball point candidate supplied by the
    /// chart.</param>
    /// <returns><c>true</c> when the source series label is "Direct";
    /// otherwise <c>false</c>.</returns>
    private static bool IsDirectSeries(TrackballPointInfo info)
        => info.Series is { Label: "Direct" };

    /// <summary>True when the supplied <paramref name="info"/> belongs
    /// to the "OTA" series.</summary>
    /// <param name="info">Trackball point candidate supplied by the
    /// chart.</param>
    /// <returns><c>true</c> when the source series label is "OTA";
    /// otherwise <c>false</c>.</returns>
    private static bool IsOtaSeries(TrackballPointInfo info)
        => info.Series is { Label: "OTA" };

    /// <summary>
    /// Renders the bold `$19,800.00` total line. Stored values are in
    /// thousands so we multiply by 1000 to get the actual currency amount.
    /// </summary>
    /// <param name="directRevenueK">Direct-series revenue expressed in
    /// thousands of dollars.</param>
    /// <param name="otaRevenueK">OTA-series revenue expressed in
    /// thousands of dollars.</param>
    /// <returns>The total formatted as a dollar amount with two decimal
    /// digits.</returns>
    private static string ToTotalLine(double directRevenueK, double otaRevenueK)
    {
        var totalUsd = (directRevenueK + otaRevenueK) * 1000.0;
        return totalUsd.ToString("$#,##0.00", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Bottom line: "Direct: $13.4k • OTA: $6.4k". Pass-through, no rounding
    /// — preserved from the per-series Y axes so the breakdown matches the
    /// line heights in the design.
    /// </summary>
    /// <param name="directK">Direct-series revenue expressed in
    /// thousands of dollars.</param>
    /// <param name="otaK">OTA-series revenue expressed in thousands
    /// of dollars.</param>
    /// <returns>The two-line breakdown text joined with the
    /// <see cref="Separator"/> token.</returns>
    private static string ToBreakdownLine(double directK, double otaK)
        => $"Direct: {directK.ToString("$#,##0.0\"k\"", CultureInfo.InvariantCulture)}"
           + Separator
           + $"OTA: {otaK.ToString("$#,##0.0\"k\"", CultureInfo.InvariantCulture)}";
}
