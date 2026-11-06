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
    public MailAccount CurrentAccount { get; } = new() { DisplayName = "Kerry Best", Email = "kerry.best@contoso.com" };

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
        Composer.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ComposeViewModel.ShowMessage) && Composer.ShowMessage)
            {
                IsComposing = false;
                Composer.ShowMessage = false;
                LoadFolder(SelectedFolder ?? mailbox.Folders.First());
            }
        };
        SelectedFolder = mailbox.Folders[^1].Children[0];
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

    public bool HasSelectedMessage => SelectedMessage is not null;

    partial void OnSelectedMessageChanged(MailMessage? value)
    {
        OnPropertyChanged(nameof(HasSelectedMessage));
        if (value is not null)
        {
            IsComposing = false;
            value.IsRead = true;
            UpdateFolderCounts();
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterChanged(string value) => ApplyFilters();

    [RelayCommand]
    void SelectMessage(MailMessage? message) => SelectedMessage = message;

    [RelayCommand]
    void SetHover(MailMessage? message)
    {
        foreach (var item in VisibleMessages.Where(x => x.IsHovered && x != message)) item.IsHovered = false;
        if (message is not null) message.IsHovered = true;
    }

    [RelayCommand]
    void ClearHover(MailMessage? message)
    {
        if (message is not null) message.IsHovered = false;
    }

    [RelayCommand]
    void ToggleFlag(MailMessage? message)
    {
        if (message is not null) message.IsFlagged = !message.IsFlagged;
    }

    [RelayCommand]
    void ToggleRead(MailMessage? message)
    {
        if (message is null) return;
        message.IsRead = !message.IsRead;
        UpdateFolderCounts();
    }

    [RelayCommand]
    void DeleteMessage(MailMessage? message)
    {
        if (message is null) return;
        foreach (var source in new[] { mailbox.Inbox, mailbox.SentItems, mailbox.Drafts, mailbox.Archive })
            source.Remove(message);
        message.IsHovered = false;
        message.DeletedOn = DateTime.Now;
        mailbox.Deleted.Insert(0, message);
        LoadFolder(SelectedFolder ?? mailbox.Folders[^1].Children[0]);
    }

    [RelayCommand]
    void ArchiveMessage(MailMessage? message)
    {
        if (message is null) return;
        foreach (var source in new[] { mailbox.Inbox, mailbox.SentItems, mailbox.Drafts })
            source.Remove(message);
        message.IsHovered = false;
        message.ArchivedOn = DateTime.Now;
        if (!mailbox.Archive.Contains(message)) mailbox.Archive.Insert(0, message);
        LoadFolder(SelectedFolder ?? mailbox.Folders[^1].Children[0]);
    }

    [RelayCommand]
    void SetFolderHover(FolderNode? folder)
    {
        if (folder is not null) folder.IsHovered = true;
    }

    [RelayCommand]
    void ClearFolderHover(FolderNode? folder)
    {
        if (folder is not null) folder.IsHovered = false;
    }

    [RelayCommand]
    void ToggleFolderFavourite(FolderNode? folder)
    {
        if (folder is null || !folder.CanFavourite) return;
        var favorites = mailbox.Folders[0];
        var existing = favorites.Children.FirstOrDefault(x => x.Name == folder.Name);
        if (existing is null)
        {
            favorites.Children.Add(new FolderNode { Name = folder.Name, CanFavourite = true, IsFavourite = true });
        }
        else
        {
            favorites.Children.Remove(existing);
        }

        foreach (var node in mailbox.Folders.SelectMany(x => x.Children).Where(x => x.Name == folder.Name && x != existing))
            node.IsFavourite = existing is null;
        UpdateFolderCounts();
    }

    [RelayCommand]
    async Task NewMailAsync()
    {
        IsComposing = true;
        Composer.Subject = string.Empty;
        Composer.Body = string.Empty;
        Composer.StatusMessage = "Unsaved Changes";
    }

    [RelayCommand]
    void MarkAllAsRead()
    {
        foreach (var message in VisibleMessages) message.IsRead = true;
        UpdateFolderCounts();
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
        SelectedMessage = null;
        UpdateFolderCounts();
    }

    void UpdateFolderCounts()
    {
        mailbox.RefreshCounts();
    }
}
