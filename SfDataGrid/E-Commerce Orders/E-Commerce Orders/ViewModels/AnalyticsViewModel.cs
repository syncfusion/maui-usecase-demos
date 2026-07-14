using E_Commerce_Orders.Services;
using System.Windows.Input;

namespace E_Commerce_Orders.ViewModels
{
    public class AnalyticsViewModel : BaseViewModel
    {
        private readonly ApiService _api = new();

        private int _totalOrders;
        public int TotalOrders { get => _totalOrders; set => SetProperty(ref _totalOrders, value); }

        private decimal _avgOrderValue;
        public decimal AvgOrderValue { get => _avgOrderValue; set => SetProperty(ref _avgOrderValue, value); }

        private decimal _totalRevenue;
        public decimal TotalRevenue { get => _totalRevenue; set => SetProperty(ref _totalRevenue, value); }

        private int _totalItems;
        public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }

        private int _deliveredCount;
        public int DeliveredCount { get => _deliveredCount; set => SetProperty(ref _deliveredCount, value); }

        private int _shippedCount;
        public int ShippedCount { get => _shippedCount; set => SetProperty(ref _shippedCount, value); }

        private int _pendingCount;
        public int PendingCount { get => _pendingCount; set => SetProperty(ref _pendingCount, value); }

        private double _deliveredPercent;
        public double DeliveredPercent { get => _deliveredPercent; set => SetProperty(ref _deliveredPercent, value); }

        private double _shippedPercent;
        public double ShippedPercent { get => _shippedPercent; set => SetProperty(ref _shippedPercent, value); }

        private double _pendingPercent;
        public double PendingPercent { get => _pendingPercent; set => SetProperty(ref _pendingPercent, value); }

        private double _avgItemsPerOrder;
        public double AvgItemsPerOrder { get => _avgItemsPerOrder; set => SetProperty(ref _avgItemsPerOrder, value); }

        private double _fulfillmentRate;
        public double FulfillmentRate { get => _fulfillmentRate; set => SetProperty(ref _fulfillmentRate, value); }

        private bool _isRefreshing = false;
        public bool IsRefreshing { get => _isRefreshing; set => SetProperty(ref _isRefreshing, value); }

        public ICommand RefreshCommand { get; }

        public AnalyticsViewModel()
        {
            RefreshCommand = new Command(async () => await RefreshAsync());
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsRefreshing = true;
                await Task.Delay(500);
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task LoadAsync()
        {
            var orders = await _api.GetOrdersAsync();

            TotalOrders = orders.Count;
            TotalRevenue = orders.Sum(o => o.TotalPrice);
            TotalItems = orders.Sum(o => o.Quantity);
            AvgOrderValue = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;

            DeliveredCount = orders.Count(o => o.OrderStatus?.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ?? false);
            ShippedCount = orders.Count(o => o.OrderStatus?.Equals("Shipped", StringComparison.OrdinalIgnoreCase) ?? false);
            PendingCount = orders.Count(o => o.OrderStatus?.Equals("Pending", StringComparison.OrdinalIgnoreCase) ?? false);

            var total = (double)(DeliveredCount + ShippedCount + PendingCount);
            DeliveredPercent = total > 0 ? DeliveredCount / total : 0;
            ShippedPercent = total > 0 ? ShippedCount / total : 0;
            PendingPercent = total > 0 ? PendingCount / total : 0;

            // Calculate additional metrics
            AvgItemsPerOrder = TotalOrders > 0 ? (double)TotalItems / TotalOrders : 0;
            FulfillmentRate = total > 0 ? (DeliveredCount + ShippedCount) / total : 0;
        }
    }
}
