using HospitalManagement.ViewModels;

namespace HospitalManagement.View;

public partial class PatientDetailsPage : ContentPage
{
    private PatientDetailsViewModel viewModel;

    public PatientDetailsPage()
    {
        InitializeComponent();
        viewModel = new PatientDetailsViewModel();
        BindingContext = viewModel;
    }

    public void LoadPatient(int patientId)
    {
        viewModel.LoadPatient(patientId);
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        viewModel.EditPatient();
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        viewModel.SavePatient();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert("Success", "Patient information saved successfully", "OK");
        });
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        viewModel.CancelEdit();
    }
}
