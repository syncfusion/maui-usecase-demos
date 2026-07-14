using System.Collections.ObjectModel;

namespace E_Commerce_Orders.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public ObservableCollection<ProductItem> Products { get; set; } = new();
        public string ProductSummary => string.Join(", ", Products?.Select(p => p.Name) ?? Enumerable.Empty<string>());
        public int Quantity => Products?.Sum(p => p.Quantity) ?? 0;
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; } = "Pending";
    }
}
