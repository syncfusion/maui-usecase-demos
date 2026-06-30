using StockMonitor.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StockMonitor.Services
{
    public class InventoryService
    {
        public const int MinStockThreshold = 50;

        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Handle snake_case from JSON
        };

        private readonly string _writablePath = Path.Combine(
            FileSystem.AppDataDirectory, "Inventroy_Stock_Data.json");

        private List<InventoryItem> _items = [];

        /// <summary>
        /// Gets current in-memory items without reloading from file
        /// Used for UI refresh after edits/deletes without losing session changes
        /// </summary>
        public List<InventoryItem> GetCurrentItems()
        {
            return _items;
        }

        public async Task<List<InventoryItem>> GetAllItemsAsync()
        {
            try
            {
                // Always load fresh data on each call to ensure all items are present
                // This ensures data is consistent across app launches
                
                // Check if file exists and is valid
                bool fileNeedsReset = false;
                if (File.Exists(_writablePath))
                {
                    try
                    {
                        var testJson = await File.ReadAllTextAsync(_writablePath);
                        var testItems = JsonSerializer.Deserialize<List<InventoryItem>>(testJson, _options) ?? [];
                        
                        // Seed file should have exactly 100 items - if not, it's corrupted
                        if (testItems.Count != 100 || testItems[0].ItemId == 0)
                        {
                            fileNeedsReset = true;
                        }
                    }
                    catch
                    {
                        fileNeedsReset = true;
                    }
                }
                
                // If file doesn't exist or is corrupted, copy fresh seed data
                if (!File.Exists(_writablePath) || fileNeedsReset)
                {
                    if (File.Exists(_writablePath))
                    {
                        File.Delete(_writablePath);
                    }
                    await CopySeedFileAsync();
                }

                // Load items from file
                var json = await File.ReadAllTextAsync(_writablePath);
                var items = JsonSerializer.Deserialize<List<InventoryItem>>(json, _options) ?? [];
                
                // Update ReorderStatus based on Quantity for consistency
                foreach (var item in items)
                {
                    item.ReorderStatus = item.Quantity <= MinStockThreshold;
                }
                
                _items = items;
                return _items;
            }
            catch (Exception ex)
            {
                return [];
            }
        }

        private async Task CopySeedFileAsync()
        {
            try
            {
                // Delete existing file if corrupted
                if (File.Exists(_writablePath))
                {
                    File.Delete(_writablePath);
                }

                // Try both paths - with and without Resources/Raw prefix
                Stream seedStream = null;
                try
                {
                    seedStream = await FileSystem.OpenAppPackageFileAsync("Resources/Raw/Inventroy_Stock_Data.json");
                }
                catch
                {
                    try
                    {
                        seedStream = await FileSystem.OpenAppPackageFileAsync("Inventroy_Stock_Data.json");
                    }
                    catch
                    {
                        throw;
                    }
                }

                using (seedStream)
                using (var destStream = File.Create(_writablePath))
                {
                    await seedStream.CopyToAsync(destStream);
                }
            }
            catch (Exception ex)
            {
                // Seed file missing from package — return empty list (handled in caller)
            }
        }

        public async Task UpdateItemAsync(InventoryItem item)
        {
            var existing = _items.FirstOrDefault(i => i.ItemId == item.ItemId);
            if (existing != null)
            {
                existing.ItemName = item.ItemName;
                existing.Quantity = item.Quantity;
                existing.Category = item.Category;
                existing.Location = item.Location;
                existing.Supplier = item.Supplier;
                existing.UnitPrice = item.UnitPrice;
                existing.LastOrdered = item.LastOrdered;
                existing.ReorderStatus = item.Quantity <= MinStockThreshold;
                existing.LastUpdated = DateTime.Now.ToString("MM/dd/yyyy");

                // Also update the passed-in item so the ViewModel reflects the computed values
                item.ReorderStatus = existing.ReorderStatus;
                item.LastUpdated = existing.LastUpdated;
            }

            await SaveAsync();
        }

        public async Task AddItemAsync(InventoryItem item)
        {
            item.ItemId = _items.Count > 0 ? _items.Max(i => i.ItemId) + 1 : 1;
            item.ReorderStatus = item.Quantity <= MinStockThreshold;
            item.LastUpdated = DateTime.Now.ToString("MM/dd/yyyy");
            _items.Add(item);
            await SaveAsync();
        }

        public async Task DeleteItemAsync(int itemId)
        {
            var item = _items.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                _items.Remove(item);
            }
            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_items, _options);
                await File.WriteAllTextAsync(_writablePath, json);
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Save Error",
                        $"Unable to save data: {ex.Message}",
                        "OK");
                }
            }
        }

        public async Task ResetToSeedDataAsync()
        {
            try
            {
                // Delete the current data file if it exists
                if (File.Exists(_writablePath))
                {
                    File.Delete(_writablePath);
                }
                
                // Copy fresh seed file
                await CopySeedFileAsync();
                
                // Clear cache
                _items.Clear();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Reset Error",
                    $"Unable to reset data: {ex.Message}",
                    "OK");
            }
        }
    }
}
