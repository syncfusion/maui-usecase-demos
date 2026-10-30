using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Todo.Common;
using Todo.Features.CustomLists.Services;
using Todo.Features.EditNote.ViewModels;
using Todo.Features.EditNote.Views;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.Services;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Search.Services;

namespace Todo.Features.Search.ViewModels;

/// <summary>
/// ViewModel for the inline Search results view. The view is a thin
/// shell around the shared header search input and an
/// <see cref="ObservableCollection{T}"/> of matching notes — the
/// design is captured in <c>search.png</c>.
///
/// <para>
/// The ViewModel owns the search query, the filtered results, and the
/// "clear" affordance. It delegates the actual matching to
/// <see cref="INoteSearchService"/> so the same search rules can be
/// shared with any other surface that might need them later.
/// </para>
/// </summary>
public partial class SearchResultsViewModel : ViewModelBase
{
    private readonly INoteSearchService _searchService;
    private readonly INoteService _noteService;
    private readonly IListService _listService;
    private readonly INavigationService _navigation;

    public SearchResultsViewModel(
        INoteSearchService searchService,
        INoteService noteService,
        IListService listService,
        INavigationService navigation)
    {
        _searchService = searchService;
        _noteService = noteService;
        _listService = listService;
        _navigation = navigation;
        Results = new ObservableCollection<Note>();
    }

    public ReadOnlyObservableCollection<Todo.Features.CustomLists.Models.NoteList> CustomLists => _listService.Lists;

    /// <summary>
    /// Currently-displayed search results. Bound to the list in
    /// <c>SearchResultsView.xaml</c>.
    /// </summary>
    public ObservableCollection<Note> Results { get; }

    /// <summary>
    /// True when the query is non-empty and there are no matching
    /// notes. Drives the "no results" empty state.
    /// </summary>
    public bool HasNoResults => !string.IsNullOrWhiteSpace(Query) && Results.Count == 0;

    public bool HasResults => !string.IsNullOrWhiteSpace(Query) && Results.Count > 0;

    /// <summary>
    /// True when the query is empty. Drives the "You can search the
    /// created notes here" empty state from <c>search.png</c>.
    /// </summary>
    public bool IsQueryEmpty => string.IsNullOrWhiteSpace(Query);

    public string ResultsDescription => IsQueryEmpty ? string.Empty : $"Results for '{Query.Trim()}'";

    public string ResultCountText => Results.Count == 1
        ? "1 matching note found"
        : $"{Results.Count} matching notes found";

    public string NoResultsMessage => $"No notes found matching '{Query.Trim()}'";

    [ObservableProperty]
    private string query = string.Empty;

    partial void OnQueryChanged(string value)
    {
        RefreshResults();
    }

    [RelayCommand]
    private void ClearQuery()
    {
        Query = string.Empty;
    }

    /// <summary>
    /// Re-runs the search and replaces <see cref="Results"/> with the
    /// fresh set of matches. Called whenever the query text changes.
    /// </summary>
    private void RefreshResults()
    {
        Results.Clear();
        foreach (var match in _searchService.Search(Query))
            Results.Add(match);

        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(IsQueryEmpty));
        OnPropertyChanged(nameof(ResultsDescription));
        OnPropertyChanged(nameof(ResultCountText));
        OnPropertyChanged(nameof(NoResultsMessage));
    }

    // ---------------------------------------------------------------
    //  Per-result commands
    // ---------------------------------------------------------------

    [RelayCommand]
    private async Task ToggleComplete(Note? note)
    {
        if (note is null) return;
        await _noteService.ToggleCompleteAsync(note.Id);
    }

    [RelayCommand]
    private async Task ToggleImportant(Note? note)
    {
        if (note is null) return;
        await _noteService.ToggleImportantAsync(note.Id);
    }

    [RelayCommand]
    private async Task Delete(Note? note)
    {
        if (note is null) return;
        await _noteService.SoftDeleteAsync(note.Id);
        RefreshResults();
    }

    [RelayCommand]
    private async Task Restore(Note? note)
    {
        if (note is null) return;
        await _noteService.RestoreAsync(note.Id);
        RefreshResults();
    }

    [RelayCommand]
    private async Task MoveNoteToTargetAsync(DropTargetRequest? request)
    {
        if (request is null || string.IsNullOrEmpty(request.NoteId)) return;
        var note = _noteService.Notes.FirstOrDefault(item => item.Id == request.NoteId);
        if (note is null) return;

        var category = NotesListViewModel.TryDecodeCategoryKey(request.TargetKey);
        if (request.TargetKey == "my")
        {
            category = NotesListCategory.MyNotes;
        }
        if (category.HasValue)
            await _noteService.MoveToCategoryAsync(note.Id, category.Value);
        else if (request.TargetKey.StartsWith("list:", StringComparison.Ordinal))
            await _noteService.MoveToListAsync(note.Id, request.TargetKey["list:".Length..]);

        RefreshResults();
    }

    [RelayCommand]
    private async Task OpenNoteAsync(Note? note)
    {
        if (note is null) return;
        var page = new EditNotePage(new EditNoteViewModel(_noteService, _navigation, note));
        await _navigation.PushModalAsync(page);
    }
}
