using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Popup;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.Services;
using VisitorPass.ViewModels;
using VisitorPass.Views;

namespace VisitorPass.Views;

/// <summary>
/// VP-UI popup+nav — Dashboard page code-behind.
///
/// Three events wired:
///   1. TopNavBar.RegisterClicked -> opens the SfPopup defined in
///      DashboardPage.xaml with RegisterVisitorView as its body.
///   2. TopNavBar.ThemeToggleClicked -> ThemeService.Toggle(); nav
///      bar's IsDarkMode rebinds from the service.
///   3. SfDataGrid.CellTapped on today's grid -> pushes
///      VisitorDetailsPage via Navigation.PushAsync.
///
/// Sidebar navigation: each tile's NavItemClicked routes via
/// Navigation.PushAsync (not Shell.GoToAsync). Shell was
/// previously crashing with "Relative routing to shell elements
/// is currently not supported"; switching to plain Navigation
/// avoids that crash AND keeps Shell out of the picture (the
/// root is now a NavigationPage, see App.xaml.cs).
///
/// Per HARNESS §6 rule 2 there is no business logic here — this
/// file routes events to the DI container and the navigation
/// stack.
/// </summary>
public partial class DashboardPage : ContentPage
{
    /// <summary>Query-parameter key used when navigating to the details page.</summary>
    public const string VisitorIdQueryKey = "passId";

    private readonly IServiceProvider _services;
    private RegisterVisitorView? _activeRegisterView;
    private EventHandler? _registerDismissHandler;
    private EventHandler<Visitor>? _passGeneratedHandler;

    public DashboardPage(
        DashboardViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        TodayGrid.CellTapped += OnTodayGridCellTapped;
        _services = services;
        if (this.FindTopNavBar() is { } navBar)
        {
            this.RegisterClicked += OnRegisterClicked;
        }

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    public event EventHandler? RegisterClicked;

    private void SfButton_Clicked(object sender, EventArgs e)
    {
        RegisterClicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Sidebar nav handler — pushes the chosen page via plain
    /// Navigation (no Shell).</summary>
    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        if (route == DashboardViewModel.Route)
            return; // already here
        var targetPage = ResolvePageForRoute(route);
        if (targetPage is null) return;

        // Pop back to the root first so we don't stack duplicates.
        await Navigation.PopToRootAsync();
        if (Navigation.NavigationStack.Count == 1
            || Navigation.NavigationStack[^1].GetType() != targetPage.GetType())
        {
            await Navigation.PushAsync(targetPage);
        }
    }

    /// <summary>Resolves a page from DI for a route id.</summary>
    private Page? ResolvePageForRoute(string route)
    {
        return route switch
        {
            DirectoryViewModel.Route => _services.GetRequiredService<DirectoryPage>(),
            _ => null,
        };
    }

    /// <summary>VP-UI register trigger — opens the SfPopup. The popup's
    /// body hosts RegisterVisitorView directly, so the form
    /// layout exists exactly once. The VM is bound to the inner
    /// view inside <see cref="OnRegisterPopupOpened"/> so each
    /// opening gets a fresh DI-resolved VM (transient registration).</summary>
    private void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (this.FindRegisterPopup() is not SfPopup popup)
            return;

        var vm = _services.GetRequiredService<RegisterVisitorViewModel>();

        var view = new RegisterVisitorView
        {
            BindingContext = vm
        };

        view.DismissRequested += (_, _) =>
        {
            popup.IsOpen = false;
        };

        view.PassGenerated += async (_, visitor) =>
        {
            popup.IsOpen = false;

            await DisplayAlertAsync(
                        "Pass Generated",
                        "Pass Generated Successfully",
                        "OK");
        };

        popup.ContentTemplate = new DataTemplate(() => view);
        popup.IsOpen = true;
    }

    /// <summary>Called by XAML each time the popup opens. Resolves a
    /// fresh <see cref="RegisterVisitorViewModel"/> from DI, walks
    /// the popup's visual tree to find the inflated
    /// <see cref="RegisterVisitorView"/>, and wires the VM +
    /// lifecycle events.</summary>
    private async void OnRegisterPopupOpened(object? sender, EventArgs e)
    {
        if (sender is not SfPopup popup) return;

        // Resolve a fresh VM (transient registration) per open.
        var vm = _services.GetRequiredService<RegisterVisitorViewModel>();
        popup.BindingContext = vm;

        // Find the inflated RegisterVisitorView inside the
        // PopupView's child tree and bind the same VM to it.
        var view = FindRegisterVisitorView(popup);
        if (view is null) return;

        if (_activeRegisterView is not null)
        {
            if (_registerDismissHandler is not null)
                _activeRegisterView.DismissRequested -= _registerDismissHandler;
            if (_passGeneratedHandler is not null)
                _activeRegisterView.PassGenerated -= _passGeneratedHandler;
        }

        _activeRegisterView = view;
        view.BindingContext = vm;
        _registerDismissHandler = (_, _) => popup.IsOpen = false;
        _passGeneratedHandler = async (_, visitor) =>
        {
            popup.IsOpen = false;
            await DisplayAlertAsync("Pass Created", $"Visitor pass {visitor.PassId} was created successfully.", "Continue");
            await Navigation.PushAsync(
                new VisitorDetailsPage(
                    _services.GetRequiredService<VisitorDetailsViewModel>(),
                    visitor.PassId));
        };
        view.DismissRequested += _registerDismissHandler;
        view.PassGenerated += _passGeneratedHandler;
    }

    private void OnRegisterPopupClosed(object? sender, EventArgs e)
    {
        if (sender is SfPopup popup && FindRegisterVisitorView(popup) is { } view)
            view.BindingContext = null;
    }

    /// <summary>Walks <paramref name="popup"/>'s child tree for the
    /// inflated <see cref="RegisterVisitorView"/>.</summary>
    private static RegisterVisitorView? FindRegisterVisitorView(SfPopup popup)
    {
        foreach (var descendant in EnumerateDescendants(popup))
        {
            if (descendant is RegisterVisitorView view)
                return view;
        }
        return null;
    }

    private static IEnumerable<IView> EnumerateDescendants(IView root)
    {
        if (root is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                yield return child;
                foreach (var deeper in EnumerateDescendants(child))
                    yield return deeper;
            }
        }
        if (root is IContentView cv && cv.Content is IView nested)
        {
            yield return nested;
            foreach (var deeper in EnumerateDescendants(nested))
                yield return deeper;
        }
    }

    private async void OnDashboardTapped(object? sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnDirectoryTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(
            Handler.MauiContext!.Services.GetRequiredService<DirectoryPage>());
    }

    /// <summary>VP-043 — today's grid cell tap -> pushes
    /// VisitorDetailsPage via Navigation.PushAsync.</summary>
    private async void OnTodayGridCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        if (e.RowData is not Visitor visitor)
            return;

        await Navigation.PushAsync(
            new VisitorDetailsPage(
                _services.GetRequiredService<VisitorDetailsViewModel>(),
                visitor.PassId));
    }
}
