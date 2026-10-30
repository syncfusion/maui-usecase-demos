using System.Collections.ObjectModel;
using System.ComponentModel;
using VisitorPass.Models;

namespace VisitorPass.Services;

/// <summary>
/// Default in-memory implementation of <see cref="IVisitorStore"/>.
/// Registered as a singleton in <c>MauiProgram.cs</c> (VP-012) so every
/// page + ViewModel reads from the same instance.
///
/// The store seeds itself with the contract-defined visitor set on
/// construction, then exposes mutation methods that raise
/// <see cref="VisitorsChanged"/> for subscribers.
/// </summary>
public sealed class VisitorStore : IVisitorStore
{
    private readonly PassCodeGenerator _passCodeGenerator;

    /// <summary>Master visitor list. Bound directly by SfDataGrid.</summary>
    public ObservableCollection<Visitor> Visitors { get; } = new();

    /// <inheritdoc/>
    public event EventHandler<Visitor?>? VisitorsChanged;

    /// <summary>
    /// Constructs a store seeded with the visitor list defined by
    /// <see cref="VisitorData.BuildVisitors"/>. The reference time is
    /// fixed at construction so the seed's timestamps stay
    /// deterministic across the app lifetime; the production code
    /// never overrides it.
    /// </summary>
    public VisitorStore()
        : this(DateTime.Now, new PassCodeGenerator())
    {
    }

    /// <summary>
    /// Test-friendly constructor. <paramref name="now"/> pins "today"
    /// for the seed; <paramref name="passCodeGenerator"/> allows a
    /// pre-seeded counter to be injected so tests don't share state
    /// across runs.
    /// </summary>
    public VisitorStore(DateTime now, PassCodeGenerator passCodeGenerator)
    {
        _passCodeGenerator = passCodeGenerator;
        foreach (var v in VisitorData.BuildVisitors(now, _passCodeGenerator))
        {
            v.PropertyChanged += OnVisitorPropertyChanged;
            Visitors.Add(v);
        }
    }

    /// <inheritdoc/>
    public Visitor? FindByPassId(string passId)
    {
        if (string.IsNullOrWhiteSpace(passId))
            return null;
        foreach (var v in Visitors)
        {
            if (string.Equals(v.PassId, passId, StringComparison.OrdinalIgnoreCase))
                return v;
        }
        return null;
    }

    /// <inheritdoc/>
    public void AddVisitor(Visitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        if (string.IsNullOrWhiteSpace(visitor.PassId))
            visitor.PassId = _passCodeGenerator.Next(visitor.VisitDate, Visitors.Count);
        if (string.IsNullOrWhiteSpace(visitor.PassCode))
            visitor.PassCode = visitor.PassId;
        visitor.PropertyChanged += OnVisitorPropertyChanged;
        Visitors.Add(visitor);
        VisitorsChanged?.Invoke(this, visitor);
    }

    /// <inheritdoc/>
    public void CheckIn(Visitor visitor, DateTime? at = null)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        if (visitor.Status is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut)
            throw new InvalidOperationException(
                $"Visitor '{visitor.VisitorName}' is already checked in / out.");
        if (visitor.Status == VisitorStatus.Cancelled)
            throw new InvalidOperationException(
                $"Visitor '{visitor.VisitorName}' was cancelled and cannot check in.");
        visitor.CheckInTime = at ?? DateTime.Now;
        visitor.Status = VisitorStatus.CheckedIn;
        VisitorsChanged?.Invoke(this, visitor);
    }

    /// <inheritdoc/>
    public void CheckOut(Visitor visitor, DateTime? at = null)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        if (visitor.Status != VisitorStatus.CheckedIn)
            throw new InvalidOperationException(
                $"Visitor '{visitor.VisitorName}' is not currently checked in.");
        visitor.CheckOutTime = at ?? DateTime.Now;
        visitor.Status = VisitorStatus.CheckedOut;
        VisitorsChanged?.Invoke(this, visitor);
    }

    /// <inheritdoc/>
    public void Cancel(Visitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);
        if (visitor.Status is VisitorStatus.CheckedOut or VisitorStatus.Cancelled)
            throw new InvalidOperationException(
                $"Visitor '{visitor.VisitorName}' cannot be cancelled in its current state ({visitor.Status}).");
        visitor.Status = VisitorStatus.Cancelled;
        VisitorsChanged?.Invoke(this, visitor);
    }

    /// <inheritdoc/>
    public void RecomputeOverdue(DateTime now)
    {
        var affected = new List<Visitor>();
        foreach (var v in Visitors)
        {
            if (v.Status != VisitorStatus.Expected) continue;
            var scheduledEnd = v.VisitDate.Date + v.VisitTime
                + TimeSpan.FromMinutes(v.ExpectedDurationMinutes);
            if (now > scheduledEnd)
            {
                v.Status = VisitorStatus.Overdue;
                affected.Add(v);
            }
        }
        foreach (var v in affected)
        {
            VisitorsChanged?.Invoke(this, v);
        }
        if (affected.Count == 0)
        {
            // Still raise one null-payload event so subscribers can
            // re-evaluate KPI totals / charts even when nothing changed.
            VisitorsChanged?.Invoke(this, null);
        }
    }

    /// <summary>
    /// Forward per-visitor property changes so an
    /// <c>INotifyPropertyChanged</c> on a single row reaches the
    /// store-level <see cref="VisitorsChanged"/> fan-out. Used by the
    /// Digital Pass screen so its button visibility updates the moment
    /// <c>Status</c> changes locally.
    /// </summary>
    private void OnVisitorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Visitor v)
        {
            VisitorsChanged?.Invoke(this, v);
        }
    }
}
