using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class DraftWorkspaceViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty]
    private ObservableCollection<MailMessage> drafts = [];

    [ObservableProperty]
    private MailMessage? selectedDraft;

    [ObservableProperty]
    private string recipient = string.Empty;

    [ObservableProperty]
    private string subject = string.Empty;

    [ObservableProperty]
    private string body = string.Empty;

    [ObservableProperty]
    private string saveStatus = "Draft Saved";

    [ObservableProperty]
    private ObservableCollection<AttachmentInfo> attachments = [];

    public DraftWorkspaceViewModel()
    {
        Reload();
        SelectedDraft = Drafts.FirstOrDefault();
    }

    partial void OnSelectedDraftChanged(MailMessage? value)
    {
        if (value is null)
        {
            return;
        }

        Recipient = value.Recipients;
        Subject = value.Subject;
        Body = value.Body;
        SaveStatus = value.Status;
    }

    partial void OnRecipientChanged(string value) => SaveStatus = "Unsaved Changes";
    partial void OnSubjectChanged(string value) => SaveStatus = "Unsaved Changes";
    partial void OnBodyChanged(string value) => SaveStatus = "Unsaved Changes";

    [RelayCommand]
    void SelectDraft(MailMessage? draft) => SelectedDraft = draft;

    [RelayCommand]
    void Save()
    {
        if (SelectedDraft is null) return;
        SelectedDraft.Recipients = Recipient;
        SelectedDraft.Subject = Subject;
        SelectedDraft.Body = Body;
        SelectedDraft.Status = "Draft Saved";
        SaveStatus = "Draft Saved";
    }

    void Reload()
    {
        Drafts = new ObservableCollection<MailMessage>(mailbox.Drafts);
    }
}
