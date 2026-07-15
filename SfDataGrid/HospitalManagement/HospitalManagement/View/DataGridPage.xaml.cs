using HospitalManagement.Services;
using HospitalManagement.ViewModels;

namespace HospitalManagement.View;

public partial class DataGridPage : ContentPage
{
	private PatientRecordsViewModel viewModel;

	public DataGridPage()
	{
		InitializeComponent();
		viewModel = new PatientRecordsViewModel();
		BindingContext = viewModel;
		PatientService.PatientsChanged += OnPatientsChanged;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		RefreshPatientsGrid();
	}

	private void RefreshPatientsGrid()
	{
		Dispatcher.Dispatch(async () =>
		{
			await Task.Delay(100);
			viewModel.LoadPatients();
			PatientDataGrid.InvalidateMeasure();
		});
	}

	private void OnPatientsChanged()
	{
		MainThread.BeginInvokeOnMainThread(RefreshPatientsGrid);
	}

	private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
	{
		viewModel.SearchText = e.NewTextValue ?? string.Empty;
	}

	private void OnFilterChanged(object sender, EventArgs e)
	{
		var picker = (Picker)sender;
		if (picker.SelectedItem is string selectedStatus)
		{
			viewModel.SelectedFilter = selectedStatus;
		}
	}

	private void OnRefreshClicked(object sender, EventArgs e)
	{
		SearchBar.Text = string.Empty;
		viewModel.SearchText = string.Empty;
		viewModel.LoadPatients();
		System.Diagnostics.Debug.WriteLine("Patient records refreshed successfully");
	}

	private async void OnAddPatientClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("addpatient");
	}

	private void OnDataGridSelectionChanged(object sender, Syncfusion.Maui.DataGrid.DataGridSelectionChangedEventArgs e)
	{
		var addedRows = e.AddedRows;
		if (addedRows != null && addedRows.Count > 0)
		{
			var selectedPatient = addedRows[0];
			// TODO: Navigate to patient details page with selected patient
			System.Diagnostics.Debug.WriteLine($"Patient Selected: {selectedPatient}");
		}
	}
}