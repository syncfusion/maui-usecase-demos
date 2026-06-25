using FinancialTracker.ViewModel;
using System.Globalization;
using FinancialTracker.Model;

namespace FinancialTracker
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnThemeToggle(object sender, EventArgs e)
        {
            Application.Current?.UserAppTheme =
                    Application.Current.UserAppTheme == AppTheme.Dark
                    ? AppTheme.Light
                    : AppTheme.Dark;
        }

        private void OnCategoryChanged(object sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var selected = e.AddedItems[0].ToString();

                if (selected != null && BindingContext is FinanceViewModel vm)
                {
                    vm.SelectedCategory = selected;
                    vm.ApplyFilters();
                }
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (BindingContext is FinanceViewModel vm)
            {
                vm.ApplyFilters();
            }
        }

        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            Application.Current?.UserAppTheme =
                e.Value ? AppTheme.Dark : AppTheme.Light;
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // value will be the row item (Transaction)
            if (value is Transaction txn)
            {
                if (txn.Type == "Income")
                    return Colors.Green;

                if (txn.Type == "Expense")
                    return Colors.Red;
            }

            return Colors.Black;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
