namespace VisitorPass.Models;

/// <summary>
/// Immutable snapshot of the four KPI counts the Dashboard Section 1
/// displays. Built by <c>DashboardViewModel</c> from
/// <c>IVisitorStore.Visitors</c> and raised whenever the store
/// mutates.
///
///   ExpectedVisitors   — count of visitors with status Expected
///   CheckedIn          — count of visitors currently on-site
///   CheckedOut         — count of visitors who have left today
///   OverdueVisitors    — count of visitors past their expected end
///                         time but not yet arrived
///   Cancelled          — count of visitors who have been cancelled
///
/// `Total` is provided for the top-of-page greeting and is the sum of
/// all four counts.
/// </summary>
public sealed record VisitorStats(
    int ExpectedVisitors,
    int CheckedIn,
    int CheckedOut,
    int OverdueVisitors,
    int Cancelled)
{
    /// <summary>Sum of all four counts; shown in the page greeting.</summary>
    public int Total => ExpectedVisitors + CheckedIn + CheckedOut + OverdueVisitors;

    /// <summary>Zero-value stats; useful for the placeholder state during page load.</summary>
    public static VisitorStats Empty { get; } = new(0, 0, 0, 0, 0);
}
