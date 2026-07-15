namespace HospitalManagement.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string DoctorAssigned { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string Status { get; set; } = string.Empty; // Admitted, Discharged, Under Treatment
        public string Department { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
