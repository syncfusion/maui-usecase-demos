using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentMail.Models;

public partial class FolderNode : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int count;

    [ObservableProperty]
    private int unreadCount;

    [ObservableProperty]
    private bool isFavourite;

    public bool CanFavourite { get; set; }

    public ObservableCollection<FolderNode> Children { get; set; } = [];
}
