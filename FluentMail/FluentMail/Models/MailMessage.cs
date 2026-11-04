namespace FluentMail.Models;

public class MailMessage
{
    public string Id { get; set; } = string.Empty;
    public string Folder { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public string Cc { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool HasAttachments { get; set; }
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
    public DateTime ReceivedOn { get; set; }
    public DateTime ArchivedOn { get; set; }
    public DateTime DeletedOn { get; set; }
}
