using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockChart.Models;

namespace StockChart.ViewModels.Watchlist;

/// <summary>
/// ViewModel responsible for managing watchlist functionality.
/// </summary>
public partial class WatchlistViewModel : ObservableObject
{
    private WatchlistModel? _selectedWatchlist;
    private bool _isWatchlistSelected;
    private string _newWatchlistName = string.Empty;
    private string _newWatchlistSearchText = string.Empty;
    private StockModel? _newWatchlistStock;
    private string _editWatchlistName = string.Empty;
    private string _editWatchlistSearchText = string.Empty;
    private StockSelectionModel? _editWatchlistCandidate;
    private WatchlistModel? _pendingRemovalWatchlist;

    /// <summary>
    /// Initializes a new instance of the <see cref="WatchlistViewModel"/> class.
    /// </summary>
    public WatchlistViewModel()
    {
        Watchlists = new ObservableCollection<WatchlistModel>();
        AddStockWatchlists = new ObservableCollection<WatchlistSelectionModel>();
        EditWatchlistStocks = new ObservableCollection<StockSelectionModel>();
        SelectedEditWatchlistStocks = new ObservableCollection<StockSelectionModel>();
        EditWatchlistCandidates = new ObservableCollection<StockSelectionModel>();
        SelectedEditWatchlistStocks.CollectionChanged += (_, _) => OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
    }

    /// <summary>
    /// Gets the collection of watchlists.
    /// </summary>
    public ObservableCollection<WatchlistModel> Watchlists { get; }
    
    /// <summary>
    /// Gets the collection of watchlist selection models for adding stocks.
    /// </summary>
    public ObservableCollection<WatchlistSelectionModel> AddStockWatchlists { get; }
    
    /// <summary>
    /// Gets the collection of stock selection models for editing watchlists.
    /// </summary>
    public ObservableCollection<StockSelectionModel> EditWatchlistStocks { get; }
    
    /// <summary>
    /// Gets the collection of selected stock selection models.
    /// </summary>
    public ObservableCollection<StockSelectionModel> SelectedEditWatchlistStocks { get; }
    
    /// <summary>
    /// Gets the collection of candidate stock selection models.
    /// </summary>
    public ObservableCollection<StockSelectionModel> EditWatchlistCandidates { get; }

    /// <summary>
    /// Gets or sets the currently selected watchlist.
    /// </summary>
    public WatchlistModel? SelectedWatchlist
    {
        get => _selectedWatchlist;
        set
        {
            if (_selectedWatchlist != value)
            {
                _selectedWatchlist = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether a watchlist is selected.
    /// </summary>
    public bool IsWatchlistSelected
    {
        get => _isWatchlistSelected;
        set
        {
            if (_isWatchlistSelected != value)
            {
                _isWatchlistSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ViewModeText));
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of a new watchlist.
    /// </summary>
    public string NewWatchlistName
    {
        get => _newWatchlistName;
        set
        {
            if (_newWatchlistName != value)
            {
                _newWatchlistName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCreateWatchlist));
                OnPropertyChanged(nameof(HasNewWatchlistNameError));
                OnPropertyChanged(nameof(NewWatchlistNameError));
            }
        }
    }

    /// <summary>
    /// Gets or sets the search text for new watchlist stocks.
    /// </summary>
    public string NewWatchlistSearchText
    {
        get => _newWatchlistSearchText;
        set
        {
            if (_newWatchlistSearchText != value)
            {
                _newWatchlistSearchText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the stock for a new watchlist.
    /// </summary>
    public StockModel? NewWatchlistStock
    {
        get => _newWatchlistStock;
        set
        {
            if (_newWatchlistStock != value)
            {
                _newWatchlistStock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of a watchlist being edited.
    /// </summary>
    public string EditWatchlistName
    {
        get => _editWatchlistName;
        set
        {
            if (_editWatchlistName != value)
            {
                _editWatchlistName = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the search text for editing watchlists.
    /// </summary>
    public string EditWatchlistSearchText
    {
        get => _editWatchlistSearchText;
        set
        {
            if (_editWatchlistSearchText != value)
            {
                _editWatchlistSearchText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the candidate stock selection model for editing.
    /// </summary>
    public StockSelectionModel? EditWatchlistCandidate
    {
        get => _editWatchlistCandidate;
        set
        {
            if (_editWatchlistCandidate != value)
            {
                _editWatchlistCandidate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selected watchlist can be deleted.
    /// </summary>
    public bool CanDeleteSelectedWatchlist => Watchlists.Count > 1 && SelectedWatchlist is not null;
    
    /// <summary>
    /// Gets the view mode text based on whether a watchlist is selected.
    /// </summary>
    public string ViewModeText => IsWatchlistSelected ? "Watchlist" : "All Stocks";
    
    /// <summary>
    /// Gets a value indicating whether a new watchlist can be created.
    /// </summary>
    public bool CanCreateWatchlist => !string.IsNullOrWhiteSpace(NewWatchlistName);
    
    /// <summary>
    /// Gets a value indicating whether there is an error with the new watchlist name.
    /// </summary>
    public bool HasNewWatchlistNameError => !CanCreateWatchlist;
    
    /// <summary>
    /// Gets the error message for new watchlist name.
    /// </summary>
    public string NewWatchlistNameError => "Enter a watchlist name.";

    /// <summary>
    /// Gets the confirmation text for deleting a watchlist.
    /// </summary>
    public string DeleteWatchlistConfirmationText => SelectedWatchlist is null
        ? "Are you sure you want to delete this watchlist?"
        : $"Are you sure you want to delete \"{SelectedWatchlist.Name}\"?";

    /// <summary>
    /// Initializes the default watchlist.
    /// </summary>
    public void InitializeDefaultWatchlist()
    {
        var defaultWatchlist = new WatchlistModel("Watchlist 1");
        Watchlists.Clear();
        Watchlists.Add(defaultWatchlist);
        SelectedWatchlist = defaultWatchlist;
    }

    /// <summary>
    /// Adds a watchlist to the collection.
    /// </summary>
    /// <param name="watchlist">The watchlist to add.</param>
    public void AddWatchlist(WatchlistModel watchlist)
    {
        Watchlists.Add(watchlist);
    }

    /// <summary>
    /// Removes a watchlist from the collection.
    /// </summary>
    /// <param name="watchlist">The watchlist to remove.</param>
    public void RemoveWatchlist(WatchlistModel watchlist)
    {
        Watchlists.Remove(watchlist);
    }

    /// <summary>
    /// Subscribes to watchlist changes.
    /// </summary>
    /// <param name="watchlist">The watchlist to subscribe to.</param>
    public void SubscribeToWatchlist(WatchlistModel watchlist)
    {
        // Implementation for subscribing to watchlist changes
    }

    /// <summary>
    /// Updates the add stock watchlists collection.
    /// </summary>
    /// <param name="watchlists">The watchlists to update with.</param>
    /// <param name="symbol">The stock symbol.</param>
    public void UpdateAddStockWatchlists(IEnumerable<WatchlistModel> watchlists, string symbol)
    {
        AddStockWatchlists.Clear();
        foreach (var watchlist in watchlists)
        {
            AddStockWatchlists.Add(new WatchlistSelectionModel(watchlist.Name)
            {
                IsSelected = watchlist.Symbols.Contains(symbol)
            });
        }
    }

    /// <summary>
    /// Updates the edit watchlist stocks collection.
    /// </summary>
    /// <param name="stocks">The stocks to update with.</param>
    public void UpdateEditWatchlistStocks(IEnumerable<StockModel> stocks)
    {
        EditWatchlistStocks.Clear();
        EditWatchlistCandidates.Clear();
        
        foreach (var stock in stocks)
        {
            var selectionModel = new StockSelectionModel(stock.Symbol, stock.Company, stock.OfficialName);
            EditWatchlistStocks.Add(selectionModel);
            EditWatchlistCandidates.Add(selectionModel);
        }
    }

    /// <summary>
    /// Updates the selected edit watchlist stocks collection.
    /// </summary>
    /// <param name="watchlist">The watchlist to update from.</param>
    /// <param name="allStocks">All available stocks.</param>
    public void UpdateSelectedEditWatchlistStocks(WatchlistModel watchlist, IEnumerable<StockModel> allStocks)
    {
        SelectedEditWatchlistStocks.Clear();
        
        foreach (var stock in allStocks.Where(s => watchlist.Symbols.Contains(s.Symbol)))
        {
            SelectedEditWatchlistStocks.Add(new StockSelectionModel(stock.Symbol, stock.Company, stock.OfficialName)
            {
                IsSelected = true
            });
        }
    }
}