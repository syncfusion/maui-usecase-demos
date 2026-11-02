using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.DataGrid;
using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// Directory page code-behind.
///
/// Events:
///   - <c>SfDataGrid.CellTapped</c> / <c>SfListView.ItemTapped</c> on
///     the visitor grid pushes
///     <see cref="VisitorDetailsPage"/> via
///     <c>Navigation.PushAsync</c>.
///   - <c>SfChipGroup.SelectionChanged</c> on the status chip row
///     forwards the chosen <see cref="DirectoryViewModel.StatusFilter"/>
///     to <c>SelectStatusCommand</c>.
///   - <c>SideNavigation.NavItemClicked</c> pops back to the
///     dashboard root.
///
/// Per the MVVM rule there is no business logic here.
/// </summary>
public partial class DirectoryPage : ContentPage
{
    private readonly IServiceProvider _services;

    public DirectoryPage(
        DirectoryViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        if (route == DirectoryViewModel.Route)
            return; // already here
        if (route == DashboardViewModel.Route)
            await Navigation.PopToRootAsync();
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

    /// <summary>"Open Full Pass Details" button on the Active
    /// Dossier preview card. Pushes the Digital Pass page with the
    /// currently selected visitor's pass id.</summary>
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
