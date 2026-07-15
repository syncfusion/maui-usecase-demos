using HospitalManagement.Models;
using HospitalManagement.Services;

namespace HospitalManagement.View;

public partial class AddPatientPage : ContentPage
{
    private PatientService _patientService;

    public AddPatientPage()
    {
        InitializeComponent();
        _patientService = new PatientService();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        #pragma warning disable CS0618
        // Validate inputs
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlertAsync("Validation Error", "Please enter patient name", "OK");
            return;
        }

        if (!int.TryParse(AgeEntry.Text, out int age))
        {
            await DisplayAlertAsync("Validation Error", "Please enter a valid age", "OK");
            return;
        }

        if (StatusPicker.SelectedIndex < 0)
        {
            await DisplayAlertAsync("Validation Error", "Please select a status", "OK");
            return;
        }

        // Create new patient object
        var newPatient = new Patient
        {
            Name = NameEntry.Text,
            Age = age,
            BloodType = BloodTypeEntry.Text,
            PhoneNumber = PhoneEntry.Text,
            EmailAddress = EmailEntry.Text,
            Diagnosis = DiagnosisEntry.Text,
            Department = DepartmentEntry.Text,
            DoctorAssigned = DoctorEntry.Text,
            Status = StatusPicker.Items[StatusPicker.SelectedIndex],
            AdmissionDate = DateTime.Now,
            Notes = NotesEditor.Text
        };

        // Add patient to service
        _patientService.AddPatient(newPatient);

        // Show success message
        await DisplayAlertAsync("Success", "Patient added successfully", "OK");

        // Return to the patient list
        await Shell.Current.GoToAsync("..");
        #pragma warning restore CS0618
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        // Return to the patient list
        await Shell.Current.GoToAsync("..");

    }
}
