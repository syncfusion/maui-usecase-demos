using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class ComposeViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty] private string fromAccount = "Me <me@contoso.com>";
    [ObservableProperty] private string recipientSearch = string.Empty;
    [ObservableProperty] private string subject = string.Empty;
    [ObservableProperty] private string body = string.Empty;
    [ObservableProperty] private ObservableCollection<Contact> recipientChips = [];
    [ObservableProperty] private ObservableCollection<Contact> recipientSuggestions = [];
    [ObservableProperty] private ObservableCollection<AttachmentInfo> attachments = [];
    [ObservableProperty] private string statusMessage = "Unsaved Changes";
    [ObservableProperty] private bool showMessage;

    public IReadOnlyList<string> Accounts { get; } = ["Me <me@contoso.com>", "Work <work@contoso.com>"];
    public ObservableCollection<Contact> Contacts => mailbox.Contacts;

    public ComposeViewModel() => UpdateSuggestions();

    partial void OnRecipientSearchChanged(string value) => UpdateSuggestions();
    partial void OnSubjectChanged(string value) => StatusMessage = "Unsaved Changes";
    partial void OnBodyChanged(string value) => StatusMessage = "Unsaved Changes";

    [RelayCommand]
    void AddRecipient(Contact? contact)
    {
        if (contact is null || RecipientChips.Any(x => x.Email == contact.Email)) return;
        RecipientChips.Add(contact);
        RecipientSearch = string.Empty;
        UpdateSuggestions();
    }

    [RelayCommand]
    void RemoveRecipient(Contact? contact)
    {
        if (contact is null) return;
        RecipientChips.Remove(contact);
        UpdateSuggestions();
    }

    [RelayCommand]
    void AddAttachment(AttachmentInfo? attachment)
    {
        if (attachment is not null && !Attachments.Any(x => x.FileName == attachment.FileName)) Attachments.Add(attachment);
    }

    [RelayCommand]
    void Send()
    {
        if (RecipientChips.Count == 0) { StatusMessage = "Add at least one recipient."; return; }
        if (string.IsNullOrWhiteSpace(Subject)) { StatusMessage = "Enter a subject."; return; }
        mailbox.SentItems.Insert(0, new MailMessage { Id=$"sent-{Guid.NewGuid():N}", Folder="Sent Items", Sender="Me", SenderEmail="me@contoso.com", Subject=Subject, Preview=Body[..Math.Min(Body.Length, 40)], Body=Body, Recipients=string.Join(", ", RecipientChips.Select(x=>x.Email)), Status="Sent", IsRead=true, HasAttachments=Attachments.Count>0 });
        mailbox.RefreshCounts();
        RecipientChips.Clear(); Attachments.Clear(); Subject=string.Empty; Body=string.Empty; StatusMessage="Message sent"; ShowMessage = true;
    }

    void UpdateSuggestions()
    {
        var term = RecipientSearch.Trim();
        RecipientSuggestions = new ObservableCollection<Contact>(string.IsNullOrWhiteSpace(term) ? mailbox.Contacts.Take(8) : mailbox.Contacts.Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)).Take(8));
    }
}
