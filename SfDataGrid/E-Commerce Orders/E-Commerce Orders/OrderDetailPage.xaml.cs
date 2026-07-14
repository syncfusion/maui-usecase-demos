using E_Commerce_Orders.Models;
using E_Commerce_Orders.ViewModels;

namespace E_Commerce_Orders
{
    public partial class OrderDetailPage : ContentPage
    {
        private readonly OrderDetailViewModel _vm = new();

        public OrderDetailPage()
        {
            InitializeComponent();
            BindingContext = _vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Get the selected order from App
            if (App.SelectedOrder != null)
            {
                _vm.SetOrder(App.SelectedOrder);
            }
        }

        private async void OnBackClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnReorderClicked(object? sender, EventArgs e)
        {
            try
            {
                // Show confirmation dialog
                bool confirmed = await DisplayAlertAsync(
                    "Confirm Reorder",
                    "Would you like to reorder this order with the same items?",
                    "Yes", "Cancel"
                );

                if (confirmed && _vm.SelectedOrder != null)
                {
                    // Create a new order with the same products
                    var newOrder = new Order
                    {
                        OrderId = new Random().Next(10000, 99999),
                        CustomerName = _vm.SelectedOrder.CustomerName,
                        TotalPrice = _vm.SelectedOrder.TotalPrice,
                        OrderStatus = "pending"
                    };

                    // Copy products from original order
                    foreach (var product in _vm.SelectedOrder.Products)
                    {
                        newOrder.Products.Add(new ProductItem
                        {
                            Name = product.Name,
                            Price = product.Price,
                            Quantity = product.Quantity
                        });
                    }

                    // Show success message
                    await DisplayAlertAsync(
                        "Reorder Successful",
                        $"Order #{newOrder.OrderId} has been created successfully!\nTotal: ${newOrder.TotalPrice:F2}",
                        "OK"
                    );

                    // Navigate back to main page
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to reorder: {ex.Message}", "OK");
            }
        }

        private void OnContactSupportClicked(object? sender, EventArgs e)
        {
            sfPopup.Show();
            //_vm.ToggleSupportReport();
        }

        private async void OnSubmitSupportReportClicked(object? sender, EventArgs e)
        {
            try
            {
                var issueType = _vm.SelectedIssueType ?? "General issue";
                var details = _vm.SupportIssueDetails?.Trim();

                if (string.IsNullOrWhiteSpace(details))
                {
                    await DisplayAlertAsync("Missing Details", "Please type a short description of the issue.", "OK");
                    return;
                }

                _vm.ClearSupportReport();

                await DisplayAlertAsync(
                    "Report Submitted",
                    $"Your {issueType.ToLowerInvariant()} request has been submitted. Our customer support team will contact you within 6 hours.",
                    "OK");

                sfPopup.Dismiss();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to submit report: {ex.Message}", "OK");
            }
        }
    }
}
