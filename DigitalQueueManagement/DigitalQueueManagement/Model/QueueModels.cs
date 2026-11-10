using Microsoft.Maui.Graphics;

namespace DigitalQueueManagement.Model;

public enum TokenPriority
{
    Normal,
    Priority
}

public enum TokenStatus
{
    Waiting,
    Serving,
    Skipped,
    Completed
}

public sealed record QueueToken(
    string Id,
    string Service,
    TokenPriority Priority,
    TokenStatus Status,
    string CounterName,
    string Wait);

public sealed record QueueTokenListItem(QueueToken Token, string QueuePositionText)
{
    public string Id => Token.Id;

    public string Service => Token.Service;

    public TokenPriority Priority => Token.Priority;

    public TokenStatus Status => Token.Status;

    public string CounterName => Token.CounterName;

    public string Wait => Token.Wait;
}

public sealed record QueueCounter(
    string Name,
    string Service,
    bool IsOnline = true);

public sealed record QueueCounterSummary(
    string Name,
    string Service,
    string TokenId,
    string TokenStatus,
    bool IsOnline,
    bool IsSelected = false)
{
    public string CounterNumber =>
        Name.StartsWith("Counter ", StringComparison.OrdinalIgnoreCase)
            ? Name["Counter ".Length..]
            : Name;

    public string AvailabilityText =>
        !IsOnline ? "Offline" : TokenStatus == "Waiting" ? "Waiting" : "Online";

    public bool IsOffline => AvailabilityText == "Offline";

    public string AvailabilityColor => AvailabilityText switch
    {
        "Online" => "#159B85",
        "Waiting" => "#B45309",
        _ => "#64748B"
    };

    public string AvailabilityBackgroundColor => AvailabilityText switch
    {
        "Online" => "#E9FBF4",
        "Waiting" => "#FFF6E8",
        _ => "#F1F5F9"
    };

    public string TokenAndStatusText => $"{TokenId} · {TokenStatus}";

    public int CompletedTokenCount { get; init; }

    public int ServingTokenCount { get; init; }

    public int WaitingTokenCount { get; init; }

    public int TrackedTokenCount => CompletedTokenCount + ServingTokenCount + WaitingTokenCount;

    public IReadOnlyList<QueueCounterActivitySegment> ActivitySegments =>
    [
        new("Completed", CompletedTokenCount, Color.FromArgb("#214A9A")),
        new("Serving", ServingTokenCount, Color.FromArgb("#0C897B")),
        new("Waiting", WaitingTokenCount, Color.FromArgb("#F59E0B"))
    ];

    public string ActivitySummary =>
        $"{CompletedTokenCount} completed · {ServingTokenCount} serving · {WaitingTokenCount} waiting";
}

public sealed record QueueCounterActivitySegment(string Category, double Weight, Color FillColor);

public sealed record QueueAnalyticsPoint(
    string Category,
    double Value,
    string ColorHex = "#183E91",
    string ShareText = "")
{
    public double Progress => Math.Clamp(Value / 15d, 0, 1);

    public Color ChartColor => Color.FromArgb(ColorHex);
}

public static class QueueServices
{
    public static IReadOnlyList<string> All { get; } =
    [
        "Payments & Billing",
        "General Inquiry",
        "Customer Support"
    ];

    public static string PrefixFor(string service) =>
        service switch
        {
            "Payments & Billing" => "A",
            "General Inquiry" => "B",
            "Customer Support" => "C",
            _ => throw new ArgumentException("Choose a supported service.", nameof(service))
        };
}
