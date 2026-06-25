using FinancialTracker.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace FinancialTracker.ViewModel
{
    public class FinanceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        private ObservableCollection<Transaction> filteredTransactions = new();
        public ObservableCollection<Transaction> FilteredTransactions
        {
            get => filteredTransactions;
            set
            {
                filteredTransactions = value;
                OnPropertyChanged();
            }
        }

        public List<string> Categories { get; set; }

        public List<string> DateFilters { get; set; } = new()
        {
            "All","This Month","Last Month","Last 3 Months",
            "Last 6 Months","This Year","Last Year"
        };

        private string selectedCategory = "All";
        public string SelectedCategory
        {
            get => selectedCategory;
            set { selectedCategory = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string selectedFilter = "All";
        public string SelectedFilter
        {
            get => selectedFilter;
            set { selectedFilter = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ✅ SUMMARY
        public double TotalIncome { get; set; }
        public double TotalExpense { get; set; }

        private double balance;
        public double Balance
        {
            get => balance;
            set { balance = value; OnPropertyChanged(); }
        }

        private double selectedCategoryTotal;
        public double SelectedCategoryTotal
        {
            get => selectedCategoryTotal;
            set
            {
                selectedCategoryTotal = value;
                OnPropertyChanged();
            }
        }

        public double IncomeChange { get; set; }
        public double ExpenseChange { get; set; }

        // ✅ CHART
        private ObservableCollection<ChartData> chartDataCollection = new();
        public ObservableCollection<ChartData> ChartDataCollection
        {
            get => chartDataCollection;
            set { chartDataCollection = value; OnPropertyChanged(); }
        }

        public double ColumnWidth { get; set; } = 0.4;
        public double AxisInterval { get; set; } = 10000;
        public double YMaximum { get; set; } = 100000;

        public FinanceViewModel()
        {
            Categories = new List<string>
            {
                "All", "Food", "Transport", "Shopping", "Bills"
            };

            LoadData();
        }

        private async void LoadData()
        {
            var list = await LoadTransactionsFromJson();

            foreach (var item in list)
                Transactions.Add(item);

            CalculateBalance();
            ApplyFilters();
        }

        private async Task<List<Transaction>> LoadTransactionsFromJson()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("transactions.json");
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new();
        }

        private void CalculateBalance()
        {
            double runningBalance = 0;

            foreach (var item in Transactions.OrderBy(t => t.Date))
            {
                runningBalance += (item.Type == "Income") ? item.Amount : -item.Amount;
                item.Balance = runningBalance;
            }
        }

        // APPLY FILTERS

        public void ApplyFilters()
        {
            var today = DateTime.Today;

            // STEP 1: DATE FILTER ONLY (for SUMMARY)
            var dateFiltered = Transactions.AsEnumerable();

            switch (SelectedFilter)
            {
                case "This Month":
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date.Month == today.Month &&
                        t.Date.Year == today.Year);
                    break;

                case "Last Month":
                    var lastMonth = today.AddMonths(-1);
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date.Month == lastMonth.Month &&
                        t.Date.Year == lastMonth.Year);
                    break;

                case "Last 3 Months":
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date >= today.AddMonths(-3));
                    break;

                case "Last 6 Months":
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date >= today.AddMonths(-6));
                    break;

                case "This Year":
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date.Year == today.Year);
                    break;

                case "Last Year":
                    dateFiltered = dateFiltered.Where(t =>
                        t.Date.Year == today.Year - 1);
                    break;
            }

            var dateResult = dateFiltered.ToList();

            // SUMMARY (ONLY DATE BASED)
            TotalIncome = dateResult
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            TotalExpense = dateResult
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            // NO NEGATIVE BALANCE
            var calcBalance = TotalIncome - TotalExpense;
            Balance = calcBalance < 0 ? 0 : calcBalance;

            // STEP 2: APPLY CATEGORY FILTER (ONLY FOR GRID + CHART)
            var finalQuery = dateResult.AsEnumerable();

            if (SelectedCategory != "All")
                finalQuery = finalQuery.Where(t => t.Category == SelectedCategory);

            var finalResult = finalQuery
                .OrderByDescending(t => t.Date)
                .ToList();

            // UPDATE GRID
            FilteredTransactions = new ObservableCollection<Transaction>(finalResult);

            // CATEGORY TOTAL (same as chart)

            if (SelectedCategory == "All")
            {
                SelectedCategoryTotal = finalResult
                    .Where(t => t.Type == "Expense")
                    .Sum(t => t.Amount);
            }
            else
            {
                SelectedCategoryTotal = finalResult
                    .Where(t => t.Category == SelectedCategory && t.Type == "Expense")
                    .Sum(t => t.Amount);
            }

            // UPDATE CHART
            UpdateChart(finalResult);

            // PERCENT CALCULATION (based only on DATE filter)
            var previousPeriod = GetPreviousPeriodData(today);

            var prevIncome = previousPeriod
                .Where(t => t.Type == "Income")
                .Sum(t => t.Amount);

            var prevExpense = previousPeriod
                .Where(t => t.Type == "Expense")
                .Sum(t => t.Amount);

            IncomeChange = prevIncome == 0
                ? 0
                : ((TotalIncome - prevIncome) / prevIncome) * 100;

            ExpenseChange = prevExpense == 0
                ? 0
                : ((TotalExpense - prevExpense) / prevExpense) * 100;

            // UI refresh
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpense));
            OnPropertyChanged(nameof(Balance));
            OnPropertyChanged(nameof(IncomeChange));
            OnPropertyChanged(nameof(ExpenseChange));
        }

        private List<Transaction> GetPreviousPeriodData(DateTime today)
        {
            switch (SelectedFilter)
            {
                case "This Month":
                    var lastMonth = today.AddMonths(-1);
                    return Transactions.Where(t =>
                        t.Date.Month == lastMonth.Month &&
                        t.Date.Year == lastMonth.Year).ToList();

                case "Last Month":
                    var twoMonthsBack = today.AddMonths(-2);
                    return Transactions.Where(t =>
                        t.Date.Month == twoMonthsBack.Month &&
                        t.Date.Year == twoMonthsBack.Year).ToList();

                case "Last 3 Months":
                    return Transactions.Where(t =>
                        t.Date >= today.AddMonths(-6) &&
                        t.Date < today.AddMonths(-3)).ToList();

                case "Last 6 Months":
                    return Transactions.Where(t =>
                        t.Date >= today.AddMonths(-12) &&
                        t.Date < today.AddMonths(-6)).ToList();

                case "This Year":
                    return Transactions.Where(t =>
                        t.Date.Year == today.Year - 1).ToList();

                case "Last Year":
                    return Transactions.Where(t =>
                        t.Date.Year == today.Year - 2).ToList();

                default:
                    return new List<Transaction>();
            }
        }

        // UPDATE CHART DATA
        private void UpdateChart(List<Transaction> data)
        {
            ChartDataCollection.Clear();

            if (data == null || data.Count == 0)
                return;

            // FILTER ONLY EXPENSES (CRITICAL FIX)
            var expenseData = data.Where(t => t.Type == "Expense").ToList();

            if (!expenseData.Any())
                return;

            var grouped = expenseData
                .GroupBy(t => t.Category)
                .Select(g => new ChartData
                {
                    Category = g.Key,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToList();

            foreach (var item in grouped)
                ChartDataCollection.Add(item);

            var max = grouped.Max(x => x.Amount);

            // SIMPLE SCALE (NO OVERCOMPLICATION)
            AxisInterval = Math.Ceiling(max / 5.0);
            YMaximum = max + AxisInterval;


            if (ChartDataCollection.Count == 1)
            {
                ColumnWidth = 0.25;
            }
            else
            {
                ColumnWidth = 0.3;
            }

            OnPropertyChanged(nameof(ColumnWidth));
            OnPropertyChanged(nameof(ChartDataCollection));
            OnPropertyChanged(nameof(YMaximum));
            OnPropertyChanged(nameof(AxisInterval));
            OnPropertyChanged(nameof(ColumnWidth));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

