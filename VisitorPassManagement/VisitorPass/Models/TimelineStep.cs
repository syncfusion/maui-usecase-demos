namespace VisitorPass.Models;

/// <summary>
/// One step in either of the two <c>SfStepProgressBar</c>s we render:
///   1. The registration form (Visitor Details -> Schedule ->
///      Generate Pass), and
///   2. The visit journey timeline on the Digital Pass screen
///      (Registered -> Expected -> Checked In -> Checked Out).
///
/// <see cref="IsCompleted"/> / <see cref="IsCurrent"/> drive the
/// visual state. <see cref="StateOverride"/> lets <c>Cancelled</c> /
/// <c>Overdue</c> terminal states swap the last step.
/// </summary>
public sealed class TimelineStep
{
    public TimelineStep(string label, bool isCompleted = false, bool isCurrent = false)
    {
        Label = label;
        IsCompleted = isCompleted;
        IsCurrent = isCurrent;
    }

    /// <summary>Human-readable step label.</summary>
    public string Label { get; }

    /// <summary>True when this step has been completed in the current journey.</summary>
    public bool IsCompleted { get; }

    /// <summary>True when this step is the active step.</summary>
    public bool IsCurrent { get; }

    /// <summary>
    /// Optional override label / state for the last step. Used by the
    /// Digital Pass screen to swap the final step to "Cancelled" or
    /// surface an "Overdue" badge on the Expected step when the
    /// visitor's status maps to a terminal alternative path
    /// (SPEC §9 Section 3).
    /// </summary>
    public string? StateOverride { get; init; }
}

/// <summary>
/// Pre-built step lists shared by both the Registration form and
/// the Digital Pass screen.
/// </summary>
public static class TimelineSteps
{
    /// <summary>Registration form: Visitor Details -> Schedule -> Generate Pass.</summary>
    public static IReadOnlyList<TimelineStep> Registration { get; } = new[]
    {
        new TimelineStep("Visitor Details"),
        new TimelineStep("Schedule"),
        new TimelineStep("Generate Pass"),
    };

    /// <summary>Visit journey: Registered -> Expected -> Checked In -> Checked Out.</summary>
    public static IReadOnlyList<TimelineStep> VisitJourney { get; } = new[]
    {
        new TimelineStep("Registered"),
        new TimelineStep("Expected"),
        new TimelineStep("Checked In"),
        new TimelineStep("Checked Out"),
    };
}
