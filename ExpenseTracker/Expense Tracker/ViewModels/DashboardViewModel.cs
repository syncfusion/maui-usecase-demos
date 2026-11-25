using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Buttons;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SmartExpense;

/// <summary>
/// ViewModel responsible for driving the SmartExpense dashboard.
/// Manages summary cards, charts, recent transactions, and period-based insights,
/// while reacting to transaction changes and authentication state.
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    #region Private fields

    private readonly ITransactionService _transactionService;
    private readonly AuthService _authService;
    private readonly IAppStateService _appState;

    // Tracks how many transactions have already been processed into RecentTransactions
    private int _lastProcessedTransactionIndex;

    private static readonly int MaxRecentTransactionRows =
        DeviceInfo.Platform == DevicePlatform.WinUI ? 6 : 5;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the dashboard ViewModel and wires up transaction change listeners.
    /// </summary>
    public DashboardViewModel(
        ITransactionService transactionService,
        AuthService authService,
        IAppStateService appState)
    {
        _transactionService = transactionService;
        _authService = authService;
        _appState = appState;

        FinancialSegments = new ObservableCollection<SfSegmentItem>
        {
            new SfSegmentItem
            {
                Text = "\uE735", // Income
                TextStyle = new SegmentTextStyle
                {
                    FontFamily = "ExpenseTrackerIcons",
                    FontSize = 16
                }
            },
            new SfSegmentItem
            {
                Text = "\uE736", // Expense
                TextStyle = new SegmentTextStyle
                {
                    FontFamily = "ExpenseTrackerIcons",
                    FontSize = 16
                }
            }
        };

        // React immediately to app-state transaction changes
        _appState.Transactions.CollectionChanged += (_, _) =>
        {
            if (!IsBusy)
                _ = LoadAsync();
        };

        // React to explicit transaction save/update/delete events
        WeakReferenceMessenger.Default.Register<TransactionsChangedMessage>(
            this,
            async (_, _) =>
            {
                await LoadCommand.ExecuteAsync(null);

                // Refresh existing recent items in-place
                for (int i = 0; i < RecentTransactions.Count; i++)
                {
                    var existing = RecentTransactions[i];
                    var updatedTx = _appState.Transactions
                        .FirstOrDefault(t => t.Id == existing.Id);

                    if (updatedTx is not null)
                    {
                        RecentTransactions[i] = new TransactionDisplayItem
                        {
                            Id = updatedTx.Id,
                            Date = updatedTx.Date,
                            Title = updatedTx.Title,
                            Category = updatedTx.Category,
                            IconGlyph = CategoryIconMapper.GetIconGlyph(updatedTx.Category),
                            IconBackground = CategoryIconMapper.GetIconBackground(updatedTx.Category),
                            Type = updatedTx.Type.ToString(),
                            Amount = updatedTx.Amount
                        };
                    }
                }

                // Append newly added transactions (FIFO order preserved)
                var newItems = _appState.Transactions
                    .Where(t => !RecentTransactions.Any(r => r.Id == t.Id))
                    .OrderBy(t => t.Date)
                    .ToList();

                foreach (var t in newItems)
                    AddToRecentTransactions(t);

                _lastProcessedTransactionIndex = _appState.Transactions.Count;

                SyncRecentTransactionsAfterDelete();
            });

        // Default selectors
        SelectedAccountPeriod = PeriodOptions.ElementAtOrDefault(2) ?? "This Year";
        SelectedFinancialPeriod = PeriodOptions.ElementAtOrDefault(2) ?? "This Year";
        SelectedAccountPeriodIndex = 2;
        SelectedFinancialPeriodIndex = 2;

        InitializeRecentTransactionsOnce();

        RecentTransactions.CollectionChanged += (_, __) =>
        {
            OnPropertyChanged(nameof(RecentTransactionsPreview));
        };
    }

    #endregion

    #region Public observable properties

    public ObservableCollection<SfSegmentItem> FinancialSegments { get; }

    public ObservableCollection<MonthlyTrendPoint> MonthlyTrends { get; } = new();
    public ObservableCollection<CategoryBreakdownItem> CategoryBreakdown { get; } = new();
    public ObservableCollection<CategoryBreakdownItem> FinancialCategoryBreakdown { get; } = new();
    public ObservableCollection<TransactionDisplayItem> RecentTransactions { get; } = new();
    public ObservableCollection<InsightCardVM> InsightCards { get; } = new();

    public IEnumerable<TransactionDisplayItem> RecentTransactionsPreview =>
        RecentTransactions.Take(MaxRecentTransactionRows);

    public ObservableCollection<string> PeriodOptions { get; } = new()
    {
        "This Month",
        "Last Month",
        "This Year",
        "Last Year",
        "Last 6 Month"
    };

    public ObservableCollection<string> MonthOptions { get; } =
    [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    public List<Brush> FinancialDoughnutPalette { get; } = new()
    {
        new SolidColorBrush(Color.FromArgb("#06AEE0")),
        new SolidColorBrush(Color.FromArgb("#205107")),
        new SolidColorBrush(Color.FromArgb("#6355C7")),
        new SolidColorBrush(Color.FromArgb("#315A74")),
        new SolidColorBrush(Color.FromArgb("#FFB400")),
        new SolidColorBrush(Color.FromArgb("#963C70")),
        new SolidColorBrush(Color.FromArgb("#2196F5")),
        new SolidColorBrush(Color.FromArgb("#473B89")),
        new SolidColorBrush(Color.FromArgb("#DBFFD8")),
        new SolidColorBrush(Color.FromArgb("#EC5C7B")),
        new SolidColorBrush(Color.FromArgb("#EC8317")),
    };

    [ObservableProperty] private int selectedAccountPeriodIndex;
    [ObservableProperty] private int selectedFinancialPeriodIndex;
    [ObservableProperty] private bool showingIncomeCategories = true;
    [ObservableProperty] private int selectedCategorySegmentIndex;
    [ObservableProperty] private string selectedGlobalPeriod = "This Month";
    [ObservableProperty] private string selectedAccountPeriod = "This Year";
    [ObservableProperty] private string selectedFinancialPeriod = "This Month";
    [ObservableProperty] private int totalBalance;
    [ObservableProperty] private int totalIncome;
    [ObservableProperty] private int totalExpense;
    [ObservableProperty] private string incomeChangeText = "0%";
    [ObservableProperty] private string expenseChangeText = "0%";
    [ObservableProperty] private string savingsChangeText = "Stable";
    [ObservableProperty] private decimal currentMonthIncome;
    [ObservableProperty] private decimal currentMonthExpense;
    [ObservableProperty] private string selectedYear = DateTime.Now.Year.ToString();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string totalBalanceText = string.Empty;
    [ObservableProperty] private string totalIncomeText = string.Empty;
    [ObservableProperty] private string totalExpenseText = string.Empty;
    [ObservableProperty] private string savingsText = string.Empty;
    [ObservableProperty] private string savingsPercentText = "0%";
    [ObservableProperty] private string overallTotalBalanceText = string.Empty;
    [ObservableProperty] private string overallTotalIncomeText = string.Empty;
    [ObservableProperty] private string overallTotalExpenseText = string.Empty;
    [ObservableProperty] private string overallSavingsText = string.Empty;
    [ObservableProperty] private string userName = "User";
    [ObservableProperty] private string selectedMonth = DateTime.Now.ToString("MMMM");
    [ObservableProperty] private Color balanceColor = Colors.White;

    #endregion

    #region Computed properties

    public string DoughnutCenterText =>
        SelectedCategorySegmentIndex == 0 ? "Income" : "Expense";

    public string GreetingText
    {
        get
        {
            var hour = DateTime.Now.Hour;
            var salutation = hour switch
            {
                < 12 => "Good Morning",
                < 17 => "Good Afternoon",
                _ => "Good Evening"
            };
            return $"{salutation}, {UserName} 👋";
        }
    }

    public string UserInitial
    {
        get
        {
            var name = string.IsNullOrWhiteSpace(UserName) ? "Guest" : UserName.Trim();
            return string.IsNullOrEmpty(name)
                ? "G"
                : name[0].ToString().ToUpper();
        }
    }

    #endregion

    #region Property change handlers

    partial void OnUserNameChanged(string value)
    {
        OnPropertyChanged(nameof(GreetingText));
        OnPropertyChanged(nameof(UserInitial));
    }

    partial void OnSelectedGlobalPeriodChanged(string value)
    {
        if (!IsBusy)
            _ = LoadAsync();
    }

    partial void OnSelectedAccountPeriodChanged(string value)
    {
        if (!IsBusy)
            _ = LoadMonthlyTrendsForPeriodAsync(value);
    }

    partial void OnSelectedFinancialPeriodChanged(string value)
    {
        if (!IsBusy)
            _ = LoadFinancialCategoryBreakdownAsync(value);
    }

    partial void OnSelectedCategorySegmentIndexChanged(int value)
    {
        OnPropertyChanged(nameof(DoughnutCenterText));
        ShowingIncomeCategories = value == 0;
        _ = LoadCategoryBreakdownAsync();
        _ = LoadFinancialCategoryBreakdownAsync(SelectedFinancialPeriod);
    }

    partial void OnSelectedMonthChanged(string value)
    {
        if (!IsBusy)
            LoadCommand.ExecuteAsync(null);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task ShowIncomeCategoriesAsync()
    {
        if (IsBusy) return;
        ShowingIncomeCategories = true;
        await LoadCategoryBreakdownAsync();
    }

    [RelayCommand]
    private async Task ShowExpenseCategoriesAsync()
    {
        if (IsBusy) return;
        ShowingIncomeCategories = false;
        await LoadCategoryBreakdownAsync();
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

            vm.ReturnTabIndex = 0; // dashboard origin

            var page = new AddTransactionPage { BindingContext = vm };

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // Try Shell first
                if (Shell.Current != null)
                {
                    await Shell.Current.Navigation.PushAsync(page);
                    return;
                }

                // Fallback to standard navigation
                var nav = Application.Current?.MainPage?.Navigation
                          ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

                if (nav != null)
                {
                    await nav.PushAsync(page);
                    return;
                }

                // Nothing available
                System.Diagnostics.Debug.WriteLine("Navigation not available");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NavigateToAdd error: {ex}");
        }
    }


    [RelayCommand]
    private void NavigateToTransactions() =>
        WeakReferenceMessenger.Default.Send(new NavigateToTabMessage(2));

    #endregion

    #region Load / refresh logic

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        IsLoading = true;

        try
        {
            var sessionUser = await _authService.GetSessionUsernameAsync();
            if (!string.IsNullOrEmpty(sessionUser))
                UserName = sessionUser;

            var summary = await _transactionService.GetDashboardSummaryAsync();

            TotalBalance = summary.TotalBalance;
            TotalIncome = summary.TotalIncome;
            TotalExpense = summary.TotalExpense;
            CurrentMonthIncome = summary.CurrentMonthIncome;
            CurrentMonthExpense = summary.CurrentMonthExpense;
            TotalBalanceText =
                TotalBalance.ToString(AppConstants.CurrencyFormat);
            TotalIncomeText =
                TotalIncome.ToString(AppConstants.CurrencyFormat);
            TotalExpenseText =
                TotalExpense.ToString(AppConstants.CurrencyFormat);
            OverallTotalBalanceText = TotalBalanceText;
            OverallTotalIncomeText = TotalIncomeText;
            OverallTotalExpenseText = TotalExpenseText;

            var overallSavings = summary.TotalIncome - summary.TotalExpense;
            OverallSavingsText =
                overallSavings.ToString(AppConstants.CurrencyFormat);

            var savings = TotalIncome - TotalExpense;
            SavingsText = savings.ToString(AppConstants.CurrencyFormat);
            SavingsPercentText =
                TotalIncome > 0
                    ? $"{(int)(savings / TotalIncome * 100)}%"
                    : "0%";

            BalanceColor =
                TotalBalance >= 0
                    ? Color.FromArgb("#43A047")
                    : Color.FromArgb("#E53935");

            await LoadMonthlyTrendsForPeriodAsync(SelectedAccountPeriod);
            await LoadCategoryBreakdownAsync();
            await LoadFinancialCategoryBreakdownAsync(SelectedFinancialPeriod);

            if (_lastProcessedTransactionIndex == 0)
                _lastProcessedTransactionIndex = _appState.Transactions.Count;
        }
        finally
        {
            IsLoading = false;
            IsBusy = false;
        }

        RefreshInsightCards();
    }

    #endregion

    #region Private helper methods

    private async Task LoadCategoryBreakdownAsync()
    {
        CategoryBreakdown.Clear();
        var now = DateTime.Now;

        if (SelectedGlobalPeriod == "This Month")
        {
            var items =
                await _transactionService.GetCategoryBreakdownAsync(
                    now.Year,
                    now.Month,
                    ShowingIncomeCategories);

            foreach (var item in items)
                CategoryBreakdown.Add(item);

            return;
        }

        if (SelectedGlobalPeriod == "Last Month")
        {
            var last = now.AddMonths(-1);
            var items =
                await _transactionService.GetCategoryBreakdownAsync(
                    last.Year,
                    last.Month,
                    ShowingIncomeCategories);

            foreach (var item in items)
                CategoryBreakdown.Add(item);

            return;
        }

        DateTime start;
        DateTime end = now;

        if (SelectedGlobalPeriod == "This Year")
        {
            start = new DateTime(now.Year, 1, 1);
        }
        else if (SelectedGlobalPeriod == "Last Year")
        {
            var year = now.Year - 1;
            start = new DateTime(year, 1, 1);
            end = new DateTime(year, 12, 31);
        }
        else
        {
            start = now.AddMonths(-5);
            start = new DateTime(start.Year, start.Month, 1);
        }

        var tx =
            await _transactionService.GetTransactionsByDateRangeAsync(start, end);

        var filtered =
            ShowingIncomeCategories
                ? tx.Where(x => (int)x.Type == (int)TransactionType.Income).ToList()
                : tx.Where(x => (int)x.Type != (int)TransactionType.Income).ToList();

        var total = filtered.Sum(x => Math.Abs(x.Amount));
        if (total <= 0) return;

        var groups =
            filtered
                .GroupBy(x => x.Category)
                .Select(g => new CategoryBreakdownItem
                {
                    Category = g.Key,
                    Amount = g.Sum(x => Math.Abs(x.Amount)),
                    Percentage =
                        total > 0
                            ? (double)(g.Sum(x => Math.Abs(x.Amount)) / total * 100)
                            : 0
                })
                .OrderByDescending(x => x.Amount);

        foreach (var item in groups)
            CategoryBreakdown.Add(item);
    }

    private async Task LoadFinancialCategoryBreakdownAsync(string period)
    {
        FinancialCategoryBreakdown.Clear();
        var now = DateTime.Now;

        if (period == "This Month")
        {
            var items =
                await _transactionService.GetCategoryBreakdownAsync(
                    now.Year,
                    now.Month,
                    ShowingIncomeCategories);

            foreach (var item in items)
                FinancialCategoryBreakdown.Add(item);

            return;
        }

        if (period == "Last Month")
        {
            var last = now.AddMonths(-1);
            var items =
                await _transactionService.GetCategoryBreakdownAsync(
                    last.Year,
                    last.Month,
                    ShowingIncomeCategories);

            foreach (var item in items)
                FinancialCategoryBreakdown.Add(item);

            return;
        }

        DateTime start;
        DateTime end = now;

        if (period == "This Year")
        {
            start = new DateTime(now.Year, 1, 1);
        }
        else if (period == "Last Year")
        {
            var year = now.Year - 1;
            start = new DateTime(year, 1, 1);
            end = new DateTime(year, 12, 31);
        }
        else
        {
            start = now.AddMonths(-5);
            start = new DateTime(start.Year, start.Month, 1);
        }

        var tx =
            await _transactionService.GetTransactionsByDateRangeAsync(start, end);

        var filtered =
            ShowingIncomeCategories
                ? tx.Where(x => (int)x.Type == (int)TransactionType.Income).ToList()
                : tx.Where(x => (int)x.Type != (int)TransactionType.Income).ToList();

        var total = filtered.Sum(x => Math.Abs(x.Amount));
        if (total <= 0) return;

        var groups =
            filtered
                .GroupBy(x => x.Category)
                .Select(g => new CategoryBreakdownItem
                {
                    Category = g.Key,
                    Amount = g.Sum(x => Math.Abs(x.Amount)),
                    Percentage =
                        total > 0
                            ? (double)(g.Sum(x => Math.Abs(x.Amount)) / total * 100)
                            : 0
                })
                .OrderByDescending(x => x.Amount);

        foreach (var item in groups)
            FinancialCategoryBreakdown.Add(item);
    }

    private async Task LoadMonthlyTrendsForPeriodAsync(string period)
    {
        MonthlyTrends.Clear();
        var now = DateTime.Now;

        if (period == "This Month")
        {
            var tx =
                await _transactionService.GetTransactionsByMonthAsync(
                    now.Year,
                    now.Month);

            var daysToShow = DateTime.Now.Day;

            for (int d = 1; d <= daysToShow; d++)
            {
                var dayItems = tx.Where(t => t.Date.Day == d).ToList();
                MonthlyTrends.Add(new MonthlyTrendPoint
                {
                    Month = d.ToString(),
                    Income =
                        dayItems
                            .Where(x => (int)x.Type == (int)TransactionType.Income)
                            .Sum(x => Math.Abs(x.Amount)),
                    Expense =
                        dayItems
                            .Where(x => (int)x.Type != (int)TransactionType.Income)
                            .Sum(x => Math.Abs(x.Amount))
                });
            }

            return;
        }

        if (period == "Last Month")
        {
            var last = now.AddMonths(-1);
            var tx =
                await _transactionService.GetTransactionsByMonthAsync(
                    last.Year,
                    last.Month);

            var days = DateTime.DaysInMonth(last.Year, last.Month);

            for (int d = 1; d <= days; d++)
            {
                var dayItems = tx.Where(t => t.Date.Day == d).ToList();
                MonthlyTrends.Add(new MonthlyTrendPoint
                {
                    Month = d.ToString(),
                    Income =
                        dayItems
                            .Where(x => (int)x.Type == (int)TransactionType.Income)
                            .Sum(x => Math.Abs(x.Amount)),
                    Expense =
                        dayItems
                            .Where(x => (int)x.Type != (int)TransactionType.Income)
                            .Sum(x => Math.Abs(x.Amount))
                });
            }

            return;
        }

        if (period == "This Year")
        {
            var year =
                int.TryParse(SelectedYear, out var v) ? v : now.Year;

            var points =
                await _transactionService.GetMonthlyTrendAsync(year);

            foreach (var p in points)
                MonthlyTrends.Add(p);

            return;
        }

        if (period == "Last Year")
        {
            var year = now.Year - 1;
            var points =
                await _transactionService.GetMonthlyTrendAsync(year);

            foreach (var p in points)
                MonthlyTrends.Add(p);

            return;
        }

        var start = now.AddMonths(-5);
        start = new DateTime(start.Year, start.Month, 1);
        var end = now;

        var txRange =
            await _transactionService.GetTransactionsByDateRangeAsync(start, end);

        var months =
            Enumerable
                .Range(
                    0,
                    ((end.Year - start.Year) * 12) + end.Month - start.Month + 1)
                .Select(i => start.AddMonths(i))
                .ToList();

        foreach (var m in months)
        {
            var monthTx =
                txRange
                    .Where(
                        t => t.Date.Year == m.Year &&
                             t.Date.Month == m.Month)
                    .ToList();

            MonthlyTrends.Add(new MonthlyTrendPoint
            {
                Month = m.ToString("MMM"),
                Income =
                    monthTx
                        .Where(x => (int)x.Type == (int)TransactionType.Income)
                        .Sum(x => Math.Abs(x.Amount)),
                Expense =
                    monthTx
                        .Where(x => (int)x.Type != (int)TransactionType.Income)
                        .Sum(x => Math.Abs(x.Amount))
            });
        }
    }

    private void InitializeRecentTransactionsOnce()
    {
        if (RecentTransactions.Count > 0)
            return;

        var latest =
            _appState.Transactions
                .OrderByDescending(t => t.Date)
                .Take(MaxRecentTransactionRows)
                .OrderBy(t => t.Date)
                .ToList();

        foreach (var t in latest)
            AddToRecentTransactions(t);

        _lastProcessedTransactionIndex = _appState.Transactions.Count;
    }

    private void AddToRecentTransactions(Transaction t)
    {
        if (RecentTransactions.Any(x => x.Id == t.Id))
            return;

        RecentTransactions.Add(
            new TransactionDisplayItem
            {
                Id = t.Id,
                Date = t.Date,
                Title = t.Title,
                Category = t.Category,
                IconGlyph = CategoryIconMapper.GetIconGlyph(t.Category),
                IconBackground =
                    CategoryIconMapper.GetIconBackground(t.Category),
                Type = t.Type.ToString(),
                Amount = t.Amount
            });

        if (RecentTransactions.Count > MaxRecentTransactionRows)
            RecentTransactions.RemoveAt(0);
    }

    private void SyncRecentTransactionsAfterDelete()
    {
        if (RecentTransactions.Count == 0)
            return;

        var existingIds =
            _appState.Transactions
                .Select(t => t.Id)
                .ToHashSet();

        for (int i = RecentTransactions.Count - 1; i >= 0; i--)
        {
            if (!existingIds.Contains(RecentTransactions[i].Id))
                RecentTransactions.RemoveAt(i);
        }
    }

    private void RefreshInsightCards()
    {
        InsightCards.Clear();

        InsightCards.Add(
            new InsightCardVM
            {
                Title = "Balance",
                Value = OverallTotalBalanceText,
                Icon = InsightIcons.Balance,
                Tint = InsightColors.Balance
            });

        InsightCards.Add(
            new InsightCardVM
            {
                Title = "Income",
                Value = OverallTotalIncomeText,
                Icon = InsightIcons.Income,
                Tint = InsightColors.Income
            });

        InsightCards.Add(
            new InsightCardVM
            {
                Title = "Expense",
                Value = OverallTotalExpenseText,
                Icon = InsightIcons.Expense,
                Tint = InsightColors.Expense
            });

        InsightCards.Add(
            new InsightCardVM
            {
                Title = "Savings",
                Value = OverallSavingsText,
                Icon = InsightIcons.Savings,
                Tint = InsightColors.Savings
            });
    }

    #endregion

    #region Nested types

    public class InsightCardVM
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public Color Tint { get; set; } = Colors.Transparent;
    }

    #endregion
}