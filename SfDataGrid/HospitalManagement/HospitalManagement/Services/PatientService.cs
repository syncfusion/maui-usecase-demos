using HospitalManagement.Models;

namespace HospitalManagement.Services
{
    public class PatientService
    {
        public static event Action? PatientsChanged;

        private static List<Patient> _patients = new();

        public PatientService()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (_patients.Count > 0)
            {
                return;
            }

            _patients = new List<Patient>
            {
                new Patient
                {
                    PatientId = 1001,
                    Name = "John Smith",
                    Age = 45,
                    Diagnosis = "Pneumonia",
                    DoctorAssigned = "Dr. Emily Johnson",
                    AdmissionDate = DateTime.Now.AddDays(-5),
                    Status = "Under Treatment",
                    Department = "Pulmonology",
                    PhoneNumber = "555-0101",
                    EmailAddress = "john.smith@email.com",
                    BloodType = "O+",
                    Notes = "Stable condition, responding well to treatment"
                },
                new Patient
                {
                    PatientId = 1002,
                    Name = "Sarah Williams",
                    Age = 32,
                    Diagnosis = "Fracture (Right Leg)",
                    DoctorAssigned = "Dr. Michael Brown",
                    AdmissionDate = DateTime.Now.AddDays(-3),
                    Status = "Under Treatment",
                    Department = "Orthopedics",
                    PhoneNumber = "555-0102",
                    EmailAddress = "sarah.williams@email.com",
                    BloodType = "A+",
                    Notes = "Post-operative care in progress"
                },
                new Patient
                {
                    PatientId = 1003,
                    Name = "Robert Taylor",
                    Age = 58,
                    Diagnosis = "Hypertension",
                    DoctorAssigned = "Dr. James Davis",
                    AdmissionDate = DateTime.Now.AddDays(-10),
                    Status = "Discharged",
                    Department = "Cardiology",
                    PhoneNumber = "555-0103",
                    EmailAddress = "robert.taylor@email.com",
                    BloodType = "B+",
                    Notes = "Discharged with medication plan"
                },
                new Patient
                {
                    PatientId = 1004,
                    Name = "Jessica Martinez",
                    Age = 27,
                    Diagnosis = "Appendicitis",
                    DoctorAssigned = "Dr. Lisa Anderson",
                    AdmissionDate = DateTime.Now.AddDays(-2),
                    Status = "Admitted",
                    Department = "General Surgery",
                    PhoneNumber = "555-0104",
                    EmailAddress = "jessica.martinez@email.com",
                    BloodType = "AB+",
                    Notes = "Pre-operative assessment completed"
                },
                new Patient
                {
                    PatientId = 1005,
                    Name = "David Thompson",
                    Age = 51,
                    Diagnosis = "Diabetes Type 2",
                    DoctorAssigned = "Dr. Christopher Wilson",
                    AdmissionDate = DateTime.Now.AddDays(-7),
                    Status = "Under Treatment",
                    Department = "Endocrinology",
                    PhoneNumber = "555-0105",
                    EmailAddress = "david.thompson@email.com",
                    BloodType = "O-",
                    Notes = "Insulin therapy initiated"
                },
                new Patient
                {
                    PatientId = 1006,
                    Name = "Amanda Garcia",
                    Age = 34,
                    Diagnosis = "Migraine",
                    DoctorAssigned = "Dr. Patricia Moore",
                    AdmissionDate = DateTime.Now.AddDays(-1),
                    Status = "Admitted",
                    Department = "Neurology",
                    PhoneNumber = "555-0106",
                    EmailAddress = "amanda.garcia@email.com",
                    BloodType = "A-",
                    Notes = "Preventive medication prescribed"
                },
                new Patient
                {
                    PatientId = 1007,
                    Name = "Kevin Johnson",
                    Age = 42,
                    Diagnosis = "Peptic Ulcer",
                    DoctorAssigned = "Dr. Richard Taylor",
                    AdmissionDate = DateTime.Now.AddDays(-4),
                    Status = "Under Treatment",
                    Department = "Gastroenterology",
                    PhoneNumber = "555-0107",
                    EmailAddress = "kevin.johnson@email.com",
                    BloodType = "B-",
                    Notes = "Endoscopy scheduled for tomorrow"
                },
                new Patient
                {
                    PatientId = 1008,
                    Name = "Laura Lee",
                    Age = 38,
                    Diagnosis = "Asthma",
                    DoctorAssigned = "Dr. Nancy Jackson",
                    AdmissionDate = DateTime.Now.AddDays(-8),
                    Status = "Discharged",
                    Department = "Pulmonology",
                    PhoneNumber = "555-0108",
                    EmailAddress = "laura.lee@email.com",
                    BloodType = "AB-",
                    Notes = "Discharged with inhaler prescription"
                },
                new Patient
                {
                    PatientId = 1009,
                    Name = "Mark White",
                    Age = 55,
                    Diagnosis = "Heart Attack",
                    DoctorAssigned = "Dr. Steven Harris",
                    AdmissionDate = DateTime.Now.AddDays(-6),
                    Status = "Under Treatment",
                    Department = "Cardiology",
                    PhoneNumber = "555-0109",
                    EmailAddress = "mark.white@email.com",
                    BloodType = "O+",
                    Notes = "Angioplasty procedure completed successfully"
                },
                new Patient
                {
                    PatientId = 1010,
                    Name = "Emma Wilson",
                    Age = 29,
                    Diagnosis = "Urinary Tract Infection",
                    DoctorAssigned = "Dr. Karen Martin",
                    AdmissionDate = DateTime.Now.AddDays(-2),
                    Status = "Admitted",
                    Department = "Urology",
                    PhoneNumber = "555-0110",
                    EmailAddress = "emma.wilson@email.com",
                    BloodType = "A+",
                    Notes = "Antibiotic treatment ongoing"
                }
            };
        }

        public List<Patient> GetAllPatients()
        {
            return _patients;
        }

        public Patient? GetPatientById(int patientId)
        {
            return _patients.FirstOrDefault(p => p.PatientId == patientId);
        }

        public List<Patient> FilterByDiagnosis(string diagnosis)
        {
            return _patients.Where(p => p.Diagnosis.Contains(diagnosis, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Patient> FilterByStatus(string status)
        {
            return _patients.Where(p => p.Status == status).ToList();
        }

        public List<Patient> FilterByDepartment(string department)
        {
            return _patients.Where(p => p.Department == department).ToList();
        }

        public List<Patient> Search(string searchTerm)
        {
            return _patients.Where(p =>
                p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Diagnosis.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.PatientId.ToString().Contains(searchTerm) ||
                p.DoctorAssigned.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        public void AddPatient(Patient patient)
        {
            if (patient.PatientId <= 0)
            {
                patient.PatientId = _patients.Any() ? _patients.Max(p => p.PatientId) + 1 : 1;
            }

            if (_patients.Any(p => p.PatientId == patient.PatientId))
                throw new InvalidOperationException("Patient with this ID already exists");
            _patients.Add(patient);
            PatientsChanged?.Invoke();
        }

        public void UpdatePatient(Patient patient)
        {
            var existingPatient = GetPatientById(patient.PatientId);
            if (existingPatient == null)
                throw new InvalidOperationException("Patient not found");

            var index = _patients.IndexOf(existingPatient);
            _patients[index] = patient;
            PatientsChanged?.Invoke();
        }

        public void DeletePatient(int patientId)
        {
            var patient = GetPatientById(patientId);
            if (patient != null)
            {
                _patients.Remove(patient);
                PatientsChanged?.Invoke();
            }
        }
    }
}
