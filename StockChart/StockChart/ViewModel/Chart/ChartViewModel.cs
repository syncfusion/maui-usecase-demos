using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using StockChart.model;
using Syncfusion.Maui.Charts;

namespace StockChart.ViewModel.Chart;

/// <summary>
/// ViewModel responsible for managing chart-related functionality.
/// </summary>
public partial class ChartViewModel : ObservableObject
{
    private string _selectedTimeRange = "1M";
    private string _selectedChartType = "Candle";
    private string _selectedTrendline = "Trendline";
    private bool _isTooltipEnabled;
    private bool _isLogarithmicAxis;
    private bool _isAxisInverted;
    private bool _isAxisOpposed = true;
    private bool _isRangeControlEnabled;
    private double _rangeMinimum;
    private double _rangeMaximum = 1;
    private double _rangeStart;
    private double _rangeEnd = 1;
    private string _lastStandardTimeRange = "1M";
    private bool _isResettingTrendlineSelection;
    private bool _isUpdatingRange;
    private int _chartRangeStartIndex;
    private int _chartRangeEndIndex;
    private int _selectedRangeStartIndex;
    private int _selectedRangeEndIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartViewModel"/> class.
    /// </summary>
    public ChartViewModel()
    {
        AvailableTimeRanges = ["1M", "3M", "5M", "1Y", "All", "Custom"];
        AvailableChartTypes = ["Area", "Candle", "Hollow candle", "Column", "Line", "HiLo", "OHLC", "Range Column", "Step Area", "Step Line"];
        AvailableTrendlines = ["Trendline", "Linear", "Exponential", "Logarithmic", "Polynomial", "Power", "Moving Average"];
        ChartSeries = new ObservableCollection<ChartSeries>();
        ActiveTrendlines = new ObservableCollection<TrendlineViewModel>();
        VisibleCandles = new ObservableCollection<CandleDataModel>();
    }

    /// <summary>
    /// Gets the available time ranges for chart display.
    /// </summary>
    public IReadOnlyList<string> AvailableTimeRanges { get; }
    
    /// <summary>
    /// Gets the available chart types.
    /// </summary>
    public IReadOnlyList<string> AvailableChartTypes { get; }
    
    /// <summary>
    /// Gets the available trendline types.
    /// </summary>
    public IReadOnlyList<string> AvailableTrendlines { get; }
    
    /// <summary>
    /// Gets the collection of chart series.
    /// </summary>
    public ObservableCollection<ChartSeries> ChartSeries { get; }
    
    /// <summary>
    /// Gets the collection of active trendlines.
    /// </summary>
    public ObservableCollection<TrendlineViewModel> ActiveTrendlines { get; }
    
    /// <summary>
    /// Gets the collection of visible candle data.
    /// </summary>
    public ObservableCollection<CandleDataModel> VisibleCandles { get; }

    /// <summary>
    /// Gets or sets the currently selected time range.
    /// </summary>
    public string SelectedTimeRange
    {
        get => _selectedTimeRange;
        set
        {
            if (_selectedTimeRange != value)
            {
                _selectedTimeRange = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected chart type.
    /// </summary>
    public string SelectedChartType
    {
        get => _selectedChartType;
        set
        {
            if (_selectedChartType != value)
            {
                _selectedChartType = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected trendline type.
    /// </summary>
    public string SelectedTrendline
    {
        get => _selectedTrendline;
        set
        {
            if (_selectedTrendline != value)
            {
                _selectedTrendline = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether tooltips are enabled.
    /// </summary>
    public bool IsTooltipEnabled
    {
        get => _isTooltipEnabled;
        set
        {
            if (_isTooltipEnabled != value)
            {
                _isTooltipEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether logarithmic axis is enabled.
    /// </summary>
    public bool IsLogarithmicAxis
    {
        get => _isLogarithmicAxis;
        set
        {
            if (_isLogarithmicAxis != value)
            {
                _isLogarithmicAxis = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the axis is inverted.
    /// </summary>
    public bool IsAxisInverted
    {
        get => _isAxisInverted;
        set
        {
            if (_isAxisInverted != value)
            {
                _isAxisInverted = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the axis is opposed.
    /// </summary>
    public bool IsAxisOpposed
    {
        get => _isAxisOpposed;
        set
        {
            if (_isAxisOpposed != value)
            {
                _isAxisOpposed = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether range control is enabled.
    /// </summary>
    public bool IsRangeControlEnabled
    {
        get => _isRangeControlEnabled;
        set
        {
            if (_isRangeControlEnabled != value)
            {
                _isRangeControlEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the range minimum value.
    /// </summary>
    public double RangeMinimum
    {
        get => _rangeMinimum;
        set
        {
            if (_rangeMinimum != value)
            {
                _rangeMinimum = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the range maximum value.
    /// </summary>
    public double RangeMaximum
    {
        get => _rangeMaximum;
        set
        {
            if (_rangeMaximum != value)
            {
                _rangeMaximum = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the range start value.
    /// </summary>
    public double RangeStart
    {
        get => _rangeStart;
        set
        {
            if (_rangeStart != value)
            {
                _rangeStart = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the range end value.
    /// </summary>
    public double RangeEnd
    {
        get => _rangeEnd;
        set
        {
            if (_rangeEnd != value)
            {
                _rangeEnd = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets the chart range start index.
    /// </summary>
    public int ChartRangeStartIndex => _chartRangeStartIndex;
    
    /// <summary>
    /// Gets the chart range end index.
    /// </summary>
    public int ChartRangeEndIndex => _chartRangeEndIndex;

    /// <summary>
    /// Gets or sets the last standard time range.
    /// </summary>
    public string LastStandardTimeRange
    {
        get => _lastStandardTimeRange;
        set => _lastStandardTimeRange = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether trendline selection is being reset.
    /// </summary>
    public bool IsResettingTrendlineSelection
    {
        get => _isResettingTrendlineSelection;
        set => _isResettingTrendlineSelection = value;
    }

    /// <summary>
    /// Updates the chart series collection.
    /// </summary>
    /// <param name="series">The series to update with.</param>
    public void UpdateChartSeries(IEnumerable<ChartSeries> series)
    {
        ChartSeries.Clear();
        foreach (var chartSeries in series)
        {
            ChartSeries.Add(chartSeries);
        }
    }

    /// <summary>
    /// Updates the visible candles collection.
    /// </summary>
    /// <param name="candles">The candles to update with.</param>
    public void UpdateVisibleCandles(IEnumerable<CandleDataModel> candles)
    {
        VisibleCandles.Clear();
        foreach (var candle in candles)
        {
            VisibleCandles.Add(candle);
        }
    }

    /// <summary>
    /// Updates the chart range indices.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="endIndex">The end index.</param>
    public void UpdateChartRange(int startIndex, int endIndex)
    {
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        OnPropertyChanged(nameof(ChartRangeStartIndex));
        OnPropertyChanged(nameof(ChartRangeEndIndex));
    }

    /// <summary>
    /// Updates the selected range indices.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="endIndex">The end index.</param>
    public void UpdateSelectedRange(int startIndex, int endIndex)
    {
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
    }

    /// <summary>
    /// Gets the selected range as a tuple.
    /// </summary>
    /// <returns>A tuple containing the start and end indices.</returns>
    public (int StartIndex, int EndIndex) GetSelectedRange()
    {
        return (_selectedRangeStartIndex, _selectedRangeEndIndex);
    }

    /// <summary>
    /// Resets the trendline selection.
    /// </summary>
    public void ResetTrendlineSelection()
    {
        _isResettingTrendlineSelection = true;
        SelectedTrendline = "Trendline";
        _isResettingTrendlineSelection = false;
    }
}