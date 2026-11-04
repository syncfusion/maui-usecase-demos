using VisitorPass.Controls;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// Digital Pass / Visitor Details page (Screen 3).
///
/// The XAML owns the layout (identity card, pass card, journey
/// timeline, telemetry tiles, operational controls). The code-behind
/// wires the lifecycle hooks:
///
///   - <c>OnAppearing</c> calls <c>LoadAsync</c> so the view-model
///     resolves the visitor from <c>IVisitorStore</c> and rebuilds
///     the journey collection.
///   - <c>SideNavigation.NavItemClicked</c> pops back to the root.
///
/// The pass-id is supplied through the optional <c>passId</c>
/// constructor parameter; pages push this page with the visitor id
/// so the view-model's <c>SetPassId</c> runs before the first
/// <c>OnAppearing</c>.
///
/// Per the MVVM rule there is no business logic in this file — the
/// action buttons bind to RelayCommands on the view-model.
/// </summary>
public partial class VisitorDetailsPage : ContentPage
{
    private readonly VisitorDetailsViewModel _viewModel;

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
        // The root IS the Dashboard — pop back so the next tap on
        // the sidebar returns the user to the same starting surface.
        await Navigation.PopToRootAsync();
    }

    // ===== Operational Controls & Status Actions =====
    // These forward the visual tiles to the view-model's relay
    // commands, which mutate the visitor through IVisitorStore.
    // CanExecute guards the tile's visibility so an
    // already-resolved status never surfaces a stale action.

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
