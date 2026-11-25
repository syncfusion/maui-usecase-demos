using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.DataPager;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SmartExpense;

/// <summary>
/// ViewModel responsible for presenting, filtering, paging, selecting, and mutating
/// transactions displayed in the Transactions view.
/// 
/// This ViewModel is tightly coupled to Syncfusion SfDataGrid and DataPager behaviors,
/// including multi-selection, checkbox selection, and on-demand paging. All logic here
/// intentionally preserves reactive bindings and selection synchronization semantics.
/// </summary>
public partial class TransactionsViewModel : BaseViewModel
{
    #region Private fields

    private readonly ITransactionService _transactionService;
    private readonly IAppStateService _appState;

    private bool _suppressSelectionSync;
    private bool _suppressAllVisibleSync;
    private bool _suppressPeriodSync;
    private int _lastKnownTransactionCount;
    private bool _hasSelection;
    private bool _isBusy;
    private bool _allVisibleSelected;

    #endregion

    #region Constructor

    public TransactionsViewModel(ITransactionService transactionService, IAppStateService appState)
    {
        _transactionService = transactionService;
        _appState = appState;

        // React to master transaction collection changes
        _appState.Transactions.CollectionChanged += (_, _) =>
        {
            if (!IsBusy)
            {
                _ = LoadAsync();
            }
        };

        // React to cross-view transaction updates
        WeakReferenceMessenger.Default.Register<TransactionsChangedMessage>(this, async (_, _) =>
        {
            await LoadCommand.ExecuteAsync(null);
        });

        // Default date range: current month
        FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        ToDate = DateTime.Now.Date;

        // Period options mirrored from Dashboard
        PeriodOptions = new ObservableCollection<string>
        {
            "This Month",
            "Last Month",
            "This Year",
            "Last Year",
            "Last 6 Month"
        };

        SelectedFinancialPeriod = PeriodOptions.ElementAtOrDefault(0) ?? "This Month";
        SelectedAccountPeriodIndex = 2;

        // Track grid selection changes
        SelectedTransactions.CollectionChanged += SelectedTransactions_CollectionChanged;
    }

    #endregion

    #region Public observable properties

    /// <summary>
    /// Available predefined financial periods for filtering.
    /// </summary>
    public ObservableCollection<string> PeriodOptions { get; }

    [ObservableProperty]
    private string selectedFinancialPeriod = string.Empty;

    [ObservableProperty]
    private int selectedAccountPeriodIndex;

    /// <summary>
    /// Selected financial segment index (0=All, 1=Income, 2=Expense).
    /// </summary>
    [ObservableProperty]
    private int selectedSegmentIndex;

    /// <summary>
    /// Tracks selected transactions from the DataGrid.SelectedItems binding.
    /// </summary>
    public ObservableCollection<Transaction> SelectedTransactions { get; } = new();

    /// <summary>
    /// Indicates whether any rows are currently selected.
    /// </summary>
    public bool HasSelection
    {
        get => _hasSelection;
        set => SetProperty(ref _hasSelection, value);
    }

    /// <summary>
    /// Indicates whether the ViewModel is currently loading or mutating data.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
        }
    }

    /// <summary>
    /// Toggles selection of all currently visible (paged) rows.
    /// </summary>
    public bool AllVisibleSelected
    {
        get => _allVisibleSelected;
        set
        {
            if (SetProperty(ref _allVisibleSelected, value))
            {
                OnAllVisibleSelectedChanged(value);
            }
        }
    }

    /// <summary>
    /// Shared master transaction collection (single source of truth).
    /// </summary>
    public ObservableCollection<Transaction> Transactions => _appState.Transactions;

    /// <summary>
    /// Filtered transaction set used as the DataPager and DataGrid source.
    /// </summary>
    public ObservableCollection<Transaction> FilteredTransactions { get; } = new();

    [ObservableProperty]
    private DateTime filterMonth = DateTime.Now;

    [ObservableProperty]
    private DateTime fromDate;

    [ObservableProperty]
    private DateTime toDate;

    [ObservableProperty]
    private Transaction? selectedTransaction;

    /// <summary>
    /// Current page’s materialized transaction subset.
    /// </summary>
    public ObservableCollection<Transaction> PagedTransactions { get; } = new();

    [ObservableProperty]
    private int pageSize = 10;

    [ObservableProperty]
    private int currentPageIndex;

    [ObservableProperty]
    private int selectedIndex = -1;

    [ObservableProperty]
    private int pageCount;

    /// <summary>
    /// Financial segment UI items (initialized lazily by the View).
    /// </summary>
    public ObservableCollection<SfSegmentItem> FinancialSegments { get; } = new();

    #endregion

    #region Computed properties

    /// <summary>
    /// Indicates whether a single transaction is selected.
    /// </summary>
    public bool HasSelectedTransaction => SelectedTransaction != null;

    /// <summary>
    /// Number of currently selected transactions.
    /// </summary>
    public int SelectedCount => SelectedTransactions.Count;

    /// <summary>
    /// Text describing current page position.
    /// </summary>
    public string PageInfoText => $"{CurrentPageIndex + 1} of {PageCount} pages";

    /// <summary>
    /// Toolbar visibility derived from selection state.
    /// </summary>
    public bool ShowToolbar => !HasSelection;

    /// <summary>
    /// Pagination summary text.
    /// </summary>
    public string PaginationText =>
        $"Showing {PagedTransactions.Count} of {FilteredTransactions.Count} transactions";

    #endregion

    #region Commands

    /// <summary>
    /// Loads a page of transactions for the DataPager.
    /// </summary>
    [RelayCommand]
    private void LoadPage(OnDemandLoadingEventArgs args)
    {
        PagedTransactions.Clear();

        var pageItems = FilteredTransactions
            .Skip(args.StartIndex)
            .Take(args.PageSize);

        foreach (var item in pageItems)
        {
            PagedTransactions.Add(item);
        }

        OnPropertyChanged(nameof(PaginationText));

        // Keep header checkbox synced to the newly materialized page
        var start = CurrentPageIndex * PageSize;
        if (start < 0) start = 0;
        var pageSlice = FilteredTransactions.Skip(start).Take(PageSize).ToList();
        var allSelected = pageSlice.Count > 0 && pageSlice.All(t => t.IsSelected);
        SetAllVisibleSelectedInternal(allSelected);
    }

    /// <summary>
    /// Clears all current selections.
    /// </summary>
    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var t in SelectedTransactions.ToList())
        {
            t.IsSelected = false;
        }

        SelectedTransactions.Clear();
        AllVisibleSelected = false;
    }

    /// <summary>
    /// Deletes all selected transactions after user confirmation.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedMultipleAsync()
    {
        if (SelectedTransactions.Count == 0)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var shouldDelete = await page.DisplayAlertAsync(
            "Delete",
            $"Delete {SelectedTransactions.Count} selected transaction(s)?",
            "Yes",
            "No");

        if (!shouldDelete)
            return;

        var toDelete = SelectedTransactions.ToList();
        foreach (var tx in toDelete)
        {
            await _transactionService.DeleteTransactionAsync(tx);
        }

        SelectedTransactions.Clear();
        foreach (var t in Transactions)
        {
            t.IsSelected = false;
        }

        NotifyTransactionsChanged();
        await LoadAsync();
    }

    /// <summary>
    /// Deletes the currently selected transaction.
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedTransaction is null)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
            return;

        var shouldDelete = await page.DisplayAlertAsync(
            "Delete",
            "Delete selected transaction?",
            "Yes",
            "No");

        if (!shouldDelete)
            return;

        await _transactionService.DeleteTransactionAsync(SelectedTransaction);
        SelectedTransaction = null;

        NotifyTransactionsChanged();
        await LoadAsync();
    }

    /// <summary>
    /// Allows editing of the selected transaction.
    /// </summary>
    [RelayCommand]
    private async Task EditSelectedAsync()
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (SelectedTransaction is null || page is null)
            return;

        var newTitle = await page.DisplayPromptAsync(
            "Edit Title",
            "Update transaction title",
            initialValue: SelectedTransaction.Title);

        if (string.IsNullOrWhiteSpace(newTitle))
            return;

        var newAmountText = await page.DisplayPromptAsync(
            "Edit Amount",
            "Update amount",
            initialValue: SelectedTransaction.Amount.ToString("F2"),
            keyboard: Keyboard.Numeric);

        if (!decimal.TryParse(newAmountText, out var newAmount) || newAmount <= 0)
            return;

        SelectedTransaction.Title = newTitle.Trim();
        SelectedTransaction.Amount = newAmount;

        await _transactionService.SaveTransactionAsync(SelectedTransaction);
        SelectedTransaction = null;

        NotifyTransactionsChanged();
        await LoadAsync();
    }

    /// <summary>
    /// Loads and filters transactions based on current UI state.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var currentCount = Transactions.Count;
            var isIncrementalAdd = currentCount == _lastKnownTransactionCount + 1;
            _lastKnownTransactionCount = currentCount;

            var items = Transactions
                .Where(t =>
                    t.Date.Date >= FromDate.Date &&
                    t.Date.Date <= ToDate.Date.AddDays(1).AddTicks(-1))
                .ToList();

            if (SelectedSegmentIndex == 1)
                items = items.Where(t => t.Type == TransactionType.Income).ToList();
            else if (SelectedSegmentIndex == 2)
                items = items.Where(t => t.Type == TransactionType.Expense).ToList();

            var newItems = items
                        .OrderByDescending(t => t.Date)
                        .ToList();

            //
            // 1️⃣ REMOVE items no longer visible
            //
            for (int i = FilteredTransactions.Count - 1; i >= 0; i--)
            {
                var existing = FilteredTransactions[i];

                if (!newItems.Contains(existing))
                {
                    existing.PropertyChanged -= Transaction_PropertyChanged;
                    FilteredTransactions.RemoveAt(i);
                }
            }

            //
            // 2️⃣ ADD new items (preserve existing instances)
            //
            foreach (var item in newItems)
            {
                if (!FilteredTransactions.Contains(item))
                {
                    item.PropertyChanged += Transaction_PropertyChanged;
                    FilteredTransactions.Add(item);
                }
            }

            //
            // 3️⃣ SORT ONLY if required (optional but safe)
            //
            for (int i = 0; i < newItems.Count; i++)
            {
                var correctItem = newItems[i];
                if (!ReferenceEquals(FilteredTransactions[i], correctItem))
                {
                    FilteredTransactions.Move(
                        FilteredTransactions.IndexOf(correctItem),
                        i);
                }
            }

            //
            // 4️⃣ Reset selection state safely
            //
            SelectedTransactions.Clear();
            HasSelection = false;
            SetAllVisibleSelectedInternal(false);

            PageCount = (int)Math.Ceiling((double)FilteredTransactions.Count / PageSize);
            OnPropertyChanged(nameof(PageCount));
        }
        finally
        {
            IsBusy = false;

            if (_lastKnownTransactionCount == -1)
            {
                _lastKnownTransactionCount = Transactions.Count;
            }
        }
    }

    [RelayCommand]
    private async Task NavigateToAdd()
    {
        try
        {
            // Resolve a transient AddTransactionViewModel from DI and assign it
            var vm = IPlatformApplication.Current?.Services.GetService<AddTransactionViewModel>();
            if (vm is null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to resolve AddTransactionViewModel");
                return;
            }

            vm.ReturnTabIndex = 1; // transactions origin (mobile tab index)

            var page = new AddTransactionPage { BindingContext = vm };

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // ✅ Try Shell first
                if (Shell.Current != null)
                {
                    await Shell.Current.Navigation.PushAsync(page);
                    return;
                }

                // ✅ Fallback to standard navigation
                var nav = Application.Current?.MainPage?.Navigation
                          ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

                if (nav != null)
                {
                    await nav.PushAsync(page);
                    return;
                }

                // ❌ Nothing available
                System.Diagnostics.Debug.WriteLine("Navigation not available");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NavigateToAdd error: {ex}");
        }
    }

    #endregion

    #region Lifecycle / initialization

    /// <summary>
    /// Lazily initializes Syncfusion segment items once the View handler is ready.
    /// </summary>
    public void EnsureFinancialSegmentsInitialized()
    {
        if (FinancialSegments.Count > 0)
            return;

        FinancialSegments.Add(new SfSegmentItem
        {
            Text = "All",
            TextStyle = new SegmentTextStyle
            {
                FontFamily = "OpenSansRegular",
                FontSize = 16
            }
        });

        FinancialSegments.Add(new SfSegmentItem
        {
            Text = "\uE735",
            TextStyle = new SegmentTextStyle
            {
                FontFamily = "ExpenseTrackerIcons",
                FontSize = 16
            }
        });

        FinancialSegments.Add(new SfSegmentItem
        {
            Text = "\uE736",
            TextStyle = new SegmentTextStyle
            {
                FontFamily = "ExpenseTrackerIcons",
                FontSize = 16
            }
        });
    }

    #endregion

    #region Event handlers

    private void SelectedTransactions_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e?.NewItems is not null)
        {
            foreach (var item in e.NewItems.Cast<Transaction>())
            {
                if (!item.IsSelected)
                    item.IsSelected = true;
            }
        }

        if (e?.OldItems is not null)
        {
            foreach (var item in e.OldItems.Cast<Transaction>())
            {
                if (item.IsSelected)
                    item.IsSelected = false;
            }
        }

        OnPropertyChanged(nameof(SelectedCount));
        HasSelection = SelectedTransactions.Count > 0;
        OnPropertyChanged(nameof(ShowToolbar));
        // Update header checkbox to reflect whether all visible rows are selected
        var start = CurrentPageIndex * PageSize;
        if (start < 0) start = 0;
        var pageSlice = FilteredTransactions.Skip(start).Take(PageSize).ToList();
        var allSelected = pageSlice.Count > 0 && pageSlice.All(t => t.IsSelected);
        SetAllVisibleSelectedInternal(allSelected);
    }

    private void Transaction_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Transaction.IsSelected))
            return;

        if (sender is not Transaction tx)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (tx.IsSelected)
            {
                if (!SelectedTransactions.Contains(tx))
                    SelectedTransactions.Add(tx);
            }
            else
            {
                if (SelectedTransactions.Contains(tx))
                    SelectedTransactions.Remove(tx);
            }

            // If this transaction is on the current page, update header checkbox
            var start = CurrentPageIndex * PageSize;
            if (start < 0) start = 0;
            var pageSlice = FilteredTransactions.Skip(start).Take(PageSize).ToList();
            if (pageSlice.Contains(tx))
            {
                var allSelected = pageSlice.Count > 0 && pageSlice.All(t => t.IsSelected);
                SetAllVisibleSelectedInternal(allSelected);
            }
        });
    }

    #endregion

    #region Partial property change hooks

    partial void OnSelectedSegmentIndexChanged(int value)
    {
        if (!IsBusy)
            _ = LoadAsync();
    }

    partial void OnSelectedFinancialPeriodChanged(string value)
    {
        if (_suppressPeriodSync)
            return;

        try
        {
            _suppressPeriodSync = true;
            ApplyPeriodToDates(value);
        }
        finally
        {
            _suppressPeriodSync = false;
        }
    }

    partial void OnFromDateChanged(DateTime value)
    {
        if (!_suppressPeriodSync)
            SelectedFinancialPeriod = "Custom";
    }

    partial void OnToDateChanged(DateTime value)
    {
        if (!_suppressPeriodSync)
            SelectedFinancialPeriod = "Custom";
    }

    partial void OnSelectedIndexChanged(int value)
    {
        if (_suppressSelectionSync)
            return;

        try
        {
            _suppressSelectionSync = true;

            if (value >= 0 && value < FilteredTransactions.Count)
                SelectedTransaction = FilteredTransactions[value];
            else
                SelectedTransaction = null;
        }
        finally
        {
            _suppressSelectionSync = false;
        }
    }

    partial void OnSelectedTransactionChanged(Transaction? value)
    {
        OnPropertyChanged(nameof(HasSelectedTransaction));

        if (_suppressSelectionSync)
            return;

        try
        {
            _suppressSelectionSync = true;
            SelectedIndex = value is null ? -1 : FilteredTransactions.IndexOf(value);
        }
        finally
        {
            _suppressSelectionSync = false;
        }
    }

    partial void OnCurrentPageIndexChanged(int value)
    {
        OnPropertyChanged(nameof(PageInfoText));
        OnPropertyChanged(nameof(PaginationText));
    }

    partial void OnPageCountChanged(int value)
    {
        OnPropertyChanged(nameof(PageInfoText));
    }

    #endregion

    #region Private helpers

    private void ApplyPeriodToDates(string period)
    {
        if (string.IsNullOrWhiteSpace(period))
            return;

        var today = DateTime.Now.Date;
        DateTime start;
        DateTime end = today;

        switch (period)
        {
            case "This Month":
                start = new DateTime(today.Year, today.Month, 1);
                break;

            case "Last Month":
                var lastMonth = today.AddMonths(-1);
                start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                end = new DateTime(
                    lastMonth.Year,
                    lastMonth.Month,
                    DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                break;

            case "This Year":
                start = new DateTime(today.Year, 1, 1);
                break;

            case "Last Year":
                var lastYear = today.AddYears(-1);
                start = new DateTime(lastYear.Year, 1, 1);
                end = new DateTime(lastYear.Year, 12, 31);
                break;

            case "Last 6 Month":
                start = today.AddMonths(-5);
                start = new DateTime(start.Year, start.Month, 1);
                break;

            default:
                return;
        }

        FromDate = start;
        ToDate = end;

        _ = LoadAsync();
    }

    private void OnAllVisibleSelectedChanged(bool value)
    {
        _suppressAllVisibleSync = true;
        try
        {
            var start = CurrentPageIndex * PageSize;
            var pageItems = FilteredTransactions.Skip(start).Take(PageSize);

            foreach (var t in pageItems)
                t.IsSelected = value;
        }
        finally
        {
            _suppressAllVisibleSync = false;
        }
    }


    private void SetAllVisibleSelectedInternal(bool value)
    {
        _suppressAllVisibleSync = true;
        try
        {
            _allVisibleSelected = value;
            OnPropertyChanged(nameof(AllVisibleSelected));
        }
        finally
        {
            _suppressAllVisibleSync = false;
        }
    }

    private static void NotifyTransactionsChanged()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                WeakReferenceMessenger.Default.Send(
                    new TransactionsChangedMessage(DateTime.UtcNow));
            });
        }
        catch
        {
            WeakReferenceMessenger.Default.Send(
                new TransactionsChangedMessage(DateTime.UtcNow));
        }
    }

    #endregion
}