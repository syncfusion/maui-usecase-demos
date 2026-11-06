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
    private const double ExpandedSidebarWidth = 220;

    private readonly NotesListViewModel _viewModel;
    private readonly bool _isPhone;
    private View? _defaultContent;

    public MainLayoutView(NotesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _isPhone = DeviceIdiomMatches(DeviceIdiom.Phone);
        BindingContext = viewModel;

        UpdateSidebarColumn(viewModel.IsSidebarCollapsed);
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
        if (_isPhone)
        {
            RootGrid.ColumnDefinitions[0].Width = isCollapsed
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            RootGrid.ColumnDefinitions[1].Width = isCollapsed
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(0);
            MainContent.IsVisible = isCollapsed;
        }
        else
        {
            RootGrid.ColumnDefinitions[0].Width = isCollapsed
                ? new GridLength(0)
                : new GridLength(ExpandedSidebarWidth);
            MainContent.IsVisible = true;
        }

        Sidebar.IsVisible = !isCollapsed;
    }

    private static bool DeviceIdiomMatches(DeviceIdiom idiom) =>
        DeviceInfo.Current?.Idiom == idiom;
}
