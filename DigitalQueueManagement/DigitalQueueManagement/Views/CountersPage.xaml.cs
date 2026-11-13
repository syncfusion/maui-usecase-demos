using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Syncfusion.Maui.Cards;

namespace DigitalQueueManagement.Views;

public partial class CountersPage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action _manageCounters;
    private readonly Action<QueueToken> _openToken;
    private readonly Action<Func<QueueToken?>> _runTokenAction;
    private readonly Action<Func<bool>> _runBooleanAction;
    private readonly Action<string> _openTransfer;
    private readonly IDispatcherTimer _servingDurationTimer;
    private bool _completeServiceSucceeded;

    public CountersPage(
        CounterOperationsViewModel viewModel,
        Action manageCounters,
        Action<QueueToken> openToken,
        Action<Func<QueueToken?>> runTokenAction,
        Action<Func<bool>> runBooleanAction,
        Action<string> openTransfer)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _viewModel = viewModel.QueueSession;
        _manageCounters = manageCounters;
        _openToken = openToken;
        _runTokenAction = runTokenAction;
        _runBooleanAction = runBooleanAction;
        _openTransfer = openTransfer;
        _completeServiceSucceeded = _viewModel.IsSelectedCounterServiceCompleted;

        _servingDurationTimer = Dispatcher.CreateTimer();
        _servingDurationTimer.Interval = TimeSpan.FromSeconds(1);
        _servingDurationTimer.Tick += OnServingDurationTimerTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnCounterTapped(object? sender, TappedEventArgs e)
    {
        if (sender is SfCardView { BindingContext: QueueCounterSummary summary })
        {
            _viewModel.SelectCounter(summary.Name);
            _completeServiceSucceeded = _viewModel.IsSelectedCounterServiceCompleted;
            UpdateCompleteButtonState();
        }
    }

    private void OnManageCountersClicked(object? sender, EventArgs e) =>
        _manageCounters();

    public void ScrollToCounter(string counterName)
    {
        Dispatcher.Dispatch(() =>
        {
            var index = -1;
            for (var rowIndex = 0; rowIndex < _viewModel.CounterSummaries.Count; rowIndex++)
            {
                if (_viewModel.CounterSummaries[rowIndex].Name == counterName)
                {
                    index = rowIndex;
                    break;
                }
            }

            if (index >= 0)
            {
                CounterListView.ItemsLayout.ScrollToRowIndex(index, ScrollToPosition.End, false);
            }
        });
    }

    private void OnViewTokenClicked(object? sender, EventArgs e)
    {
        if (_viewModel.SelectedCounterToken is { } token)
        {
            _openToken(token);
        }
    }

    private void OnCallNextClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            _runTokenAction(() => _viewModel.CallNext(counter.Name));
        }
    }

    private void OnCompleteClicked(object? sender, EventArgs e)
    {
        _completeServiceSucceeded = false;
        UpdateCompleteButtonState();
        if (_viewModel.SelectedCounter is { } counter)
        {
            var completed = false;
            _runBooleanAction(() => completed = _viewModel.CompleteCurrent(counter.Name));
            _completeServiceSucceeded = completed;
            UpdateCompleteButtonState();
        }
    }

    private void OnSkipClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            _runBooleanAction(() => _viewModel.SkipCurrent(counter.Name));
        }
    }

    private void OnRecallClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            _runBooleanAction(() => _viewModel.RecallCurrent(counter.Name));
        }
    }

    private void OnTransferClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            _openTransfer(counter.Name);
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
        {
            return;
        }

        var compact = Width < 768;
        var wide = Width >= 1100;
        var threeColumn = Width >= 1280;
        var tablet = Width >= 768;
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

        WorkspaceGrid.ColumnDefinitions = threeColumn
            ? new ColumnDefinitionCollection
            {
                new(new GridLength(1, GridUnitType.Star)),
                new(new GridLength(2, GridUnitType.Star)),
                new(new GridLength(1, GridUnitType.Star))
            }
            : wide
                ? new ColumnDefinitionCollection { new(320), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) };
        WorkspaceGrid.RowDefinitions = threeColumn
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : wide
                ? new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
                : compact
                    ? new RowDefinitionCollection
                    {
                        new(GridLength.Auto),
                        new(GridLength.Auto),
                        new(GridLength.Auto),
                        new(GridLength.Auto)
                    }
                    : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) };
        WorkspaceGrid.SetColumn(CounterListPanel, 0);
        WorkspaceGrid.SetRow(CounterListPanel, 0);
        CounterListPanel.IsVisible = !compact;
        MobileCounterSelectorPanel.IsVisible = compact;
        WorkspaceGrid.SetColumn(MobileCounterSelectorPanel, 0);
        WorkspaceGrid.SetRow(MobileCounterSelectorPanel, 0);
        MobileQueueSummaryPanel.IsVisible = compact;
        WorkspaceGrid.SetColumn(MobileQueueSummaryPanel, 0);
        WorkspaceGrid.SetRow(MobileQueueSummaryPanel, 2);
        WorkspaceGrid.SetColumn(OperationsPanel, wide ? 1 : 0);
        WorkspaceGrid.SetRow(OperationsPanel, wide ? 0 : 1);
        WorkspaceGrid.SetColumn(QueueStatusPanel, threeColumn ? 2 : wide ? 1 : 0);
        WorkspaceGrid.SetRow(QueueStatusPanel, threeColumn ? 0 : wide ? 1 : compact ? 3 : 2);
        PageContentLayout.Spacing = compact ? 14 : 18;
        PageContentLayout.Padding = compact
            ? new Thickness(16, 16, 16, 24)
            : new Thickness(wide ? 28 : 18, 22, wide ? 28 : 18, 36);
        OperationsPanel.Padding = compact ? 0 : 18;
        OperationsPanel.BackgroundColor = compact ? Colors.Transparent : Colors.White;
        OperationsPanel.StrokeThickness = compact ? 0 : 1;
        MobileCurrentTokenPanel.Padding = compact ? 16 : 0;
        MobileCurrentTokenPanel.BackgroundColor = compact ? Colors.White : Colors.Transparent;
        MobileCurrentTokenPanel.Stroke = compact ? Color.FromArgb("#DCE7F5") : Colors.Transparent;
        MobileCurrentTokenPanel.StrokeThickness = compact ? 1 : 0;
        MobileCounterActionsPanel.Padding = compact ? 14 : 0;
        MobileCounterActionsPanel.BackgroundColor = compact ? Colors.White : Colors.Transparent;
        MobileCounterActionsPanel.Stroke = compact ? Color.FromArgb("#DCE7F5") : Colors.Transparent;
        MobileCounterActionsPanel.StrokeThickness = compact ? 1 : 0;
        CounterActionsDivider.IsVisible = compact;
        SelectedTokenIdLabel.TextColor = compact ? Color.FromArgb("#102A8C") : Color.FromArgb("#0F172A");
        SelectedTokenIdLabel.FontSize = 58;
        InitialWaitLabel.Text = compact ? _viewModel.SelectedCounterTokenWait : "3m 12s";
        CallNextButton.Text = compact ? "Call Next" : "Call next";
        CallNextButton.FontSize = 16;
        CallNextButton.MinimumHeightRequest = compact ? 76 : 50;
        CallNextButton.BackgroundColor = compact ? Color.FromArgb("#102A8C") : Color.FromArgb("#153F9A");
        CallNextButton.ImageSource = new FontImageSource
        {
            Glyph = compact ? "volume_up" : "north_east",
            FontFamily = "MaterialIcons",
            Color = Colors.White,
            Size = compact ? 18 : 16
        };
        CompleteButton.Text = compact ? "Complete\nService" : "Complete service";
        CompleteButton.FontSize = 16;
        CompleteButton.MinimumHeightRequest = compact ? 76 : 50;
        CompleteButton.BackgroundColor = Color.FromArgb("#153F9A");
        CompleteButton.ImageSource = new FontImageSource
        {
            Glyph = compact ? "check_circle" : "verified_user",
            FontFamily = "MaterialIcons",
            Color = Colors.White,
            Size = compact ? 18 : 16
        };
        SkipButton.Text = compact ? "Skip Token" : "Skip token";
        RecallButton.Text = compact ? "Recall Token" : "Recall token";
        TransferButton.Text = compact ? "Transfer Token" : "Transfer token";
        SkipButton.FontSize = compact ? 12 : 14;
        RecallButton.FontSize = compact ? 12 : 14;
        TransferButton.FontSize = compact ? 12 : 14;
        SkipButton.ImageSource = compact
            ? new FontImageSource { Glyph = "fast_forward", FontFamily = "MaterialIcons", Color = Color.FromArgb("#C62828"), Size = 16 }
            : null;
        RecallButton.ImageSource = compact
            ? new FontImageSource { Glyph = "campaign", FontFamily = "MaterialIcons", Color = Color.FromArgb("#183E91"), Size = 16 }
            : null;
        TransferButton.ImageSource = compact
            ? new FontImageSource { Glyph = "swap_horiz", FontFamily = "MaterialIcons", Color = Color.FromArgb("#9A5A00"), Size = 16 }
            : null;
        QueueStatusPanel.Padding = compact ? 14 : 16;
        UpdateCompleteButtonState();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _viewModel.RefreshServingDuration();
        _servingDurationTimer.Start();
    }

    private void OnUnloaded(object? sender, EventArgs e) =>
        _servingDurationTimer.Stop();

    private void OnServingDurationTimerTick(object? sender, EventArgs e) =>
        _viewModel.RefreshServingDuration();

    private void ResetCompleteServiceFeedback()
    {
        _completeServiceSucceeded = false;
        UpdateCompleteButtonState();
    }

    private void UpdateCompleteButtonState()
    {
        CompleteButton.Text = _completeServiceSucceeded
            ? "Completed"
            : Width < 768
                ? "Complete\nService"
                : "Complete service";
        CompleteButton.TextColor = Colors.White;
        CompleteButton.BackgroundColor = _completeServiceSucceeded
            ? Color.FromArgb("#16A34A")
            : Color.FromArgb("#153F9A");
    }
}
