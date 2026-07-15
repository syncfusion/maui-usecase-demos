using System.ComponentModel;
using System.Runtime.CompilerServices;
using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.ViewModels
{
    public class PatientDetailsViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private Patient? _currentPatient;
        private bool _isEditMode = false;

        private event PropertyChangedEventHandler? PropertyChangedInternal;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChangedInternal += value;
            remove => PropertyChangedInternal -= value;
        }

        public PatientDetailsViewModel()
        {
            _patientService = new PatientService();
        }

        public Patient? CurrentPatient
        {
            get => _currentPatient;
            set
            {
                _currentPatient = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
        }

        public void LoadPatient(int patientId)
        {
            CurrentPatient = _patientService.GetPatientById(patientId);
            IsEditMode = false;
        }

        public void SavePatient()
        {
            if (CurrentPatient != null)
            {
                _patientService.UpdatePatient(CurrentPatient);
                IsEditMode = false;
            }
        }

        public void EditPatient()
        {
            IsEditMode = true;
        }

        public void CancelEdit()
        {
            IsEditMode = false;
            if (CurrentPatient != null)
            {
                LoadPatient(CurrentPatient.PatientId);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedInternal?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
