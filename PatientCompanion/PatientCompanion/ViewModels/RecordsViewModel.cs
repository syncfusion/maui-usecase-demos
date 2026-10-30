using CommunityToolkit.Mvvm.ComponentModel;
using PatientCompanion.Models;
using System.Collections.ObjectModel;

namespace PatientCompanion.ViewModels;

public partial class RecordsViewModel : ObservableObject
{
    public ObservableCollection<RecordItem> MedicalRecords { get; }

    public RecordsViewModel()
    {
        MedicalRecords = new ObservableCollection<RecordItem>
        {
            new()
            {
                Title = "Comprehensive Metabolic Panel",
                Description = "Lab Results • Oct 24, 2026 • LabCorp",
                FileName = "CMP_Report.pdf",
                Icon = MaterialIcons.Science,
                IconBackgroundColor = "#DDE8FF",
                IconColor = "#4F7CF3"
            },

            new()
            {
                Title = "Lisinopril 10mg",
                Description = "Prescription • Oct 12, 2026 • Dr. Smith",
                FileName = "Lisinopril_Prescription.pdf",
                Icon = MaterialIcons.Prescription,
                IconBackgroundColor = "#DDF3EF",
                IconColor = "#0F766E"
            },

            new()
            {
                Title = "Influenza Vaccine",
                Description = "Immunization • Sep 15, 2026 • CVS Pharmacy",
                FileName = "Influenza_Vaccine_Record.pdf",
                Icon = MaterialIcons.Vaccine,
                IconBackgroundColor = "#E5E7EB",
                IconColor = "#64748B"
            }
        };
    }
}
