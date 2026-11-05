using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;
using MailContact = FluentMail.Models.Contact;

namespace FluentMail.ViewModels;

public partial class ComposeViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty] private string fromAccount = "Me <me@contoso.com>";
    [ObservableProperty] private string recipientSearch = string.Empty;
    [ObservableProperty] private string to = string.Empty;
    [ObservableProperty] private string cc = string.Empty;
    [ObservableProperty] private string subject = string.Empty;
    [ObservableProperty] private string body = string.Empty;
    [ObservableProperty] private ObservableCollection<MailContact> recipientChips = [];
    [ObservableProperty] private ObservableCollection<MailContact> recipientSuggestions = [];
    [ObservableProperty] private ObservableCollection<AttachmentInfo> attachments = [];
    [ObservableProperty] private string statusMessage = "Unsaved Changes";
    [ObservableProperty] private bool showMessage;

    public IReadOnlyList<string> Accounts { get; } = ["Me <me@contoso.com>", "Work <work@contoso.com>"];
    public ObservableCollection<MailContact> Contacts => mailbox.Contacts;

    public ComposeViewModel() => UpdateSuggestions();

    partial void OnRecipientSearchChanged(string value) => UpdateSuggestions();
    partial void OnSubjectChanged(string value) => StatusMessage = "Unsaved Changes";
    partial void OnBodyChanged(string value) => StatusMessage = "Unsaved Changes";

    [RelayCommand]
    void AddRecipient(MailContact? contact)
    {
        if (contact is null || RecipientChips.Any(x => x.Email == contact.Email)) return;
        RecipientChips.Add(contact);
        RecipientSearch = string.Empty;
        UpdateSuggestions();
    }

    [RelayCommand]
    void RemoveRecipient(MailContact? contact)
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
    void Attach() => AddAttachment(mailbox.Attachments.FirstOrDefault());

    [RelayCommand]
    async Task SendAsync()
    {
        var plainBody = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(Body ?? string.Empty, "<[^>]+>", " ")).Trim();
        plainBody = System.Text.RegularExpressions.Regex.Replace(plainBody, @"\s+", " ");
        var recipients = string.IsNullOrWhiteSpace(To) ? string.Join(", ", RecipientChips.Select(x => x.Email)) : To.Trim();
        if (string.IsNullOrWhiteSpace(recipients)) { StatusMessage = "Add at least one recipient."; return; }
        if (string.IsNullOrWhiteSpace(Subject)) { StatusMessage = "Enter a subject."; return; }
        mailbox.SentItems.Insert(0, new MailMessage { Id=$"sent-{Guid.NewGuid():N}", Folder="Sent Items", Sender="Me", SenderEmail="me@contoso.com", Subject=Subject, Preview=plainBody[..Math.Min(plainBody.Length, 80)], Body=plainBody, ReceivedOn=DateTime.Now, Recipients=recipients, Cc=Cc.Trim(), Status="Sent", Greeting=$"Hi {recipients.Split(',')[0].Trim().Split('@')[0]},", IsRead=true, HasAttachments=Attachments.Count>0 });
        mailbox.RefreshCounts();
        RecipientChips.Clear(); Attachments.Clear(); To=string.Empty; Cc=string.Empty; Subject=string.Empty; Body=string.Empty; StatusMessage="Message sent"; ShowMessage = true;
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
        {
            await page.DisplayAlertAsync("Mail sent", "Your message has been sent.", "OK");
        }
    }

    void UpdateSuggestions()
    {
        var term = RecipientSearch.Trim();
        RecipientSuggestions = new ObservableCollection<MailContact>(string.IsNullOrWhiteSpace(term) ? mailbox.Contacts.Take(8) : mailbox.Contacts.Where(c => c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)).Take(8));
    }
}
