using CommunityToolkit.Mvvm.ComponentModel;

namespace FluentMail.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;
}
