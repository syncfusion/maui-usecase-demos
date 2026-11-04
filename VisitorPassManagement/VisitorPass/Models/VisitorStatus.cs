using System.ComponentModel;
using System.Reflection;

namespace VisitorPass.Models;

/// <summary>
/// Lifecycle status of a single visit. Per SPEC §5 + §4.
///
/// Allowed transitions:
///   Expected    -> CheckedIn
///   Expected    -> Cancelled
///   Expected    -> Overdue  (computed when current time > expected
///                            end time and status is still Expected)
///   CheckedIn   -> CheckedOut
///   CheckedIn   -> Cancelled
///   CheckedOut  -> (terminal)
///   Cancelled   -> (terminal)
///   Overdue     -> CheckedIn | Cancelled
/// </summary>
public enum VisitorStatus
{
    /// <summary>Visit scheduled; visitor has not arrived yet.</summary>
    [Description("Expected")]
    Expected = 0,

    /// <summary>Visitor has arrived at the gate / reception.</summary>
    [Description("Checked In")]
    CheckedIn = 1,

    /// <summary>Visitor has left the facility.</summary>
    [Description("Checked Out")]
    CheckedOut = 2,

    /// <summary>Visit was cancelled before arrival.</summary>
    [Description("Cancelled")]
    Cancelled = 3,

    /// <summary>Visit has overrun its planned duration without arrival.</summary>
    [Description("Overdue")]
    Overdue = 4,
}

/// <summary>
/// Helper extensions for <see cref="VisitorStatus"/>. Every binding that
/// needs to render the status (label, brush, pale background, short
/// label for compact pills on phones) reads from here so the strings
/// stay in lockstep with the enum.
/// </summary>
public static class VisitorStatusExtensions
{
    /// <summary>Full human-readable label, e.g. <c>Checked In</c>.</summary>
    public static string Display(this VisitorStatus status)
        => GetDescription(status);

    /// <summary>Compact label for use on phone pills / chip rows.</summary>
    public static string Short(this VisitorStatus status) => status switch
    {
        VisitorStatus.Expected => "Expected",
        VisitorStatus.CheckedIn => "In",
        VisitorStatus.CheckedOut => "Out",
        VisitorStatus.Cancelled => "✕",
        VisitorStatus.Overdue => "!",
        _ => status.ToString()
    };

    /// <summary>Foreground brush key (no ResourceManager reference — consumers resolve the brush by key).</summary>
    public static string BrushKey(this VisitorStatus status) => status switch
    {
        VisitorStatus.Expected => "StatusExpected",
        VisitorStatus.CheckedIn => "StatusCheckedIn",
        VisitorStatus.CheckedOut => "StatusCheckedOut",
        VisitorStatus.Cancelled => "StatusCancelled",
        VisitorStatus.Overdue => "StatusOverdue",
        _ => "TextPrimary"
    };

    /// <summary>Pale background brush key (used by SfBadgeView's BadgeBackground + StatusBadgePill).</summary>
    public static string BackgroundBrushKey(this VisitorStatus status) => status switch
    {
        VisitorStatus.Expected => "StatusExpectedBg",
        VisitorStatus.CheckedIn => "StatusCheckedInBg",
        VisitorStatus.CheckedOut => "StatusCheckedOutBg",
        VisitorStatus.Cancelled => "StatusCancelledBg",
        VisitorStatus.Overdue => "StatusOverdueBg",
        _ => "SurfaceAlt"
    };

    /// <summary>True for statuses the dashboard KPI section counts (everything except Cancelled).</summary>
    public static bool IsCountableOnKpis(this VisitorStatus status)
        => status is VisitorStatus.Expected
                   or VisitorStatus.CheckedIn
                   or VisitorStatus.CheckedOut
                   or VisitorStatus.Overdue;

    /// <summary>True for terminal statuses where the action buttons hide.</summary>
    public static bool IsTerminal(this VisitorStatus status)
        => status is VisitorStatus.CheckedOut or VisitorStatus.Cancelled;

    private static string GetDescription(VisitorStatus status)
    {
        var name = status.ToString();
        var field = typeof(VisitorStatus).GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (field?.GetCustomAttribute<DescriptionAttribute>() is { } desc)
            return desc.Description;
        return name;
    }
}
