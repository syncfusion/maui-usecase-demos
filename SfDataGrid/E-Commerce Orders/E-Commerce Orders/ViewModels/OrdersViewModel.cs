using E_Commerce_Orders.Models;
using E_Commerce_Orders.Services;
using Syncfusion.Maui.DataGrid;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace E_Commerce_Orders.ViewModels
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();

        private ObservableCollection<Order> _orders = new();
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set => SetProperty(ref _orders, value);
        }

        private List<Order> _allOrders = new();
        private IEnumerable<Order> _filteredOrders = Enumerable.Empty<Order>();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilters();
            }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                ApplyFilters();
            }
        }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand SelectOrderCommand { get; }

        public Action<Order>? OrderSelectedAction { get; set; }

        public OrdersViewModel()
        {
            RefreshCommand = new Command(async () => await RefreshAsync());
            SelectOrderCommand = new Command<object>((item) => OnOrderSelected(item));
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                // Clear existing data to force fresh load
                _allOrders = new List<Order>();
                Orders.Clear();
                // Reload data from API
                await LoadAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Refresh error: {ex.Message}");
                var window = Application.Current?.Windows.FirstOrDefault();
                var page = window?.Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync("Refresh Error", $"Failed to refresh data: {ex.Message}", "OK");
                }
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void OnOrderSelected(object item)
        {
            Order? selectedOrder = null;

            if (item is DataGridCellTappedEventArgs args)
            {
                selectedOrder = args.RowData as Order;
            }
            else if (item is Order order)
            {
                selectedOrder = order;
            }

            if (selectedOrder != null)
            {
                // Store the selected order globally
                App.SelectedOrder = selectedOrder;
                OrderSelectedAction?.Invoke(selectedOrder);
            }
        }

        public async Task LoadAsync()
        {
            var list = await _api.GetOrdersAsync();
            _allOrders = list;
            // Apply filters which will update the existing Orders collection in-place
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<Order> filtered = _allOrders;
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var st = SearchText.Trim().ToLowerInvariant();
                filtered = filtered.Where(o => o.OrderId.ToString().Contains(st) || (o.CustomerName ?? string.Empty).ToLowerInvariant().Contains(st) || (o.ProductSummary ?? string.Empty).ToLowerInvariant().Contains(st));
            }
            if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "All")
            {
                filtered = filtered.Where(o => (o.OrderStatus ?? string.Empty).Equals(SelectedStatus, StringComparison.OrdinalIgnoreCase));
            }
            _filteredOrders = filtered.OrderByDescending(o => o.OrderId).ToList();

            // Update the existing ObservableCollection in-place so UI controls (DataPager, DataGrid)
            // that observe the collection see changes immediately when Refresh is invoked.
            if (Orders == null)
            {
                Orders = new ObservableCollection<Order>(_filteredOrders);
            }
            else
            {
                Orders.Clear();
                foreach (var o in _filteredOrders)
                    Orders.Add(o);
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            TotalRevenue = _filteredOrders.Sum(o => o.TotalPrice);
            TotalItems = _filteredOrders.Sum(o => o.Quantity);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        private int _totalItems;
        public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }
    }
}
