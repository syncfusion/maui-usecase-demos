using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;

namespace SmartExpense
{
    public partial class DashboardInsightsView : ContentView
    {
        DashboardViewModel? _vm;

        private bool IsMobileSingleColumn =>
    DeviceInfo.Idiom == DeviceIdiom.Phone;


        public DashboardInsightsView()
        {
            InitializeComponent();
            SizeChanged += OnSizeChanged;

            // Ensure initial layout logic runs after the view is measured on macOS
            // Some platforms (MacCatalyst) may not fire a SizeChanged with a
            // usable Width immediately; schedule a short dispatch to run the
            // layout logic once the control has a valid width.
            Dispatcher.Dispatch(async () =>
            {
                await Task.Delay(50);
                if (Width > 0)
                    OnSizeChanged(this, EventArgs.Empty);
            });
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext is not DashboardViewModel vm)
                return;

            // unsubscribe old
            if (_vm is not null)
                _vm.PropertyChanged -= Vm_PropertyChanged;

            _vm = vm;
            // set initial card contexts
            UpdateCardsFromVm(vm);

            // listen for changes and refresh cards when totals update
            _vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not DashboardViewModel vm) return;

            if (e.PropertyName is null) return;

            if (e.PropertyName == nameof(vm.TotalBalance) || e.PropertyName == nameof(vm.TotalBalanceText)
                || e.PropertyName == nameof(vm.TotalIncome) || e.PropertyName == nameof(vm.TotalIncomeText)
                || e.PropertyName == nameof(vm.TotalExpense) || e.PropertyName == nameof(vm.TotalExpenseText)
                || e.PropertyName == nameof(vm.SavingsText))
            {
                MainThread.BeginInvokeOnMainThread(() => UpdateCardsFromVm(vm));
            }
        }

        void UpdateCardsFromVm(DashboardViewModel vm)
        {
            BalanceCard.BindingContext = new InsightCardViewModel
            {
                Title = "Balance",
                Value = vm.TotalBalance,
                IconGlyph = InsightIcons.Balance,
                IconColor = Colors.White,
                IconBackground = InsightColors.Balance
            };
            IncomeCard.BindingContext = new InsightCardViewModel
            {
                Title = "Income",
                Value =  vm.TotalIncome,
                IconGlyph = InsightIcons.Income,
                IconColor = Colors.White,
                IconBackground = InsightColors.Income
            };
            ExpenseCard.BindingContext = new InsightCardViewModel
            {
                Title = "Expense",
                Value = vm.TotalExpense,
                IconGlyph = InsightIcons.Expense,
                IconColor = Colors.White,
                IconBackground = InsightColors.Expense
            };
            SavingsCard.BindingContext = new InsightCardViewModel
            {
                Title = "Savings",
                Value = vm.TotalIncome - vm.TotalExpense,
                IconGlyph = InsightIcons.Savings,
                IconColor = Colors.White,
                IconBackground = InsightColors.Savings
            };
        }


        private void OnSizeChanged(object? sender, EventArgs e)
        {
            if (Width <= 0)
                return;

        bool isTabletOrNarrowDesktop = Width < 720;
            bool useSingleColumnMobile = IsMobileSingleColumn;

            RootGrid.RowDefinitions.Clear();
            RootGrid.ColumnDefinitions.Clear();

            // ✅ KEEP EXISTING 720px → 2×2 (UNCHANGED)
            if (isTabletOrNarrowDesktop  || useSingleColumnMobile)
            {
                RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                RootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                Grid.SetRow(BalanceCard, 0);
                Grid.SetColumn(BalanceCard, 0);

                Grid.SetRow(IncomeCard, 0);
                Grid.SetColumn(IncomeCard, 1);

                Grid.SetRow(ExpenseCard, 1);
                Grid.SetColumn(ExpenseCard, 0);

                Grid.SetRow(SavingsCard, 1);
                Grid.SetColumn(SavingsCard, 1);
            }
            else
            {
                // ✅ DESKTOP: 1 × 4 (UNCHANGED)
                RootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                for (int i = 0; i < 4; i++)
                    RootGrid.ColumnDefinitions.Add(
                        new ColumnDefinition(GridLength.Star));

                Grid.SetRow(BalanceCard, 0);
                Grid.SetColumn(BalanceCard, 0);

                Grid.SetRow(IncomeCard, 0);
                Grid.SetColumn(IncomeCard, 1);

                Grid.SetRow(ExpenseCard, 0);
                Grid.SetColumn(ExpenseCard, 2);

                Grid.SetRow(SavingsCard, 0);
                Grid.SetColumn(SavingsCard, 3);
            }
        }
    }
}