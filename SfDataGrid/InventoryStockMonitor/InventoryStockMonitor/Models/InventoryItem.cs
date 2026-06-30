using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace StockMonitor.Models
{
    public partial class InventoryItem : ObservableObject
    {
        // Use both JsonPropertyName AND JsonConverter for proper deserialization
        [ObservableProperty]
        [JsonPropertyName("item_id")]
        private int itemId;

        [ObservableProperty]
        [JsonPropertyName("item_name")]
        private string itemName = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("quantity")]
        private int quantity;

        [ObservableProperty]
        [JsonPropertyName("reorder_status")]
        private bool reorderStatus;

        [ObservableProperty]
        [JsonPropertyName("category")]
        private string category = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("location")]
        private string location = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("last_updated")]
        private string lastUpdated = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("supplier")]
        private string supplier = string.Empty;

        [ObservableProperty]
        [JsonPropertyName("unit_price")]
        private decimal unitPrice;

        [ObservableProperty]
        [JsonPropertyName("last_ordered")]
        private string lastOrdered = string.Empty;

        /// <summary>
        /// Creates a deep clone of this item for editing without affecting the original
        /// </summary>
        public InventoryItem Clone()
        {
            return new InventoryItem
            {
                ItemId = this.ItemId,
                ItemName = this.ItemName,
                Quantity = this.Quantity,
                ReorderStatus = this.ReorderStatus,
                Category = this.Category,
                Location = this.Location,
                LastUpdated = this.LastUpdated,
                Supplier = this.Supplier,
                UnitPrice = this.UnitPrice,
                LastOrdered = this.LastOrdered
            };
        }
    }
}
