using StockChart.ViewModels;

namespace StockChart
{
    // Mobile drawer gestures and responsive (fullscreen / breakpoint) layout.
    public partial class MainPage
    {
        private void OnMobileStockDrawerTapped(object? sender, TappedEventArgs e)
        {
            if (_mobileStockDrawerPanMoved)
            {
                return;
            }

            ToggleMobileStockDrawer();
        }

        private void OnMobileStockDrawerPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            if (BindingContext is not StockChartViewModel { LastSelectedStock: not null } || _isChartFullscreen)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _mobileStockDrawerPanMoved = false;
                    StockPanel.IsVisible = true;
                    _mobileStockDrawerPanStartHeight = StockPanel.Height;
                    break;
                case GestureStatus.Running:
                    _mobileStockDrawerPanMoved = Math.Abs(e.TotalY) > 4;
                    var maxHeight = Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap);
                    _mobileStockDrawerPendingHeight = Math.Clamp(
                        _mobileStockDrawerPanStartHeight - e.TotalY,
                        MobileStockDrawerPreviewHeight,
                        maxHeight);
                    ScheduleMobileStockDrawerHeightUpdate();
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                    SnapMobileStockDrawer();
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () => _mobileStockDrawerPanMoved = false);
                    break;
            }
        }

        // Coalesces rapid pan-gesture ticks into at most one layout pass per dispatcher cycle.
        private void ScheduleMobileStockDrawerHeightUpdate()
        {
            if (_mobileStockDrawerPanUpdateScheduled)
            {
                return;
            }

            _mobileStockDrawerPanUpdateScheduled = true;
            Dispatcher.Dispatch(() =>
            {
                _mobileStockDrawerPanUpdateScheduled = false;
                StockPanel.HeightRequest = _mobileStockDrawerPendingHeight;
            });
        }

        private void ToggleMobileStockDrawer()
        {
            if (BindingContext is not StockChartViewModel { LastSelectedStock: not null } || _isChartFullscreen)
            {
                return;
            }

            SetMobileStockDrawerExpanded(!_mobileStockDrawerExpanded, true);
        }

        private void SnapMobileStockDrawer()
        {
            var maxHeight = Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap);
            var expanded = StockPanel.Height > (MobileStockDrawerPreviewHeight + maxHeight) / 2;
            SetMobileStockDrawerExpanded(expanded, true);
        }

        private void SetMobileStockDrawerExpanded(bool expanded, bool animate)
        {
            _mobileStockDrawerExpanded = expanded;
            var target = expanded
                ? Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap)
                : MobileStockDrawerPreviewHeight;
            StockPanel.IsVisible = true;

            if (!animate)
            {
                StockPanel.HeightRequest = target;
                return;
            }

            this.AbortAnimation("MobileStockDrawer");
            var animation = new Animation(
                value => StockPanel.HeightRequest = value,
                StockPanel.Height,
                target);
            animation.Commit(this, "MobileStockDrawer", 16, 220, Easing.CubicOut);
        }

        private void UpdateMobileStockDrawerLayout()
        {
            var isMobile = Width > 0 && Width < MobileBreakpoint;
            var hasSelectedStock = BindingContext is StockChartViewModel { LastSelectedStock: not null };
            if (!isMobile || _isChartFullscreen)
            {
                this.AbortAnimation("MobileStockDrawer");
                StockPanel.ClearValue(VisualElement.HeightRequestProperty);
                MobileStockDrawerHandle.IsVisible = false;
                return;
            }

            StockPanel.IsVisible = true;
            MobileStockDrawerHandle.IsVisible = hasSelectedStock;
            StockPanel.HeightRequest = _mobileStockDrawerExpanded
                ? Math.Max(MobileStockDrawerPreviewHeight, WorkspaceGrid.Height - MobileStockDrawerGap)
                : MobileStockDrawerPreviewHeight;
        }

        private void OnFullscreenClicked(object? sender, EventArgs e)
        {
            _isChartFullscreen = !_isChartFullscreen;
            ApplyResponsiveLayout();
        }
        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            ApplyResponsiveLayout();
        }

        private void OnWorkspaceSizeChanged(object? sender, EventArgs e)
        {
            if (Width > 0 && Width < MobileBreakpoint && !_isChartFullscreen)
            {
                UpdateMobileStockDrawerLayout();
                Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), UpdateMobileStockDrawerLayout);
            }
        }

        private void ApplyResponsiveLayout()
        {
            var isMobile = Width > 0 && Width < MobileBreakpoint;
            if (BindingContext is StockChartViewModel toolbarViewModel)
            {
                toolbarViewModel.IsMobileToolbar = isMobile;
            }

            if (_isChartFullscreen)
            {
                HeaderGrid.IsVisible = false;
                StockPanel.IsVisible = false;
                MobileStockDrawerHandle.IsVisible = false;
                MainContentGrid.Padding = new Thickness(12);
                MainContentGrid.RowSpacing = 0;
                WorkspaceGrid.ColumnDefinitions.Clear();
                WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                Grid.SetRow(StockPanel, 0);
                Grid.SetColumn(ChartPanel, 0);
                FullscreenButton.Text = "\uE5D1";
                SemanticProperties.SetDescription(FullscreenButton, "Exit chart fullscreen");
                MobileFullscreenButton.Text = "\uE5D1";
                SemanticProperties.SetDescription(MobileFullscreenButton, "Exit chart fullscreen");
                ChartRangeSelector.IsVisible = BindingContext is StockChartViewModel viewModel && viewModel.IsRangeControlEnabled;
                return;
            }

            HeaderGrid.IsVisible = true;
            MainContentGrid.Padding = isMobile
                ? new Thickness((double)Application.Current!.Resources["PagePadding"], 20, (double)Application.Current!.Resources["PagePadding"], 4)
                : new Thickness((double)Application.Current!.Resources["PagePadding"]);
            MainContentGrid.RowSpacing = 16;
            FullscreenButton.Text = "\uE5D0";
            SemanticProperties.SetDescription(FullscreenButton, "Enter chart fullscreen");
            MobileFullscreenButton.Text = "\uE5D0";
            SemanticProperties.SetDescription(MobileFullscreenButton, "Enter chart fullscreen");
            ChartRangeSelector.IsVisible = false;
            StockPanel.IsVisible = true;

            if (isMobile)
            {
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                WorkspaceGrid.RowSpacing = MobileStockDrawerGap;
                Grid.SetRow(ChartPanel, 0);
                Grid.SetRow(StockPanel, 1);
                StockPanel.Margin = new Thickness(0);
                WatchlistSelectorBorder.WidthRequest = 160;
                WatchlistSelector.HorizontalOptions = LayoutOptions.Start;
                WatchlistSelector.DropdownWidth = Math.Max(200, Width - 100);
            }
            else
            {
                WorkspaceGrid.RowDefinitions.Clear();
                WorkspaceGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                WorkspaceGrid.RowSpacing = 0;
                Grid.SetRow(ChartPanel, 0);
                Grid.SetRow(StockPanel, 0);
                StockPanel.Margin = new Thickness(0);
                WatchlistSelectorBorder.WidthRequest = 160;
                WatchlistSelector.HorizontalOptions = LayoutOptions.Start;
                WatchlistSelector.DropdownWidth = 160;
            }

            WorkspaceGrid.ColumnDefinitions.Clear();
            WorkspaceGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (!isMobile)
            {
                WorkspaceGrid.ColumnDefinitions.Insert(0, new ColumnDefinition(new GridLength(360)));
                Grid.SetColumn(StockPanel, 0);
                Grid.SetColumn(ChartPanel, 1);
            }
            else
            {
                Grid.SetColumn(ChartPanel, 0);
            }

            UpdateMobileStockDrawerLayout();
        }
    }
}
