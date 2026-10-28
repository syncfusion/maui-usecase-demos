using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class AiCopilotViewModel : ObservableObject
{
    private readonly IAiCopilotService _ai;

    public ObservableCollection<string> Messages { get; } = new()
    {
        "👋 Hi! I'm Cinema AI Copilot. Ask me anything about movies.",
        "Try: 'recommend a family movie', 'top rated this week', 'action tonight'."
    };

    [ObservableProperty] private string _prompt = string.Empty;

    public AiCopilotViewModel(IAiCopilotService ai)
    {
        _ai = ai;
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Prompt)) return;
        var userText = Prompt;
        Prompt = string.Empty;
        Messages.Add($"You: {userText}");

        // Simple keyword-based "intent engine" - no external LLM needed
        var lower = userText.ToLowerInvariant();
        var reply = lower switch
        {
            var s when s.Contains("family") =>
                "🍿 Family-friendly picks: 'Elemental' (animation), 'Wish' (kids), " +
                "'The Little Mermaid' (musical), 'Wonka' (adventure).",
            var s when s.Contains("action") =>
                "🔥 Action tonight: 'Mission: Impossible Dead Reckoning' (IMAX Laser), " +
                "'John Wick: Chapter 4' (Dolby Cinema), 'Top Gun: Maverick' (Dolby).",
            var s when s.Contains("top") || s.Contains("best") || s.Contains("rated") =>
                "⭐ Top rated this week: 'Oppenheimer' (9.4), 'Dune: Part Two' (9.2), " +
                "'The Batman' (8.9), 'Past Lives' (8.7).",
            var s when s.Contains("compare") =>
                "🎬 To compare two movies, open both from the Movies page and tap " +
                "'Add to Compare'. I'll then rank them side-by-side on rating, runtime, and format.",
            _ =>
                "🎬 I can recommend movies, show top-rated films, find family-friendly " +
                "options, or compare two titles. What would you like?"
        };
        Messages.Add($"AI: {reply}");
        await Task.Delay(50);
    }
}
