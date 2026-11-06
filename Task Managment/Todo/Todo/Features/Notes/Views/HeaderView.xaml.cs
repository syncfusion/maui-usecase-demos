using System.ComponentModel;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class HeaderView : ContentView
{
    private NotesListViewModel? _viewModel;

    public HeaderView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        base.OnBindingContextChanged();
        _viewModel = BindingContext as NotesListViewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateMobileHeader();
    }

    private static void OnProfilePointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.SetHovered(border);
    }

    private static void OnProfilePointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is Border border)
            PointerHoverSurface.Reset(border);
    }

    /// <summary>
    /// Activates inline search when the user focuses the shared header entry.
    /// </summary>
    private void OnSearchEntryFocused(object? sender, FocusEventArgs e)
    {
        if (BindingContext is NotesListViewModel vm && !vm.IsSearchActive)
            vm.OpenSearchCommand.Execute(null);
    }

    private void OnMobileSearchClicked(object? sender, EventArgs e)
    {
        if (BindingContext is NotesListViewModel vm)
            vm.IsSidebarCollapsed = true;

        HeaderTitle.IsVisible = false;
        MobileAppTitle.IsVisible = false;
        MobilePageTitle.IsVisible = false;
        MobileSearchButton.IsVisible = false;
        SearchPill.WidthRequest = -1;
        SearchPill.IsVisible = true;
        SearchEntry.Focus();
    }

    private void OnSearchEntryUnfocused(object? sender, FocusEventArgs e)
    {
        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone)
            return;

        SearchPill.IsVisible = false;
        MobileSearchButton.IsVisible = true;
        UpdateMobileHeader();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NotesListViewModel.IsSidebarCollapsed)
            or nameof(NotesListViewModel.HeaderTitle))
        {
            UpdateMobileHeader();
        }
    }

    private void UpdateMobileHeader()
    {
        var isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
        DesktopHeaderContent.IsVisible = !isPhone;
        DesktopNavigationButton.IsVisible = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        MobileNavigationButton.IsVisible = false;
        MobileSearchButton.IsVisible = isPhone;

        if (!isPhone)
            return;

        var showingContent = _viewModel?.IsSidebarCollapsed == true;
        MobileAppTitle.IsVisible = !showingContent;
        MobilePageTitle.IsVisible = showingContent;
        MobilePageTitle.Text = _viewModel?.HeaderTitle ?? string.Empty;
        MobileNavigationButton.IsVisible = showingContent;
        MobileSearchButton.IsVisible = !showingContent;
    }
}
