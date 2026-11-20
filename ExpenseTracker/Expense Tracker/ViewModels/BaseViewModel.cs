using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartExpense;

/// <summary>
/// BaseViewModel serves as the foundational class for all view models in the application, providing common properties and functionality. It includes properties for tracking busy state and the title of the view, which can be utilized by derived view models to maintain consistency across the application's user interface.
/// </summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;
}
