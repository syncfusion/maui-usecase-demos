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

    private void OnMobileNavigationClicked(object? sender, EventArgs e)
    {
        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone
            || BindingContext is not NotesListViewModel vm)
            return;

        if (vm.IsSearchActive)
            vm.CloseSearchCommand.Execute(null);

        vm.ToggleSidebarCommand.Execute(null);
    }

    private void OnMobileMoreClicked(object? sender, EventArgs e)
    {
        if (sender is not View anchor || BindingContext is not NotesListViewModel vm)
            return;

        if (!string.IsNullOrEmpty(vm.SelectedListId))
        {
            var list = vm.CustomLists.FirstOrDefault(item => item.Id == vm.SelectedListId);
            if (list is not null)
            {
                ListOptionsFlyout.ShowFor(anchor, list, vm);
                return;
            }
        }

        ListOptionsFlyout.ShowFor(anchor, vm.SelectedCategory, vm);
    }

    private void OnSearchEntryUnfocused(object? sender, FocusEventArgs e)
    {
        if (DeviceInfo.Current.Idiom != DeviceIdiom.Phone)
            return;

        SearchPill.IsVisible = false;
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
        MobileNavigationButton.IsVisible = isPhone;
        MobileMoreButton.IsVisible = isPhone;
        SearchPill.IsVisible = false;
    }
}
