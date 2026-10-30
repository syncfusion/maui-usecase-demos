using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Todo.Features.CustomLists.Models;

/// <summary>
/// Represents a single user-defined list (sidebar entry below the
/// four built-in categories). A <c>Note</c> belongs to a list
/// when its <c>ListId</c> matches this list's <see cref="Id"/>.
/// </summary>
/// <remarks>
/// Inherits from <see cref="ObservableObject"/> so the sidebar and any
/// future list-level views can react to changes (e.g. rename) without
/// any external bookkeeping.
/// </remarks>
public partial class NoteList : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string name = string.Empty;

    /// <summary>
    /// Reserved for the four built-in categories. Always <c>false</c>
    /// for user-created lists; kept here so a future task can promote
    /// one without a model break.
    /// </summary>
    [ObservableProperty]
    private bool isSystem;

    [ObservableProperty]
    private int displayOrder;

    [ObservableProperty]
    private NoteListSortOrder sortOrder;

    [ObservableProperty]
    private string? backgroundColorHex;

    [ObservableProperty]
    private string? backgroundImageSource;

    public ObservableCollection<string> CustomBackgroundImageSources { get; } = new();

    [ObservableProperty]
    private DateTime createdAt = DateTime.Now;
}

public enum NoteListSortOrder
{
    Custom,
    Important,
    Reminder,
    DueDate,
    Alphabetical
}