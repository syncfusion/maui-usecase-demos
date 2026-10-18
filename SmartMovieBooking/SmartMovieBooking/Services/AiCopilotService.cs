using SmartMovieBooking.Models;

namespace SmartMovieBooking.Services;

public interface IAiCopilotService
{
    Task<IReadOnlyList<string>> SuggestAsync(string? contextMovieId);
}

/// <summary>
/// Lightweight intent-engine over the in-memory catalog. No external LLM.
/// Returns a list of canned response lines for the AI Copilot sheet.
/// Phase 4 (T4.2) replaces this with the Syncfusion AIAssistView integration.
/// </summary>
public sealed class AiCopilotService : IAiCopilotService
{
    public Task<IReadOnlyList<string>> SuggestAsync(string? contextMovieId)
    {
        var top = AppSeed.Movies
            .OrderByDescending(m => double.TryParse(m.Rating, out var r) ? r : 0)
            .Take(3)
            .Select(m => $"🎬 {m.Title} — rated {m.Rating} ({m.PrimaryGenre})")
            .ToList();

        var lines = new List<string>
        {
            "👋 Hi! I'm Cinema AI Copilot. Here's what's hot right now:",
            ""
        };
        lines.AddRange(top);
        lines.Add("");
        lines.Add("Tip: ask me for \"family movies\" or \"action with high rating\" to get focused picks.");

        return Task.FromResult<IReadOnlyList<string>>(lines);
    }
}