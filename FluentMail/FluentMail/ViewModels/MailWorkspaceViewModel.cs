using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class MailWorkspaceViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    public ObservableCollection<FolderNode> Folders => mailbox.Folders;

    [ObservableProperty]
    private FolderNode? selectedFolder;

    [ObservableProperty]
    private ObservableCollection<MailMessage> visibleMessages = [];

    [ObservableProperty]
    private MailMessage? selectedMessage;

    [ObservableProperty]
    private bool isEmptyFolder;

    public MailWorkspaceViewModel()
    {
        LoadFolder(mailbox.Folders.First());
    }

    partial void OnSelectedFolderChanged(FolderNode? value)
    {
        if (value is not null)
        {
            LoadFolder(value);
        }
    }

    partial void OnSelectedMessageChanged(MailMessage? value)
    {
        if (value is not null)
        {
            value.IsRead = true;
        }
    }

    [RelayCommand]
    void SelectMessage(MailMessage? message) => SelectedMessage = message;

    void LoadFolder(FolderNode folder)
    {
        VisibleMessages = new ObservableCollection<MailMessage>(mailbox.GetMessages(folder.Name));
        IsEmptyFolder = VisibleMessages.Count == 0;
        SelectedMessage = VisibleMessages.FirstOrDefault();
    }
}
