namespace FluentMail.Models;

public class FolderNode
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<FolderNode> Children { get; set; } = [];
}
