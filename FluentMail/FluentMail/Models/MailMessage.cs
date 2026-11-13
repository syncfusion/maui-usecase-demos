using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentMail.Models;

public partial class MailMessage : ObservableObject
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
    public string AttachmentName { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public bool HasTag => !string.IsNullOrEmpty(Tag);
    public bool HasAttachmentName => !string.IsNullOrEmpty(AttachmentName);
    public bool IsUnread => !IsRead;
    public string TimeText => ReceivedOn.Date == DateTime.Today ? $"{ReceivedOn:h:mm tt}" : ReceivedOn.Date == DateTime.Today.AddDays(-1) ? "Yesterday" : $"{ReceivedOn:MMM d}";
    public string Greeting { get; set; } = string.Empty;
    public bool HasAttachments { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnread))]
    private bool isRead;

    [ObservableProperty]
    private bool isFlagged;

    [ObservableProperty]
    private bool isHovered;

    public DateTime ReceivedOn { get; set; }
}
