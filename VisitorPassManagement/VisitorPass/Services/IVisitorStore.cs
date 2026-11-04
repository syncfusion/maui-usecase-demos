using System.Collections.ObjectModel;
using VisitorPass.Models;

namespace VisitorPass.Services;

/// <summary>
/// Single source of truth for visitor records. Registered as a
/// singleton in <c>MauiProgram.cs</c> (VP-012). Every ViewModel reads
/// from it; the store raises <see cref="VisitorsChanged"/> after every
/// mutation so subscribers can rebuild derived collections.
///
/// Per HARNESS §6 rule 1 (single source of truth), no page or
/// ViewModel holds a private list of visitors — they all read through
/// this interface.
///
/// Note: this interface intentionally does NOT extend
/// <c>INotifyPropertyChanged</c>. The <see cref="Visitors"/> collection
/// itself is an <see cref="ObservableCollection{T}"/> which raises
/// <c>CollectionChanged</c> on adds / removes; per-visitor property
/// changes flow through <see cref="VisitorsChanged"/>.
/// </summary>
public interface IVisitorStore
{
    /// <summary>
    /// Master collection of every visitor the app knows about, in
    /// registration order. Pages bind this directly to their
    /// <c>SfDataGrid.ItemsSource</c>; mutations are observed through
    /// the collection's own <c>CollectionChanged</c> notifications AND
    /// through the <see cref="VisitorsChanged"/> event so subscribers
    /// can rebuild derived KPI / chart series.
    /// </summary>
    ObservableCollection<Visitor> Visitors { get; }

    /// <summary>
    /// Raised after <see cref="AddVisitor"/>, after any status mutation
    /// (CheckIn / CheckOut / Cancel), or after the store recomputes
    /// Overdue rows. Subscribers reset their grids, refresh KPI tiles
    /// and re-apply any local filter. The argument is the visitor that
    /// was added or affected (or <c>null</c> when no specific visitor
    /// applies, e.g. a bulk Overdue recompute).
    /// </summary>
    event EventHandler<Visitor?>? VisitorsChanged;

    /// <summary>
    /// Returns the visitor matching <paramref name="passId"/> or
    /// <c>null</c> when the id is unknown. Used by the Digital Pass
    /// page to load a row from a navigation parameter.
    /// </summary>
    Visitor? FindByPassId(string passId);

    /// <summary>
    /// Adds a freshly registered visitor. The store assigns a unique
    /// pass code (via <see cref="PassCodeGenerator"/>), appends to
    /// <see cref="Visitors"/> and raises <see cref="VisitorsChanged"/>.
    /// </summary>
    void AddVisitor(Visitor visitor);

    /// <summary>
    /// Transitions the supplied visitor to <see cref="VisitorStatus.CheckedIn"/>
    /// and stamps <c>CheckInTime</c>. Throws when the visitor is
    /// already checked in / out / cancelled.
    /// </summary>
    void CheckIn(Visitor visitor, DateTime? at = null);

    /// <summary>
    /// Transitions the supplied visitor to <see cref="VisitorStatus.CheckedOut"/>
    /// and stamps <c>CheckOutTime</c>. Throws when the visitor isn't
    /// currently <see cref="VisitorStatus.CheckedIn"/>.
    /// </summary>
    void CheckOut(Visitor visitor, DateTime? at = null);

    /// <summary>
    /// Transitions the supplied visitor to <see cref="VisitorStatus.Cancelled"/>.
    /// Throws when the visitor has already checked out (terminal).
    /// </summary>
    void Cancel(Visitor visitor);

    /// <summary>
    /// Recomputes which currently-Expected visits should be flipped to
    /// <see cref="VisitorStatus.Overdue"/> based on the supplied clock.
    /// Called periodically by the dashboard; one event per affected
    /// visitor is raised so the KPI tiles update incrementally.
    /// </summary>
    void RecomputeOverdue(DateTime now);
}
