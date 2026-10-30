using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.DataGrid;
using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.Services;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-051 / VP-052 — Directory page code-behind.
///
/// Events:
///   - SfDataGrid.CellTapped on the visitor grid -> pushes
///     VisitorDetailsPage via Navigation.PushAsync (no Shell).
///   - TopNavBar.ThemeToggleClicked -> ThemeService.Toggle();
///     nav bar's IsDarkMode rebinds from the service.
///   - Sidebar.NavItemClicked -> pops back to root then pushes
///     Dashboard (no Shell).
///
/// Per HARNESS §6 rule 2 there is no business logic here.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    private readonly IServiceProvider _services;
    private readonly ThemeService _theme;

    public DirectoryPage(
        DirectoryViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;

        if (this.FindTopNavBar() is { } navBar)
        {
            this.RegisterClicked += OnRegisterClicked;
        }

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        if (route == DirectoryViewModel.Route)
            return; // already here
        // Pop back to root then push the target page so we don't
        // stack duplicates (uses Navigation, not Shell).
        if (route == DashboardViewModel.Route)
            await Navigation.PopToRootAsync();
    }

    public event EventHandler? RegisterClicked;

    private void SfButton_Clicked(object sender, EventArgs e)
    {
        RegisterClicked?.Invoke(this, EventArgs.Empty);
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
        => await OpenRegistrationAsync();

    private async void OnRegisterVisitor(object? sender, TappedEventArgs e)
        => await OpenRegistrationAsync();

    private async Task OpenRegistrationAsync()
    {
        var page = new RegisterVisitorPage(_services.GetRequiredService<RegisterVisitorViewModel>());
        page.DismissRequested += async (_, _) => await Navigation.PopAsync();
        page.PassGenerated += async (_, visitor) =>
        {
            await Navigation.PopAsync();
            await OpenVisitorDetailsAsync(visitor);
        };
        await Navigation.PushAsync(page);
    }

    private void OnStatusChipSelectionChanged(object? sender, ChipSelectionChangedEventArgs e)
    {
        if (BindingContext is not DirectoryViewModel viewModel)
            return;

        if (e.AddedItem is DirectoryViewModel.StatusFilter filter)
            viewModel.SelectStatusCommand.Execute(filter);
    }

    private async void OnCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        if (e.RowData is not Visitor visitor || BindingContext is not DirectoryViewModel viewModel)
            return;

        viewModel.SelectedVisitor = visitor;
        await OpenVisitorDetailsAsync(visitor);
    }

    /// <summary>
    /// "Open Full Pass Details" button on the Active Dossier
    /// preview card. Pushes the Digital Pass page with the
    /// currently selected visitor's pass id.
    /// </summary>
    private async void OnOpenFullPass(object? sender, EventArgs e)
    {
        if (BindingContext is not DirectoryViewModel vm
            || vm.SelectedVisitor is not { } visitor)
            return;

        await OpenVisitorDetailsAsync(visitor);
    }

    private Task OpenVisitorDetailsAsync(Visitor visitor)
        => Navigation.PushAsync(
            new VisitorDetailsPage(
                _services.GetRequiredService<VisitorDetailsViewModel>(),
                visitor.PassId));

    private async void SfListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is not Visitor visitor || BindingContext is not DirectoryViewModel viewModel)
            return;

        viewModel.SelectedVisitor = visitor;
        await OpenVisitorDetailsAsync(visitor);
    }
}