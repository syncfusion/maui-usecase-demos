using Microsoft.Maui.Graphics;
using StockChart.Models;
using Syncfusion.Maui.Charts;

namespace StockChart.ViewModels;

// Series construction: builds the SfCartesianChart series and trendline overlays
// for the selected chart type.
public partial class StockChartViewModel
{
    private void RefreshChartSeries()
    {
        ChartSeries.Clear();

        if (LastSelectedStock is null)
        {
            return;
        }

        ChartSeries series = SelectedChartType switch
        {
            "Area" => CreateValueSeries(new AreaSeries()),
            "Column" => CreateValueSeries(new ColumnSeries()),
            "Line" => CreateValueSeries(new LineSeries()),
            "Step Area" => CreateValueSeries(new StepAreaSeries()),
            "Step Line" => CreateValueSeries(new StepLineSeries()),
            "Range Column" => new RangeColumnSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                EnableTooltip = IsTooltipEnabled
            },
            "HiLo" or "OHLC" => new HiLoOpenCloseSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableTooltip = IsTooltipEnabled
            },
            "Hollow candle" => new CandleSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableSolidCandle = false,
                BullishFill = Color.FromArgb("#16A34A"),
                BearishFill = Color.FromArgb("#DC2626"),
                Width = 0.75,
                Spacing = 0.15,
                EnableTooltip = IsTooltipEnabled
            },
            _ => new CandleSeries
            {
                ItemsSource = VisibleCandles,
                XBindingPath = nameof(CandleDataModel.Date),
                Open = nameof(CandleDataModel.Open),
                High = nameof(CandleDataModel.High),
                Low = nameof(CandleDataModel.Low),
                Close = nameof(CandleDataModel.Close),
                EnableSolidCandle = true,
                BullishFill = Color.FromArgb("#16A34A"),
                BearishFill = Color.FromArgb("#DC2626"),
                Width = 0.75,
                Spacing = 0.15,
                EnableTooltip = IsTooltipEnabled
            }
        };

        if (series is CartesianSeries cartesianSeries)
        {
            cartesianSeries.YAxisName = "PriceAxis";
        }

        ChartSeries.Add(series);

        if (series is CartesianSeries trendlineHost)
        {
            foreach (var overlay in ActiveTrendlines)
            {
                var trendline = CreateTrendline(overlay.Name);
                trendline.IsVisible = overlay.IsVisible;
                trendlineHost.Trendlines.Add(trendline);
            }
        }

    }

    private XYDataSeries CreateValueSeries(XYDataSeries series)
    {
        series.ItemsSource = VisibleCandles;
        series.XBindingPath = nameof(CandleDataModel.Date);
        series.YBindingPath = nameof(CandleDataModel.Close);
        series.EnableTooltip = IsTooltipEnabled;
        return series;
    }

    private ChartTrendline CreateTrendline(string name)
    {
        ChartTrendline trendline = name switch
        {
            "Linear" => new LinearTrendline(),
            "Exponential" => new ExponentialTrendline(),
            "Logarithmic" => new LogarithmicTrendline(),
            "Polynomial" => new PolynomialTrendline { Order = 3 },
            "Power" => new PowerTrendline(),
            "Moving Average" => new MovingAverageTrendline { Period = 5 },
            _ => new LinearTrendline()
        };

        trendline.Stroke = new SolidColorBrush(Color.FromArgb("#2196F3"));
        trendline.StrokeWidth = 2;
        trendline.IsVisible = true;
        return trendline;
    }

    public void ToggleTrendline(TrendlineViewModel? overlay)
    {
        if (overlay is null)
        {
            return;
        }

        overlay.IsVisible = !overlay.IsVisible;
        RefreshChartSeries();
    }

    public void RemoveTrendline(TrendlineViewModel? overlay)
    {
        if (overlay is null)
        {
            return;
        }

        ActiveTrendlines.Remove(overlay);
        RefreshChartSeries();
    }
}
