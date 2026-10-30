using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using StockChart.Helpers;
using StockChart.ViewModels;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.Sliders;

namespace StockChart
{
    // View-side chart surface synchronization: series mutation, zoom/range application,
    // price-axis scaling, range-selector wiring, and settings applied to the chart.
    public partial class MainPage
    {
        private void OnChartSeriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_chartSeriesUpdateScheduled)
            {
                return;
            }

            _chartSeriesUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _chartSeriesUpdateScheduled = false;
                if (!_isPageActive || BindingContext is not StockChartViewModel viewModel || Handler is null)
                {
                    return;
                }

                var series = viewModel.ChartSeries.ToArray();
                StockChartView.SuspendSeriesNotification();
                try
                {
                    StockChartView.Series.Clear();
                    foreach (var chartSeries in series)
                    {
                        StockChartView.Series.Add(chartSeries);
                    }
                }
                catch (COMException)
                {
                    // The native chart can reject a collection mutation while its handler is detaching.
                    return;
                }
                finally
                {
                    StockChartView.ResumeSeriesNotification();
                }

                ResetChartZoomControl.IsVisible = false;
                UpdatePriceAxisRange(viewModel);
                ScheduleChartRangeApplication(viewModel);
            });
        }

        private void ScheduleChartRangeApplication(StockChartViewModel viewModel)
        {
            ApplyChartRangeWhenReady(viewModel, 0);
        }

        private void ApplyChartRangeWhenReady(StockChartViewModel viewModel, int attempt)
        {
            if (!_isPageActive || Handler is null || !ReferenceEquals(BindingContext, viewModel))
            {
                return;
            }

            if (StockChartView.Width > 0 &&
                StockChartView.Height > 0 &&
                StockChartView.Series.Count > 0 &&
                StockChartView.XAxes.Count > 0)
            {
                ApplyChartRange(viewModel);
            }

            if (attempt < 5)
            {
                var delay = TimeSpan.FromMilliseconds(50 * (attempt + 1));
                Dispatcher.DispatchDelayed(delay, () => ApplyChartRangeWhenReady(viewModel, attempt + 1));
            }
        }
        private void ApplyChartSettings(StockChartViewModel viewModel)
        {
            var isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;
            var borderColor = isDarkTheme ? Color.FromArgb("#334155") : Color.FromArgb("#E2E8F0");

            RangeAxisBase axis = viewModel.IsLogarithmicAxis
                ? new LogarithmicAxis()
                : new NumericalAxis();
            axis.Name = "PriceAxis";
            axis.ShowMajorGridLines = true;
            axis.MajorGridLineStyle = new ChartLineStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 0.8 };
            axis.AxisLineStyle = new ChartLineStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 1 };
            axis.MajorTickStyle = new ChartAxisTickStyle { Stroke = new SolidColorBrush(borderColor), StrokeWidth = 1 };
            if (viewModel.IsAxisOpposed)
            {
                axis.CrossesAt = double.MaxValue;
            }
            axis.LabelStyle = new ChartAxisLabelStyle
            {
                FontSize = 11,
                TextColor = isDarkTheme
                    ? Color.FromArgb("#CBD5E1")
                    : Color.FromArgb("#64748B")
            };

            if (axis is NumericalAxis numericalAxis)
            {
                numericalAxis.RangePadding = NumericalPadding.Auto;
                numericalAxis.LabelCreated += OnPriceAxisLabelCreated;
            }

            SafeChartOperation(() =>
            {
                foreach (var series in StockChartView.Series.OfType<ChartSeries>())
                {
                    series.EnableTooltip = viewModel.IsTooltipEnabled;
                }

                StockChartView.YAxes.Clear();
                StockChartView.YAxes.Add(axis);
                StockChartView.IsTransposed = viewModel.IsAxisInverted;
            });

            ChartRangeSelector.IsVisible = _isChartFullscreen && viewModel.IsRangeControlEnabled;

            viewModel.RefreshChart();
            UpdatePriceAxisRange(viewModel);
        }

        private void OnRangeSelectorLabelCreated(object? sender, SliderLabelCreatedEventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel ||
                !double.TryParse(e.Text, out var index) ||
                viewModel.LastSelectedStock is null)
            {
                return;
            }

            var dataIndex = Math.Clamp((int)Math.Round(index), 0, viewModel.LastSelectedStock.Data.Count - 1);
            e.Text = viewModel.LastSelectedStock.Data[dataIndex].Date.ToString("MMM yyyy");
        }

        private void OnRangeSelectorValueChanged(object? sender, RangeSelectorValueChangedEventArgs e)
        {
            if (_isSynchronizingChartRange ||
                BindingContext is not StockChartViewModel viewModel ||
                viewModel.LastSelectedStock is null ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            var lastIndex = viewModel.LastSelectedStock.Data.Count - 1;
            var startIndex = Math.Clamp((int)Math.Floor(e.NewRangeStart), 0, lastIndex);
            var endIndex = Math.Clamp((int)Math.Ceiling(e.NewRangeEnd), startIndex, lastIndex);

            _isSynchronizingChartRange = true;
            try
            {
                viewModel.ApplyVisibleRangeSelection(startIndex, endIndex);
                ResetChartZoomControl.IsVisible = true;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }

            // ZoomByRange and axis rescaling are the expensive part of this update; coalesce
            // rapid drag ticks into at most one native chart update per dispatcher cycle.
            ScheduleChartRangeUpdate(viewModel, startIndex, endIndex);
        }

        private void ScheduleChartRangeUpdate(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            _pendingChartRangeStart = startIndex;
            _pendingChartRangeEnd = endIndex;

            if (_chartRangeUpdateScheduled)
            {
                return;
            }

            _chartRangeUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _chartRangeUpdateScheduled = false;
                if (!_isPageActive || !ReferenceEquals(BindingContext, viewModel))
                {
                    return;
                }

                ApplyChartRange(viewModel, _pendingChartRangeStart, _pendingChartRangeEnd);
                UpdatePriceAxisRange(viewModel, _pendingChartRangeStart, _pendingChartRangeEnd);
            });
        }

        private void OnRangeSelectorValueChangeEnd(object? sender, EventArgs e)
        {
            if (_isSynchronizingChartRange || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                viewModel.CommitRangeSelection();
                ApplyChartRange(viewModel);
                UpdatePriceAxisRange(viewModel);
                // Keep any still-queued drag update (from ScheduleChartRangeUpdate) in sync with the commit
                // so it can't overwrite this final range with a stale mid-drag value.
                _pendingChartRangeStart = viewModel.ChartRangeStartIndex;
                _pendingChartRangeEnd = viewModel.ChartRangeEndIndex;
                ResetChartZoomControl.IsVisible = true;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnChartZoomEnd(object? sender, ChartZoomEventArgs e)
        {
            if (_isSynchronizingChartRange ||
                BindingContext is not StockChartViewModel viewModel ||
                StockChartView.XAxes.Count == 0 ||
                !ReferenceEquals(e.Axis, StockChartView.XAxes[0]))
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                var dateTimeAxis = (DateTimeAxis)e.Axis;
                var visibleRange = GetVisibleDataRange(viewModel, dateTimeAxis);
                viewModel.ApplyVisibleRangeSelection(visibleRange.Start, visibleRange.End);
                UpdatePriceAxisRange(viewModel, visibleRange.Start, visibleRange.End);
                ResetChartZoomControl.IsVisible = true;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnResetChartZoomClicked(object? sender, EventArgs e)
        {
            if (_isSynchronizingChartRange || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            _isSynchronizingChartRange = true;
            try
            {
                var selectedRange = viewModel.RestoreCommittedRange();
                ChartRangeSelector.RangeStart = selectedRange.Start;
                ChartRangeSelector.RangeEnd = selectedRange.End;
                ApplyChartRange(viewModel, selectedRange.Start, selectedRange.End);
                UpdatePriceAxisRange(viewModel, selectedRange.Start, selectedRange.End);
                ResetChartZoomControl.IsVisible = false;
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void OnTrendlineVisibilityClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: TrendlineViewModel overlay })
            {
                viewModel.ToggleTrendline(overlay);
            }
        }

        private void OnTrendlineRemoveClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: TrendlineViewModel overlay })
            {
                viewModel.RemoveTrendline(overlay);
            }
        }

        private void ApplyChartRange(StockChartViewModel viewModel)
        {
            if (viewModel.LastSelectedStock is null ||
                StockChartView.XAxes.FirstOrDefault() is not DateTimeAxis dateTimeAxis ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            ApplyChartRange(viewModel, viewModel.ChartRangeStartIndex, viewModel.ChartRangeEndIndex);
        }

        private void ApplyChartRange(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            if (viewModel.LastSelectedStock is null ||
                StockChartView.XAxes.FirstOrDefault() is not DateTimeAxis dateTimeAxis ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            startIndex = Math.Clamp(startIndex, 0, viewModel.LastSelectedStock.Data.Count - 1);
            endIndex = Math.Clamp(endIndex, startIndex, viewModel.LastSelectedStock.Data.Count - 1);
            var rangeStartDate = viewModel.LastSelectedStock.Data[startIndex].Date;
            var rangeEndDate = viewModel.LastSelectedStock.Data[endIndex].Date;
            _isSynchronizingChartRange = true;
            try
            {
                SafeChartOperation(() => StockChartView.ZoomPanBehavior.ZoomByRange(dateTimeAxis, rangeStartDate, rangeEndDate));
            }
            finally
            {
                _isSynchronizingChartRange = false;
            }
        }

        private void UpdatePriceAxisRange(StockChartViewModel viewModel)
        {
            UpdatePriceAxisRange(viewModel, viewModel.ChartRangeStartIndex, viewModel.ChartRangeEndIndex);
        }

        private void UpdatePriceAxisRange(StockChartViewModel viewModel, int startIndex, int endIndex)
        {
            if (StockChartView.YAxes.FirstOrDefault(axis => axis.Name == "PriceAxis") is not NumericalAxis numericalAxis ||
                viewModel.LastSelectedStock is null ||
                viewModel.LastSelectedStock.Data.Count == 0)
            {
                return;
            }

            var data = viewModel.LastSelectedStock.Data;
            startIndex = Math.Clamp(startIndex, 0, data.Count - 1);
            endIndex = Math.Clamp(endIndex, startIndex, data.Count - 1);

            var minimum = double.MaxValue;
            var maximum = double.MinValue;
            for (var i = startIndex; i <= endIndex; i++)
            {
                var candle = data[i];
                if (candle.Low < minimum) minimum = candle.Low;
                if (candle.High > maximum) maximum = candle.High;
            }

            var padding = Math.Max((maximum - minimum) * 0.05, 0.01);
            var axisMinimum = Math.Max(0.01, minimum - padding);
            var axisMaximum = maximum + padding;

            SafeChartOperation(() =>
            {
                numericalAxis.Minimum = axisMinimum;
                numericalAxis.Maximum = axisMaximum;
                numericalAxis.RangePadding = NumericalPadding.None;
            });
        }

        private void UpdatePriceAxisRangeFromVisibleAxis(StockChartViewModel viewModel, DateTimeAxis dateTimeAxis)
        {
            var visibleRange = GetVisibleDataRange(viewModel, dateTimeAxis);
            UpdatePriceAxisRange(viewModel, visibleRange.Start, visibleRange.End);
        }

        private static (int Start, int End) GetVisibleDataRange(StockChartViewModel viewModel, DateTimeAxis dateTimeAxis)
        {
            if (viewModel.LastSelectedStock is null || viewModel.LastSelectedStock.Data.Count == 0)
            {
                return (0, 0);
            }

            var data = viewModel.LastSelectedStock.Data;
            var visibleStart = DateTime.FromOADate(dateTimeAxis.VisibleMinimum);
            var visibleEnd = DateTime.FromOADate(dateTimeAxis.VisibleMaximum);
            var startIndex = data.FindIndex(candle => candle.Date >= visibleStart);
            var endIndex = data.FindLastIndex(candle => candle.Date <= visibleEnd);
            startIndex = Math.Clamp(startIndex < 0 ? 0 : startIndex, 0, data.Count - 1);
            endIndex = Math.Clamp(endIndex < 0 ? data.Count - 1 : endIndex, startIndex, data.Count - 1);
            return (startIndex, endIndex);
        }

        private void OnPriceAxisLabelCreated(object? sender, ChartAxisLabelEventArgs e)
        {
            if (double.TryParse(e.Label, out var value))
            {
                e.Label = $"${value:N0}";
            }
        }
    }
}
