using Todo.Common;
using Todo.Features.Notes.Models;
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
