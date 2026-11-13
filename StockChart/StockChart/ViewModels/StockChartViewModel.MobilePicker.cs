using CommunityToolkit.Mvvm.Input;

namespace StockChart.ViewModels;

// Mobile bottom-sheet picker state for time-range / chart-type / trendline selection.
public partial class StockChartViewModel
{
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
            "TimeRange" => AvailableTimeRanges,
            "ChartType" => AvailableChartTypes,
            "Trendline" => AvailableTrendlines,
            _ => []
        })
        {
            MobilePickerItems.Add(new MobilePickerItem(item)
            {
                IsSelected = string.Equals(item, kind switch
                {
                    "TimeRange" => SelectedTimeRange,
                    "ChartType" => SelectedChartType,
                    "Trendline" => SelectedTrendline,
                    _ => string.Empty
                }, StringComparison.Ordinal)
            });
        }

        IsMobilePickerVisible = MobilePickerItems.Count > 0;
    }

    public void SelectMobilePickerItem(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        switch (MobilePickerKind)
        {
            case "TimeRange": SelectedTimeRange = value; break;
            case "ChartType": SelectedChartType = value; break;
            case "Trendline": SelectedTrendline = value; break;
        }

        if (MobilePickerKind == "TimeRange" && value == "Custom")
        {
            PrepareCustomDateRange();
            IsCustomDateRangeVisible = true;
        }

        IsMobilePickerVisible = false;
    }

    [RelayCommand]
    private void CloseMobilePicker() => IsMobilePickerVisible = false;

}
