using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.Services;

namespace Todo.Features.CustomLists.Services;

/// <summary>
/// Singleton in-memory implementation of <see cref="IListService"/>
/// that restores and saves custom-list metadata through application
/// preferences. When a list is deleted, any notes that referenced it
/// are returned to <c>My Notes</c> by clearing their <c>ListId</c>
/// through the shared <see cref="INoteService"/>.
/// </summary>
public sealed class InMemoryListService : IListService
{
    private const string PersistedListsKey = "CustomNoteLists";
    private readonly ObservableCollection<NoteList> _lists;
    private readonly INoteService _noteService;

    public InMemoryListService(INoteService noteService)
    {
        _noteService = noteService;
        _lists = new ObservableCollection<NoteList>();
        var storedLists = JsonSerializer.Deserialize<List<StoredNoteList>>(
            Preferences.Default.Get(PersistedListsKey, "[]")) ?? [];
        var requiresThemeMigration = false;
        foreach (var stored in storedLists.OrderBy(item => item.DisplayOrder))
        {
            var backgroundColorHex = stored.BackgroundColorHex;
            if (string.IsNullOrWhiteSpace(backgroundColorHex))
            {
                backgroundColorHex = NoteListThemePalette.GetRandomThemeColor();
                requiresThemeMigration = true;
            }

            var list = new NoteList
            {
                Id = stored.Id,
                Name = stored.Name,
                IsSystem = false,
                DisplayOrder = stored.DisplayOrder,
                SortOrder = stored.SortOrder,
                BackgroundColorHex = backgroundColorHex,
                BackgroundImageSource = stored.BackgroundImageSource,
                CreatedAt = stored.CreatedAt
            };
            foreach (var source in stored.CustomBackgroundImageSources ?? [])
                list.CustomBackgroundImageSources.Add(source);

            _lists.Add(list);
            Subscribe(list);
        }
        _lists.CollectionChanged += OnListsChanged;
        Lists = new ReadOnlyObservableCollection<NoteList>(_lists);
        if (requiresThemeMigration)
            PersistLists();
    }

    public ReadOnlyObservableCollection<NoteList> Lists { get; }

    public Task<NoteList> CreateAsync(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("List name is required.", nameof(name));
        if (NameExists(trimmed))
            throw new ArgumentException($"A list named '{trimmed}' already exists.", nameof(name));

        var nextOrder = _lists.Count == 0 ? 0 : _lists.Max(l => l.DisplayOrder) + 1;
        var list = new NoteList
        {
            Id = Guid.NewGuid().ToString(),
            Name = trimmed,
            IsSystem = false,
            DisplayOrder = nextOrder,
            BackgroundColorHex = NoteListThemePalette.GetRandomThemeColor(),
            CreatedAt = DateTime.Now
        };
        _lists.Add(list);
        return Task.FromResult(list);
    }

    public Task RenameAsync(string id, string newName)
    {
        var trimmed = (newName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("List name is required.", nameof(newName));

        var existing = _lists.FirstOrDefault(l => l.Id == id)
            ?? throw new KeyNotFoundException($"List '{id}' was not found.");

        if (NameExists(trimmed, excludeId: id))
            throw new ArgumentException($"A list named '{trimmed}' already exists.", nameof(newName));

        existing.Name = trimmed;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        var existing = _lists.FirstOrDefault(l => l.Id == id);
        if (existing is null) return Task.CompletedTask;

        _lists.Remove(existing);

        // Re-home any notes that were in this list back to My Notes so
        // the user doesn't lose them (per spec §7.5: deleting a list
        // removes the list, not its notes).
        foreach (var note in _noteService.Notes)
        {
            if (note.ListId == id)
                note.ListId = null;
        }

        return Task.CompletedTask;
    }

    public Task ReorderAsync(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _lists.Count)
            return Task.CompletedTask;

        newIndex = Math.Clamp(newIndex, 0, _lists.Count - 1);
        if (oldIndex == newIndex)
            return Task.CompletedTask;

        _lists.Move(oldIndex, newIndex);
        for (var index = 0; index < _lists.Count; index++)
            _lists[index].DisplayOrder = index;

        return Task.CompletedTask;
    }

    public bool NameExists(string name, string? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        var comparison = StringComparison.OrdinalIgnoreCase;
        return _lists.Any(l => l.Id != excludeId
                                && string.Equals(l.Name, name.Trim(), comparison));
    }

    private void OnListsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (NoteList list in e.OldItems)
                Unsubscribe(list);
        if (e.NewItems is not null)
            foreach (NoteList list in e.NewItems)
                Subscribe(list);
        PersistLists();
    }

    private void Subscribe(NoteList list)
    {
        list.PropertyChanged += OnListPropertyChanged;
        list.CustomBackgroundImageSources.CollectionChanged += OnBackgroundImagesChanged;
    }

    private void Unsubscribe(NoteList list)
    {
        list.PropertyChanged -= OnListPropertyChanged;
        list.CustomBackgroundImageSources.CollectionChanged -= OnBackgroundImagesChanged;
    }

    private void OnListPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NoteList.Name)
            or nameof(NoteList.DisplayOrder)
            or nameof(NoteList.SortOrder)
            or nameof(NoteList.BackgroundImageSource)
            or nameof(NoteList.BackgroundColorHex))
            PersistLists();
    }

    private void OnBackgroundImagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => PersistLists();

    private void PersistLists()
    {
        var storedLists = _lists.Select(list => new StoredNoteList(
            list.Id,
            list.Name,
            list.DisplayOrder,
            list.SortOrder,
            list.BackgroundColorHex,
            list.CreatedAt,
            list.BackgroundImageSource,
            list.CustomBackgroundImageSources.ToList())).ToList();
        Preferences.Default.Set(PersistedListsKey, JsonSerializer.Serialize(storedLists));
    }

    private sealed record StoredNoteList(
        string Id,
        string Name,
        int DisplayOrder,
        NoteListSortOrder SortOrder,
        string? BackgroundColorHex,
        DateTime CreatedAt,
        string? BackgroundImageSource = null,
        List<string>? CustomBackgroundImageSources = null);
}