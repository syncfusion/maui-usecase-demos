using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace EmployeeDirectory.Models
{
    /// <summary>
    /// Represents an employee in the directory.
    /// Uses [JsonPropertyName] for proper JSON deserialization from external data sources.
    /// </summary>
    public partial class Employee : ObservableObject
    {
        private string _userName = string.Empty;
        [JsonPropertyName("userName")]
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _firstName = string.Empty;
        [JsonPropertyName("firstName")]
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        [JsonPropertyName("lastName")]
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email = string.Empty;
        [JsonPropertyName("email")]
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _jobType = string.Empty;
        [JsonPropertyName("jobType")]
        public string JobType
        {
            get => _jobType;
            set => SetProperty(ref _jobType, value);
        }

        private string _city = string.Empty;
        [JsonPropertyName("city")]
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        private string _country = string.Empty;
        [JsonPropertyName("country")]
        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        private string _sex = string.Empty;
        [JsonPropertyName("sex")]
        public string Sex
        {
            get => _sex;
            set => SetProperty(ref _sex, value);
        }

        private string _birthdate = string.Empty;
        [JsonPropertyName("birthdate")]
        public string Birthdate
        {
            get => _birthdate;
            set => SetProperty(ref _birthdate, value);
        }

        private double _amount;
        [JsonPropertyName("amount")]
        public double Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _password = string.Empty;
        [JsonPropertyName("password")]
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        /// <summary>
        /// Creates a deep clone of this employee for editing without affecting the original
        /// </summary>
        public Employee Clone()
        {
            return new Employee
            {
                UserName = this.UserName,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                JobType = this.JobType,
                City = this.City,
                Country = this.Country,
                Sex = this.Sex,
                Birthdate = this.Birthdate,
                Amount = this.Amount,
                Password = this.Password
            };
        }
    }
}
