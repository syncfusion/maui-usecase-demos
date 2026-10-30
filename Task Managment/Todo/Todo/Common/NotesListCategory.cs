namespace Todo.Common;

/// <summary>
/// Built-in lists that appear in the sidebar and represent the
/// four filter views of the To-Do List sample.
/// </summary>
public enum NotesListCategory
{
    MyNotes,
    Important,
    Reminder,
    Bin
}

public static class NotesListCategoryExtensions
{
    public static string ToDisplayName(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "My Notes",
        NotesListCategory.Important => "Important",
        NotesListCategory.Reminder => "Reminder",
        NotesListCategory.Bin => "Bin",
        _ => category.ToString()
    };

    public static string ToAccentColorKey(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "MyNotesColor",
        NotesListCategory.Important => "ImportantColor",
        NotesListCategory.Reminder => "ReminderColor",
        NotesListCategory.Bin => "BinColor",
        _ => "MyNotesColor"
    };

    public static string ToDefaultThemeColor(this NotesListCategory category) => category switch
    {
        NotesListCategory.MyNotes => "#F4F5F7",
        NotesListCategory.Important => "#FFF2CC",
        NotesListCategory.Reminder => "#EEE8FF",
        NotesListCategory.Bin => "#FDE8E7",
        _ => "#F4F5F7"
    };
}

public static class NoteListThemePalette
{
    public static IReadOnlyList<(string Name, string Hex)> Themes { get; } =
    [
        ("Amber", "#FFF2CC"),
        ("Lavender", "#EEE8FF"),
        ("Rose", "#FDE8E7"),
        ("Mint", "#E8F5E9"),
        ("Sky", "#E3F2FD"),
        ("Soft Rose", "#FCE4EC"), 
    ];

    public static string GetRandomThemeColor()
        => Themes[Random.Shared.Next(Themes.Count)].Hex;
}