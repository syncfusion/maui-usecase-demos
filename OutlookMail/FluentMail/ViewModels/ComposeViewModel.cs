using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;
using FluentMail.Services;
using MailContact = FluentMail.Models.Contact;

namespace FluentMail.ViewModels;

public partial class ComposeViewModel : ObservableObject
{
    readonly Mailbox mailbox;
    readonly IAlertService alerts;

    [ObservableProperty, NotifyCanExecuteChangedFor(nameof(SendCommand))] private string toText = string.Empty;
    [ObservableProperty] private string ccText = string.Empty;
    [ObservableProperty] private string subject = string.Empty;
    [ObservableProperty] private string body = string.Empty;
    [ObservableProperty] private ObservableCollection<object> toContacts = [];
    [ObservableProperty] private ObservableCollection<object> ccContacts = [];
    [ObservableProperty] private bool showMessage;

    static bool IsPhone => DeviceInfo.Idiom == DeviceIdiom.Phone;

    public ObservableCollection<MailContact> DesktopContacts { get; }
    public ObservableCollection<MailContact> PhoneContacts { get; }

    public ComposeViewModel(Mailbox mailbox, IAlertService alerts)
    {
        this.mailbox = mailbox;
        this.alerts = alerts;
        DesktopContacts = IsPhone ? [] : mailbox.Contacts;
        PhoneContacts = IsPhone ? mailbox.Contacts : [];
        ToContacts.CollectionChanged += OnToContactsCollectionChanged;
    }

    partial void OnToContactsChanged(ObservableCollection<object>? oldValue, ObservableCollection<object> newValue)
    {
        if (oldValue is not null) oldValue.CollectionChanged -= OnToContactsCollectionChanged;
        newValue.CollectionChanged += OnToContactsCollectionChanged;
        SendCommand.NotifyCanExecuteChanged();
    }

    void OnToContactsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => SendCommand.NotifyCanExecuteChanged();

    public void Reset()
    {
        ToContacts = [];
        CcContacts = [];
        ToText = string.Empty;
        CcText = string.Empty;
        Subject = string.Empty;
        Body = string.Empty;
    }

    static string Resolve(IEnumerable<object> selected, string typed)
    {
        var emails = selected.OfType<MailContact>().Select(x => x.Email).ToList();
        var text = typed.Trim();
        if (text.Length > 0 && System.Net.Mail.MailAddress.TryCreate(text, out var address) && !emails.Contains(address.Address))
            emails.Add(address.Address);
        return string.Join(", ", emails);
    }

    bool CanSend() => Resolve(ToContacts, ToText).Length > 0;

    [RelayCommand(CanExecute = nameof(CanSend))]
    async Task SendAsync()
    {
        // Demo only: a regex is enough to flatten the mock editor's HTML into a preview.
        // Do not reuse this for sanitising HTML in production code; use a real HTML parser/sanitizer.
        var plainBody = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(Body ?? string.Empty, "<[^>]+>", " ")).Trim();
        plainBody = System.Text.RegularExpressions.Regex.Replace(plainBody, @"\s+", " ");
        var recipients = Resolve(ToContacts, ToText);
        var cc = Resolve(CcContacts, CcText);
        mailbox.SentItems.Insert(0, new MailMessage { Id = $"sent-{Guid.NewGuid():N}", Folder = FolderNames.SentItems, Sender = CurrentUser.DisplayName, SenderEmail = CurrentUser.Email, Subject = string.IsNullOrWhiteSpace(Subject) ? "(No subject)" : Subject, Preview = plainBody[..Math.Min(plainBody.Length, 80)], Body = plainBody, ReceivedOn = DateTime.Now, Recipients = recipients, RecipientsDisplay = recipients, Cc = cc, CcDisplay = cc, IsRead = true });
        mailbox.RefreshCounts();
        Reset();
        ShowMessage = true;
        await alerts.ShowAsync("Mail sent", "Your message has been sent.", "OK");
    }
}
