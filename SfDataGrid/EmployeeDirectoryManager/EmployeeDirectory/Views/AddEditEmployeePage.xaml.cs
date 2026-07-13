using EmployeeDirectory.Models;
using EmployeeDirectory.ViewModels;

namespace EmployeeDirectory.Views
{
    public partial class AddEditEmployeePage : ContentPage
    {
        private readonly EmployeeViewModel _viewModel;
        private readonly Employee? _originalEmployee;

        public AddEditEmployeePage(Employee? employee, EmployeeViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _originalEmployee = employee;

            BindingContext = _viewModel;

            if (employee != null)
            {
                // Edit mode - clone the employee for editing
                FormTitleLabel.Text = "Edit Employee";
                _viewModel.SelectedEmployee = employee.Clone();
            }
            else
            {
                // Add mode
                FormTitleLabel.Text = "Add New Employee";
                _viewModel.SelectedEmployee = new Employee();
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(_viewModel.SelectedEmployee?.FirstName))
            {
                await DisplayAlertAsync("Validation Error", "First Name is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.SelectedEmployee?.LastName))
            {
                await DisplayAlertAsync("Validation Error", "Last Name is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.SelectedEmployee?.Email))
            {
                await DisplayAlertAsync("Validation Error", "Email is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.SelectedEmployee?.JobType))
            {
                await DisplayAlertAsync("Validation Error", "Job Type is required.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_viewModel.SelectedEmployee?.UserName))
            {
                await DisplayAlertAsync("Validation Error", "User Name is required.", "OK");
                return;
            }

            // Handle edit mode - update the original employee
            if (_originalEmployee != null && _viewModel.SelectedEmployee != null)
            {
                _originalEmployee.FirstName = _viewModel.SelectedEmployee.FirstName;
                _originalEmployee.LastName = _viewModel.SelectedEmployee.LastName;
                _originalEmployee.UserName = _viewModel.SelectedEmployee.UserName;
                _originalEmployee.Email = _viewModel.SelectedEmployee.Email;
                _originalEmployee.JobType = _viewModel.SelectedEmployee.JobType;
                _originalEmployee.City = _viewModel.SelectedEmployee.City;
                _originalEmployee.Country = _viewModel.SelectedEmployee.Country;
                _originalEmployee.Sex = _viewModel.SelectedEmployee.Sex;
                _originalEmployee.Birthdate = _viewModel.SelectedEmployee.Birthdate;
                _originalEmployee.Amount = _viewModel.SelectedEmployee.Amount;
                _originalEmployee.Password = _viewModel.SelectedEmployee.Password;
            }
            else if (_originalEmployee == null && _viewModel.SelectedEmployee != null)
            {
                // Handle add mode - add the new employee to collections
                _viewModel.AddEmployeeCommand.Execute(_viewModel.SelectedEmployee);
            }

            await DisplayAlertAsync("Success", "Employee saved successfully.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
