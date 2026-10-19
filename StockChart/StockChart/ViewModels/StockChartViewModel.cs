using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using StockChart.Models;
using StockChart.Services;
using StockChart.ViewModels.Chart;
using StockChart.ViewModels.Settings;
using StockChart.ViewModels.Stock;
using StockChart.ViewModels.Watchlist;

namespace StockChart.ViewModels;

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
    /// Applies settings.
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
}