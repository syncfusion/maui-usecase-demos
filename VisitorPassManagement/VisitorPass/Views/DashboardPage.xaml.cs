using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Popup;
using VisitorPass.Controls;
using VisitorPass.Models;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// Dashboard page code-behind.
///
/// Events:
///   1. <c>SfButton.Clicked</c> on the header's "Register Visitor"
///      button opens the <see cref="SfPopup"/> declared in
///      DashboardPage.xaml. The popup's body hosts
///      <see cref="RegisterVisitorView"/> directly so the form
///      layout exists exactly once.
///   2. <c>SfDataGrid.CellTapped</c> on today's grid pushes
///      <see cref="VisitorDetailsPage"/> via
///      <c>Navigation.PushAsync</c>.
///   3. <c>SideNavigation.NavItemClicked</c> routes via plain
///      <c>Navigation.PushAsync</c> (no Shell).
///
/// Per the MVVM rule there is no business logic here — the file only
/// routes events to the DI container and the navigation stack.
/// </summary>
public partial class DashboardPage : ContentPage
{
    private readonly IServiceProvider _services;

    public DashboardPage(
        DashboardViewModel viewModel,
        IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _services = services;

        TodayGrid.CellTapped += OnTodayGridCellTapped;

        if (this.FindSideNavigation() is { } sidebar)
            sidebar.NavItemClicked += OnSidebarNavItemClicked;
    }

    /// <summary>Sidebar nav handler — pushes the chosen page via plain
    /// Navigation (no Shell).</summary>
    private async void OnSidebarNavItemClicked(object? sender, string route)
    {
        if (route == DashboardViewModel.Route)
            return; // already here

        if (route == DirectoryViewModel.Route)
        {
            // Pop back to root then push the directory page so we
            // don't stack duplicates.
            await Navigation.PopToRootAsync();
            await Navigation.PushAsync(_services.GetRequiredService<DirectoryPage>());
        }
    }

    /// <summary>Header "Register Visitor" button click — opens the
    /// SfPopup declared in the page XAML. The popup's
    /// <c>ContentTemplate</c> inflates a fresh
    /// <see cref="RegisterVisitorView"/> per open so each
    /// registration gets a transient view-model.</summary>
    private void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (this.FindRegisterPopup() is not SfPopup popup)
            return;

        var vm = _services.GetRequiredService<RegisterVisitorViewModel>();

        var view = new RegisterVisitorView
        {
            BindingContext = vm,
        };

        view.DismissRequested += (_, _) => popup.IsOpen = false;
        view.PassGenerated += async (_, visitor) =>
        {
            popup.IsOpen = false;
            await DisplayAlertAsync(
                "Pass Created",
                $"Visitor pass {visitor.PassId} was created successfully.",
                "Continue");
            await Navigation.PushAsync(
                new VisitorDetailsPage(
                    _services.GetRequiredService<VisitorDetailsViewModel>(),
                    visitor.PassId));
        };

        popup.ContentTemplate = new DataTemplate(() => view);
        popup.IsOpen = true;
    }

    /// <summary>Today's grid cell tap → pushes
    /// <see cref="VisitorDetailsPage"/> for the tapped visitor.</summary>
    private async void OnTodayGridCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        if (e.RowData is not Visitor visitor)
            return;

        await Navigation.PushAsync(
            new VisitorDetailsPage(
                _services.GetRequiredService<VisitorDetailsViewModel>(),
                visitor.PassId));
    }

    /// <summary>Phone bottom-nav "Directory" tab — pushes the
    /// <see cref="DirectoryPage"/> singleton from DI.</summary>
    private async void OnDirectoryTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(_services.GetRequiredService<DirectoryPage>());
    }
}
