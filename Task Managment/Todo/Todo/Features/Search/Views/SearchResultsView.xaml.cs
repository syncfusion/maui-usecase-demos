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
            ActionFlyout.ShowFor(anchor, note, vm.CustomLists);
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
