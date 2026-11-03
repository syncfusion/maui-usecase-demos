using System.Collections.ObjectModel;
using FluentMail.Models;

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

    public void RefreshCounts()
    {
        Folders[0].Count = Inbox.Count;
        Folders[1].Count = SentItems.Count;
        Folders[2].Count = Drafts.Count;
        Folders[3].Count = Archive.Count;
        Folders[4].Count = Deleted.Count;
    }

    public IEnumerable<MailMessage> GetMessages(string folder) => folder switch
    {
        "Inbox" => Inbox,
        "Sent Items" => SentItems,
        "Drafts" => Drafts,
        "Archive" => Archive,
        "Deleted" => Deleted,
        _ => []
    };
}
