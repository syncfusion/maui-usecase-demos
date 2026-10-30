using System.ComponentModel;
using System.Runtime.InteropServices;
using StockChart.Helpers;
using StockChart.Services;
using StockChart.ViewModels;
using Syncfusion.Maui.Popup;

namespace StockChart
{
    public partial class MainPage : ContentPage
    {
        private const double MobileBreakpoint = 800;
        private bool _disclaimerShown;
        private bool _pageLoaded;
        private bool _isPageActive;
        private bool _isChartFullscreen;
        private bool _chartSeriesUpdateScheduled;
        private bool _isSynchronizingChartRange;
        private bool _customDateRangePopupOpen;
        private bool _windowActivated;
        private bool _mobileStockDrawerExpanded;
        private bool _mobileStockDrawerPanMoved;
        private double _mobileStockDrawerPanStartHeight;
        private bool _mobileStockDrawerPanUpdateScheduled;
        private double _mobileStockDrawerPendingHeight;
        private bool _chartRangeUpdateScheduled;
        private int _pendingChartRangeStart;
        private int _pendingChartRangeEnd;

        private const double MobileStockDrawerPreviewHeight = 52;
        private const double MobileStockDrawerGap = 16;

        private SfPopup SettingsPopup => (SfPopup)Resources["SettingsPopup"];
        private SfPopup DisclaimerPopup => (SfPopup)Resources["DisclaimerPopup"];
        private SfPopup InformationPopup => (SfPopup)Resources["InformationPopup"];
        private SfPopup AddStockPopup => (SfPopup)Resources["AddStockPopup"];
        private SfPopup NewWatchlistPopup => (SfPopup)Resources["NewWatchlistPopup"];
        private SfPopup EditWatchlistPopup => (SfPopup)Resources["EditWatchlistPopup"];
        private SfPopup WatchlistMenuPopup => (SfPopup)Resources["WatchlistMenuPopup"];
        private SfPopup DeleteWatchlistPopup => (SfPopup)Resources["DeleteWatchlistPopup"];
        private SfPopup FavoritePopup => (SfPopup)Resources["FavoritePopup"];
        private SfPopup RemoveStockPopup => (SfPopup)Resources["RemoveStockPopup"];
        private VisualElement ResetChartZoomControl => RootPage.FindByName<VisualElement>("ResetChartZoomButton");

        public MainPage()
        {
            InitializeComponent();
            var viewModel = new StockChartViewModel(new StockDataService());
            BindingContext = viewModel;
            SettingsPopup.BindingContext = viewModel;
            DisclaimerPopup.BindingContext = viewModel;
            InformationPopup.BindingContext = viewModel;
            AddStockPopup.BindingContext = viewModel;
            NewWatchlistPopup.BindingContext = viewModel;
            EditWatchlistPopup.BindingContext = viewModel;
            WatchlistMenuPopup.BindingContext = viewModel;
            DeleteWatchlistPopup.BindingContext = viewModel;
            FavoritePopup.BindingContext = viewModel;
            RemoveStockPopup.BindingContext = viewModel;
            viewModel.ChartSeries.CollectionChanged += OnChartSeriesChanged;
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            SizeChanged += OnPageSizeChanged;
            Loaded += OnPageLoaded;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _isPageActive = true;
            if (Window is not null)
            {
                Window.Activated += OnWindowActivated;
            }

            if (BindingContext is StockChartViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }

            if (_windowActivated)
            {
                ShowDisclaimerIfReady();
            }
        }

        protected override void OnDisappearing()
        {
            _isPageActive = false;
            if (Window is not null)
            {
                Window.Activated -= OnWindowActivated;
            }

            base.OnDisappearing();
            _disclaimerShown = false;
            DismissPopups();
        }

        private void DismissPopups()
        {
            // OnDisappearing sets _isPageActive to false before this cleanup runs.
            // Do not use SafePopupOperation here because it intentionally rejects
            // operations on inactive pages and would leave native popups attached.
            DismissPopup(SettingsPopup);
            DismissPopup(DisclaimerPopup);
            DismissPopup(InformationPopup);
            DismissPopup(AddStockPopup);
            DismissPopup(NewWatchlistPopup);
            DismissPopup(EditWatchlistPopup);
            DismissPopup(WatchlistMenuPopup);
            DismissPopup(DeleteWatchlistPopup);
            DismissPopup(FavoritePopup);
            DismissPopup(RemoveStockPopup);
            _customDateRangePopupOpen = false;
        }

        private static void DismissPopup(SfPopup popup)
        {
            try
            {
                popup.Dismiss();
            }
            catch (InvalidOperationException)
            {
                // The native popup may already be detached during shutdown.
            }
            catch (COMException)
            {
                // WinUI can reject cleanup after the native popup has closed.
            }
        }

        // Native chart/axis mutations can be rejected mid-teardown (e.g. navigating away or
        // switching stocks while a drag/zoom is still in flight); swallow and log rather than crash.
        private static void SafeChartOperation(Action operation)
        {
            try
            {
                operation();
            }
            catch (InvalidOperationException exception)
            {
                System.Diagnostics.Trace.TraceWarning($"Chart operation rejected: {exception.Message}");
            }
            catch (COMException exception)
            {
                System.Diagnostics.Trace.TraceWarning($"Chart operation rejected: {exception.Message}");
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StockChartViewModel.LastSelectedStock) &&
                Width > 0 && Width < MobileBreakpoint &&
                !_isChartFullscreen)
            {
                Dispatcher.Dispatch(() =>
                {
                    if (BindingContext is StockChartViewModel { LastSelectedStock: not null } && !_isChartFullscreen)
                    {
                        _mobileStockDrawerExpanded = false;
                        StockPanel.IsVisible = true;
                        ChartPanel.IsVisible = true;
                        UpdateMobileStockDrawerLayout();
                    }
                });
            }

            if (sender is StockChartViewModel changedViewModel &&
                e.PropertyName == nameof(StockChartViewModel.IsCustomDateRangeVisible) &&
                !changedViewModel.IsCustomDateRangeVisible)
            {
                _customDateRangePopupOpen = false;
            }

            if (e.PropertyName == nameof(StockChartViewModel.SelectedTimeRange) &&
                sender is StockChartViewModel viewModel &&
                viewModel.SelectedTimeRange == "Custom" &&
                CanUsePopup &&
                !_customDateRangePopupOpen)
            {
                ShowCustomDateRangePopup(viewModel);
            }
        }
        private void OnPageLoaded(object? sender, EventArgs e)
        {
            _pageLoaded = true;
            if (_windowActivated)
            {
                ShowDisclaimerIfReady();
            }
        }

        private void OnWindowActivated(object? sender, EventArgs e)
        {
            _windowActivated = true;
            ShowDisclaimerIfReady();
        }

        private void ShowDisclaimerIfReady()
        {
            if (CanUsePopup && !_disclaimerShown)
            {
                // Width can still be 0/-1 the first time this runs (before the page's initial
                // layout pass); fall back to the device idiom so phones never get the compact
                // desktop-sized popup.
                var isMobile = Width > 0
                    ? Width < MobileBreakpoint
                    : DeviceInfo.Current.Idiom == DeviceIdiom.Phone;

                DisclaimerPopup.WidthRequest = isMobile && Width > 0 ? Width : isMobile ? 420 : 680;
                // Size to the message content rather than a fraction of the screen height,
                // otherwise the card is left with a large empty area below the text.
                DisclaimerPopup.HeightRequest = isMobile ? 300 : 200;
                PopupPresentation.ApplyDesktopCardStyle(DisclaimerPopup);
                Dispatcher.Dispatch(() =>
                {
                    if (CanUsePopup && !_disclaimerShown)
                    {
                        SafePopupOperation(() => DisclaimerPopup.Show());
                        _disclaimerShown = true;
                    }
                });
            }
        }

        private void OnInformationClicked(object? sender, EventArgs e)
        {
            if (CanUsePopup && sender is View informationButton && informationButton.Handler is not null)
            {
                if (Width >= MobileBreakpoint)
                {
                    InformationPopup.WidthRequest = 350;
                    InformationPopup.HeightRequest = 220;
                    PopupPresentation.ApplyDesktopCardStyle(InformationPopup);
                }
                else
                {
                    InformationPopup.WidthRequest = Math.Max(280, Width - 32);
                    InformationPopup.HeightRequest = 220;
                    InformationPopup.PopupStyle.CornerRadius = 12;
                }
                SafePopupOperation(() => InformationPopup.ShowRelativeToView(informationButton, PopupRelativePosition.AlignBottomRight, 0, -20));
            }
        }

        private void OnMobileTimeRangeClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("TimeRange");
        }

        private void OnMobileChartTypeClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("ChartType");
        }

        private void OnMobileTrendlineClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel) viewModel.OpenMobilePicker("Trendline");
        }

        private void OnMobilePickerItemClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && sender is Element { BindingContext: MobilePickerItem item })
            {
                viewModel.SelectMobilePickerItem(item.Value);
            }
        }
    }
}
