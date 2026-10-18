using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class AiCopilotViewModel : ObservableObject
{
    private readonly IAiCopilotService _ai;

    public AiCopilotViewModel(IAiCopilotService ai)
    {
        _ai = ai;
        Messages.Add(new AiMessage { Role = AiRole.Assistant, Text = "👋 Hi! I'm Cinema AI Copilot. Tap a suggestion or ask me anything." });
        Suggestions.Add("Recommend a family movie");
        Suggestions.Add("Top rated this week");
        Suggestions.Add("Action movies tonight");
        Suggestions.Add("Compare two movies");
    }

    public ObservableCollection<AiMessage> Messages { get; } = new();
    public ObservableCollection<string> Suggestions { get; } = new();

    [ObservableProperty] private string _prompt = string.Empty;
    [ObservableProperty] private bool _isBusy;

    [RelayCommand]
    private async Task AskAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Messages.Add(new AiMessage { Role = AiRole.User, Text = text });
        Prompt = string.Empty;
        IsBusy = true;
        try
        {
            var reply = await _ai.SuggestAsync(null);
            foreach (var line in reply) Messages.Add(new AiMessage { Role = AiRole.Assistant, Text = line });
        }
        finally { IsBusy = false; }
    }
}

public enum AiRole { User, Assistant }

public class AiMessage
{
    public AiRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
}