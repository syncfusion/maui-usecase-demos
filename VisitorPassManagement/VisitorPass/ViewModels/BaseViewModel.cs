using CommunityToolkit.Mvvm.ComponentModel;

namespace VisitorPass.ViewModels;

/// <summary>
/// Common base for every ViewModel in the app. Inherits
/// <see cref="ObservableObject"/> so derived VMs get
/// <c>[ObservableProperty]</c> + <c>[RelayCommand]</c> source
/// generators for free. All page-level VMs inherit this class per
/// HARNESS §6 rule 3 ("MVVM only").
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
}
