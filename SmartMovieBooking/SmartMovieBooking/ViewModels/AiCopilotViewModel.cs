using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public enum AiRole { User, Assistant }

public class AiMessage
{
    public AiRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public partial class AiCopilotViewModel : ObservableObject
{
    private readonly IAiCopilotService _ai;

    public AiCopilotViewModel(IAiCopilotService ai)
    {
        _ai = ai;
        Messages.Add(new AiMessage
        {
            Role = AiRole.Assistant,
            SenderName = "Lumina Concierge",
            Text = "Hello Alex! 👋 Looking for the perfect movie today? Tell me what genre, mood, or format you're in the mood for, or pick a recommendation above.",
            Timestamp = DateTime.Now.AddMinutes(-2)
        });
        Messages.Add(new AiMessage
        {
            Role = AiRole.User,
            SenderName = "Alex Carter",
            Text = "I loved Interstellar and Arrival. Any mind-bending sci-fi movies playing in IMAX this evening?",
            Timestamp = DateTime.Now.AddMinutes(-1)
        });
        Messages.Add(new AiMessage
        {
            Role = AiRole.Assistant,
            SenderName = "Lumina Concierge",
            Text = "Based on deep space exploration and mind-bending narratives, here are two top-rated sci-fi epics currently showing in IMAX laser near you.",
            Timestamp = DateTime.Now
        });

        Suggestions.Add("✨ Recommend a Movie");
        Suggestions.Add("🍿 Family Movies");
        Suggestions.Add("🔥 Trending Now");
        Suggestions.Add("🎭 Top Rated This Week");
    }

    public ObservableCollection<AiMessage> Messages { get; } = new();
    public ObservableCollection<string> Suggestions { get; } = new();

    [ObservableProperty] private string _prompt = string.Empty;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task AskAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Messages.Add(new AiMessage { Role = AiRole.User, SenderName = "Alex Carter", Text = text });
        Prompt = string.Empty;
        IsBusy = true;
        try
        {
            var reply = await _ai.SuggestAsync(null);
            foreach (var line in reply)
                Messages.Add(new AiMessage { Role = AiRole.Assistant, SenderName = "Lumina Concierge", Text = line });
        }
        finally { IsBusy = false; }
    }

    /// <summary>Public so the view code-behind can invoke it.</summary>
    [RelayCommand]
    public async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Prompt)) return;
        var text = Prompt;
        await AskAsync(text);
    }
}