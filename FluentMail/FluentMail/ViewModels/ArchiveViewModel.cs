using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;

namespace FluentMail.ViewModels;

public partial class ArchiveViewModel : ObservableObject
{
    readonly Mailbox mailbox = MockData.Mailbox;

    [ObservableProperty]
    private ObservableCollection<MailMessage> visibleItems = [];

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private bool isEmpty;

    public IReadOnlyList<string> Filters { get; } = ["All", "Has Files", "This Year", "Flagged"];

    public ArchiveViewModel()
    {
        ApplyFilter("All");
    }

    [RelayCommand]
    void ApplyFilter(string filter)
    {
        SelectedFilter = filter;
        IEnumerable<MailMessage> items = mailbox.Archive;
        VisibleItems = new ObservableCollection<MailMessage>(filter switch
        {
            "Has Files" => items.Where(x => x.HasAttachments),
            "Flagged" => items.Where(x => x.IsFlagged),
            "This Year" => items,
            _ => items
        });
        IsEmpty = VisibleItems.Count == 0;
    }
}
