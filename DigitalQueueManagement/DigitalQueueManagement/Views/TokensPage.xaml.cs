using DigitalQueueManagement.Model;
using DigitalQueueManagement.ViewModel;
namespace DigitalQueueManagement.Views;

public partial class TokensPage : ContentView
{
    private readonly QueueSessionViewModel _viewModel;
    private readonly Action<QueueToken> _openToken;
    private readonly Action<QueueToken?> _openTokenForm;

    public TokensPage(
        TokenManagementViewModel viewModel,
        Action<QueueToken> openToken,
        Action<QueueToken?> openTokenForm)
    {
        BindingContext = viewModel;
        InitializeComponent();

        _viewModel = viewModel.QueueSession;
        _openToken = openToken;
        _openTokenForm = openTokenForm;

        SizeChanged += OnSizeChanged;
    }

    private void OnCreateTokenClicked(object? sender, EventArgs e) =>
        _openTokenForm(null);

    private void OnStatusFilterTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string filter)
        {
            _viewModel.SetStatusFilter(filter);
        }
    }

    private void OnTokenTapped(object? sender, TappedEventArgs e)
    {
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
            ? new ColumnDefinitionCollection { new(270), new(GridLength.Star) }
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
