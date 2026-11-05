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
        RenderSection();
        SizeChanged += OnSizeChanged;
    }

    private void RenderSection()
    {
        View pageContent = _viewModel.ActiveTab switch
        {
            "Counters" => new CountersPage(
                new CounterOperationsViewModel(_viewModel),
                _manageCounters,
                _openToken,
                _runTokenAction,
                _runBooleanAction,
                _openTransfer),
            _ => new TokensPage(new TokenManagementViewModel(_viewModel), _openToken)
        };
        PageContentHost.Content = new ScrollView { Content = pageContent };
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

        var horizontalPadding = Width >= 1100 ? 64 : 44;
        SectionTabs.WidthRequest = Math.Min(340, Math.Max(100, Width - horizontalPadding));
        OperationalStatus.IsVisible = Width >= 600;
        NavigationRow.Margin = new Thickness(Width >= 1100 ? 28 : 18, Width < 768 ? 12 : 22, Width >= 1100 ? 28 : 18, 0);
    }
}
