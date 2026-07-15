using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels
{
    public class PatientRecordsViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private ObservableCollection<Patient> _patients;
        private string _searchText = string.Empty;
        private string _selectedFilter = "All";
        private bool _isLoading = false;
        private Patient? _selectedPatient;

        private event PropertyChangedEventHandler? PropertyChangedInternal;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChangedInternal += value;
            remove => PropertyChangedInternal -= value;
        }

        public PatientRecordsViewModel()
        {
            _patientService = new PatientService();
            _patients = new ObservableCollection<Patient>();
            LoadPatients();
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
                OnPropertyChanged();
            }
        }

        private void UpdatePatients(IEnumerable<Patient> patients)
        {
            _patients.Clear();
            foreach (var patient in patients)
            {
                _patients.Add(patient);
            }

            OnPropertyChanged(nameof(Patients));
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    PerformSearch();
                }
            }
        }

        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
            }
        }

        public void LoadPatients()
        {
            IsLoading = true;
            try
            {
                var patients = _patientService.GetAllPatients();
                UpdatePatients(patients);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadPatients();
            }
            else
            {
                var results = _patientService.Search(SearchText);
                UpdatePatients(results);
            }
        }

        private void ApplyFilter()
        {
            var allPatients = _patientService.GetAllPatients();
            List<Patient> filtered = new();

            switch (SelectedFilter)
            {
                case "Admitted":
                    filtered = _patientService.FilterByStatus("Admitted");
                    break;
                case "Discharged":
                    filtered = _patientService.FilterByStatus("Discharged");
                    break;
                case "Under Treatment":
                    filtered = _patientService.FilterByStatus("Under Treatment");
                    break;
                case "All":
                default:
                    filtered = allPatients;
                    break;
            }

            UpdatePatients(filtered);
        }

        public void AddPatient(Patient patient)
        {
            _patientService.AddPatient(patient);
            LoadPatients();
        }

        public void UpdatePatient(Patient patient)
        {
            _patientService.UpdatePatient(patient);
            LoadPatients();
        }

        public void DeletePatient(int patientId)
        {
            _patientService.DeletePatient(patientId);
            LoadPatients();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedInternal?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
