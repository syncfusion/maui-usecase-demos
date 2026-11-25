using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.Popup;

namespace SmartExpense;

/// <summary>
/// DashboardPage is a ContentView that serves as the main dashboard for the SmartExpense application. It displays an overview of the user's financial data, including recent transactions, budget summaries, and other relevant information. The page initializes with a fade-in animation and loads data asynchronously through its associated DashboardViewModel. This design ensures a smooth user experience while providing essential financial insights at a glance.
/// </summary>
public partial class DashboardPage : ContentView
{
    private DashboardViewModel? ViewModel => BindingContext as DashboardViewModel;
    private bool _initialized;
        private AddTransactionPage? _currentAddPage;

    public DashboardPage()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        BindingContext = IPlatformApplication.Current?.Services.GetService<DashboardViewModel>();
    }

    private void OnDesktopAddClicked(object? sender, EventArgs e)
    {
        try
        {
            var services = IPlatformApplication.Current?.Services;
            if (services is null) return;

            // Resolve a fresh AddTransactionViewModel from DI and provide a desktop-specific content view
            var vm = services.GetRequiredService<AddTransactionViewModel>();
            vm.ReturnTabIndex = 0; // origin: dashboard

            // Create content view for popup bound to the viewmodel
            var contentView = new AddTransactionPopupContent { BindingContext = vm };


            contentView.CancelCommand = new Command(() =>
            {
                AddTransactionPopup.IsOpen = false;
            });


            AddTransactionPopup.ContentTemplate = new DataTemplate(() => contentView);

            // Ensure categories are loaded when popup appears
            if (vm.LoadCategoriesCommand is not null && vm.Categories.Count == 0)
                _ = vm.LoadCategoriesCommand.ExecuteAsync(null);

            // Wire Accept / Decline commands to use the AddTransactionViewModel's Save behavior
            AddTransactionPopup.AcceptCommand = new Command(async () =>
            {
                try
                {
                    if (vm is not null)
                    {
                        if (vm.SaveCommand is IAsyncRelayCommand aasync && aasync.CanExecute(null))
                        {
                            await aasync.ExecuteAsync(null);
                        }
                        else if (vm.SaveCommand is IRelayCommand rc && rc.CanExecute(null))
                        {
                            rc.Execute(null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Add popup save error: {ex}");
                }
            });

            AddTransactionPopup.DeclineCommand = new Command(() => { AddTransactionPopup.IsOpen = false; });

            // Open popup
            AddTransactionPopup.IsOpen = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnDesktopAddClicked error: {ex}");
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null && !_initialized)
        {
            _initialized = true;

            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    // Run animation after initial layout; then load data
                    if (Opacity == 0)
                        await this.FadeToAsync(1, 320, Easing.CubicOut);

                    if (ViewModel?.LoadCommand is not null)
                    {
                        // Start load but don't await here so initial layout can render
                        // (avoids blocking the UI while the ViewModel performs async work)
                        await ViewModel.LoadCommand.ExecuteAsync(null);
                    }
                    // Register to close popup when transactions change
                    WeakReferenceMessenger.Default.Register<TransactionsChangedMessage>(this, (r, m) =>
                    {
                        try
                        {
                            Dispatcher.Dispatch(() =>
                            {
                                if (AddTransactionPopup is not null)
                                    AddTransactionPopup.IsOpen = false;
                            });
                        }
                        catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DashboardPage init error: {ex}");
                }
            });
        }
    }

    private void OnYAxisLabelCreated(object sender, ChartAxisLabelEventArgs e)
    {
        var value = e.Position; // THIS is the numeric axis value in MAUI

        if (value >= 1000)
        {
            e.Label = $"${value / 1000:0.#}K";
        }
        else
        {
            e.Label = $"${value:0}";
        }
    }


    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0)
            return;

        // Desktop charts responsive behavior
        HandleDesktopChartsLayout();
    }

    private void HandleDesktopChartsLayout()
    {
        if (DesktopChartsGrid == null)
            return;

        DesktopChartsGrid.RowDefinitions.Clear();
        DesktopChartsGrid.ColumnDefinitions.Clear();

        bool stackVertically = Width < 780;

        DesktopChartsGrid.RowSpacing = stackVertically ? 18 : 0;
        DesktopChartsGrid.ColumnSpacing = stackVertically ? 0 : 18;

        if (stackVertically)
        {
            // STACKED (Financial → Recent)
            DesktopChartsGrid.RowDefinitions.Add(
                new RowDefinition(GridLength.Auto));
            DesktopChartsGrid.RowDefinitions.Add(
                new RowDefinition(GridLength.Auto));

            DesktopChartsGrid.ColumnDefinitions.Add(
                new ColumnDefinition(GridLength.Star));

            Grid.SetRow(DesktopFinancialOverview, 0);
            Grid.SetColumn(DesktopFinancialOverview, 0);

            Grid.SetRow(DesktopRecentTransactions, 1);
            Grid.SetColumn(DesktopRecentTransactions, 0);
        }
        else
        {
            // SIDE-BY-SIDE
            DesktopChartsGrid.RowDefinitions.Add(
                new RowDefinition(GridLength.Auto));

            DesktopChartsGrid.ColumnDefinitions.Add(
                new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
            DesktopChartsGrid.ColumnDefinitions.Add(
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

            Grid.SetRow(DesktopFinancialOverview, 0);
            Grid.SetColumn(DesktopFinancialOverview, 0);

            Grid.SetRow(DesktopRecentTransactions, 0);
            Grid.SetColumn(DesktopRecentTransactions, 1);
        }
    }

    private void OnCategorySegmentSelectionChanged(object sender, Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
    {
        if (ViewModel is null) return;
        // Ensure ViewModel reflects the selection change (fallback for binding)
        if (e is null) return;
        // SelectionChangedEventArgs.NewIndex is nullable; guard against null and negative values
        var newIndex = e.NewIndex ?? -1;
        if (newIndex >= 0)
            ViewModel.SelectedCategorySegmentIndex = newIndex;
    }
}