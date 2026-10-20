using System.ComponentModel;
using Todo.Features.Notes.ViewModels;
using Todo.Features.Search.Views;

namespace Todo.Features.Notes.Views;

/// <summary>
/// Hosts the application shell: header + sidebar + main content area.
/// The sidebar column collapses to zero width when
/// <see cref="NotesListViewModel.IsSidebarCollapsed"/> is true, so the
/// main content area automatically occupies the full available width.
/// </summary>
public partial class MainLayoutView : ContentView
{
    // Module-08 task 8-1: expanded sidebar matches the wide layout in
    // mynotes.png. Phone layout starts collapsed (matches collapsed.png);
    // the hamburger button in the header toggles between the two.
    private const double ExpandedSidebarWidth = 220;

    private readonly NotesListViewModel _viewModel;
    private View? _defaultContent;

    public MainLayoutView(NotesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Module-08 task 8-1: on Phone idiom the XAML sets the sidebar
        // column to 0 (matches collapsed.png). For non-Phone idioms we
        // honor the ViewModel flag so a user who collapsed the sidebar
        // on a tablet sees the same state on the next appearance.
        var startCollapsed = DeviceIdiomMatches(DeviceIdiom.Phone) || viewModel.IsSidebarCollapsed;
        viewModel.IsSidebarCollapsed = startCollapsed;
        UpdateSidebarColumn(startCollapsed);
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Swaps the page-specific content view (e.g. a notes list rendered
    /// for a given filter) in the main content area.
    /// </summary>
    public void SetContent(View content)
    {
        _defaultContent = content;
        UpdateMainContent();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NotesListViewModel.IsSidebarCollapsed))
            UpdateSidebarColumn(_viewModel.IsSidebarCollapsed);
        else if (e.PropertyName == nameof(NotesListViewModel.IsSearchActive))
            UpdateMainContent();
    }

    private void UpdateMainContent()
    {
        MainContent.Content = _viewModel.IsSearchActive
            ? new SearchResultsView { BindingContext = _viewModel.SearchViewModel }
            : _defaultContent;
    }

    private void UpdateSidebarColumn(bool isCollapsed)
    {
        // Collapse the sidebar column completely so the main content
        // area occupies the full available width.
        RootGrid.ColumnDefinitions[0].Width = isCollapsed ? new GridLength(0) : new GridLength(ExpandedSidebarWidth);
        Sidebar.IsVisible = !isCollapsed;
    }

    private static bool DeviceIdiomMatches(DeviceIdiom idiom) =>
        DeviceInfo.Current?.Idiom == idiom;
}
