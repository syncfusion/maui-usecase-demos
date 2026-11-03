using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentMail.Models;

public partial class FolderNode : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private int count;

    public List<FolderNode> Children { get; set; } = [];
}
