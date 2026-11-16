using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Cards;

namespace DigitalQueueManagement.Views;

public partial class CountersPage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action _manageCounters;
    private readonly Action<QueueToken> _openToken;
    private readonly Func<Func<QueueToken?>, Task> _runTokenAction;
    private readonly Func<Func<bool>, Task> _runBooleanAction;
    private readonly Func<string, Task> _openTransfer;
    private readonly IDispatcherTimer _servingDurationTimer;
    private bool _completeServiceSucceeded;

    public CountersPage(
        CounterOperationsViewModel viewModel,
        Action manageCounters,
        Action<QueueToken> openToken,
        Func<Func<QueueToken?>, Task> runTokenAction,
        Func<Func<bool>, Task> runBooleanAction,
        Func<string, Task> openTransfer)
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

            var itemsLayout = CounterListView?.ItemsLayout;
            if (index >= 0 && itemsLayout is not null)
            {
                itemsLayout.ScrollToRowIndex(index, ScrollToPosition.End, false);
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

    private async void OnCallNextClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            await _runTokenAction(() => _viewModel.CallNext(counter.Name));
        }
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        _completeServiceSucceeded = false;
        UpdateCompleteButtonState();
        if (_viewModel.SelectedCounter is { } counter)
        {
            var completed = false;
            await _runBooleanAction(() => completed = _viewModel.CompleteCurrent(counter.Name));
            _completeServiceSucceeded = completed;
            UpdateCompleteButtonState();
        }
    }

    private async void OnSkipClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            await _runBooleanAction(() => _viewModel.SkipCurrent(counter.Name));
        }
    }

    private async void OnRecallClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            await _runBooleanAction(() => _viewModel.RecallCurrent(counter.Name));
        }
    }

    private async void OnTransferClicked(object? sender, EventArgs e)
    {
        ResetCompleteServiceFeedback();
        if (_viewModel.SelectedCounter is { } counter)
        {
            await _openTransfer(counter.Name);
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
        var workspaceCardMinimumHeight = compact ? -1 : 450;
        CounterListPanel.MinimumHeightRequest = workspaceCardMinimumHeight;
        OperationsPanel.MinimumHeightRequest = workspaceCardMinimumHeight;
        QueueStatusPanel.MinimumHeightRequest = workspaceCardMinimumHeight;

        PageContentLayout.Spacing = compact ? 14 : 18;
        PageContentLayout.Padding = compact
            ? new Thickness(16, 16, 16, 24)
            : new Thickness(wide ? 28 : 18, 22, wide ? 28 : 18, 36);
        OperationsPanel.Padding = compact ? 0 : 18;
        OperationsPanel.BackgroundColor = compact ? Colors.Transparent : Colors.White;
        //OperationsPanel.BorderWidth = compact ? 0 : 1;
        MobileCurrentTokenPanel.Padding = compact ? 16 : 0;
        MobileCurrentTokenPanel.BackgroundColor = compact ? Colors.White : Colors.Transparent;
        //MobileCurrentTokenPanel.BorderColor = compact ? Color.FromArgb("#DCE7F5") : Colors.Transparent;
        //MobileCurrentTokenPanel.BorderWidth = compact ? 1 : 0;
        MobileCounterActionsPanel.Padding = compact ? 14 : 0;
        MobileCounterActionsPanel.BackgroundColor = compact ? Colors.White : Colors.Transparent;
        //MobileCounterActionsPanel.BorderColor = compact ? Color.FromArgb("#DCE7F5") : Colors.Transparent;
        //MobileCounterActionsPanel.BorderWidth = compact ? 1 : 0;
        CounterActionsDivider.IsVisible = compact;
        SelectedTokenIdLabel.TextColor = compact ? Color.FromArgb("#102A8C") : Color.FromArgb("#0F172A");
        SelectedTokenIdLabel.FontSize = 58;

        CallNextButton.Text = compact ? "Call Next" : "Call next";
        CallNextButton.FontSize = 16;
        CallNextButton.MinimumHeightRequest = compact ? 76 : 50;
        CallNextButton.SetDynamicResource(SfButton.BackgroundProperty, compact ? "ActionPrimaryMobileBrush" : "ActionPrimaryBrush");
        CallNextButton.ImageSource = new FontImageSource
        {
            Glyph = compact ? "volume_up" : "north_east",
            FontFamily = "MaterialIcons",
            Color = Colors.White,
            Size = compact ? 18 : 16
        };
        //CallNextButton.ShowIcon = true;
        //CallNextButton.ImageSize = compact ? 18 : 16;
        CompleteButton.Text = compact ? "Complete\nService" : "Complete service";
        CompleteButton.FontSize = 16;
        CompleteButton.MinimumHeightRequest = compact ? 76 : 50;
        CompleteButton.ImageSource = new FontImageSource
        {
            Glyph = compact ? "check_circle" : "verified_user",
            FontFamily = "MaterialIcons",
            Color = Colors.White,
            Size = compact ? 18 : 16
        };
        //CompleteButton.ShowIcon = true;
        //CompleteButton.ImageSize = compact ? 18 : 16;
        SkipButton.Text = compact ? "Skip Token" : "Skip token";
        RecallButton.Text = compact ? "Recall Token" : "Recall token";
        TransferButton.Text = compact ? "Transfer Token" : "Transfer token";
        SkipButton.FontSize = compact ? 12 : 14;
        RecallButton.FontSize = compact ? 12 : 14;
        TransferButton.FontSize = compact ? 12 : 14;
        if (compact)
        {
            SkipButton.ImageSource = new FontImageSource { Glyph = "fast_forward", FontFamily = "MaterialIcons", Color = Color.FromArgb("#C62828"), Size = 16 };
            RecallButton.ImageSource = new FontImageSource { Glyph = "campaign", FontFamily = "MaterialIcons", Color = Color.FromArgb("#183E91"), Size = 16 };
            TransferButton.ImageSource = new FontImageSource { Glyph = "swap_horiz", FontFamily = "MaterialIcons", Color = Color.FromArgb("#9A5A00"), Size = 16 };
        }
        //SkipButton.ShowIcon = compact;
        //SkipButton.ImageSize = 16;
        //RecallButton.ShowIcon = compact;
        //RecallButton.ImageSize = 16;
        //TransferButton.ShowIcon = compact;
        //TransferButton.ImageSize = 16;
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
        CompleteButton.SetDynamicResource(
            SfButton.BackgroundProperty,
            _completeServiceSucceeded ? "ActionSuccessBrush" : "ActionPrimaryBrush");
    }
}
