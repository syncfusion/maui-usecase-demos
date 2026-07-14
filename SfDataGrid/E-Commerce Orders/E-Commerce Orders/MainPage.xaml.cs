using E_Commerce_Orders.Models;
using E_Commerce_Orders.ViewModels;
using Syncfusion.Maui.DataGrid.Exporting;
using Syncfusion.Pdf;

namespace E_Commerce_Orders
{
    public partial class MainPage : ContentPage
    {
        private readonly OrdersViewModel _vm = new();

        public MainPage()
        {
            InitializeComponent();

            BindingContext = _vm;

            // Register shell route for OrderDetailPage
            Routing.RegisterRoute("orderdetail", typeof(OrderDetailPage));

            // Wire up the order selection
            _vm.OrderSelectedAction = OnOrderSelected;
        }

        private async void OnOrderSelected(Order order)
        {
            // Navigate to detail page
            await Shell.Current.GoToAsync($"orderdetail?id={order.OrderId}");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _vm.LoadAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Startup Error", $"Failed to load orders: {ex.Message}", "OK");
            }
        }

        // Export to PDF action
        public async void OnExportClicked(object? sender, EventArgs e)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                DataGridPdfExportingController pdfExport = new DataGridPdfExportingController();
                DataGridPdfExportingOption option = new DataGridPdfExportingOption()
                {
                    CanFitAllColumnsInOnePage = true,
                    CanApplyGridStyle = true
                };

                var pdfDoc = pdfExport.ExportToPdf(this.OrdersGrid, option);
                pdfDoc.Save(stream);
                pdfDoc.Close(true);

                SaveService saveService = new();
                saveService.SaveAndView("Orders_Export.pdf", "application/pdf", stream);

                //await DisplayAlertAsync("Success", "PDF exported successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to export PDF: {ex.Message}", "OK");
            }
        }

        // Download action - CSV Export
        public async void OnDownloadClicked(object? sender, EventArgs e)
        {
            try
            {
                if (_vm.Orders?.Count > 0)
                {
                    DataGridExcelExportingController excelExport = new DataGridExcelExportingController();
                    DataGridExcelExportingOption option = new DataGridExcelExportingOption();
                    var excelEngine = excelExport.ExportToExcel(this.OrdersGrid, option);
                    var workbook = excelEngine.Excel.Workbooks[0];
                    MemoryStream stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    workbook.Close();
                    excelEngine.Dispose();
                    string OutputFilename = "ExportFeature.xlsx";
                    SaveService saveService = new();
                    saveService.SaveAndView(OutputFilename, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", stream);

                   // await DisplayAlertAsync("Success", $"CSV exported with {_vm.Orders.Count} orders", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Failed to export CSV: {ex.Message}", "OK");
            }
        }
    }
}
