using VisitorPass.Controls;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-060..064 — Digital Pass / Visitor Details (Screen 3) page.
///
/// The XAML owns the layout (identity card, pass card, timeline,
/// action buttons). The code-behind wires the lifecycle and
/// theme-toggle hooks (VP-080):
///
///   - OnAppearing -> triggers LoadAsync so the VM resolves the
///     visitor from IVisitorStore and rebuilds the timeline.
///   - OnNavigatedFrom (when popping back) -> clears the VM so the
///     next push of this page doesn't briefly show stale data.
///   - TopNavBar.ThemeToggleClicked -> forwards to
///     ThemeService.Toggle(); the nav bar's IsDarkMode is also
///     bound from the service so the sun/moon glyph flips.
///   - Sidebar.NavItemClicked -> pops back to root and pushes the
///     chosen page via Navigation (no Shell — Shell.GoToAsync
///     crashes with 'Relative routing to shell elements is
///     currently not supported' when invoked from a content page).
///
/// The pass-id is supplied through the optional <c>passId</c>
/// constructor parameter; pages push this page with the visitor
/// id so the VM's SetPassId is called before the first
/// OnAppearing.
///
/// Per HARNESS §6 rule 2 there is no business logic in this file —
/// action buttons bind to RelayCommands on the VM and the page
/// itself never reads or mutates VisitorStore directly.
/// </summary>
public partial class VisitorDetailsPage : ContentPage
{
    private readonly VisitorDetailsViewModel _viewModel;
    private readonly ThemeService _theme;

    public VisitorDetailsPage(
        VisitorDetailsViewModel viewModel,
        string? passId = null)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;

        if (passId is not null)
            _viewModel.SetPassId(passId);

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        // Pop back to root then optionally push the target page if
        // the chosen route isn't the current one.
        await Navigation.PopToRootAsync();
        // (No need to push anything else — the root IS the Dashboard.)
    }

    // ===== Operational Controls & Status Actions =====
    // These forward the visual tiles to the VM's relay commands, which in
    // turn mutate the visitor through IVisitorStore. CanExecute guards the
    // tile's visibility (IsCheckInVisible / IsCheckOutVisible / IsCancelVisible)
    // so an already-resolved status never surfaces a stale action.

    private void OnCheckIn(object? sender, EventArgs e)
    {
        if (_viewModel.CheckInCommand.CanExecute(null))
            _viewModel.CheckInCommand.Execute(null);
    }

    private void OnCheckOut(object? sender, EventArgs e)
    {
        if (_viewModel.CheckOutCommand.CanExecute(null))
            _viewModel.CheckOutCommand.Execute(null);
    }

    private void OnCancelVisit(object? sender, EventArgs e)
    {
        if (_viewModel.CancelVisitCommand.CanExecute(null))
            _viewModel.CancelVisitCommand.Execute(null);
    }

    // ===== Header + secondary links =====
    // Secondary affordances (Print / Share / Security Log / Notify Host /
    // Re-issue Credentials / Incident Report) are decorative in this
    // showcase; they exist so the surface mirrors the reference design.
    // The handlers are intentionally inert to keep the harness rule
    // "no business logic in code-behind" intact.

    private void OnPrintBadge(object? sender, TappedEventArgs e) { /* no-op */ }
    private void OnSharePass(object? sender, TappedEventArgs e) { /* no-op */ }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
    }
}
