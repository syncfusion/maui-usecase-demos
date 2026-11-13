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
    public ObservableCollection<FolderNode> FolderItems { get; } = [];
    public CalendarViewModel Calendar { get; } = new();

    [ObservableProperty]
    private bool isCalendarOpen;
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
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private bool isComposing;

    public ComposeViewModel Composer { get; } = new();

    public IReadOnlyList<string> SortOptions { get; } = ["Unread first", "Newest first"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOtherTab))]
    private bool isFocusedTab = true;

    public bool IsOtherTab => !IsFocusedTab;

    [ObservableProperty]
    private string selectedSort = "Unread first";

    [ObservableProperty]
    private ObservableCollection<MailGroup> mobileGroups = [];

    [ObservableProperty]
    private int focusedUnread;

    [ObservableProperty]
    private int otherUnread;

    [ObservableProperty]
    private int totalUnread;

    [ObservableProperty]
    private MailMessage? openedMessage;

    [ObservableProperty]
    private bool isMessageOpen;

    partial void OnIsFocusedTabChanged(bool value) => RefreshMobile();
    partial void OnSelectedSortChanged(string value) => RefreshMobile();

    [RelayCommand]
    void ShowFocused() => IsFocusedTab = true;

    [RelayCommand]
    void ShowOther() => IsFocusedTab = false;

    [RelayCommand]
    void OpenMessage(MailMessage? message)
    {
        if (message is null) return;
        OpenedMessage = message;
        IsMessageOpen = true;
        message.IsRead = true;
        UpdateFolderCounts();
    }

    [RelayCommand]
    void CloseMessage() => IsMessageOpen = false;

    void RefreshMobile()
    {
        var focused = mailbox.Inbox.Where(x => x.Folder == "Inbox").ToList();
        var other = mailbox.Inbox.Where(x => x.Folder != "Inbox").ToList();
        FocusedUnread = focused.Count(x => !x.IsRead);
        OtherUnread = other.Count(x => !x.IsRead);
        TotalUnread = FocusedUnread + OtherUnread;

        var source = IsFocusedTab ? focused : other;
        var ordered = SelectedSort == "Unread first"
            ? source.OrderBy(x => x.IsRead).ThenByDescending(x => x.ReceivedOn)
            : source.OrderByDescending(x => x.ReceivedOn);

        MobileGroups = new ObservableCollection<MailGroup>(ordered
            .GroupBy(x => x.ReceivedOn.Date)
            .OrderByDescending(g => g.Key)
            .Select(g => new MailGroup
            {
                Title = g.Key == DateTime.Today ? "TODAY" : g.Key == DateTime.Today.AddDays(-1) ? "YESTERDAY" : g.Key.ToString("MMM d").ToUpperInvariant(),
                CountText = g.Count(x => !x.IsRead) is var n && n > 0 ? $"{n} updates" : string.Empty,
                Items = g.ToList()
            }));
    }

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

    public MailWorkspaceViewModel()
    {
        Composer.PropertyChanged += Composer_PropertyChanged;

        foreach (var root in mailbox.Folders)
            foreach (var child in root.Children)
            {
                child.Section = root.Name;
                FolderItems.Add(child);
            }

        SelectedFolder = mailbox.Folders[^1].Children[0];
        LoadFolder(SelectedFolder);
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
    void ToggleCalendar() => IsCalendarOpen = !IsCalendarOpen;

    [RelayCommand]
    void ToggleFolderFavourite(FolderNode? folder)
    {
        if (folder is null || !folder.CanFavourite) return;

        var makeFavourite = !folder.IsFavourite;
        var favorites = mailbox.Folders[0];

        foreach (var node in FolderItems.Where(x => x.Name == folder.Name))
            node.IsFavourite = makeFavourite;

        var existing = favorites.Children.FirstOrDefault(x => x.Name == folder.Name);
        if (makeFavourite && existing is null)
        {
            var added = new FolderNode { Name = folder.Name, CanFavourite = true, IsFavourite = true, Section = favorites.Name };
            favorites.Children.Add(added);
            FolderItems.Insert(favorites.Children.Count - 1, added);
        }
        else if (!makeFavourite && existing is not null)
        {
            var wasSelected = SelectedFolder == existing;
            favorites.Children.Remove(existing);
            FolderItems.Remove(existing);
            if (wasSelected)
                SelectedFolder = FolderItems.FirstOrDefault(x => x.Name == folder.Name && x.Section != favorites.Name);
        }

        UpdateFolderCounts();
    }

    [RelayCommand]
    async Task NewMailAsync()
    {
        IsComposing = true;
        Composer.Reset();
    }

    [RelayCommand]
    void CloseComposer() => IsComposing = false;

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
        RefreshMobile();
    }
}
