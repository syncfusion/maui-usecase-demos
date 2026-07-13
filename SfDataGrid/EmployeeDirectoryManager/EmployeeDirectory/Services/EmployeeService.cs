using System.Diagnostics;
using System.Text.Json;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    /// <summary>
    /// Service responsible for loading and managing employee data.
    /// </summary>
    public class EmployeeService
    {
        /// <summary>
        /// Loads employee data from the JSON file in Resources/Raw.
        /// Uses JsonPropertyName attributes for proper deserialization mapping.
        /// </summary>
        public async Task<List<Employee>> GetEmployeesAsync()
        {
            try
            {
                // Access the JSON file from the app package resources
                using var stream = await FileSystem.OpenAppPackageFileAsync("EmployeeData.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();

                // Configure JSON serializer options for proper deserialization
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                // Deserialize JSON to List<Employee> using JsonPropertyName mappings
                var employees = JsonSerializer.Deserialize<List<Employee>>(json, options);

                if (employees == null)
                {
                    Debug.WriteLine("Warning: Employee list was null after deserialization");
                    return new List<Employee>();
                }

                Debug.WriteLine($"Successfully loaded {employees.Count} employees from JSON");
                return employees;
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                Debug.WriteLine($"Line: {jsonEx.LineNumber}, Position: {jsonEx.BytePositionInLine}");
                return new List<Employee>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading employees: {ex.GetType().Name} - {ex.Message}");
                return new List<Employee>();
            }
        }
    }
}
