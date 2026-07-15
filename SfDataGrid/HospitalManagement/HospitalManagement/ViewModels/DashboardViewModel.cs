using System.ComponentModel;
using System.Runtime.CompilerServices;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private int _totalPatients;
        private int _admittedPatients;
        private int _dischargedPatients;
        private int _underTreatmentPatients;

        private event PropertyChangedEventHandler? PropertyChangedInternal;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChangedInternal += value;
            remove => PropertyChangedInternal -= value;
        }

        public DashboardViewModel()
        {
            _patientService = new PatientService();
            LoadDashboardStats();
        }

        public int TotalPatients
        {
            get => _totalPatients;
            set
            {
                _totalPatients = value;
                OnPropertyChanged();
            }
        }

        public int AdmittedPatients
        {
            get => _admittedPatients;
            set
            {
                _admittedPatients = value;
                OnPropertyChanged();
            }
        }

        public int DischargedPatients
        {
            get => _dischargedPatients;
            set
            {
                _dischargedPatients = value;
                OnPropertyChanged();
            }
        }

        public int UnderTreatmentPatients
        {
            get => _underTreatmentPatients;
            set
            {
                _underTreatmentPatients = value;
                OnPropertyChanged();
            }
        }

        private void LoadDashboardStats()
        {
            var allPatients = _patientService.GetAllPatients();
            TotalPatients = allPatients.Count;
            AdmittedPatients = allPatients.Count(p => p.Status == "Admitted");
            DischargedPatients = allPatients.Count(p => p.Status == "Discharged");
            UnderTreatmentPatients = allPatients.Count(p => p.Status == "Under Treatment");
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedInternal?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
