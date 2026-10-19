using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using StockChart.Models;

namespace StockChart.ViewModels.Stock;

/// <summary>
/// ViewModel responsible for managing stock-related functionality.
/// </summary>
public partial class StockViewModel : ObservableObject
{
    private StockModel? _selectedStock;
    private StockModel? _lastSelectedStock;
    private StockModel? _menuStock;
    private StockModel? _pendingStock;
    private string _searchText = string.Empty;
    private string _addStockSearchText = string.Empty;
    private int _selectedViewIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockViewModel"/> class.
    /// </summary>
    public StockViewModel()
    {
        Stocks = new ObservableCollection<StockModel>();
        FilteredStocks = new ObservableCollection<StockModel>();
        AddStockCandidates = new ObservableCollection<StockModel>();
    }

    /// <summary>
    /// Gets the collection of all stocks.
    /// </summary>
    public ObservableCollection<StockModel> Stocks { get; }
    
    /// <summary>
    /// Gets the collection of filtered stocks.
    /// </summary>
    public ObservableCollection<StockModel> FilteredStocks { get; }
    
    /// <summary>
    /// Gets the collection of candidate stocks for adding.
    /// </summary>
    public ObservableCollection<StockModel> AddStockCandidates { get; }

    /// <summary>
    /// Gets or sets the currently selected stock.
    /// </summary>
    public StockModel? SelectedStock
    {
        get => _selectedStock;
        set
        {
            if (_selectedStock != value)
            {
                _selectedStock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the last selected stock.
    /// </summary>
    public StockModel? LastSelectedStock
    {
        get => _lastSelectedStock;
        set
        {
            if (_lastSelectedStock != value)
            {
                _lastSelectedStock = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedStockPrice));
                OnPropertyChanged(nameof(SelectedStockChange));
                OnPropertyChanged(nameof(SelectedStockChangeColor));
                OnPropertyChanged(nameof(IsChartVisible));
                OnPropertyChanged(nameof(ShowChartPlaceholder));
                OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
            }
        }
    }

    /// <summary>
    /// Gets or sets the menu stock.
    /// </summary>
    public StockModel? MenuStock
    {
        get => _menuStock;
        set
        {
            if (_menuStock != value)
            {
                _menuStock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the pending stock.
    /// </summary>
    public StockModel? PendingStock
    {
        get => _pendingStock;
        set
        {
            if (_pendingStock != value)
            {
                _pendingStock = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the search text.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the add stock search text.
    /// </summary>
    public string AddStockSearchText
    {
        get => _addStockSearchText;
        set
        {
            if (_addStockSearchText != value)
            {
                _addStockSearchText = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected view index.
    /// </summary>
    public int SelectedViewIndex
    {
        get => _selectedViewIndex;
        set
        {
            if (_selectedViewIndex != value)
            {
                _selectedViewIndex = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the selected stock price formatted as currency.
    /// </summary>
    public string SelectedStockPrice => LastSelectedStock?.PriceText ?? "--";
    
    /// <summary>
    /// Gets the selected stock change formatted as value and percentage.
    /// </summary>
    public string SelectedStockChange => LastSelectedStock?.ChangeText ?? "--";
    
    /// <summary>
    /// Gets a value indicating whether the chart is visible.
    /// </summary>
    public bool IsChartVisible => LastSelectedStock is not null;
    
    /// <summary>
    /// Gets a value indicating whether the chart placeholder should be shown.
    /// </summary>
    public bool ShowChartPlaceholder => LastSelectedStock is null;
    
    /// <summary>
    /// Gets the chart placeholder text.
    /// </summary>
    public string ChartPlaceholderText => "Select a stock to view its chart.";
    
    /// <summary>
    /// Gets a value indicating whether the last selected stock is in the selected watchlist.
    /// </summary>
    public bool IsLastSelectedStockInSelectedWatchlist => LastSelectedStock is not null;

    /// <summary>
    /// Gets the color for the selected stock change based on whether it's positive or negative.
    /// </summary>
    public Color SelectedStockChangeColor => LastSelectedStock?.LatestCandle is null
        ? Colors.Gray
        : LastSelectedStock.LatestCandle.Close >= LastSelectedStock.LatestCandle.Open
            ? Color.FromArgb("#16A34A")
            : Color.FromArgb("#DC2626");

    /// <summary>
    /// Gets the stock count text.
    /// </summary>
    public string StockCountText => $"{FilteredStocks.Count} stocks";

    /// <summary>
    /// Updates the stocks collection.
    /// </summary>
    /// <param name="stocks">The stocks to update with.</param>
    public void UpdateStocks(IEnumerable<StockModel> stocks)
    {
        Stocks.Clear();
        FilteredStocks.Clear();

        foreach (var stock in stocks)
        {
            Stocks.Add(stock);
            FilteredStocks.Add(stock);
        }
    }

    /// <summary>
    /// Updates the filtered stocks collection.
    /// </summary>
    /// <param name="stocks">The stocks to update with.</param>
    public void UpdateFilteredStocks(IEnumerable<StockModel> stocks)
    {
        FilteredStocks.Clear();
        foreach (var stock in stocks)
        {
            FilteredStocks.Add(stock);
        }
    }

    /// <summary>
    /// Updates the add stock candidates collection.
    /// </summary>
    /// <param name="stocks">The stocks to update with.</param>
    public void UpdateAddStockCandidates(IEnumerable<StockModel> stocks)
    {
        AddStockCandidates.Clear();
        foreach (var stock in stocks)
        {
            AddStockCandidates.Add(stock);
        }
    }
}