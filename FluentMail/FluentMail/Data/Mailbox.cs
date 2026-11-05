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
        foreach (var root in Folders)
        {
            root.Count = 0;
        }

        foreach (var folder in new[] { "Inbox", "Clutter", "Drafts", "Sent Items", "Deleted Items", "Archive", "Junk Mail", "Personnel", "Sales Reports", "Marketing Reports", "My Team", "Trash", "Outbox" })
        {
            SetCount(folder, CountFor(folder));
        }
    }

    int CountFor(string folder) => folder switch
    {
        "Inbox" => Inbox.Count(x => x.Folder == "Inbox" && !x.IsRead),
        "Sent Items" => SentItems.Count,
        "Drafts" => Drafts.Count,
        "Archive" => Archive.Count,
        "Deleted" or "Deleted Items" => Deleted.Count,
        _ => Inbox.Count(x => x.Folder == folder && !x.IsRead)
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
        "Sent Items" => SentItems,
        "Drafts" => Drafts,
        "Archive" => Archive,
        "Deleted" or "Deleted Items" => Deleted,
        _ => Inbox.Where(x => x.Folder == folder)
    };
}
