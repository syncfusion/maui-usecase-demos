using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;
using Syncfusion.Maui.Toolkit.TextInputLayout;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MailAccount = FluentMail.Models.MailAccount;

namespace FluentMail.ViewModels;

public partial class MailWorkspaceViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;
    readonly List<MailMessage> folderMessages = [];

    public ObservableCollection<FolderNode> Folders => mailbox.Folders;
    public IReadOnlyList<string> FilterOptions { get; } = ["All", "Unread", "Flagged"];
    public MailAccount CurrentAccount { get; } = new() { DisplayName = "Kerry Best", Email = "kerry.best@contoso.com" };

    [ObservableProperty]
    private FolderNode? selectedFolder;

    [ObservableProperty]
    private ObservableCollection<MailMessage> visibleMessages = [];

    [ObservableProperty]
    private bool hasAttemptedSend;

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

    [ObservableProperty]
    private bool hasToError;

    [ObservableProperty]
    private string toErrorText = string.Empty;

    [ObservableProperty]
    private string to;

    private void Composer_PropertyChanged(object? sender,
     PropertyChangedEventArgs e)
    {

        if (e.PropertyName == nameof(ComposeViewModel.ShowMessage)
            && Composer.ShowMessage)
        {
            IsComposing = false;
            Composer.ShowMessage = false;
            LoadFolder(SelectedFolder ?? mailbox.Folders.First());
        }
    }

    private bool ValidateTo(string? email)
    {
        if (!HasAttemptedSend)
        {
            HasToError = false;
            ToErrorText = string.Empty;
            return true;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            HasToError = true;
            ToErrorText = "Recipient is required.";
            return false;
        }

        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            HasToError = false;
            ToErrorText = string.Empty;
            return true;
        }
        catch
        {
            HasToError = true;
            ToErrorText = "Enter a valid email address.";
            return false;
        }
    }


    public MailWorkspaceViewModel()
    {
        Composer.PropertyChanged += Composer_PropertyChanged;

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

        var makeFavourite = !folder.IsFavourite;
        var favorites = mailbox.Folders[0];

        foreach (var node in mailbox.Folders.SelectMany(x => x.Children).Where(x => x.Name == folder.Name))
            node.IsFavourite = makeFavourite;
        folder.IsFavourite = makeFavourite;

        var existing = favorites.Children.FirstOrDefault(x => x.Name == folder.Name);
        if (makeFavourite && existing is null)
            favorites.Children.Add(new FolderNode { Name = folder.Name, CanFavourite = true, IsFavourite = true });
        else if (!makeFavourite && existing is not null)
            favorites.Children.Remove(existing);

        UpdateFolderCounts();
    }

    [RelayCommand]
    async Task NewMailAsync()
    {
        HasAttemptedSend = false;
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
