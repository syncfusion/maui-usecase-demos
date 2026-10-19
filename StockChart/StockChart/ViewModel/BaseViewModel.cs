using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.ViewModel;

/// <summary>
/// Base ViewModel class that provides common functionality for all ViewModels.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the ViewModel is currently busy.
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    /// <summary>
    /// Gets or sets the error message to display when an error occurs.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the ViewModel is not busy.
    /// </summary>
    public bool IsNotBusy => !IsBusy;
    
    /// <summary>
    /// Gets a value indicating whether an error has occurred.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Initializes the ViewModel asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task InitializeAsync() => Task.CompletedTask;
}