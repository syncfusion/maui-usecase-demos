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
    private bool isFavourite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowStar))]
    private bool isHovered;

    public bool ShowStar => IsHovered && CanFavourite;

    public string Section { get; set; } = string.Empty;

    public bool CanFavourite { get; set; }

    public ObservableCollection<FolderNode> Children { get; set; } = [];
}
