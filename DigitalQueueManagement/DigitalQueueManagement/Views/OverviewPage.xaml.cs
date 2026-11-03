using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Syncfusion.Maui.Buttons;

namespace DigitalQueueManagement.Views;

public partial class OverviewPage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action<QueueToken> _openToken;
    private readonly Action<QueueCounter> _openCounter;

    public OverviewPage(
        QueueOverviewViewModel viewModel,
        Action<QueueToken> openToken,
        Action<QueueCounter> openCounter)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _viewModel = viewModel.QueueSession;
        _openToken = openToken;
        _openCounter = openCounter;

        UpcomingFilterControl.ItemsSource = new[] { "All", "Normal", "Priority" };
        UpcomingFilterControl.SetBinding(
            SfSegmentedControl.SelectedIndexProperty,
            new Binding($"{nameof(QueueOverviewViewModel.QueueSession)}.{nameof(QueueSessionViewModel.UpcomingFilterIndex)}", BindingMode.TwoWay));

        SizeChanged += OnSizeChanged;
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        if (sender is Border { BindingContext: QueueCounterSummary summary })
        {
            var counter = _viewModel.Counters.FirstOrDefault(item => item.Name == summary.Name);
            if (counter is not null)
            {
                _openCounter(counter);
            }
        }
    }

    private void OnTokenClicked(object? sender, EventArgs e)
    {
        if (sender is Border { BindingContext: QueueToken token })
        {
            _openToken(token);
        }
    }

    private void OnViewAllTokensClicked(object? sender, EventArgs e)
    {
        _viewModel.SetManageTab("Tokens");
        _viewModel.SetHeader("Manage");
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
        {
            return;
        }

        var wide = Width >= 1100;

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
        }

        MainContentGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection
            {
                new(new GridLength(0.9, GridUnitType.Star)),
                new(new GridLength(1.2, GridUnitType.Star))
            }
            : new ColumnDefinitionCollection { new(GridLength.Star) };
        MainContentGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) };
        MainContentGrid.SetColumn(UpcomingCard, wide ? 1 : 0);
        MainContentGrid.SetRow(UpcomingCard, wide ? 0 : 1);
        var mobile = Width < 768;
        PageContentLayout.Padding = new Thickness(wide ? 28 : 20, mobile ? 10 : 22, wide ? 28 : 20, mobile ? 24 : 36);
    }
}
