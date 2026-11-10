using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
namespace DigitalQueueManagement.Views;

public partial class TokensPage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action<QueueToken> _openToken;
    private bool? _isMobileLayout;

    public TokensPage(
        TokenManagementViewModel viewModel,
        Action<QueueToken> openToken)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _viewModel = viewModel.QueueSession;
        _openToken = openToken;

        SizeChanged += OnSizeChanged;
    }

    private void OnPageTapped(object? sender, TappedEventArgs e)
    {
        if (FilterPanel.IsVisible && IsTapOutsideSearchBorder(e, DesktopSearchBorder))
        {
            DesktopSearchEntry.Unfocus();
        }

        if (MobileTokenFilterPanel.IsVisible && IsTapOutsideSearchBorder(e, MobileSearchBorder))
        {
            MobileSearchEntry.Unfocus();
        }
    }

    private static bool IsTapOutsideSearchBorder(TappedEventArgs e, Border searchBorder)
    {
        var position = e.GetPosition(searchBorder);
        return position is { } point
            && (point.X < 0 || point.Y < 0 || point.X > searchBorder.Width || point.Y > searchBorder.Height);
    }

    private void UnfocusSearchEntries()
    {
        DesktopSearchEntry.Unfocus();
        MobileSearchEntry.Unfocus();
    }

    private void OnStatusFilterTapped(object? sender, TappedEventArgs e)
    {
        UnfocusSearchEntries();

        if (e.Parameter is string filter)
        {
            _viewModel.SetStatusFilter(filter);
        }
    }

    private void OnTokenTapped(object? sender, TappedEventArgs e)
    {
        UnfocusSearchEntries();

        if (sender is Border { BindingContext: QueueTokenListItem item })
        {
            _openToken(item.Token);
        }
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
        {
            return;
        }

        var wide = Width >= 1100;
        var mobile = Width < 768;
        if (_isMobileLayout != mobile)
        {
            _isMobileLayout = mobile;
            BindableLayout.SetItemsSource(
                MobileTokenList,
                mobile ? _viewModel.VisibleTokenRows : null);
        }

        MetricGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }
            : !mobile
                ? new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }
                : new ColumnDefinitionCollection { new(GridLength.Star) };
        MetricGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : !mobile
                ? new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
                : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) };

        for (var index = 0; index < MetricGrid.Children.Count; index++)
        {
            MetricGrid.SetColumn(MetricGrid.Children[index], wide ? index : !mobile ? index % 2 : 0);
            MetricGrid.SetRow(MetricGrid.Children[index], wide ? 0 : !mobile ? index / 2 : index);
        }

        MetricGrid.IsVisible = !mobile;
        FilterPanel.IsVisible = !mobile;
        MobileTokenFilterPanel.IsVisible = mobile;
        DirectoryHeader.IsVisible = !mobile;
        DesktopTokenListHost.IsVisible = !mobile;
        MobileTokenContent.IsVisible = mobile;

        DirectoryGrid.ColumnDefinitions = wide
            ? new ColumnDefinitionCollection { new(336), new(GridLength.Star) }
            : new ColumnDefinitionCollection { new(GridLength.Star) };
        DirectoryGrid.RowDefinitions = wide
            ? new RowDefinitionCollection { new(GridLength.Auto) }
            : new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) };
        DirectoryGrid.SetColumn(FilterPanel, 0);
        DirectoryGrid.SetRow(FilterPanel, 0);
        DirectoryGrid.SetColumn(MobileTokenFilterPanel, 0);
        DirectoryGrid.SetRow(MobileTokenFilterPanel, 0);
        DirectoryGrid.SetColumn(DirectoryPanel, wide ? 1 : 0);
        DirectoryGrid.SetRow(DirectoryPanel, wide ? 0 : 1);
        DirectoryPanel.Padding = mobile ? 0 : 18;
        DirectoryPanel.BackgroundColor = mobile ? Colors.Transparent : Colors.White;
        DirectoryPanel.StrokeThickness = mobile ? 0 : 1;
        PageContentLayout.Padding = new Thickness(wide ? 28 : 18, mobile ? 16 : 22, wide ? 28 : 18, mobile ? 24 : 36);
    }
}
