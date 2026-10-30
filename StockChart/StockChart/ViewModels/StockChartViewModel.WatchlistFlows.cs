using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using StockChart.Models;

namespace StockChart.ViewModels;

// Watchlist and stock-membership flows: create/edit/delete watchlists, add/remove
// and favorite stocks, and the add-to-watchlist context menu.
public partial class StockChartViewModel
{
    [RelayCommand]
    private void PrepareAddStock()
    {
        AddStockSearchText = string.Empty;
        PendingStock = null;
        AddStockCandidates.Clear();
        foreach (var stock in Stocks)
        {
            AddStockCandidates.Add(stock);
        }

        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(watchlist, ReferenceEquals(watchlist, SelectedWatchlist)));
        }
    }

    [RelayCommand]
    private void AddPendingStockToWatchlists()
    {
        if (PendingStock is null)
        {
            return;
        }

        foreach (var selection in AddStockWatchlists.Where(item => item.IsSelected))
        {
            if (!selection.Watchlist.Symbols.Contains(PendingStock.Symbol))
            {
                selection.Watchlist.Symbols.Add(PendingStock.Symbol);
            }
        }

        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void PrepareNewWatchlist()
    {
        NewWatchlistName = $"Watchlist {Watchlists.Count + 1}";
        NewWatchlistSearchText = string.Empty;
        NewWatchlistStock = null;
    }

    [RelayCommand]
    private void CreateWatchlist()
    {
        var name = NewWatchlistName.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        var watchlist = new WatchlistModel(name);
        SubscribeToWatchlist(watchlist);
        if (NewWatchlistStock is not null)
        {
            watchlist.Symbols.Add(NewWatchlistStock.Symbol);
        }

        Watchlists.Add(watchlist);
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        SelectedWatchlist = watchlist;
        OnSearchTextChanged(SearchText);
    }

    public void DeleteSelectedWatchlist()
    {
        if (!CanDeleteSelectedWatchlist || SelectedWatchlist is null)
        {
            return;
        }

        var selectedIndex = Watchlists.IndexOf(SelectedWatchlist);
        Watchlists.Remove(SelectedWatchlist);
        SelectedWatchlist = Watchlists[Math.Min(selectedIndex, Watchlists.Count - 1)];
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void PrepareEditWatchlist()
    {
        EditWatchlistName = SelectedWatchlist?.Name ?? string.Empty;
        EditWatchlistSearchText = string.Empty;
        EditWatchlistCandidate = null;
        EditWatchlistStocks.Clear();
        SelectedEditWatchlistStocks.Clear();

        foreach (var stock in Stocks)
        {
            var selection = new StockSelectionModel(
                stock,
                SelectedWatchlist?.Symbols.Contains(stock.Symbol) == true);
            EditWatchlistStocks.Add(selection);
            if (selection.IsSelected)
            {
                SelectedEditWatchlistStocks.Add(selection);
            }
        }

        OnEditWatchlistSearchTextChanged(EditWatchlistSearchText);
    }

    [RelayCommand]
    private void SaveWatchlistEdit()
    {
        if (SelectedWatchlist is null)
        {
            return;
        }

        var name = EditWatchlistName.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            SelectedWatchlist.Name = name;
        }

        SelectedWatchlist.Symbols.Clear();
        foreach (var stock in EditWatchlistStocks.Where(stock => stock.IsSelected))
        {
            SelectedWatchlist.Symbols.Add(stock.Symbol);
        }

        OnPropertyChanged(nameof(SelectedWatchlist));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));

        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void RemoveEditWatchlistStock(StockSelectionModel? stock)
    {
        if (stock is not null)
        {
            stock.IsSelected = false;
        }
    }

    public void SyncEditWatchlistChipSelection()
    {
        foreach (var stock in EditWatchlistStocks)
        {
            stock.IsSelected = SelectedEditWatchlistStocks.Contains(stock);
        }
    }

    private void OnSelectedEditWatchlistStocksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            foreach (StockSelectionModel stock in e.OldItems)
            {
                stock.IsSelected = false;
            }
        }
    }

    [RelayCommand]
    private void AddSelectedToWatchlist()
    {
        var watchlist = SelectedWatchlist ?? Watchlists.FirstOrDefault();
        if (LastSelectedStock is null || watchlist is null)
        {
            return;
        }

        if (!watchlist.Symbols.Contains(LastSelectedStock.Symbol))
        {
            watchlist.Symbols.Add(LastSelectedStock.Symbol);
        }

        if (IsWatchlistSelected)
        {
            OnSearchTextChanged(SearchText);
        }
    }

    public void PrepareFavoriteStock()
    {
        PrepareFavoriteStock(LastSelectedStock);
    }

    public void PrepareFavoriteStock(StockModel? stock)
    {
        if (stock is null)
        {
            return;
        }

        PendingStock = stock;
        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(
                watchlist,
                watchlist.Symbols.Contains(stock.Symbol)));
        }
    }

    public void PrepareRemoveStock()
    {
        PrepareRemoveStock(LastSelectedStock, IsWatchlistSelected ? SelectedWatchlist : null);
    }

    public void PrepareRemoveStock(StockModel? stock)
    {
        PrepareRemoveStock(stock, IsWatchlistSelected ? SelectedWatchlist : null);
    }

    public void PrepareRemoveStock(StockModel? stock, WatchlistModel? sourceWatchlist)
    {
        if (stock is null)
        {
            return;
        }

        PendingStock = stock;
        _pendingRemovalWatchlist = sourceWatchlist;
        AddStockWatchlists.Clear();
        foreach (var watchlist in Watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(
                watchlist,
                watchlist.Symbols.Contains(stock.Symbol)));
        }
    }

    public void ApplyRemoveStock()
    {
        if (PendingStock is null)
        {
            return;
        }

        var selectedWatchlists = AddStockWatchlists
            .Where(item => item.IsSelected)
            .Select(item => item.Watchlist)
            .ToList();

        if (_pendingRemovalWatchlist is not null &&
            _pendingRemovalWatchlist.Symbols.Contains(PendingStock.Symbol) &&
            !selectedWatchlists.Contains(_pendingRemovalWatchlist))
        {
            selectedWatchlists.Add(_pendingRemovalWatchlist);
        }

        foreach (var watchlist in selectedWatchlists)
        {
            watchlist.Symbols.Remove(PendingStock.Symbol);
        }

        _pendingRemovalWatchlist = null;
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    public void ApplyFavoriteStock()
    {
        if (PendingStock is null)
        {
            return;
        }

        foreach (var selection in AddStockWatchlists)
        {
            var hasStock = selection.Watchlist.Symbols.Contains(PendingStock.Symbol);
            if (selection.IsSelected && !hasStock)
            {
                selection.Watchlist.Symbols.Add(PendingStock.Symbol);
            }
            else if (!selection.IsSelected && hasStock)
            {
                selection.Watchlist.Symbols.Remove(PendingStock.Symbol);
            }
        }

        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnSearchTextChanged(SearchText);
    }

    [RelayCommand]
    private void ToggleAddToWatchlistMenu(StockModel? stock)
    {
        MenuStock = ReferenceEquals(MenuStock, stock) ? null : stock;
    }

    [RelayCommand]
    private void AddStockToWatchlist(StockModel? stock)
    {
        var watchlist = SelectedWatchlist ?? Watchlists.FirstOrDefault();
        if (stock is null || watchlist is null)
        {
            return;
        }

        if (!watchlist.Symbols.Contains(stock.Symbol))
        {
            watchlist.Symbols.Add(stock.Symbol);
        }

        MenuStock = null;

        if (IsWatchlistSelected)
        {
            OnSearchTextChanged(SearchText);
        }
    }
    partial void OnNewWatchlistNameChanged(string value)
    {
        OnPropertyChanged(nameof(HasNewWatchlistNameError));
    }

    partial void OnEditWatchlistSearchTextChanged(string value)
    {
        var query = value.Trim();
        var matches = string.IsNullOrEmpty(query)
            ? EditWatchlistStocks
            : EditWatchlistStocks.Where(stock => stock.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                  stock.Company.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                                  stock.OfficialName.Contains(query, StringComparison.OrdinalIgnoreCase));

        EditWatchlistCandidates.Clear();
        foreach (var stock in matches)
        {
            EditWatchlistCandidates.Add(stock);
        }
    }

    partial void OnEditWatchlistCandidateChanged(StockSelectionModel? value)
    {
        if (value is null)
        {
            return;
        }

        value.IsSelected = true;
        if (!SelectedEditWatchlistStocks.Contains(value))
        {
            SelectedEditWatchlistStocks.Add(value);
        }

        EditWatchlistCandidate = null;
        EditWatchlistSearchText = string.Empty;
    }
}
