using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Charts;

namespace DigitalQueueManagement.Views;

public partial class OverviewPage : ContentView
{
    private bool _counterCardHeightBindingsApplied = true;

    public OverviewPage(QueueOverviewViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        UpcomingFilterControl.ItemsSource = new[] { "All", "Normal", "Priority" };
        UpcomingFilterControl.SetBinding(
            SfSegmentedControl.SelectedIndexProperty,
            new Binding(nameof(QueueSessionViewModel.UpcomingFilterIndex), BindingMode.TwoWay));

        ServiceChartHost.Content = CreateDoughnutChart(
            viewModel.QueueSession.ServiceAnalytics,
            viewModel.QueueSession.ServiceAnalytics
                .Select(point => (Brush)new SolidColorBrush(point.ChartColor))
                .ToList());

        SizeChanged += OnSizeChanged;
    }

    private static SfCircularChart CreateDoughnutChart(
        IReadOnlyList<QueueAnalyticsPoint> points,
        List<Brush> palette)
    {
        var chart = new SfCircularChart
        {
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        chart.Series.Add(new DoughnutSeries
        {
            ItemsSource = points,
            XBindingPath = nameof(QueueAnalyticsPoint.Category),
            YBindingPath = nameof(QueueAnalyticsPoint.Value),
            InnerRadius = 0.68,
            PaletteBrushes = palette
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
        var dashboardWide = Width >= 1280;

        if (compact && _counterCardHeightBindingsApplied)
        {
            CounterCard.RemoveBinding(HeightRequestProperty);
            CounterStatusCard.RemoveBinding(HeightRequestProperty);
            _counterCardHeightBindingsApplied = false;
        }
        else if (!compact && !_counterCardHeightBindingsApplied)
        {
            CounterCard.SetBinding(
                HeightRequestProperty,
                new Binding(nameof(QueueSessionViewModel.CounterPerformanceCardHeight)));
            CounterStatusCard.SetBinding(
                HeightRequestProperty,
                new Binding(nameof(QueueSessionViewModel.CounterStatusCardHeight)));
            _counterCardHeightBindingsApplied = true;
        }

        DashboardGrid.ColumnDefinitions = dashboardWide
            ? new ColumnDefinitionCollection
            {
                new(new GridLength(3, GridUnitType.Star)),
                new(new GridLength(2, GridUnitType.Star))
            }
            : new ColumnDefinitionCollection { new(GridLength.Star) };
        DashboardGrid.RowDefinitions = dashboardWide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) };
        DashboardGrid.SetColumn(OperationalColumn, dashboardWide ? 1 : 0);
        DashboardGrid.SetRow(OperationalColumn, dashboardWide ? 0 : 1);

        MetricGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }
            : new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) };
        MetricGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) };

        for (var index = 0; index < MetricGrid.Children.Count; index++)
        {
            MetricGrid.SetColumn(MetricGrid.Children[index], wide ? index : index % 2);
            MetricGrid.SetRow(MetricGrid.Children[index], wide ? 0 : index / 2);
            if (MetricGrid.Children[index] is View card)
            {
                card.HeightRequest = compact ? 164 : 174;
            }
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
                new(GridLength.Auto)
            };

        var cards = new[] { ServiceCard, WaitCard, CounterCard };
        for (var index = 0; index < cards.Length; index++)
        {
            ChartGrid.SetColumn(cards[index], wide && index < 2 ? index : 0);
            ChartGrid.SetRow(cards[index], wide && index < 2 ? 0 : wide ? 1 : index);
        }
        Grid.SetColumnSpan(CounterCard, wide ? 2 : 1);

        var cardPadding = compact ? new Thickness(16) : new Thickness(24);
        ServiceCard.Padding = cardPadding;
        WaitCard.Padding = cardPadding;
        ServiceCard.MinimumHeightRequest = compact ? 300 : wide ? 350 : -1;
        WaitCard.MinimumHeightRequest = compact ? 300 : wide ? 350 : -1;
        ServiceAnalyticsPanel.Padding = compact ? new Thickness(12) : new Thickness(16);
        WaitAnalyticsPanel.Padding = compact ? new Thickness(14) : new Thickness(20);
        ServiceAnalyticsPanel.MinimumHeightRequest = compact || wide ? 212 : -1;
        WaitAnalyticsPanel.MinimumHeightRequest = compact || wide ? 212 : -1;
        PageContentLayout.Padding = compact
            ? new Thickness(16, 16, 16, 24)
            : new Thickness(wide ? 28 : 18, 22, wide ? 28 : 18, 36);
    }
}
