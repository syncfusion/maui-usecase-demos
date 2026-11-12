using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty] private string query = string.Empty;
    [ObservableProperty] private bool unreadOnly;
    [ObservableProperty] private bool flaggedOnly;
    [ObservableProperty] private bool toMeOnly;
    [ObservableProperty] private bool withAttachments;
    [ObservableProperty] private ObservableCollection<MailMessage> results = [];
    [ObservableProperty] private bool showResults;

    public ObservableCollection<string> RecentSearches { get; } = ["Architecture spec v3", "Syncfusion SfCalendar license", "Q3 Budget review & headcount"];

    public IReadOnlyList<TopContact> TopContacts { get; } =
    [
        new() { Name = "Sarah J.", Role = "PM Lead", Status = "#17B26A" },
        new() { Name = "Marcus V.", Role = "Architect", Status = "#F79009" },
        new() { Name = "Elena R.", Role = "Design", Status = "#17B26A" },
        new() { Name = "David Chen", Role = "Dev Ops", Status = "#F04462" }
    ];

    public IReadOnlyList<FileItem> Files { get; } =
    [
        new() { Name = "Enterprise_Architecture_Draft_2.pdf", Owner = "Marcus Vance", SizeText = "4.8 MB", DateText = "Yesterday", Kind = "PDF", Tint = "#FDECEC" },
        new() { Name = "Q3_FY25_Regional_Projections.xlsx", Owner = "Maya Lin", SizeText = "1.2 MB", DateText = "Oct 14", Kind = "XLS", Tint = "#E8F7EF" },
        new() { Name = "MAUI_Fluent2_Design_System.pptx", Owner = "Elena Rostova", SizeText = "18.4 MB", DateText = "Oct 11", Kind = "PPT", Tint = "#FFF4DD" }
    ];

    partial void OnQueryChanged(string value) => Apply();
    partial void OnUnreadOnlyChanged(bool value) => Apply();
    partial void OnFlaggedOnlyChanged(bool value) => Apply();
    partial void OnToMeOnlyChanged(bool value) => Apply();
    partial void OnWithAttachmentsChanged(bool value) => Apply();

    [RelayCommand] void ToggleUnread() => UnreadOnly = !UnreadOnly;
    [RelayCommand] void ToggleFlagged() => FlaggedOnly = !FlaggedOnly;
    [RelayCommand] void ToggleToMe() => ToMeOnly = !ToMeOnly;
    [RelayCommand] void ToggleAttachments() => WithAttachments = !WithAttachments;
    [RelayCommand] void UseRecent(string? text) => Query = text ?? string.Empty;
    [RelayCommand] void RemoveRecent(string? text) { if (text is not null) RecentSearches.Remove(text); }
    [RelayCommand] void ClearRecent() => RecentSearches.Clear();

    void Apply()
    {
        var active = !string.IsNullOrWhiteSpace(Query) || UnreadOnly || FlaggedOnly || ToMeOnly || WithAttachments;
        ShowResults = active;
        if (!active) { Results = []; return; }

        IEnumerable<MailMessage> list = mailbox.Inbox.Concat(mailbox.SentItems);
        var term = Query.Trim();
        if (term.Length > 0)
            list = list.Where(x => x.Sender.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Subject.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Preview.Contains(term, StringComparison.OrdinalIgnoreCase));
        if (UnreadOnly) list = list.Where(x => !x.IsRead);
        if (FlaggedOnly) list = list.Where(x => x.IsFlagged);
        if (ToMeOnly) list = list.Where(x => x.Recipients.Contains("kerry.best", StringComparison.OrdinalIgnoreCase));
        if (WithAttachments) list = list.Where(x => x.HasAttachments);
        Results = new ObservableCollection<MailMessage>(list.OrderByDescending(x => x.ReceivedOn).Take(30));
    }
}
