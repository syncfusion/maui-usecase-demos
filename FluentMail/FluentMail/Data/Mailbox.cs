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
