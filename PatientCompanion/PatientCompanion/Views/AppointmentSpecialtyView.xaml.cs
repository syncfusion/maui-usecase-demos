using PatientCompanion.Models;
using PatientCompanion.ViewModels;

namespace PatientCompanion.Views;

public partial class AppointmentSpecialtyView : ContentView
{
    public AppointmentSpecialtyView(
        AppointmentViewModel appointmentViewModel)
    {
        InitializeComponent();
        BindingContext = new AppointmentSpecialtyViewModel(appointmentViewModel);
    }

    private void SpecialtyChipGroup_SelectionChanged(
        object sender,
        EventArgs e)
    {
        if (BindingContext is AppointmentSpecialtyViewModel vm)
        {
            var chipGroup =
                sender as Syncfusion.Maui.Core.SfChipGroup;

            var selectedItem =
                chipGroup?.SelectedItem as SpecialtyItem;

            if (selectedItem != null)
            {
                vm.SelectedSpecialty = selectedItem;
            }
        }
    }
}
