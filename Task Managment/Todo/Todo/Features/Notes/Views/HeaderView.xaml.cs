using System.ComponentModel;
using Syncfusion.Maui.Buttons;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public partial class HeaderView : ContentView
{
    private NotesListViewModel? _viewModel;
    private ListOptionsFlyout? _mobileListOptionsFlyout;

    public event EventHandler? DesktopNavigationRequested;

    public HeaderView()
    {
        InitializeComponent();

        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
        {
            _mobileListOptionsFlyout = new ListOptionsFlyout
            {
                WidthRequest = 0,
                HeightRequest = 0,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                ZIndex = 1001
            };
            Grid.SetColumnSpan(_mobileListOptionsFlyout, HeaderGrid.ColumnDefinitions.Count);
            HeaderGrid.Children.Add(_mobileListOptionsFlyout);
        }
    }

    protected override void OnBindingContextChanged()
    {
        if (_viewModel is not null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        base.OnBindingContextChanged();
        _viewModel = BindingContext as NotesListViewModel;

        if (_viewModel is not null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateHeaderSurface();
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
        SearchPill.WidthRequest = -1;
        SearchPill.IsVisible = true;
        if (BindingContext is NotesListViewModel vm)
            vm.IsSidebarCollapsed = true;

        UpdateMobileHeader();
        SearchEntry.Focus();
    }

    private void OnDesktopNavigationClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            DesktopNavigationRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnMobileNavigationClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone
            || BindingContext is not NotesListViewModel vm)
            return;

        if (vm.IsSearchActive)
            vm.CloseSearchCommand.Execute(null);

        SearchEntry.Unfocus();
        SearchPill.IsVisible = false;
        UpdateMobileHeader();
        vm.ToggleSidebarCommand.Execute(null);
    }

    private void OnMobileMoreOptionsClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone
            || sender is not View anchor
            || BindingContext is not NotesListViewModel vm
            || _mobileListOptionsFlyout is null)
            return;

        if (vm.SelectedCustomList is { } selectedList)
            _mobileListOptionsFlyout.ShowFor(anchor, selectedList, vm);
        else
            _mobileListOptionsFlyout.ShowFor(anchor, vm.SelectedCategory, vm);
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
        if (e.PropertyName is nameof(NotesListViewModel.HasBackgroundImage)
            or nameof(NotesListViewModel.BackgroundImage)
            or nameof(NotesListViewModel.ListBackgroundColor))
        {
            UpdateHeaderSurface();
            UpdateMobileHeader();
            return;
        }

        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone)
            return;

        if (e.PropertyName == nameof(NotesListViewModel.IsSearchActive))
        {
            if (_viewModel?.IsSearchActive == false)
            {
                SearchEntry.Unfocus();
                SearchPill.IsVisible = false;
            }

            UpdateMobileHeader();
        }
        else if (e.PropertyName is nameof(NotesListViewModel.IsSidebarCollapsed)
                 or nameof(NotesListViewModel.HeaderTitle)
                 or nameof(NotesListViewModel.SelectedCategory)
                 or nameof(NotesListViewModel.SelectedListId))
            UpdateMobileHeader();
    }

    private void UpdateHeaderSurface()
    {
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            return;

        if (_viewModel?.HasBackgroundImage == true
            || DeviceInfo.Current.Idiom == DeviceIdiom.Desktop)
        {
            HeaderGrid.BackgroundColor = Colors.Transparent;
        }
        else
        {
            HeaderGrid.SetAppThemeColor(
                Grid.BackgroundColorProperty,
                Color.FromArgb("#FFFBFE"),
                Color.FromArgb("#1C1B1F"));
        }
    }

    private void UpdateMobileHeader()
    {
        var isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
        DesktopHeaderContent.IsVisible = !isPhone;
        DesktopNavigationButton.IsVisible = DeviceInfo.Current.Platform == DevicePlatform.WinUI;
        MobileNavigationButton.IsVisible = false;
        MobileSearchButton.IsVisible = isPhone;
        MobileMoreOptionsButton.IsVisible = false;

        if (!isPhone)
            return;

        var showingContent = _viewModel?.IsSidebarCollapsed == true;
        var showingSearchResults = _viewModel?.IsSearchActive == true;
        var showingSearchInput = SearchPill.IsVisible;
        var showingListHeader = showingContent && !showingSearchResults && !showingSearchInput;
        MobileAppTitle.IsVisible = !showingContent;
        MobileNavigationButton.IsVisible = showingContent;
        MobileSearchButton.IsVisible = !showingContent && !showingSearchInput;
        MobileMoreOptionsButton.IsVisible = showingListHeader;
        ProfileImageContainer.IsVisible = !showingContent && !showingSearchInput;

        if (showingListHeader && _viewModel is not null)
        {
            HeaderGrid.BackgroundColor = _viewModel.HasBackgroundImage
                ? Colors.Transparent
                : _viewModel.ListBackgroundColor;
            MobileNavigationButton.TextColor = _viewModel.MobileHeaderForegroundColor;
            MobileMoreOptionsButton.TextColor = _viewModel.MobileHeaderForegroundColor;
        }
        else
        {
            HeaderGrid.SetAppThemeColor(Grid.BackgroundColorProperty, Colors.White, Color.FromArgb("#1C1B1F"));
            MobileAppTitle.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#29272F"), Color.FromArgb("#E6E1E5"));
            MobileSearchButton.SetAppThemeColor(SfButton.TextColorProperty, Color.FromArgb("#29272F"), Color.FromArgb("#E6E1E5"));
            MobileNavigationButton.SetAppThemeColor(SfButton.TextColorProperty, Color.FromArgb("#29272F"), Color.FromArgb("#E6E1E5"));
            MobileMoreOptionsButton.SetAppThemeColor(SfButton.TextColorProperty, Color.FromArgb("#29272F"), Color.FromArgb("#E6E1E5"));
        }

        if (_viewModel?.HasBackgroundImage == true && showingContent)
        {
            HeaderGrid.BackgroundColor = Colors.Transparent;
            MobileAppTitle.TextColor = Colors.White;
            MobileSearchButton.TextColor = Colors.White;
            MobileNavigationButton.TextColor = Colors.White;
            MobileMoreOptionsButton.TextColor = Colors.White;
        }
    }
}
