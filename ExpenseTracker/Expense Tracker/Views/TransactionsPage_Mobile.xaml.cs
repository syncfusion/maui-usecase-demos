using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Data;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataPager;
using Syncfusion.Maui.Popup;
using Syncfusion.Maui.TabView;
using System.Linq;

namespace SmartExpense;

public partial class TransactionsPage_Mobile : ContentView
{
    #region Private fields

    private bool _initialized;
    private bool _isInternalSelectionChange;
    private bool _isTransactionsTabActive;

    private TransactionsViewModel? ViewModel => BindingContext as TransactionsViewModel;

    #endregion

    #region Constructor

    public TransactionsPage_Mobile()
    {
        InitializeComponent();
        BindingContext = IPlatformApplication.Current?.Services
            .GetService(typeof(TransactionsViewModel));
    }

    #endregion

    #region Lifecycle methods

    /// <summary>
    /// Invoked when the native handler is created or destroyed.
    /// Used to perform one-time initialization that must occur
    /// after the native view hierarchy is ready.
    /// </summary>
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        _isTransactionsTabActive = Handler != null;

        // When native handler is destroyed, detach collection change listener to avoid leaks
        if (Handler is null)
        {
            if (ViewModel is not null)
            {
                try { ViewModel.SelectedTransactions.CollectionChanged -= SelectedTransactions_CollectionChanged_FromViewModel; } catch { }
            }
            return;
        }

        // Run once after native handler creation to avoid premature
        // Syncfusion initialization and unnecessary layout passes.
        if (_initialized)
            return;

        _initialized = true;

        // Defer one UI tick so initial measure/arrange completes
        Dispatcher.DispatchDelayed(
            TimeSpan.FromMilliseconds(1),
            async () =>
            {
                try
                {
                    if (ViewModel is null)
                        return;

                    ViewModel.IsBusy = true;

                    // Allow UI to render busy overlay before heavy work
                    await Task.Yield();

                    // Ensure Syncfusion segment items are created only after handler is ready
                    ViewModel.EnsureFinancialSegmentsInitialized();

                    if (_isTransactionsTabActive)
                    {
                        // Attach DataPager source exactly once after handler creation
                        if (dataPager != null)
                            dataPager.Source = ViewModel.FilteredTransactions;

                        if (ViewModel.LoadCommand is not null)
                            await ViewModel.LoadCommand.ExecuteAsync(null);
                        // Subscribe to ViewModel selection changes so checkbox-only toggles
                        // update grid.SelectedRows (row highlight).
                        try
                        {
                            ViewModel.SelectedTransactions.CollectionChanged += SelectedTransactions_CollectionChanged_FromViewModel;
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"TransactionsPage init error: {ex}");
                }
                finally
                {
                    if (ViewModel is not null)
                        ViewModel.IsBusy = false;
                }
            });
    }

    #endregion

    #region DataGrid event handlers

    private void OnDataGridSelectionChanged(
        object sender,
        DataGridSelectionChangedEventArgs e)
    {
        if (_isInternalSelectionChange)
            return;

        if (ViewModel is null)
            return;

        // Be defensive: Syncfusion may supply either Transaction
        // or DataGridRowInfo depending on configuration.
        foreach (var obj in e.AddedRows)
        {
            if (obj is Transaction t)
                t.IsSelected = true;
            else if (obj is DataGridRowInfo rowInfo && rowInfo.RowData is Transaction tx)
                tx.IsSelected = true;
        }

        foreach (var obj in e.RemovedRows)
        {
            if (obj is Transaction t)
                t.IsSelected = false;
            else if (obj is DataGridRowInfo rowInfo && rowInfo.RowData is Transaction tx)
                tx.IsSelected = false;
        }
    }

    private void OnDataGridCellValueChanged(
        object sender,
        DataGridCellValueChangedEventArgs e)
    {
        if (_isInternalSelectionChange)
            return;

        if (e.Column.MappingName != nameof(Transaction.IsSelected))
            return;

        if (sender is not SfDataGrid grid)
            return;

        if (e.RowData is not Transaction tx)
            return;

        bool isChecked = e.NewValue is bool b && b;

        try
        {
            // Prevent recursive selection sync between grid and ViewModel
            _isInternalSelectionChange = true;

            if (isChecked)
            {
                // Preserve existing multi-selection
                if (!grid.SelectedRows.Contains(tx))
                    grid.SelectedRows.Add(tx);
            }
            else
            {
                if (grid.SelectedRows.Contains(tx))
                    grid.SelectedRows.Remove(tx);
            }
        }
        finally
        {
            _isInternalSelectionChange = false;
        }
    }

    private void SelectedTransactions_CollectionChanged_FromViewModel(
        object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (dataGrid == null)
            return;

        try
        {
            _isInternalSelectionChange = true;

            if (e.NewItems is not null)
            {
                foreach (Transaction tx in e.NewItems.Cast<Transaction>())
                {
                    if (!dataGrid.SelectedRows.Contains(tx))
                        dataGrid.SelectedRows.Add(tx);
                }
            }

            if (e.OldItems is not null)
            {
                foreach (Transaction tx in e.OldItems.Cast<Transaction>())
                {
                    if (dataGrid.SelectedRows.Contains(tx))
                        dataGrid.SelectedRows.Remove(tx);
                }
            }
        }
        finally
        {
            _isInternalSelectionChange = false;
        }
    }

    #endregion

    #region Edit / navigation logic

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        try
        {
            if (ViewModel is null)
                return;

            // Ensure exactly one transaction is selected
            if (ViewModel.SelectedCount != 1)
                return;

            var tx = ViewModel.SelectedTransactions.FirstOrDefault();
            if (tx is null)
                return;

            var services = IPlatformApplication.Current?.Services as IServiceProvider;
            if (services is null)
                return;

            // Always create a fresh AddTransactionViewModel
            var vm = services.GetService(typeof(AddTransactionViewModel)) as AddTransactionViewModel;
            if (vm is null)
                return;

            // Mobile (Android / iOS): navigate to Add tab
            if (vm.LoadCategoriesCommand is not null && vm.Categories.Count == 0)
                await vm.LoadCategoriesCommand.ExecuteAsync(null);

            vm.LoadForEdit(tx);

            await NavigateToAddTabAndAssignBindingAsync(vm);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnEditClicked error: {ex}");
        }
    }

    #endregion

    #region Private helpers

    private async Task NavigateToAddTabAndAssignBindingAsync(AddTransactionViewModel vm)
    {
        try
        {
            // Navigate to AddTransactionPage directly instead of switching tabs.
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page as Page
                           ?? Application.Current?.MainPage;

            if (mainPage is null)
                return;

            // Ensure categories are loaded before navigating
            if (vm.LoadCategoriesCommand is not null && vm.Categories.Count == 0)
                await vm.LoadCategoriesCommand.ExecuteAsync(null);

            var addPage = new AddTransactionPage { BindingContext = vm };

            // Push the page onto the current navigation stack using the resolved nav instance
            try
            {
                var nav = mainPage.Navigation;
                if (nav is null)
                {
                    System.Diagnostics.Debug.WriteLine("Navigate PushAsync error: navigation instance is null");
                    return;
                }

                // When edit completes we must pop the pushed page using its Navigation instance
                async void OnEditCompleted()
                {
                    try
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            try { await addPage.Navigation.PopAsync(); } catch { }
                        });
                    }
                    finally
                    {
                        try { vm.EditCompleted -= OnEditCompleted; } catch { }
                    }
                }

                vm.EditCompleted += OnEditCompleted;

                await nav.PushAsync(addPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigate PushAsync error: {ex}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"NavigateToAddTabAndAssignBindingAsync error: {ex}");
        }
    }

    #endregion
}