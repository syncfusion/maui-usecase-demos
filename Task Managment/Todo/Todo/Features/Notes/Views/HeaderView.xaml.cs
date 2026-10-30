using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class HeaderView : ContentView
{
    public HeaderView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Activates inline search when the user focuses the shared header entry.
    /// </summary>
    private void OnSearchEntryFocused(object? sender, FocusEventArgs e)
    {
        if (BindingContext is NotesListViewModel vm && !vm.IsSearchActive)
            vm.OpenSearchCommand.Execute(null);
    }
}
