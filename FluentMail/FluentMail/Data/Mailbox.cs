using System.Collections.ObjectModel;
using FluentMail.Models;

namespace FluentMail.Data;

public class Mailbox
{
    public ObservableCollection<FolderNode> Folders { get; } = [];
    public ObservableCollection<MailMessage> Messages { get; } = [];
    public ObservableCollection<MailMessage> Drafts { get; } = [];
    public ObservableCollection<MailMessage> Archive { get; } = [];
    public ObservableCollection<MailMessage> Deleted { get; } = [];
    public ObservableCollection<Contact> Contacts { get; } = [];
    public ObservableCollection<AttachmentInfo> Attachments { get; } = [];
}
