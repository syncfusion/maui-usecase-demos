using CommunityToolkit.Mvvm.Input;
using StockChart.Models;

namespace StockChart.ViewModels;

// Time-range and custom date-range selection driving the visible candle window.
public partial class StockChartViewModel
{
    private void ApplyTimeRange(string range)
    {
        if (LastSelectedStock is null)
        {
            VisibleCandles.Clear();
            return;
        }

        var data = LastSelectedStock.Data;
        var lastDate = data[^1].Date;
        if (range == "Custom")
        {
            ApplyCustomDateRange();
            return;
        }

        var firstDate = range switch
        {
            "1M" => lastDate.AddMonths(-1),
            "3M" => lastDate.AddMonths(-3),
            "5M" => lastDate.AddMonths(-5),
            "1Y" => lastDate.AddYears(-1),
            _ => data[0].Date
        };

        var startIndex = data.Select((candle, index) => (candle, index))
            .FirstOrDefault(item => item.candle.Date >= firstDate).index;
        var endIndex = data.Count - 1;

        _isUpdatingRange = true;
        RangeMinimum = 0;
        RangeMaximum = Math.Max(1, endIndex);
        RangeStart = startIndex;
        RangeEnd = endIndex;
        _isUpdatingRange = false;

        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
        LoadAllCandles();
        RefreshChartSeries();
    }

    public void PrepareCustomDateRange()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        var availableStart = LastSelectedStock.Data[0].Date.Date;
        var availableEnd = LastSelectedStock.Data[^1].Date.Date;
        var selectedStart = CustomStartDate == default ? availableStart : CustomStartDate.Date;
        var selectedEnd = CustomEndDate == default ? availableEnd : CustomEndDate.Date;
        CustomStartDate = selectedStart < availableStart ? availableStart : selectedStart > availableEnd ? availableEnd : selectedStart;
        CustomEndDate = selectedEnd < CustomStartDate ? CustomStartDate : selectedEnd > availableEnd ? availableEnd : selectedEnd;
    }

    public DateTime AvailableDataStartDate => LastSelectedStock?.Data.FirstOrDefault()?.Date.Date ?? DateTime.Today;
    public DateTime AvailableDataEndDate => LastSelectedStock?.Data.LastOrDefault()?.Date.Date ?? DateTime.Today;
    [RelayCommand]
    public void ApplyCustomDateRange()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        var startDate = CustomStartDate.Date < AvailableDataStartDate ? AvailableDataStartDate : CustomStartDate.Date;
        var endDate = CustomEndDate.Date > AvailableDataEndDate ? AvailableDataEndDate : CustomEndDate.Date;
        if (endDate < startDate)
        {
            endDate = startDate;
        }

        CustomStartDate = startDate;
        CustomEndDate = endDate;
        var startIndex = LastSelectedStock.Data.FindIndex(candle => candle.Date.Date >= startDate);
        var endIndex = LastSelectedStock.Data.FindLastIndex(candle => candle.Date.Date <= endDate);
        startIndex = Math.Max(0, startIndex);
        endIndex = Math.Max(startIndex, endIndex);
        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        _selectedRangeStartIndex = startIndex;
        _selectedRangeEndIndex = endIndex;
        LoadAllCandles();
        RefreshChartSeries();
        IsCustomDateRangeVisible = false;
    }

    [RelayCommand]
    public void CancelCustomDateRange()
    {
        SelectedTimeRange = _lastStandardTimeRange;
        IsCustomDateRangeVisible = false;
    }

    public void CommitRangeSelection()
    {
        if (_isUpdatingRange || LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        if (LastSelectedStock.Data.Count == 1)
        {
            SetRangeSelection(0, 0);
            _chartRangeStartIndex = 0;
            _chartRangeEndIndex = 0;
            LoadAllCandles();
            return;
        }

        // Only updates the current chart view; the committed baseline (_selectedRangeStartIndex/End)
        // is left untouched so Reset can restore to it after a drag or zoom.
        var lastIndex = LastSelectedStock.Data.Count - 1;
        var startIndex = Math.Clamp((int)Math.Floor(RangeStart), 0, Math.Max(0, lastIndex - 1));
        var endIndex = Math.Clamp((int)Math.Ceiling(RangeEnd), startIndex + 1, lastIndex);
        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
        LoadAllCandles();
    }

    public void ResetZoomSelection()
    {
        RestoreCommittedRange();
    }

    public (int Start, int End) RestoreCommittedRange()
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return (0, 0);
        }

        _chartRangeStartIndex = _selectedRangeStartIndex;
        _chartRangeEndIndex = _selectedRangeEndIndex;
        SetRangeSelection(_chartRangeStartIndex, _chartRangeEndIndex);
        return (_chartRangeStartIndex, _chartRangeEndIndex);
    }

    public void ApplyZoomSelection(double zoomFactor, double zoomPosition)
    {
        if (_isUpdatingRange || LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        if (LastSelectedStock.Data.Count == 1)
        {
            return;
        }

        var lastIndex = LastSelectedStock.Data.Count - 1;
        var currentStart = Math.Clamp(_chartRangeStartIndex, 0, Math.Max(0, lastIndex - 1));
        var currentEnd = Math.Clamp(_chartRangeEndIndex, currentStart + 1, lastIndex);
        var currentSpan = Math.Max(1, currentEnd - currentStart);

        var visibleSpan = Math.Clamp(zoomFactor, 0, 1) * currentSpan;
        var start = currentStart + (Math.Clamp(zoomPosition, 0, 1) * currentSpan);
        var end = start + visibleSpan;
        var startIndex = Math.Clamp((int)Math.Round(start), currentStart, Math.Max(0, currentEnd - 1));
        var endIndex = Math.Clamp((int)Math.Round(end), startIndex + 1, currentEnd);

        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
    }

    public void ApplyVisibleRangeSelection(int startIndex, int endIndex)
    {
        if (LastSelectedStock is null || LastSelectedStock.Data.Count == 0)
        {
            return;
        }

        var lastIndex = LastSelectedStock.Data.Count - 1;
        startIndex = Math.Clamp(startIndex, 0, lastIndex);
        endIndex = Math.Clamp(endIndex, startIndex, lastIndex);
        SetRangeSelection(startIndex, endIndex);
        _chartRangeStartIndex = startIndex;
        _chartRangeEndIndex = endIndex;
    }

    private void SetRangeSelection(int startIndex, int endIndex)
    {
        _isUpdatingRange = true;
        RangeStart = startIndex;
        RangeEnd = endIndex;
        _isUpdatingRange = false;
    }

    private void LoadAllCandles()
    {
        VisibleCandles.Clear();
        foreach (var candle in LastSelectedStock!.Data)
        {
            VisibleCandles.Add(candle);
        }

    }
}
