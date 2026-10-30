using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using StockChart.Models;
using StockChart.Services;
using Syncfusion.Maui.Charts;

namespace StockChart.ViewModels;

public partial class StockChartViewModel : ObservableObject
{
    private readonly StockDataService _stockDataService;
    private bool _hasLoaded;

    public StockChartViewModel(StockDataService stockDataService)
    {
        _stockDataService = stockDataService;
        SelectedEditWatchlistStocks.CollectionChanged += OnSelectedEditWatchlistStocksChanged;
    }

    public ObservableCollection<StockModel> Stocks { get; } = [];
    public ObservableCollection<StockModel> FilteredStocks { get; } = [];
    public ObservableCollection<CandleDataModel> VisibleCandles { get; } = [];
    public ObservableCollection<WatchlistModel> Watchlists { get; } = [];
    public ObservableCollection<StockModel> AddStockCandidates { get; } = [];
    public ObservableCollection<WatchlistSelectionModel> AddStockWatchlists { get; } = [];
    public ObservableCollection<StockSelectionModel> EditWatchlistStocks { get; } = [];
    public ObservableCollection<StockSelectionModel> SelectedEditWatchlistStocks { get; } = [];
    public ObservableCollection<StockSelectionModel> EditWatchlistCandidates { get; } = [];
    public IReadOnlyList<string> AvailableTimeRanges { get; } = ["1M", "3M", "5M", "1Y", "All", "Custom"];
    public IReadOnlyList<string> AvailableChartTypes { get; } = ["Area", "Candle", "Hollow candle", "Column", "Line", "HiLo", "OHLC", "Range Column", "Step Area", "Step Line"];
    public IReadOnlyList<string> AvailableTrendlines { get; } = ["Trendline", "Linear", "Exponential", "Logarithmic", "Polynomial", "Power", "Moving Average"];
    public ObservableCollection<ChartSeries> ChartSeries { get; } = [];
    public ObservableCollection<TrendlineViewModel> ActiveTrendlines { get; } = [];
    public ObservableCollection<MobilePickerItem> MobilePickerItems { get; } = [];
    public ChartSettingsViewModel SettingsDraft { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockModel? SelectedStock { get; set; }

    [ObservableProperty]
    public partial StockModel? LastSelectedStock { get; set; }

    [ObservableProperty]
    public partial bool IsWatchlistSelected { get; set; }

    [ObservableProperty]
    public partial int SelectedViewIndex { get; set; }

    [ObservableProperty]
    public partial string SelectedTimeRange { get; set; } = "1M";

    [ObservableProperty]
    public partial DateTime CustomStartDate { get; set; }

    [ObservableProperty]
    public partial DateTime CustomEndDate { get; set; }

    [ObservableProperty]
    public partial bool IsCustomDateRangeVisible { get; set; }

    [ObservableProperty]
    public partial bool IsMobilePickerVisible { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCustomStartCalendarVisible))]
    [NotifyPropertyChangedFor(nameof(IsCustomEndCalendarVisible))]
    public partial bool IsSelectingCustomStartDate { get; set; } = true;

    [ObservableProperty]
    public partial string MobilePickerTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MobilePickerKind { get; set; } = string.Empty;

    private string _lastStandardTimeRange = "1M";

    [ObservableProperty]
    public partial string SelectedChartType { get; set; } = "Candle";

    [ObservableProperty]
    public partial string SelectedTrendline { get; set; } = "Trendline";
    private bool _isResettingTrendlineSelection;

    [ObservableProperty]
    public partial bool IsTooltipEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsLogarithmicAxis { get; set; }

    [ObservableProperty]
    public partial bool IsAxisInverted { get; set; }

    [ObservableProperty]
    public partial bool IsAxisOpposed { get; set; } = true;

    [ObservableProperty]
    public partial bool IsRangeControlEnabled { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDesktopToolbar))]
    public partial bool IsMobileToolbar { get; set; }

    [ObservableProperty]
    public partial double RangeMinimum { get; set; }

    [ObservableProperty]
    public partial double RangeMaximum { get; set; } = 1;

    [ObservableProperty]
    public partial double RangeStart { get; set; }

    [ObservableProperty]
    public partial double RangeEnd { get; set; } = 1;

    private bool _isUpdatingRange;
    private int _chartRangeStartIndex;
    private int _chartRangeEndIndex;
    private int _selectedRangeStartIndex;
    private int _selectedRangeEndIndex;

    [ObservableProperty]
    public partial StockModel? MenuStock { get; set; }

    [ObservableProperty]
    public partial WatchlistModel? SelectedWatchlist { get; set; }

    [ObservableProperty]
    public partial StockModel? PendingStock { get; set; }

    private WatchlistModel? _pendingRemovalWatchlist;

    [ObservableProperty]
    public partial string AddStockSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCreateWatchlist))]
    public partial string NewWatchlistName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NewWatchlistSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockModel? NewWatchlistStock { get; set; }

    [ObservableProperty]
    public partial string EditWatchlistName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditWatchlistSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial StockSelectionModel? EditWatchlistCandidate { get; set; }

    public bool IsNotBusy => !IsBusy;
    public bool IsDesktopToolbar => !IsMobileToolbar;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasLoaded => _hasLoaded;
    public string StockCountText => $"{FilteredStocks.Count} stocks";
    public bool IsChartVisible => LastSelectedStock is not null;
    public bool ShowChartPlaceholder => LastSelectedStock is null;
    public string ChartPlaceholderText => "Select a stock to view its chart.";
    public bool CanOpenFavoritePopup => LastSelectedStock is not null;
    public bool IsLastSelectedStockInSelectedWatchlist => LastSelectedStock is not null &&
        Watchlists.Any(watchlist => watchlist.Symbols.Contains(LastSelectedStock.Symbol));
    public string SelectedStockPrice => LastSelectedStock?.PriceText ?? "--";
    public string SelectedStockChange => LastSelectedStock?.ChangeText ?? "--";
    public Color SelectedStockChangeColor
    {
        get
        {
            var latest = LastSelectedStock?.LatestCandle;
            if (latest is null)
            {
                return Colors.Gray;
            }

            return latest.Close >= latest.Open ? Color.FromArgb("#16A34A") : Color.FromArgb("#DC2626");
        }
    }
    public string ViewModeText => IsWatchlistSelected ? "Watchlist" : "All Stocks";
    public bool ShowEmptyWatchlistMessage => IsWatchlistSelected && FilteredStocks.Count == 0;
    public bool CanDeleteSelectedWatchlist => Watchlists.Count > 1 && SelectedWatchlist is not null;
    public string DeleteWatchlistConfirmationText => SelectedWatchlist is null
        ? "Are you sure you want to delete this watchlist?"
        : $"Are you sure you want to delete \"{SelectedWatchlist.Name}\"?";
    public bool CanCreateWatchlist => !string.IsNullOrWhiteSpace(NewWatchlistName);
    public bool HasNewWatchlistNameError => !CanCreateWatchlist;
    public string NewWatchlistNameError => "Enter a watchlist name.";
    public int ChartRangeStartIndex => _chartRangeStartIndex;
    public int ChartRangeEndIndex => _chartRangeEndIndex;
    public bool IsCustomStartCalendarVisible => !IsMobileToolbar || IsSelectingCustomStartDate;
    public bool IsCustomEndCalendarVisible => !IsMobileToolbar || !IsSelectingCustomStartDate;

    partial void OnIsMobileToolbarChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCustomStartCalendarVisible));
        OnPropertyChanged(nameof(IsCustomEndCalendarVisible));
    }

    [RelayCommand]
    private void SelectCustomStartDate() => IsSelectingCustomStartDate = true;

    [RelayCommand]
    private void SelectCustomEndDate() => IsSelectingCustomStartDate = false;

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
            Stocks.Clear();
            FilteredStocks.Clear();

            foreach (var stock in stocks)
            {
                Stocks.Add(stock);
                FilteredStocks.Add(stock);
            }

            var defaultWatchlist = new WatchlistModel("Watchlist 1");
            SubscribeToWatchlist(defaultWatchlist);
            Watchlists.Clear();
            Watchlists.Add(defaultWatchlist);
            SelectedWatchlist = defaultWatchlist;

            SelectedStock = Stocks.FirstOrDefault(stock => stock.Stock.Equals("apple", StringComparison.OrdinalIgnoreCase))
                ?? Stocks.FirstOrDefault();
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

    [RelayCommand]
    private async Task RetryAsync()
    {
        _hasLoaded = false;
        await LoadAsync();
    }

    partial void OnLastSelectedStockChanged(StockModel? value)
    {
        OnPropertyChanged(nameof(SelectedStockPrice));
        OnPropertyChanged(nameof(SelectedStockChange));
        OnPropertyChanged(nameof(SelectedStockChangeColor));
        OnPropertyChanged(nameof(IsChartVisible));
        OnPropertyChanged(nameof(ShowChartPlaceholder));
        OnPropertyChanged(nameof(IsLastSelectedStockInSelectedWatchlist));
        OnPropertyChanged(nameof(AvailableDataStartDate));
        OnPropertyChanged(nameof(AvailableDataEndDate));
    }

    partial void OnSelectedChartTypeChanged(string value)
    {
        RefreshChartSeries();
    }

    partial void OnSelectedTrendlineChanged(string value)
    {
        if (_isResettingTrendlineSelection || value == "Trendline")
        {
            return;
        }

        if (!ActiveTrendlines.Any(trendline => trendline.Name == value))
        {
            ActiveTrendlines.Add(new TrendlineViewModel(value));
        }

        _isResettingTrendlineSelection = true;
        SelectedTrendline = "Trendline";
        _isResettingTrendlineSelection = false;
        RefreshChartSeries();
    }

    partial void OnSelectedViewIndexChanged(int value)
    {
        IsWatchlistSelected = value == 1;
    }

    [RelayCommand]
    private void ShowAllStocks()
    {
        IsWatchlistSelected = false;
    }

    [RelayCommand]
    private void ShowWatchlist()
    {
        IsWatchlistSelected = true;
    }

}
