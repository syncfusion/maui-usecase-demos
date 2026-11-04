using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;
using MailAccount = FluentMail.Models.MailAccount;

namespace FluentMail.ViewModels;

public partial class MailWorkspaceViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;
    readonly List<MailMessage> folderMessages = [];

    public ObservableCollection<FolderNode> Folders => mailbox.Folders;
    public IReadOnlyList<string> FilterOptions { get; } = ["All", "Unread", "Flagged", "With attachments"];
    public MailAccount CurrentAccount { get; } = new() { DisplayName = "Andrew Fuller", Email = "andrew.fuller@contoso.com" };

    [ObservableProperty]
    private FolderNode? selectedFolder;

    [ObservableProperty]
    private ObservableCollection<MailMessage> visibleMessages = [];

    [ObservableProperty]
    private MailMessage? selectedMessage;

    [ObservableProperty]
    private bool isEmptyFolder;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private bool isComposing;

    public ComposeViewModel Composer { get; } = new();

    public MailWorkspaceViewModel()
    {
        SelectedFolder = mailbox.Folders.First();
        LoadFolder(SelectedFolder);
        IsLoading = false;
    }

    partial void OnSelectedFolderChanged(FolderNode? value)
    {
        if (value is not null)
        {
            LoadFolder(value);
        }
    }

    partial void OnSelectedMessageChanged(MailMessage? value)
    {
        if (value is not null)
        {
            value.IsRead = true;
            UpdateFolderCounts();
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterChanged(string value) => ApplyFilters();

    [RelayCommand]
    void SelectMessage(MailMessage? message) => SelectedMessage = message;

    [RelayCommand]
    void NewMail()
    {
        IsComposing = true;
        Composer.Subject = string.Empty;
        Composer.Body = string.Empty;
        Composer.StatusMessage = "Unsaved Changes";
    }

    [RelayCommand]
    void CloseComposer() => IsComposing = false;

    [RelayCommand]
    void SelectFolder(FolderNode? folder)
    {
        if (folder is not null) SelectedFolder = folder;
    }

    void LoadFolder(FolderNode folder)
    {
        folderMessages.Clear();
        folderMessages.AddRange(mailbox.GetMessages(folder.Name));
        ApplyFilters();
    }

    void ApplyFilters()
    {
        IEnumerable<MailMessage> query = folderMessages;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim();
            query = query.Where(message =>
                message.Sender.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                message.SenderEmail.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                message.Subject.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                message.Preview.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedFilter switch
        {
            "Unread" => query.Where(x => !x.IsRead),
            "Flagged" => query.Where(x => x.IsFlagged),
            "With attachments" => query.Where(x => x.HasAttachments),
            _ => query
        };

        VisibleMessages = new ObservableCollection<MailMessage>(query);
        IsEmptyFolder = VisibleMessages.Count == 0;
        SelectedMessage = VisibleMessages.FirstOrDefault();
        UpdateFolderCounts();
    }

    void UpdateFolderCounts()
    {
        mailbox.Folders[0].UnreadCount = mailbox.Inbox.Count(x => !x.IsRead);
        mailbox.Folders[1].UnreadCount = 0;
        mailbox.Folders[2].UnreadCount = mailbox.Drafts.Count(x => x.Status == "Unsaved Changes");
        mailbox.Folders[3].UnreadCount = mailbox.Archive.Count(x => x.IsFlagged);
        mailbox.Folders[4].UnreadCount = mailbox.Deleted.Count;
    }
}
