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
    public ObservableCollection<MailContact> Contacts { get; } = [];

    public void RefreshCounts()
    {
        foreach (var root in Folders)
        {
            root.Count = 0;
        }

        foreach (var folder in new[] { FolderNames.Inbox, FolderNames.DeletedItems })
        {
            SetCount(folder, CountFor(folder));
        }
    }

    int CountFor(string folder) => folder switch
    {
        FolderNames.Inbox => Inbox.Count(x => x.Folder == FolderNames.Inbox && !x.IsRead),
        FolderNames.SentItems => SentItems.Count,
        FolderNames.Drafts => Drafts.Count,
        FolderNames.Archive => Archive.Count,
        FolderNames.Deleted or FolderNames.DeletedItems => Deleted.Count,
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
        FolderNames.SentItems => SentItems,
        FolderNames.Drafts => Drafts,
        FolderNames.Archive => Archive,
        FolderNames.Deleted or FolderNames.DeletedItems => Deleted,
        _ => Inbox.Where(x => x.Folder == folder)
    };
}
