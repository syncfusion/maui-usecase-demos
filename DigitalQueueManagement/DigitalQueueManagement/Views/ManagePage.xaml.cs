using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;

namespace DigitalQueueManagement.Views;

public partial class ManagePage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action<QueueToken> _openToken;
    private readonly Action _manageCounters;
    private readonly Action<Func<QueueToken?>> _runTokenAction;
    private readonly Action<Func<bool>> _runBooleanAction;
    private readonly Action<string> _openTransfer;
    private ScrollView? _tokensScrollHost;
    private ScrollView? _countersScrollHost;

    public ManagePage(
        ManagePageViewModel viewModel,
        Action<QueueToken> openToken,
        Action manageCounters,
        Action<Func<QueueToken?>> runTokenAction,
        Action<Func<bool>> runBooleanAction,
        Action<string> openTransfer)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _viewModel = viewModel.QueueSession;
        _openToken = openToken;
        _manageCounters = manageCounters;
        _runTokenAction = runTokenAction;
        _runBooleanAction = runBooleanAction;
        _openTransfer = openTransfer;

        SectionTabs.ItemsSource = new[] { "Tokens", "Counters" };
        SetActiveSection();
        SizeChanged += OnSizeChanged;
    }

    public void SetActiveSection()
    {
        var scrollHost = _viewModel.ActiveTab switch
        {
            "Counters" => _countersScrollHost ??= new ScrollView
            {
                Content = new CountersPage(
                    new CounterOperationsViewModel(_viewModel),
                    _manageCounters,
                    _openToken,
                    _runTokenAction,
                    _runBooleanAction,
                    _openTransfer)
            },
            _ => _tokensScrollHost ??= new ScrollView
            {
                Content = new TokensPage(new TokenManagementViewModel(_viewModel), _openToken)
            }
        };

        if (!ReferenceEquals(PageContentHost.Content, scrollHost))
        {
            PageContentHost.Content = scrollHost;
        }
    }

    public void ScrollToCounter(string counterName)
    {
        var countersPage = PageContentHost.Content switch
        {
            CountersPage directPage => directPage,
            ScrollView { Content: CountersPage wrappedPage } => wrappedPage,
            _ => null
        };
        if (countersPage is not null)
        {
            countersPage.ScrollToCounter(counterName);
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
        {
            return;
        }

        var mobile = Width < 768;
        NavigationRow.ColumnDefinitions = mobile
            ? new ColumnDefinitionCollection
            {
                new(GridLength.Star),
                new(GridLength.Auto),
                new(GridLength.Star)
            }
            : new ColumnDefinitionCollection
            {
                new(GridLength.Auto),
                new(GridLength.Star)
            };
        Grid.SetColumn(SectionTabsHost, mobile ? 1 : 0);
        Grid.SetColumn(OperationalStatus, mobile ? 2 : 1);
        SectionTabsHost.HorizontalOptions = mobile ? LayoutOptions.Center : LayoutOptions.Start;

        var horizontalPadding = Width >= 1100 ? 64 : 44;
        SectionTabs.WidthRequest = Math.Min(340, Math.Max(100, Width - horizontalPadding));
        OperationalStatus.IsVisible = Width >= 600;
        NavigationRow.Margin = new Thickness(Width >= 1100 ? 28 : 18, Width < 768 ? 12 : 22, Width >= 1100 ? 28 : 18, 0);
    }
}
