using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisitorPass.Models;
using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// VP-040 — ViewModel for the Dashboard screen (Screen 1).
///
/// Subscribes to <see cref="IVisitorStore.VisitorsChanged"/> and
/// rebuilds five derived collections whenever the store mutates:
///
///   1. <see cref="Stats"/>        — KPI snapshot (Expected / CheckedIn /
///                                   CheckedOut / Overdue counts).
///   2. <see cref="StatusBreakdown"/> — Donut chart series (one row per
///                                   status, always five rows even
///                                   when a status has zero count so
///                                   the legend stays stable).
///   3. <see cref="PurposeBreakdown"/> — Column chart series (one row
///                                   per purpose, always four rows).
///   4. <see cref="TodayVisitors"/> — Capped 8-row list of today's
///                                   visits ordered by VisitTime, used
///                                   by the dashboard's activity grid.
///   5. <see cref="TodayVisitorsCount"/> — total count of today's
///                                   visits for the activity-grid
///                                   footer label.
///
/// Also exposes <see cref="RegisterVisitorCommand"/> (raised when the
/// page's TopNavBar register trigger is tapped) so the page code-behind
/// can route to <c>SfPopup</c> (desktop) or
/// <c>Navigation.PushModalAsync</c> (phone) without holding business
/// logic.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "dashboard";

    /// <summary>Maximum rows in the today's-activity grid.</summary>
    public const int TodayVisitorCap = 8;

    private readonly IVisitorStore _store;

    [ObservableProperty]
    private VisitorStats _stats = VisitorStats.Empty;

    [ObservableProperty]
    private int _todayVisitorsCount;

    [ObservableProperty]
    private DateTime _referenceTime = DateTime.Now;

    /// <summary>
    /// Donut-chart series: one <see cref="StatusBreakdownPoint"/> per
    /// <see cref="VisitorStatus"/>. The collection always has 5 rows
    /// so the legend stays stable when the underlying counts change.
    /// </summary>
    public ObservableCollection<StatusBreakdownPoint> StatusBreakdown { get; } = new();

    /// <summary>
    /// Column-chart series: one <see cref="PurposeBreakdownPoint"/>
    /// per <see cref="VisitPurpose"/>. The collection always has 4
    /// rows for the same reason.
    /// </summary>
    public ObservableCollection<PurposeBreakdownPoint> PurposeBreakdown { get; } = new();

    /// <summary>
    /// Today's activity grid source: up to <see cref="TodayVisitorCap"/>
    /// visitors whose <c>VisitDate</c> is today, ordered by VisitTime.
    /// </summary>
    public ObservableCollection<Visitor> TodayVisitors { get; } = new();

    /// <summary>
    /// Raised when the user taps the "Register Visitor" pill in the
    /// Dashboard's TopNavBar. The page code-behind routes this to
    /// either <c>SfPopup.IsOpen = true</c> (desktop) or
    /// <c>Navigation.PushModalAsync(new RegisterVisitorPage())</c> (phone).
    /// </summary>
    public event EventHandler? RegisterVisitorRequested;

    /// <summary>
    /// RelayCommand bound to the TopNavBar's RegisterClicked event by
    /// the page code-behind. Raising <see cref="RegisterVisitorRequested"/>
    /// keeps the page free of business logic per HARNESS §6 rule 2.
    /// </summary>
    [RelayCommand]
    private void RegisterVisitor() => RegisterVisitorRequested?.Invoke(this, EventArgs.Empty);

    public DashboardViewModel(IVisitorStore store)
    {
        _store = store;
        _store.VisitorsChanged += OnVisitorsChanged;
        // Seed the derived collections immediately so the first paint
        // already shows real data.
        RecomputeAll();
    }

    public double GridHeight =>
           34 + (TodayVisitors?.Count ?? 0) * 50;

    public List<Brush> PurposeBrushes { get; } =
    [
        new SolidColorBrush(Color.FromArgb("#F59E0B")), // Overdue
        new SolidColorBrush(Color.FromArgb("#10B981")), // Checked In
        new SolidColorBrush(Color.FromArgb("#6B7280")), // Checked Out
        new SolidColorBrush(Color.FromArgb("#3B82F6")), // Expected
        new SolidColorBrush(Color.FromArgb("#DC2626")),  // Cancelled
        
    ];

    private void OnVisitorsChanged(object? sender, Visitor? e)
        => RecomputeAll();

    private void RecomputeAll()
    {
        var visitors = _store.Visitors;
        var today = ReferenceTime.Date;

        // 1. KPI snapshot.
        Stats = new VisitorStats(
                ExpectedVisitors: CountWhere(visitors, v => v.Status == VisitorStatus.Expected),
                CheckedIn: CountWhere(visitors, v => v.Status == VisitorStatus.CheckedIn),
                CheckedOut: CountWhere(visitors, v => v.Status == VisitorStatus.CheckedOut),
                Cancelled: CountWhere(visitors, v => v.Status == VisitorStatus.Cancelled),
                OverdueVisitors: CountWhere(visitors, v => v.Status == VisitorStatus.Overdue));

        // 2. Donut chart series — always 5 rows.
        ReplaceWith(StatusBreakdown, Enum
            .GetValues<VisitorStatus>()
            .Select(s => new StatusBreakdownPoint(
                Status: s,
                Label: s.Display(),
                Count: CountWhere(visitors, v => v.Status == s))));

        // 3. Column chart series — always 4 rows.
        ReplaceWith(PurposeBreakdown, VisitPurposeExtensions.All
            .Select(p => new PurposeBreakdownPoint(
                Purpose: p,
                Label: p.Display(),
                Count: CountWhere(visitors, v => v.Purpose == p.Display()))));

        // 4 + 5. Today's activity list (cap to 8 rows for the grid).
        var todays = visitors
            .Where(v => v.VisitDate.Date == today)
            .OrderBy(v => v.VisitTime)
            .Take(TodayVisitorCap)
            .ToList();
        ReplaceWith(TodayVisitors, todays);
        TodayVisitorsCount = visitors.Count(v => v.VisitDate.Date == today);
    }

    private static int CountWhere(IEnumerable<Visitor> source, Func<Visitor, bool> predicate)
        => source.Count(predicate);

    /// <summary>
    /// Atomically replace the contents of <paramref name="target"/>
    /// with <paramref name="items"/> so the bound SfChart /
    /// SfDataGrid sees a single Reset rather than a flurry of Adds.
    /// </summary>
    private static void ReplaceWith<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }
}
