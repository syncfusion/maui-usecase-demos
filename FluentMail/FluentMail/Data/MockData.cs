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

        for (var i = 1; i <= 25; i++)
            mailbox.Contacts.Add(new Contact { Name = $"Contact {i}", Email = $"contact{i}@contoso.com" });

        for (var i = 1; i <= 10; i++)
            mailbox.Attachments.Add(new AttachmentInfo { FileName = $"File{i}.pdf", FileType = "PDF", FileSize = 1024 * i });

        for (var i = 1; i <= 50; i++)
            mailbox.Messages.Add(new MailMessage { Sender = $"Sender {i}", SenderEmail = $"sender{i}@contoso.com", Subject = $"Inbox thread {i}", Preview = $"Preview text for message {i}." });

        for (var i = 1; i <= 20; i++)
            mailbox.Drafts.Add(new MailMessage { Sender = "Me", SenderEmail = "me@contoso.com", Subject = $"Draft {i}", Preview = $"Draft preview {i}." });

        for (var i = 1; i <= 30; i++)
            mailbox.Archive.Add(new MailMessage { Sender = $"Archived Sender {i}", SenderEmail = $"archived{i}@contoso.com", Subject = $"Archived thread {i}", Preview = $"Archived preview {i}." });

        for (var i = 1; i <= 15; i++)
            mailbox.Deleted.Add(new MailMessage { Sender = $"Deleted Sender {i}", SenderEmail = $"deleted{i}@contoso.com", Subject = $"Deleted thread {i}", Preview = $"Deleted preview {i}." });

        UpdateCounts(mailbox);
        return mailbox;
    }

    static void UpdateCounts(Mailbox mailbox)
    {
        mailbox.Folders[0].Count = mailbox.Messages.Count;
        mailbox.Folders[1].Count = 0;
        mailbox.Folders[2].Count = mailbox.Drafts.Count;
        mailbox.Folders[3].Count = mailbox.Archive.Count;
        mailbox.Folders[4].Count = mailbox.Deleted.Count;
    }
}
