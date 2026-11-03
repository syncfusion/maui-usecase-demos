using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Syncfusion.Maui.Charts;

namespace DigitalQueueManagement.Views;

public partial class InsightsPage : ContentView
{
    private static readonly Color Cobalt = Color.FromArgb("#183E91");
    private static readonly Color Teal = Color.FromArgb("#159B85");
    private static readonly Color Slate500 = Color.FromArgb("#64748B");

    public InsightsPage(QueueInsightsViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        MobileServiceChartHost.Content = CreateServiceChart(viewModel.QueueSession.ServiceAnalytics);
        FulfillmentChartHost.Content = CreateDoughnutChart(
            viewModel.QueueSession.FulfillmentAnalytics,
            [
                new SolidColorBrush(Teal),
                new SolidColorBrush(Color.FromArgb("#DCE8F5"))
            ]);
        CounterChartHost.Content = CreateCounterChart(viewModel.QueueSession.CounterAnalytics);

        SizeChanged += OnSizeChanged;
    }

    private static SfCircularChart CreateDoughnutChart(
        IReadOnlyList<QueueAnalyticsPoint> points,
        List<Brush> palette)
    {
        var chart = new SfCircularChart
        {
            HeightRequest = 160,
            BackgroundColor = Colors.Transparent
        };
        chart.Series.Add(new DoughnutSeries
        {
            ItemsSource = points,
            XBindingPath = nameof(QueueAnalyticsPoint.Category),
            YBindingPath = nameof(QueueAnalyticsPoint.Value),
            InnerRadius = 0.72,
            PaletteBrushes = palette
        });
        return chart;
    }

    private static SfCartesianChart CreateServiceChart(IReadOnlyList<QueueAnalyticsPoint> points)
    {
        var chart = new SfCartesianChart
        {
            HeightRequest = 180,
            BackgroundColor = Colors.Transparent
        };
        chart.XAxes.Add(new CategoryAxis
        {
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 9 }
        });
        chart.YAxes.Add(new NumericalAxis
        {
            Minimum = 0,
            Maximum = 30,
            Interval = 10,
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 8 }
        });
        chart.Series.Add(new ColumnSeries
        {
            ItemsSource = points,
            XBindingPath = nameof(QueueAnalyticsPoint.Category),
            YBindingPath = nameof(QueueAnalyticsPoint.Value),
            PaletteBrushes = points.Select(point => (Brush)new SolidColorBrush(point.ChartColor)).ToList(),
            Width = 0.6,
            Spacing = 0.25,
            ShowDataLabels = true
        });
        return chart;
    }

    private static SfCartesianChart CreateCounterChart(IReadOnlyList<QueueAnalyticsPoint> points)
    {
        var chart = new SfCartesianChart
        {
            HeightRequest = 180,
            BackgroundColor = Colors.Transparent
        };
        chart.XAxes.Add(new CategoryAxis
        {
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 9 }
        });
        chart.YAxes.Add(new NumericalAxis
        {
            LabelStyle = new ChartAxisLabelStyle { TextColor = Slate500, FontSize = 9 }
        });
        chart.Series.Add(new ColumnSeries
        {
            ItemsSource = points,
            XBindingPath = nameof(QueueAnalyticsPoint.Category),
            YBindingPath = nameof(QueueAnalyticsPoint.Value),
            Fill = Cobalt,
            PaletteBrushes =
            [
                new SolidColorBrush(Cobalt),
                new SolidColorBrush(Teal),
                new SolidColorBrush(Color.FromArgb("#7589D4"))
            ],
            Width = 0.6,
            Spacing = 0.25,
            ShowDataLabels = true
        });
        return chart;
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
        {
            return;
        }

        var compact = Width < 768;
        var wide = Width >= 1100;
        var tablet = Width >= 760;
        MobileOverviewCard.IsVisible = compact;
        MetricGrid.IsVisible = !compact;
        MetricGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }
            : tablet
                ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) };
        MetricGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : tablet
                ? new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
                : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) };

        for (var index = 0; index < MetricGrid.Children.Count; index++)
        {
            MetricGrid.SetColumn(MetricGrid.Children[index], wide ? index : tablet ? index % 2 : 0);
            MetricGrid.SetRow(MetricGrid.Children[index], wide ? 0 : tablet ? index / 2 : index);
        }

        ChartGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }
            : new ColumnDefinitionCollection { new(GridLength.Star) };
        ChartGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
            : new RowDefinitionCollection
            {
                new(GridLength.Auto),
                new(GridLength.Auto),
                new(GridLength.Auto),
                new(GridLength.Auto)
            };

        var cards = compact
            ? new[] { ServiceCard, WaitCard, CounterCard, FulfillmentCard }
            : new[] { ServiceCard, WaitCard, FulfillmentCard, CounterCard };
        for (var index = 0; index < cards.Length; index++)
        {
            ChartGrid.SetColumn(cards[index], wide ? index % 2 : 0);
            ChartGrid.SetRow(cards[index], wide ? index / 2 : index);
        }

        MobileServiceChartHost.IsVisible = compact;
        MobileServiceLegend.IsVisible = compact;
        DesktopServiceDistributionBar.IsVisible = !compact;
        DesktopServiceBreakdown.IsVisible = !compact;
        DesktopServiceTotal.IsVisible = !compact;
        PageContentLayout.Padding = compact
            ? new Thickness(16, 16, 16, 24)
            : new Thickness(wide ? 28 : 18, 22, wide ? 28 : 18, 36);
    }
}
