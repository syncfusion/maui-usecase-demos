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
    private readonly bool _isDesktop;
    private View? _defaultContent;

    public MainLayoutView(NotesListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _isPhone = DeviceIdiomMatches(DeviceIdiom.Phone);
        _isDesktop = DeviceIdiomMatches(DeviceIdiom.Desktop);
        BindingContext = viewModel;
        Header.DesktopNavigationRequested += OnDesktopNavigationRequested;

        UpdateSidebarColumn(viewModel.IsSidebarCollapsed);
        UpdatePageBackgroundImage();
        if (!_isPhone && !_isDesktop)
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void Activate()
    {
        if (_isDesktop)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateSidebarColumn(_viewModel.IsSidebarCollapsed);
            UpdateMainContent();
            UpdatePageBackgroundImage();
            return;
        }

        if (!_isPhone)
            return;

        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateSidebarColumn(_viewModel.IsSidebarCollapsed);
        UpdateMainContent();
        UpdatePageBackgroundImage();
    }

    public void Deactivate()
    {
        if (_isPhone || _isDesktop)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
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
        {
            UpdateSidebarColumn(_viewModel.IsSidebarCollapsed);
            UpdatePageBackgroundImage();
        }
        else if (e.PropertyName == nameof(NotesListViewModel.IsSearchActive))
        {
            UpdateHeaderDivider();
            UpdateMainContent();
            UpdatePageBackgroundImage();
        }
        else if (e.PropertyName is nameof(NotesListViewModel.HasBackgroundImage)
                 or nameof(NotesListViewModel.BackgroundImage)
                 or nameof(NotesListViewModel.ListBackgroundColor))
        {
            UpdateHeaderDivider();
            UpdatePageBackgroundImage();
            UpdateSearchBackground();
        }
    }

    private void OnDesktopNavigationRequested(object? sender, EventArgs e)
    {
        if (_isPhone)
            return;

        _viewModel.ToggleSidebarCommand.Execute(null);
        UpdateSidebarColumn(_viewModel.IsSidebarCollapsed);
    }

    private void UpdateMainContent()
    {
        if (!_viewModel.IsSearchActive)
        {
            MainContent.Content = _defaultContent;
            return;
        }

        MainContent.Content = new SearchResultsView
        {
            BindingContext = _viewModel.SearchViewModel,
            ListBackgroundColor = _viewModel.ListBackgroundColor,
            ListBackgroundImage = _viewModel.BackgroundImage
        };
    }

    private void UpdateSearchBackground()
    {
        if (MainContent.Content is SearchResultsView searchResults)
        {
            searchResults.ListBackgroundColor = _viewModel.ListBackgroundColor;
            searchResults.ListBackgroundImage = _viewModel.BackgroundImage;
        }
    }

    private void UpdatePageBackgroundImage()
    {
        PageBackgroundImage.IsVisible = _viewModel.HasBackgroundImage && MainContent.IsVisible;
    }

    private void UpdateSidebarColumn(bool isCollapsed)
    {
        UpdateHeaderDivider();

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

    private void UpdateHeaderDivider()
    {
        var hasSharedBackground = _viewModel.HasBackgroundImage && MainContent.IsVisible;
        HeaderDivider.IsVisible = !hasSharedBackground && (_isDesktop
            ? false
            : !_isPhone
              || !_viewModel.IsSidebarCollapsed
              || _viewModel.IsSearchActive);
    }

    private static bool DeviceIdiomMatches(DeviceIdiom idiom) =>
        DeviceInfo.Current?.Idiom == idiom;
}
