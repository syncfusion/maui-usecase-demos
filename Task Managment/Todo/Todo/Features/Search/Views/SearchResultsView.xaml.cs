using Todo.Common;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Notes.Views;
using Todo.Features.Search.ViewModels;

namespace Todo.Features.Search.Views;

/// <summary>
/// Search results displayed inside the main content area while the shared
/// application header and sidebar remain visible.
/// </summary>
public partial class SearchResultsView : ContentView
{
    public SearchResultsView()
    {
        InitializeComponent();
        ActionFlyout.ActionRequested += OnNoteActionRequested;
    }

    private static void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.SetHovered(border);
    }

    private static void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.Reset(border);
    }

    private void OnResultPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.SetHovered(border);
    }

    private static void OnResultPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            var backgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#1E1E1E")
                : Colors.White;
            PointerHoverSurface.Reset(border, backgroundColor);
        }
    }

    private void OnNoteMenuClicked(object? sender, EventArgs e)
    {
        if (sender is View anchor)
            ShowNoteActions(anchor);
    }

    private void OnNoteSecondaryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is View anchor)
            ShowNoteActions(anchor);
    }

    private void ShowNoteActions(View anchor)
    {
        if (BindingContext is SearchResultsViewModel vm && anchor.BindingContext is Note note)
            ActionFlyout.ShowFor(anchor, note, vm.CustomLists, vm.Results);
    }

    private void OnNoteActionRequested(object? sender, NoteActionRequestedEventArgs e)
    {
        if (BindingContext is not SearchResultsViewModel vm)
            return;

        switch (e.Action)
        {
            case NoteActionKind.ToggleCompletion:
                vm.ToggleCompleteCommand.Execute(e.Note);
                break;
            case NoteActionKind.SetVisibleCompletion when e.IsCompleted.HasValue && e.NoteIds is not null:
                vm.SetNotesCompletionCommand.Execute(new SetNotesCompletionRequest(e.NoteIds, e.IsCompleted.Value));
                break;
            case NoteActionKind.Edit:
                vm.OpenNoteCommand.Execute(e.Note);
                break;
            case NoteActionKind.Move when e.TargetKey is not null:
                vm.MoveNoteToTargetCommand.Execute(new DropTargetRequest(e.Note.Id, e.TargetKey));
                break;
            case NoteActionKind.Restore:
                vm.RestoreCommand.Execute(e.Note);
                break;
            case NoteActionKind.Delete:
                vm.DeleteCommand.Execute(e.Note);
                break;
        }
    }

    /// <summary>
    /// Routes a checkbox toggle on a result card into the ViewModel's
    /// <c>ToggleCompleteCommand</c>. See <c>NotesContentView.OnCheckChanged</c>
    /// for the same pattern in the main list (PR #37 review issue #7).
    /// </summary>
    private void OnCheckChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (sender is not BindableObject bindable || bindable.BindingContext is not Note note)
            return;
        if (note.IsCompleted == e.Value) return;
        if (BindingContext is SearchResultsViewModel vm)
            vm.ToggleCompleteCommand.Execute(note);
    }
}
