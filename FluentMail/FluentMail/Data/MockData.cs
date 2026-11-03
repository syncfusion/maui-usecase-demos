using FluentMail.Models;

namespace FluentMail.Data;

public static class MockData
{
    public static Mailbox Mailbox { get; } = CreateMailbox();

    static Mailbox CreateMailbox()
    {
        var mailbox = new Mailbox();
        mailbox.Folders.Add(new FolderNode { Name = "Inbox", Count = 0, Children = [new FolderNode { Name = "Projects" }, new FolderNode { Name = "Clients" }] });
        mailbox.Folders.Add(new FolderNode { Name = "Sent Items", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Drafts", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Archive", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Deleted", Count = 0 });

        for (var i = 1; i <= 40; i++)
        {
            mailbox.Inbox.Add(new MailMessage
            {
                Id = $"inbox-{i}", Folder = "Inbox", Sender = $"Sender {i}", SenderEmail = $"sender{i}@contoso.com",
                Subject = $"Inbox thread {i}", Preview = $"Preview text for message {i}.", Body = $"Body for inbox message {i}.",
                Recipients = "me@contoso.com", Cc = i % 3 == 0 ? "manager@contoso.com" : string.Empty,
                Status = i % 5 == 0 ? "Attachments" : "Unread", HasAttachments = i % 5 == 0, IsRead = i % 2 == 0, IsFlagged = i % 7 == 0
            });
        }

        for (var i = 1; i <= 10; i++)
        {
            mailbox.SentItems.Add(new MailMessage
            {
                Id = $"sent-{i}", Folder = "Sent Items", Sender = "Me", SenderEmail = "me@contoso.com",
                Subject = $"Sent thread {i}", Preview = $"Sent preview {i}.", Body = $"Body for sent message {i}.",
                Recipients = $"contact{i}@contoso.com", Status = "Sent", IsRead = true, HasAttachments = i % 2 == 0
            });
        }

        for (var i = 1; i <= 20; i++)
            mailbox.Drafts.Add(new MailMessage { Id = $"draft-{i}", Folder = "Drafts", Sender = "Me", SenderEmail = "me@contoso.com", Subject = $"Draft {i}", Preview = $"Draft preview {i}.", Body = $"Draft body {i}.", Recipients = $"contact{i % 10 + 1}@contoso.com", Status = i % 2 == 0 ? "Saved" : "Unsaved Changes", HasAttachments = i % 4 == 0 });

        for (var i = 1; i <= 30; i++)
            mailbox.Archive.Add(new MailMessage { Id = $"archive-{i}", Folder = "Archive", Sender = $"Archived Sender {i}", SenderEmail = $"archived{i}@contoso.com", Subject = $"Archived thread {i}", Preview = $"Archived preview {i}.", Body = $"Archived body {i}.", Recipients = $"contact{i % 15 + 1}@contoso.com", Status = "Archived", HasAttachments = i % 3 == 0, IsFlagged = i % 6 == 0 });

        for (var i = 1; i <= 15; i++)
            mailbox.Deleted.Add(new MailMessage { Id = $"deleted-{i}", Folder = "Deleted", Sender = $"Deleted Sender {i}", SenderEmail = $"deleted{i}@contoso.com", Subject = $"Deleted thread {i}", Preview = $"Deleted preview {i}.", Body = $"Deleted body {i}.", Recipients = $"contact{i % 10 + 1}@contoso.com", Status = "Deleted", HasAttachments = i % 3 == 0 });

        mailbox.Attachments.Add(new AttachmentInfo { FileName = "Proposal.pdf", FileType = "PDF", FileSize = 218000 });
        mailbox.RefreshCounts();
        return mailbox;
    }

}
