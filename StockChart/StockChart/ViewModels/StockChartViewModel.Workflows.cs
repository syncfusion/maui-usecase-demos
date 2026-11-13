using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockChart.Models;

namespace StockChart.ViewModels;

/// <summary>
/// Workflow orchestration for the sample's popup flows. The ViewModel decides
/// which popup is active and runs the prepare/confirm business logic; the view
/// only observes <see cref="RequestedPopup"/> and handles popup presentation.
/// </summary>
public partial class StockChartViewModel
{
    public enum WorkflowPopup
    {
        None,
        Settings,
        AddStock,
        NewWatchlist,
        EditWatchlist,
        DeleteWatchlist,
        RemoveStock,
        Favorite,
    }

    [ObservableProperty]
    public partial WorkflowPopup RequestedPopup { get; set; }

    private void RequestPopup(WorkflowPopup kind) => RequestedPopup = kind;

    [RelayCommand]
    private void OpenSettingsPopup()
    {
        LoadSettingsDraft();
        RequestPopup(WorkflowPopup.Settings);
    }

    [RelayCommand]
    private void OpenAddStockPopup()
    {
        PrepareAddStock();
        RequestPopup(WorkflowPopup.AddStock);
    }

    [RelayCommand]
    private void OpenNewWatchlistPopup()
    {
        PrepareNewWatchlist();
        RequestPopup(WorkflowPopup.NewWatchlist);
    }

    [RelayCommand]
    private void OpenEditWatchlistPopup()
    {
        if (SelectedWatchlist is null)
        {
            return;
        }

        PrepareEditWatchlist();
        RequestPopup(WorkflowPopup.EditWatchlist);
    }

    [RelayCommand]
    private void OpenDeleteWatchlistPopup()
    {
        if (!CanDeleteSelectedWatchlist)
        {
            return;
        }

        RequestPopup(WorkflowPopup.DeleteWatchlist);
    }

    /// <summary>Favorite/remove decision for the chart header favorite button.</summary>
    [RelayCommand]
    private void OpenFavoriteOrRemovePopup()
    {
        if (!CanOpenFavoritePopup)
        {
            return;
        }

        if (IsLastSelectedStockInSelectedWatchlist)
        {
            PrepareRemoveStock();
            RequestPopup(WorkflowPopup.RemoveStock);
        }
        else
        {
            PrepareFavoriteStock();
            RequestPopup(WorkflowPopup.Favorite);
        }
    }

    /// <summary>Favorite/remove decision for a stock list item's overflow action.</summary>
    [RelayCommand]
    private void OpenStockActionPopup(StockModel? stock)
    {
        if (stock is null)
        {
            return;
        }

        if (IsWatchlistSelected)
        {
            PrepareRemoveStock(stock, SelectedWatchlist);
            RequestPopup(WorkflowPopup.RemoveStock);
        }
        else
        {
            PrepareFavoriteStock(stock);
            RequestPopup(WorkflowPopup.Favorite);
        }
    }

    [RelayCommand]
    private void ClosePopup() => RequestPopup(WorkflowPopup.None);

    [RelayCommand]
    private void ConfirmDeleteWatchlist()
    {
        DeleteSelectedWatchlist();
        RequestPopup(WorkflowPopup.None);
    }

    [RelayCommand]
    private void ConfirmFavoriteStock()
    {
        ApplyFavoriteStock();
        RequestPopup(WorkflowPopup.None);
    }

    [RelayCommand]
    private void ConfirmRemoveStock()
    {
        ApplyRemoveStock();
        RequestPopup(WorkflowPopup.None);
    }

    [RelayCommand]
    private void ConfirmAddStock()
    {
        if (PendingStock is null)
        {
            return;
        }

        AddPendingStockToWatchlists();
        RequestPopup(WorkflowPopup.None);
    }

    [RelayCommand]
    private void ConfirmNewWatchlist()
    {
        if (string.IsNullOrWhiteSpace(NewWatchlistName))
        {
            return;
        }

        CreateWatchlist();
        PrepareAddStock();
        RequestPopup(WorkflowPopup.None);
    }

    [RelayCommand]
    private void ConfirmEditWatchlist()
    {
        if (string.IsNullOrWhiteSpace(EditWatchlistName))
        {
            return;
        }

        SaveWatchlistEdit();
        RequestPopup(WorkflowPopup.None);
    }

    /// <summary>Invoked by the view after chart settings have been applied to the chart surface.</summary>
    public void CompleteSettingsWorkflow() => RequestPopup(WorkflowPopup.None);
}
