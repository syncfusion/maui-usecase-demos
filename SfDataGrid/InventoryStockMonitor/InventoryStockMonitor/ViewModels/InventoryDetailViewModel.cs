using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockMonitor.Models;
using StockMonitor.Services;
using System.Collections.ObjectModel;

namespace StockMonitor.ViewModels
{
    [QueryProperty(nameof(ItemId), "id")]
    public partial class InventoryDetailViewModel : ObservableObject
    {
        private readonly InventoryService _inventoryService;
        private InventoryItem? _originalItem; // Keep reference to original for Cancel

        [ObservableProperty]
        private InventoryItem? item;

        [ObservableProperty]
        private bool isReorderRequired;

        [ObservableProperty]
        private int itemId;

        [ObservableProperty]
        private bool hasChanges = false;

        [ObservableProperty]
        private ObservableCollection<string> categoryOptions = new ObservableCollection<string> { "Groceries", "Clothing", "Electronics", "Furniture" };

        public InventoryDetailViewModel(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        partial void OnItemIdChanged(int value)
        {
            if (value > 0)
            {
                _ = LoadItemAsync(value);
            }
        }

        private async Task LoadItemAsync(int id)
        {
            try
            {
                var items = await _inventoryService.GetAllItemsAsync();
                _originalItem = items.FirstOrDefault(i => i.ItemId == id);
                
                if (_originalItem != null)
                {
                    // Create a clone for editing - this prevents Cancel from affecting the original
                    Item = _originalItem.Clone();
                    
                    IsReorderRequired = Item.Quantity <= InventoryService.MinStockThreshold;
                    
                    // Subscribe to property changes for live reorder preview and enable Save button
                    Item.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(InventoryItem.Quantity))
                        {
                            IsReorderRequired = Item.Quantity <= InventoryService.MinStockThreshold;
                        }
                        
                        // Re-evaluate CanSave when any property changes
                        HasChanges = true;
                        SaveCommand.NotifyCanExecuteChanged();
                    };
                    
                    // Enable Save button now that item is loaded
                    HasChanges = true;
                    SaveCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    await Shell.Current?.DisplayAlert("Error", $"Item with ID {id} not found", "OK")!;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current?.DisplayAlert("Error", $"Failed to load item: {ex.Message}", "OK")!;
            }
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        public async Task Save()
        {
            if (Item == null) return;

            try
            {
                // Validation is handled by CanSave
                await _inventoryService.UpdateItemAsync(Item);
                await Shell.Current!.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current!.DisplayAlert("Error", $"Failed to save item: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Delete()
        {
            if (Item == null) return;

            try
            {
                bool confirm = await Shell.Current!.DisplayAlert("Confirm Delete", 
                    $"Permanently delete '{Item.ItemName}'?", "Yes", "No");
                
                if (confirm)
                {
                    await _inventoryService.DeleteItemAsync(Item.ItemId);
                    await Shell.Current!.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current!.DisplayAlert("Error", $"Failed to delete item: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task Cancel()
        {
            await Shell.Current!.GoToAsync("..");
        }

        private bool CanSave()
        {
            if (Item == null) return false;

            return !string.IsNullOrWhiteSpace(Item.ItemName) &&
                   !string.IsNullOrWhiteSpace(Item.Category) &&
                   !string.IsNullOrWhiteSpace(Item.Supplier) &&
                   !string.IsNullOrWhiteSpace(Item.Location) &&
                   Item.Quantity >= 0 &&
                   Item.UnitPrice >= 0;
        }
    }
}
