using E_Commerce_Orders.Models;
using System.Windows.Input;

namespace E_Commerce_Orders.ViewModels
{
    public class OrderDetailViewModel : BaseViewModel
    {
        private Order _selectedOrder = new();
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set => SetProperty(ref _selectedOrder, value);
        }

        private decimal _subtotalAmount;
        public decimal SubtotalAmount
        {
            get => _subtotalAmount;
            set => SetProperty(ref _subtotalAmount, value);
        }

        private decimal _taxAmount;
        public decimal TaxAmount
        {
            get => _taxAmount;
            set => SetProperty(ref _taxAmount, value);
        }

        private decimal _shippingCost;
        public decimal ShippingCost
        {
            get => _shippingCost;
            set => SetProperty(ref _shippingCost, value);
        }

        private string _orderDateDisplay = DateTime.Now.ToString("ddd, MMM dd, yyyy");
        public string OrderDateDisplay
        {
            get => _orderDateDisplay;
            set => SetProperty(ref _orderDateDisplay, value);
        }

        private bool _isSupportReportVisible;
        public bool IsSupportReportVisible
        {
            get => _isSupportReportVisible;
            set => SetProperty(ref _isSupportReportVisible, value);
        }

        private string _selectedIssueType = "Order not received";
        public string SelectedIssueType
        {
            get => _selectedIssueType;
            set => SetProperty(ref _selectedIssueType, value);
        }

        private string _supportIssueDetails = string.Empty;
        public string SupportIssueDetails
        {
            get => _supportIssueDetails;
            set => SetProperty(ref _supportIssueDetails, value);
        }

        public List<string> SupportIssueOptions { get; } = new()
        {
            "Delivery delayed",
            "Wrong item received",
            "Item damaged or missing"
        };

        public ICommand ItemSelectedCommand { get; }

        public OrderDetailViewModel()
        {
            ItemSelectedCommand = new Command<object>(OnItemSelected);
        }

        public void ToggleSupportReport()
        {
            IsSupportReportVisible = !IsSupportReportVisible;
        }

        public void ClearSupportReport()
        {
            IsSupportReportVisible = false;
            SelectedIssueType = SupportIssueOptions.FirstOrDefault() ?? "General issue";
            SupportIssueDetails = string.Empty;
        }

        public void SetOrder(Order order)
        {
            SelectedOrder = order;
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            SubtotalAmount = SelectedOrder.Products.Sum(p => p.Price * p.Quantity);
            TaxAmount = SubtotalAmount * 0.10m; // 10% tax
            ShippingCost = SubtotalAmount > 100 ? 0 : 10; // Free shipping over $100
        }

        private void OnItemSelected(object obj)
        {
            // Handle item selection if needed
        }
    }
}
