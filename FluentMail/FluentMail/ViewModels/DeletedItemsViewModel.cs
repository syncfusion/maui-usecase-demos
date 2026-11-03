using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class DeletedItemsViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty]
    private ObservableCollection<MailMessage> deletedItems = [];

    [ObservableProperty]
    private bool isEmpty;

    public DeletedItemsViewModel()
    {
        Reload();
    }

    [RelayCommand]
    void Restore(MailMessage? item)
    {
        if (item is null)
        {
            return;
        }

        mailbox.Deleted.Remove(item);
        item.Folder = "Inbox";
        mailbox.Inbox.Insert(0, item);
        mailbox.RefreshCounts();
        Reload();
    }

    [RelayCommand]
    void EmptyTrash()
    {
        mailbox.Deleted.Clear();
        mailbox.RefreshCounts();
        Reload();
    }

    void Reload()
    {
        DeletedItems = new ObservableCollection<MailMessage>(mailbox.Deleted);
        IsEmpty = DeletedItems.Count == 0;
    }
}
