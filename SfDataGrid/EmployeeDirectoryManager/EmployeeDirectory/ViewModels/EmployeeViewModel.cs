using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EmployeeDirectory.Models;
using EmployeeDirectory.Services;

namespace EmployeeDirectory.ViewModels
{
    /// <summary>
    /// ViewModel for managing employee data and operations.
    /// Implements MVVM pattern with INotifyPropertyChanged.
    /// </summary>
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private readonly EmployeeService _employeeService;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Employee> _filteredEmployees;
        private string _searchText = string.Empty;
        private Employee? _selectedEmployee;
        private bool _isLoading = false;

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set
            {
                if (_employees != value)
                {
                    _employees = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Employee> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                if (_filteredEmployees != value)
                {
                    _filteredEmployees = value;
                    OnPropertyChanged();
                }
            }
        }

        public Employee? SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoadEmployeesCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeViewModel()
        {
            _employeeService = new EmployeeService();
            _employees = new ObservableCollection<Employee>();
            _filteredEmployees = new ObservableCollection<Employee>();

            LoadEmployeesCommand = new Command(async () => await LoadEmployees());
            AddEmployeeCommand = new Command(AddEmployee);
            DeleteEmployeeCommand = new Command(async (obj) => await DeleteEmployee(obj));
        }

        /// <summary>
        /// Loads employees from the service.
        /// </summary>
        private async Task LoadEmployees()
        {
            try
            {
                IsLoading = true;
                var employees = await _employeeService.GetEmployeesAsync();
                
                Employees.Clear();
                FilteredEmployees.Clear();
                
                foreach (var employee in employees)
                {
                    Employees.Add(employee);
                    FilteredEmployees.Add(employee);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading employees: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new employee to the collection.
        /// </summary>
        private void AddEmployee(object? obj)
        {
            if (obj is Employee newEmployee)
            {
                Employees.Add(newEmployee);
                FilteredEmployees.Add(newEmployee);
            }
            
        }

        /// <summary>
        /// Deletes an employee from the collection.
        /// </summary>
        private async Task DeleteEmployee(object? obj)
        {
            if (obj is Employee employee)
            {
                Employees.Remove(employee);
                FilteredEmployees.Remove(employee);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
