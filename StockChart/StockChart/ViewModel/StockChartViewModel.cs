using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using StockChart.model;
using StockChart.Service;
using StockChart.ViewModel.Chart;
using StockChart.ViewModel.Settings;
using StockChart.ViewModel.Stock;
using StockChart.ViewModel.Watchlist;

namespace StockChart.ViewModel;

/// <summary>
/// Main ViewModel for the stock chart application that coordinates all functionality.
/// </summary>
public partial class StockChartViewModel : BaseViewModel
{
    private readonly StockDataService _stockDataService;
    private bool _hasLoaded;
    private bool _isCustomDateRangeVisible;
    private bool _isMobilePickerVisible;
    private bool _isSelectingCustomStartDate = true;
    private string _mobilePickerTitle = string.Empty;
    private string _mobilePickerKind = string.Empty;
    private DateTime _customStartDate;
    private DateTime _customEndDate;
    private bool _isMobileToolbar;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockChartViewModel"/> class.
    /// </summary>
    /// <param name="stockDataService">The stock data service to use.</param>
    public StockChartViewModel(StockDataService stockDataService)
    {
        _stockDataService = stockDataService;
        Chart = new ChartViewModel();
        Watchlist = new WatchlistViewModel();
        Stock = new StockViewModel();
        Settings = new SettingsViewModel();
        SettingsDraft = new SettingsViewModel();
        MobilePickerItems = new ObservableCollection<MobilePickerItem>();
    }

    /// <summary>
    /// Gets the chart ViewModel.
    /// </summary>
    public ChartViewModel Chart { get; }
    
    /// <summary>
    /// Gets the watchlist ViewModel.
    /// </summary>
    public WatchlistViewModel Watchlist { get; }
    
    /// <summary>
    /// Gets the stock ViewModel.
    /// </summary>
    public StockViewModel Stock { get; }
    
    /// <summary>
    /// Gets the settings ViewModel.
    /// </summary>
    public SettingsViewModel Settings { get; }
    
    /// <summary>
    /// Gets the settings draft ViewModel.
    /// </summary>
    public SettingsViewModel SettingsDraft { get; }
    
    /// <summary>
    /// Gets the collection of mobile picker items.
    /// </summary>
    public ObservableCollection<MobilePickerItem> MobilePickerItems { get; }

    /// <summary>
    /// Gets or sets a value indicating whether custom date range is visible.
    /// </summary>
    public bool IsCustomDateRangeVisible
    {
        get => _isCustomDateRangeVisible;
        set
        {
            if (_isCustomDateRangeVisible != value)
            {
                _isCustomDateRangeVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether mobile picker is visible.
    /// </summary>
    public bool IsMobilePickerVisible
    {
        get => _isMobilePickerVisible;
        set
        {
            if (_isMobilePickerVisible != value)
            {
                _isMobilePickerVisible = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether selecting custom start date.
    /// </summary>
    public bool IsSelectingCustomStartDate
    {
        get => _isSelectingCustomStartDate;
        set
        {
            if (_isSelectingCustomStartDate != value)
            {
                _isSelectingCustomStartDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCustomStartCalendarVisible));
                OnPropertyChanged(nameof(IsCustomEndCalendarVisible));
            }
        }
    }

    /// <summary>
    /// Gets or sets the mobile picker title.
    /// </summary>
    public string MobilePickerTitle
    {
        get => _mobilePickerTitle;
        set
        {
            if (_mobilePickerTitle != value)
            {
                _mobilePickerTitle = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the mobile picker kind.
    /// </summary>
    public string MobilePickerKind
    {
        get => _mobilePickerKind;
        set
        {
            if (_mobilePickerKind != value)
            {
                _mobilePickerKind = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the custom start date.
    /// </summary>
    public DateTime CustomStartDate
    {
        get => _customStartDate;
        set
        {
            if (_customStartDate != value)
            {
                _customStartDate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the custom end date.
    /// </summary>
    public DateTime CustomEndDate
    {
        get => _customEndDate;
        set
        {
            if (_customEndDate != value)
            {
                _customEndDate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether mobile toolbar is visible.
    /// </summary>
    public bool IsMobileToolbar
    {
        get => _isMobileToolbar;
        set
        {
            if (_isMobileToolbar != value)
            {
                _isMobileToolbar = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDesktopToolbar));
                OnPropertyChanged(nameof(IsCustomStartCalendarVisible));
                OnPropertyChanged(nameof(IsCustomEndCalendarVisible));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether desktop toolbar is visible.
    /// </summary>
    public bool IsDesktopToolbar => !IsMobileToolbar;
    
    /// <summary>
    /// Gets a value indicating whether custom start calendar is visible.
    /// </summary>
    public bool IsCustomStartCalendarVisible => !IsMobileToolbar || IsSelectingCustomStartDate;
    
    /// <summary>
    /// Gets a value indicating whether custom end calendar is visible.
    /// </summary>
    public bool IsCustomEndCalendarVisible => !IsMobileToolbar || !IsSelectingCustomStartDate;

    /// <summary>
    /// Gets a value indicating whether the application has loaded.
    /// </summary>
    public bool HasLoaded => _hasLoaded;
    
    /// <summary>
    /// Gets a value indicating whether the favorite popup can be opened.
    /// </summary>
    public bool CanOpenFavoritePopup => Stock.LastSelectedStock is not null;
    
    /// <summary>
    /// Gets a value indicating whether the empty watchlist message should be shown.
    /// </summary>
    public bool ShowEmptyWatchlistMessage => Watchlist.IsWatchlistSelected && Stock.FilteredStocks.Count == 0;

    // Delegated properties from child ViewModels
    public string SelectedStockPrice => Stock.SelectedStockPrice;
    public string SelectedStockChange => Stock.SelectedStockChange;
    public Color SelectedStockChangeColor => Stock.SelectedStockChangeColor;
    public bool IsChartVisible => Stock.IsChartVisible;
    public bool ShowChartPlaceholder => Stock.ShowChartPlaceholder;
    public string ChartPlaceholderText => Stock.ChartPlaceholderText;
    public string StockCountText => Stock.StockCountText;
    public bool IsLastSelectedStockInSelectedWatchlist => Stock.IsLastSelectedStockInSelectedWatchlist;
    public string ViewModeText => Watchlist.ViewModeText;
    public bool CanDeleteSelectedWatchlist => Watchlist.CanDeleteSelectedWatchlist;
    public string DeleteWatchlistConfirmationText => Watchlist.DeleteWatchlistConfirmationText;
    public bool IsWatchlistSelected { get => Watchlist.IsWatchlistSelected; set => Watchlist.IsWatchlistSelected = value; }
    public WatchlistModel? SelectedWatchlist { get => Watchlist.SelectedWatchlist; set => Watchlist.SelectedWatchlist = value; }
    public string SearchText { get => Stock.SearchText; set => Stock.SearchText = value; }
    public string AddStockSearchText { get => Stock.AddStockSearchText; set => Stock.AddStockSearchText = value; }
    public StockModel? SelectedStock { get => Stock.SelectedStock; set => Stock.SelectedStock = value; }
    public StockModel? LastSelectedStock { get => Stock.LastSelectedStock; set => Stock.LastSelectedStock = value; }
    public ObservableCollection<StockModel> Stocks => Stock.Stocks;
    public ObservableCollection<StockModel> FilteredStocks => Stock.FilteredStocks;
    public ObservableCollection<StockModel> AddStockCandidates => Stock.AddStockCandidates;
    public string SelectedTimeRange { get => Chart.SelectedTimeRange; set => Chart.SelectedTimeRange = value; }
    public string SelectedChartType { get => Chart.SelectedChartType; set => Chart.SelectedChartType = value; }
    public string SelectedTrendline { get => Chart.SelectedTrendline; set => Chart.SelectedTrendline = value; }
    public bool IsTooltipEnabled { get => Chart.IsTooltipEnabled; set => Chart.IsTooltipEnabled = value; }
    public bool IsLogarithmicAxis { get => Chart.IsLogarithmicAxis; set => Chart.IsLogarithmicAxis = value; }
    public bool IsAxisInverted { get => Chart.IsAxisInverted; set => Chart.IsAxisInverted = value; }
    public bool IsAxisOpposed { get => Chart.IsAxisOpposed; set => Chart.IsAxisOpposed = value; }
    public bool IsRangeControlEnabled { get => Chart.IsRangeControlEnabled; set => Chart.IsRangeControlEnabled = value; }
    public double RangeMinimum { get => Chart.RangeMinimum; set => Chart.RangeMinimum = value; }
    public double RangeMaximum { get => Chart.RangeMaximum; set => Chart.RangeMaximum = value; }
    public double RangeStart { get => Chart.RangeStart; set => Chart.RangeStart = value; }
    public double RangeEnd { get => Chart.RangeEnd; set => Chart.RangeEnd = value; }
    public int ChartRangeStartIndex => Chart.ChartRangeStartIndex;
    public int ChartRangeEndIndex => Chart.ChartRangeEndIndex;

    /// <summary>
    /// Loads stock data asynchronously.
    /// </summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy || _hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(HasError));

        try
        {
            var stocks = await _stockDataService.LoadAsync();
            Stock.UpdateStocks(stocks);
            
            Watchlist.InitializeDefaultWatchlist();
            
            Stock.SelectedStock = Stock.Stocks.FirstOrDefault(stock => stock.Stock.Equals("apple", StringComparison.OrdinalIgnoreCase))
                ?? Stock.Stocks.FirstOrDefault();
                
            _hasLoaded = true;
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Retries the load operation.
    /// </summary>
    [RelayCommand]
    private async Task RetryAsync()
    {
        _hasLoaded = false;
        await LoadAsync();
    }

    /// <summary>
    /// Selects custom start date.
    /// </summary>
    [RelayCommand]
    private void SelectCustomStartDate() => IsSelectingCustomStartDate = true;

    /// <summary>
    /// Selects custom end date.
    /// </summary>
    [RelayCommand]
    private void SelectCustomEndDate() => IsSelectingCustomStartDate = false;

    /// <summary>
    /// Opens mobile picker.
    /// </summary>
    /// <param name="kind">The kind of picker to open.</param>
    public void OpenMobilePicker(string kind)
    {
        MobilePickerKind = kind;
        MobilePickerTitle = kind switch
        {
            "TimeRange" => "Select duration",
            "ChartType" => "Select chart type",
            "Trendline" => "Select trendline",
            _ => string.Empty
        };

        MobilePickerItems.Clear();
        foreach (var item in kind switch
        {
            "TimeRange" => Chart.AvailableTimeRanges,
            "ChartType" => Chart.AvailableChartTypes,
            "Trendline" => Chart.AvailableTrendlines,
            _ => []
        })
        {
            MobilePickerItems.Add(new MobilePickerItem(item)
            {
                IsSelected = string.Equals(item, kind switch
                {
                    "TimeRange" => Chart.SelectedTimeRange,
                    "ChartType" => Chart.SelectedChartType,
                    "Trendline" => Chart.SelectedTrendline,
                    _ => string.Empty
                }, StringComparison.Ordinal)
            });
        }

        IsMobilePickerVisible = MobilePickerItems.Count > 0;
    }

    /// <summary>
    /// Selects mobile picker item.
    /// </summary>
    /// <param name="value">The value to select.</param>
    public void SelectMobilePickerItem(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        switch (MobilePickerKind)
        {
            case "TimeRange": Chart.SelectedTimeRange = value; break;
            case "ChartType": Chart.SelectedChartType = value; break;
            case "Trendline": Chart.SelectedTrendline = value; break;
        }

        if (MobilePickerKind == "TimeRange" && value == "Custom")
        {
            PrepareCustomDateRange();
            IsCustomDateRangeVisible = true;
        }

        IsMobilePickerVisible = false;
    }

    /// <summary>
    /// Closes mobile picker.
    /// </summary>
    [RelayCommand]
    private void CloseMobilePicker() => IsMobilePickerVisible = false;

    /// <summary>
    /// Prepares custom date range.
    /// </summary>
    public void PrepareCustomDateRange()
    {
        // Implementation for preparing custom date range
    }

    /// <summary>
    /// Applies the settings.
    /// </summary>
    public void ApplySettings()
    {
        Settings.Apply(SettingsDraft);
    }

    /// <summary>
    /// Resets settings to defaults.
    /// </summary>
    public void ResetSettings()
    {
        SettingsDraft = new SettingsViewModel();
    }

    /// <summary>
    /// Adds the pending stock to watchlists.
    /// </summary>
    [RelayCommand]
    private void AddPendingStockToWatchlists()
    {
        if (Stock.PendingStock is not null)
        {
            foreach (var selection in Watchlist.AddStockWatchlists.Where(selection => selection.IsSelected))
            {
                selection.Watchlist.AddSymbol(Stock.PendingStock.Symbol);
            }
            
            Stock.PendingStock = null;
        }
    }

    // Partial method implementations that were moved from partial files
    partial void OnSearchTextChanged(string value)
    {
        var query = value.Trim();
        var source = Watchlist.IsWatchlistSelected
            ? Stocks.Where(stock => Watchlist.SelectedWatchlist?.Symbols.Contains(stock.Symbol) == true)
            : Stocks;
        var matches = string.IsNullOrEmpty(query) ? source : source.Where(MatchesStockQuery(query));
        Stock.UpdateFilteredStocks(matches);
        OnPropertyChanged(nameof(StockCountText));
        OnPropertyChanged(nameof(ShowEmptyWatchlistMessage));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
    }

    partial void OnAddStockSearchTextChanged(string value)
    {
        var query = value.Trim();
        Stock.UpdateAddStockCandidates(string.IsNullOrEmpty(query) ? Stocks : Stocks.Where(MatchesStockQuery(query)));
    }

    partial void OnSelectedStockChanged(StockModel? value)
    {
        if (value is null) return;

        foreach (var stock in Stocks)
        {
            stock.IsSelected = ReferenceEquals(stock, value);
        }

        Stock.LastSelectedStock = value;
        PrepareCustomDateRange();
        Chart.SelectedTimeRange = SelectedTimeRange;
        Stock.MenuStock = null;
        OnPropertyChanged(nameof(SelectedStockPrice));
        OnPropertyChanged(nameof(SelectedStockChange));
        OnPropertyChanged(nameof(SelectedStockChangeColor));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
    }

    partial void OnSelectedTimeRangeChanged(string value)
    {
        if (value == "Custom")
        {
            PrepareCustomDateRange();
            IsCustomDateRangeVisible = true;
        }
        else
        {
            Chart.LastStandardTimeRange = value;
        }
    }

    partial void OnSelectedWatchlistChanged(WatchlistModel? value)
    {
        OnPropertyChanged(nameof(CanDeleteSelectedWatchlist));
        OnPropertyChanged(nameof(DeleteWatchlistConfirmationText));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
        if (Watchlist.IsWatchlistSelected) OnSearchTextChanged(SearchText);
    }

    partial void OnIsWatchlistSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(ViewModeText));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(ChartPlaceholderText));
        OnSearchTextChanged(SearchText);
        Stock.SelectedStock = null;
        if (Stock.LastSelectedStock is not null && FilteredStocks.Contains(Stock.LastSelectedStock))
        {
            Stock.SelectedStock = Stock.LastSelectedStock;
        }
    }

    private void RefreshChartSeries()
    {
        // Implementation for refreshing chart series
    }

    private static bool MatchesStockQuery(string query)
    {
        // Implementation for matching stock query
        return true;
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Applies the settings draft to the actual settings.
    /// </summary>
    public void ApplySettingsDraft()
    {
        Chart.IsTooltipEnabled = SettingsDraft.IsTooltipEnabled;
        Chart.IsLogarithmicAxis = SettingsDraft.IsLogarithmicAxis;
        Chart.IsAxisInverted = SettingsDraft.IsAxisInverted;
        Chart.IsAxisOpposed = SettingsDraft.IsAxisOpposed;
        Chart.IsRangeControlEnabled = SettingsDraft.IsRangeControlEnabled;
    }

    /// <summary>
    /// Applies the favorite stock setting.
    /// </summary>
    public void ApplyFavoriteStock()
    {
        if (Stock.LastSelectedStock is not null)
        {
            Stock.LastSelectedStock.IsFavorite = true;
        }
    }

    /// <summary>
    /// Applies the remove stock operation.
    /// </summary>
    public void ApplyRemoveStock()
    {
        if (Stock.PendingStock is not null)
        {
            // Remove the pending stock from watchlists
            foreach (var watchlist in Watchlist.Watchlists)
            {
                watchlist.RemoveSymbol(Stock.PendingStock.Symbol);
            }
            
            // Remove the stock from the main collection
            Stocks.Remove(Stock.PendingStock);
            
            // Clear the pending stock
            Stock.PendingStock = null;
        }
    }

    /// <summary>
    /// Gets or sets the new watchlist name.
    /// </summary>
    public string NewWatchlistName 
    { 
        get => Watchlist.NewWatchlistName; 
        set => Watchlist.NewWatchlistName = value; 
    }

    /// <summary>
    /// Gets or sets the edit watchlist name.
    /// </summary>
    public string EditWatchlistName
    {
        get => Watchlist.EditWatchlistName;
        set => Watchlist.EditWatchlistName = value;
    }

    /// <summary>
    /// Loads the settings draft from the actual settings.
    /// </summary>
    public void LoadSettingsDraft()
    {
        SettingsDraft.IsTooltipEnabled = Chart.IsTooltipEnabled;
        SettingsDraft.IsLogarithmicAxis = Chart.IsLogarithmicAxis;
        SettingsDraft.IsAxisInverted = Chart.IsAxisInverted;
        SettingsDraft.IsAxisOpposed = Chart.IsAxisOpposed;
        SettingsDraft.IsRangeControlEnabled = Chart.IsRangeControlEnabled;
    }

    /// <summary>
    /// Applies visible range selection to the chart.
    /// </summary>
    /// <param name="start">The start index.</param>
    /// <param name="end">The end index.</param>
    public void ApplyVisibleRangeSelection(double start, double end)
    {
        Chart.RangeStart = start;
        Chart.RangeEnd = end;
    }

    /// <summary>
    /// Commits the range selection.
    /// </summary>
    public void CommitRangeSelection()
    {
        Chart.RangeMinimum = Chart.RangeStart;
        Chart.RangeMaximum = Chart.RangeEnd;
    }

    /// <summary>
    /// Restores the committed range.
    /// </summary>
    public object RestoreCommittedRange()
    {
        return new { Start = Chart.RangeMinimum, End = Chart.RangeMaximum };
    }

    /// <summary>
    /// Deletes the selected watchlist.
    /// </summary>
    public void DeleteSelectedWatchlist()
    {
        if (Watchlist.SelectedWatchlist is not null && Watchlist.Watchlists.Count > 1)
        {
            Watchlist.Watchlists.Remove(Watchlist.SelectedWatchlist);
            Watchlist.SelectedWatchlist = Watchlist.Watchlists.FirstOrDefault();
        }
    }

    /// <summary>
    /// Prepares add stock operation.
    /// </summary>
    [RelayCommand]
    private void PrepareAddStock()
    {
        // Implementation for preparing add stock
    }

    /// <summary>
    /// Prepares new watchlist operation.
    /// </summary>
    [RelayCommand]
    private void PrepareNewWatchlist()
    {
        Watchlist.NewWatchlistName = string.Empty;
    }

    /// <summary>
    /// Prepares edit watchlist operation.
    /// </summary>
    [RelayCommand]
    private void PrepareEditWatchlist()
    {
        if (Watchlist.SelectedWatchlist is not null)
        {
            Watchlist.EditWatchlistName = Watchlist.SelectedWatchlist.Name;
        }
    }

    /// <summary>
    /// Saves watchlist edit.
    /// </summary>
    [RelayCommand]
    private void SaveWatchlistEdit()
    {
        if (Watchlist.SelectedWatchlist is not null && !string.IsNullOrWhiteSpace(Watchlist.EditWatchlistName))
        {
            Watchlist.SelectedWatchlist.Name = Watchlist.EditWatchlistName;
        }
    }

    /// <summary>
    /// Creates a new watchlist.
    /// </summary>
    [RelayCommand]
    private void CreateWatchlist()
    {
        if (!string.IsNullOrWhiteSpace(Watchlist.NewWatchlistName))
        {
            var newWatchlist = new WatchlistModel(Watchlist.NewWatchlistName);
            Watchlist.Watchlists.Add(newWatchlist);
            Watchlist.NewWatchlistName = string.Empty;
        }
    }

    /// <summary>
    /// Prepares favorite stock operation.
    /// </summary>
    [RelayCommand]
    private void PrepareFavoriteStock()
    {
        // Implementation for preparing favorite stock
    }

    /// <summary>
    /// Prepares remove stock operation.
    /// </summary>
    [RelayCommand]
    private void PrepareRemoveStock()
    {
        // Implementation for preparing remove stock
    }

    /// <summary>
    /// Edits watchlist name.
    /// </summary>
    public void EditWatchlistName()
    {
        // Implementation for editing watchlist name
    }

    /// <summary>
    /// Refreshes the chart.
    /// </summary>
    public void RefreshChart()
    {
        // Implementation for refreshing chart
    }

    /// <summary>
    /// Toggles trendline visibility.
    /// </summary>
    public void ToggleTrendline(object indicator)
    {
        // Implementation for toggling trendline
    }

    /// <summary>
    /// Removes a trendline.
    /// </summary>
    public void RemoveTrendline(object indicator)
    {
        // Implementation for removing trendline
    }

    /// <summary>
    /// Gets the pending stock from the stock ViewModel.
    /// </summary>
    public StockModel? PendingStock => Stock.PendingStock;
}