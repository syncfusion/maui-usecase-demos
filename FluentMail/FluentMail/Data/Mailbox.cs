using System.Collections.ObjectModel;
using FluentMail.Models;
using MailContact = FluentMail.Models.Contact;

namespace FluentMail.Data;

public class Mailbox
{
    public ObservableCollection<FolderNode> Folders { get; } = [];
    public ObservableCollection<MailMessage> Inbox { get; } = [];
    public ObservableCollection<MailMessage> SentItems { get; } = [];
    public ObservableCollection<MailMessage> Drafts { get; } = [];
    public ObservableCollection<MailMessage> Archive { get; } = [];
    public ObservableCollection<MailMessage> Deleted { get; } = [];
    public ObservableCollection<AttachmentInfo> Attachments { get; } = [];
    public ObservableCollection<MailContact> Contacts { get; } = [];

    public void RefreshCounts()
    {
        if (Folders.Count < 2)
        {
            return;
        }

        var favorites = Folders[0];
        var account = Folders[1];

        favorites.Count = 0;
        account.Count = 0;

        var favoriteFolders = new[] { "Inbox", "Sales Reports", "Personnel", "Marketing Reports", "Sent Items" };
        var accountFolders = new[] { "Inbox", "Clutter", "Drafts", "Sent Items", "Deleted Items", "Archive", "Junk Mail", "Personnel", "Sales Reports", "Marketing Reports" };

        foreach (var folder in favoriteFolders)
            SetCount(folder, CountFor(folder));

        foreach (var folder in accountFolders)
            SetCount(folder, CountFor(folder));
    }

    int CountFor(string folder) => folder switch
    {
        "Inbox" => Inbox.Count,
        "Sent Items" => SentItems.Count,
        "Drafts" => Drafts.Count,
        "Archive" => Archive.Count,
        "Deleted" or "Deleted Items" => Deleted.Count,
        _ => Inbox.Count(x => x.Subject.Contains(folder, StringComparison.OrdinalIgnoreCase) || x.Preview.Contains(folder, StringComparison.OrdinalIgnoreCase))
    };

    void SetCount(string folder, int count)
    {
        foreach (var root in Folders)
        {
            foreach (var child in root.Children)
            {
                if (child.Name == folder)
                    child.Count = count;
            }
        }
    }

    public IEnumerable<MailMessage> GetMessages(string folder) => folder switch
    {
        "Inbox" => Inbox,
        "Sent Items" => SentItems,
        "Drafts" => Drafts,
        "Archive" => Archive,
        "Deleted" or "Deleted Items" => Deleted,
        _ => Inbox
    };
}
