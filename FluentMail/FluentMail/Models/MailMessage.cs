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
    public string Greeting { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool HasAttachments { get; set; }

    [ObservableProperty]
    private bool isRead;

    [ObservableProperty]
    private bool isFlagged;

    [ObservableProperty]
    private bool isHovered;

    [ObservableProperty]
    private bool isFavourite;

    public DateTime ReceivedOn { get; set; }
    public DateTime ArchivedOn { get; set; }
    public DateTime DeletedOn { get; set; }
}
