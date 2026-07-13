using EmployeeDirectory.Models;
using EmployeeDirectory.ViewModels;
using Syncfusion.Maui.DataGrid;

namespace EmployeeDirectory.Views
{
    public partial class EmployeeListPage : ContentPage
    {
        private EmployeeViewModel _viewModel;

        public EmployeeListPage()
        {
            InitializeComponent();
            _viewModel = new EmployeeViewModel();
            BindingContext = _viewModel;
            _viewModel.LoadEmployeesCommand.Execute(null);
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            EmployeeDataGrid.SearchController.Search(e.NewTextValue);
            EmployeeDataGrid.SearchController.AllowFiltering = true;
        }


        private async void OnAddEmployeeClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddEditEmployeePage(null, _viewModel));
        }

        private async void OnEditSelectedClicked(object sender, EventArgs e)
        {
            var selectedItem = EmployeeDataGrid.SelectionController.SelectedRows.FirstOrDefault();
            // Get selected row from DataGrid
            if (selectedItem != null && selectedItem.RowData is Employee selectedEmployee)
            {
                await Navigation.PushAsync(new AddEditEmployeePage(selectedEmployee, _viewModel));
            }
            else
            {
                await DisplayAlertAsync("No Selection", "Please select an employee row to edit.", "OK");
            }

        }

        private async void OnDeleteSelectedClicked(object sender, EventArgs e)
        {
            // Get selected row from DataGrid
            var selectedItem = EmployeeDataGrid.SelectionController.SelectedRows.FirstOrDefault();
            if (selectedItem != null && selectedItem.RowData is Employee selectedEmployee)
            {
                // Show confirmation dialog before deleting
                bool confirm = await DisplayAlertAsync("Confirm Delete", 
                    $"Are you sure you want to delete {selectedEmployee.FirstName} {selectedEmployee.LastName}?", 
                    "Delete", "Cancel");
                
                if (confirm)
                {
                    _viewModel.DeleteEmployeeCommand.Execute(selectedEmployee);
                }
            }
            else
            {
                await DisplayAlertAsync("No Selection", "Please select an employee row to delete.", "OK");
            }

        }

    }
}
