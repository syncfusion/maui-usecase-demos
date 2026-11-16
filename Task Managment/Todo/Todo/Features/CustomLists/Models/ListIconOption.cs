using CommunityToolkit.Mvvm.ComponentModel;

namespace Todo.Features.CustomLists.Models;

/// <summary>An emoji choice in the create / rename list dialogs.</summary>
public partial class ListIconOption : ObservableObject
{
    public ListIconOption(string glyph) => Glyph = glyph;

    public string Glyph { get; }

    [ObservableProperty]
    private bool isSelected;

    public static List<ListIconOption> CreateAll(string? selected)
    {
        var all = new[] { "\U0001F4BB", "\U0001F3A8", "\U0001F4C5", "\U0001F4A1", "\U0001F6B6",
                          "\U0001F525", "\U0001FA7A", "\U0001F9FA", "\u2764\uFE0F" }
            .Select(g => new ListIconOption(g) { IsSelected = g == selected })
            .ToList();
        return all;
    }
}
