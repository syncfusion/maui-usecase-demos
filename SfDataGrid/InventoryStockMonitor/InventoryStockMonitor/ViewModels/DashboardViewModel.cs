using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockMonitor.Models;
using StockMonitor.Services;
using System.Collections.ObjectModel;

namespace StockMonitor.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly InventoryService _inventoryService;
        private List<InventoryItem> _allItems = [];

        [ObservableProperty]
        private ObservableCollection<InventoryItem> inventoryItems = [];

        [ObservableProperty]
        private int reorderCount;

        [ObservableProperty]
        private InventoryItem? selectedRow;

        [ObservableProperty]
        private string searchText = string.Empty;

        public DashboardViewModel(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [RelayCommand]
        public async Task LoadData()
        {
            try
            {
                var items = await _inventoryService.GetAllItemsAsync();
                _allItems = items.ToList();
                // Set InventoryItems to the actual items from service so edits are reflected
                InventoryItems = new ObservableCollection<InventoryItem>(_allItems);
                ComputeReorderCount();
            }
            catch (Exception ex)
            {
                await Shell.Current?.DisplayAlert("Error", $"Failed to load inventory: {ex.Message}", "OK")!;
            }
        }

        /// <summary>
        /// Refreshes the grid display without reloading from file
        /// Keeps all in-memory edits and shows deletions properly
        /// Call this when returning from edit page
        /// </summary>
        public void RefreshGridDisplay()
        {
            // Get current in-memory items (includes edits and deletions, doesn't reload from file)
            var currentItems = _inventoryService.GetCurrentItems();
            
            // Update local list
            _allItems = currentItems.ToList();
            
            // Sync the collection - this updates the grid display
            // First, remove items that are no longer in the service
            var itemsToRemove = InventoryItems.Where(i => !currentItems.Any(c => c.ItemId == i.ItemId)).ToList();
            foreach (var item in itemsToRemove)
            {
                InventoryItems.Remove(item);
            }
            
            // Then, update existing items and add any new ones
            foreach (var currentItem in currentItems)
            {
                var existing = InventoryItems.FirstOrDefault(i => i.ItemId == currentItem.ItemId);
                if (existing != null)
                {
                    // Item exists - it's already updated in memory, just update reference if needed
                    var index = InventoryItems.IndexOf(existing);
                    InventoryItems[index] = currentItem;
                }
                else
                {
                    // New item - shouldn't happen in normal flow but handle it
                    InventoryItems.Add(currentItem);
                }
            }
            
            // Update reorder count
            ComputeReorderCount();
        }

        [RelayCommand]
        public async Task NavigateToDetail()
        {
            if (SelectedRow != null)
            {
                await Shell.Current!.GoToAsync($"InventoryDetailPage?id={SelectedRow.ItemId}");
            }
        }

        [RelayCommand]
        public async Task DeleteItem(int itemId)
        {
            var item = InventoryItems.FirstOrDefault(i => i.ItemId == itemId);
            if (item != null)
            {
                bool confirm = await Shell.Current!.DisplayAlert("Confirm Delete", 
                    $"Delete '{item.ItemName}'?", "Yes", "No");
                
                if (confirm)
                {
                    try
                    {
                        await _inventoryService.DeleteItemAsync(itemId);
                        InventoryItems.Remove(item);
                        _allItems.Remove(item);
                        SelectedRow = null;
                        ComputeReorderCount();
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current!.DisplayAlert("Error", $"Failed to delete item: {ex.Message}", "OK");
                    }
                }
            }
        }

        partial void OnSelectedRowChanged(InventoryItem? value)
        {
            // Row selection is now used for deletion, not navigation
            // Navigation happens when clicking a row's content (future enhancement)
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                InventoryItems = new ObservableCollection<InventoryItem>(_allItems);
            }
            else
            {
                var filtered = _allItems.Where(i =>
                    i.ItemName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    i.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

                InventoryItems = new ObservableCollection<InventoryItem>(filtered);
            }
            ComputeReorderCount();
        }

        private void ComputeReorderCount()
        {
            // Count items with quantity <= 50 (matches the template converter logic)
            ReorderCount = InventoryItems.Count(i => i.Quantity <= 50);
        }

        public void RefreshData()
        {
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        public async Task ResetData()
        {
            try
            {
                bool confirm = await Shell.Current?.DisplayAlert("Reset Data", "Reset to original seed data? All edits will be lost.", "Yes", "No")!;
                if (confirm)
                {
                    await _inventoryService.ResetToSeedDataAsync();
                    await LoadDataCommand.ExecuteAsync(null);
                    await Shell.Current?.DisplayAlert("Success", "Data reset to original seed.", "OK")!;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current?.DisplayAlert("Error", $"Reset failed: {ex.Message}", "OK")!;
            }
        }
    }
}
