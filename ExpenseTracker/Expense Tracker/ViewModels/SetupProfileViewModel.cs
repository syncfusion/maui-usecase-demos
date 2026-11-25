
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartExpense;

/// <summary>
/// ViewModel responsible for collecting and persisting basic user profile details
/// during the initial setup flow. Manages validation, UI state, and navigation signals
/// without directly handling navigation logic.
/// </summary>
public partial class SetupProfileViewModel : BaseViewModel
{
    // ---------------------------------------------------------------------
    // Fields
    // ---------------------------------------------------------------------

    private readonly AuthService _authService;

    private bool _isNextEnabled;

    // ---------------------------------------------------------------------
    // Constructors
    // ---------------------------------------------------------------------

    /// <summary>
    /// Initializes the view model with required authentication services
    /// and sets up UI commands.
    /// </summary>
    public SetupProfileViewModel(AuthService authService)
    {
        _authService = authService;

        // Initial button state
        IsNextEnabled = false;

        OpenCalendarCommand = new Command(() =>
        {
            OnOpenCalendarRequested?.Invoke();
        });

        DateSelectedCommand = new Command<DateTime?>(date =>
        {
            if (date != null)
                DateOfBirth = date;
        });
    }

    // ---------------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------------

    /// <summary>
    /// Command to request opening of the date picker/calendar UI.
    /// </summary>
    public ICommand OpenCalendarCommand { get; }

    /// <summary>
    /// Command invoked when a date is selected from the calendar.
    /// </summary>
    public ICommand DateSelectedCommand { get; }

    // ---------------------------------------------------------------------
    // Events
    // ---------------------------------------------------------------------

    /// <summary>
    /// Raised to request the UI to open the calendar control.
    /// </summary>
    public event Action? OnOpenCalendarRequested;

    /// <summary>
    /// Raised when profile setup completes successfully.
    /// Used by the caller to navigate forward.
    /// </summary>
    public event Action? LoginSucceeded;

    // ---------------------------------------------------------------------
    // Observable Properties (User Inputs)
    // ---------------------------------------------------------------------

    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private DateTime? dateOfBirth = DateTime.Now;

    [ObservableProperty]
    private string? selectedGender;

    [ObservableProperty]
    private string? selectedCurrency;

    // ---------------------------------------------------------------------
    // UI State Properties
    // ---------------------------------------------------------------------

    /// <summary>
    /// Indicates whether the Next button is enabled based on validation.
    /// </summary>
    public bool IsNextEnabled
    {
        get => _isNextEnabled;
        private set => SetProperty(ref _isNextEnabled, value);
    }

    // ---------------------------------------------------------------------
    // Dropdown Data Sources
    // ---------------------------------------------------------------------

    /// <summary>
    /// Supported gender options.
    /// </summary>
    public ObservableCollection<string> Genders { get; } =
        new() { "Male", "Female", "Others" };

    /// <summary>
    /// Supported currency options.
    /// </summary>
    public ObservableCollection<string> Currencies { get; } =
        new() { "Dollar", "Euro", "Rupee" };

    // ---------------------------------------------------------------------
    // Shared Profile Model
    // ---------------------------------------------------------------------

    /// <summary>
    /// Profile object populated during setup and shared with the dashboard.
    /// </summary>
    public UserProfile Profile { get; } = new();

    // ---------------------------------------------------------------------
    // Property Change Hooks
    // ---------------------------------------------------------------------

    partial void OnFirstNameChanged(string value) => UpdateNextButtonState();
    partial void OnLastNameChanged(string value) => UpdateNextButtonState();
    partial void OnDateOfBirthChanged(DateTime? value) => UpdateNextButtonState();
    partial void OnSelectedGenderChanged(string? value) => UpdateNextButtonState();

    // ---------------------------------------------------------------------
    // Commands (RelayCommand)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Skips profile setup and continues as a guest user.
    /// </summary>
    [RelayCommand]
    private async Task SkipAsync()
    {
        Profile.FirstName = "Guest";
        Profile.LastName = string.Empty;

        // Persist guest name for dashboard display
        try
        {
            await _authService.SetSessionUsernameAsync(Profile.FirstName);
        }
        catch
        {
            // Intentionally ignored to preserve behavior
        }

        LoginSucceeded?.Invoke();
    }

    /// <summary>
    /// Saves entered profile details and proceeds to the next step.
    /// </summary>
    [RelayCommand]
    private async Task NextAsync()
    {
        Profile.FirstName = FirstName.Trim();
        Profile.LastName = LastName.Trim();
        Profile.DateOfBirth = DateOfBirth;
        Profile.Gender = SelectedGender;
        Profile.Currency = SelectedCurrency;

        var nameToUse =
            string.IsNullOrWhiteSpace(Profile.FirstName)
                ? "Guest"
                : Profile.FirstName;

        try
        {
            await _authService.SetSessionUsernameAsync(nameToUse);
        }
        catch
        {
            // Intentionally ignored to preserve behavior
        }

        LoginSucceeded?.Invoke();
    }

    // ---------------------------------------------------------------------
    // Private Helpers
    // ---------------------------------------------------------------------

    /// <summary>
    /// Updates the enabled state of the Next button
    /// based on current input validation rules.
    /// </summary>
    private void UpdateNextButtonState()
    {
        IsNextEnabled =
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            DateOfBirth.HasValue &&
            !string.IsNullOrWhiteSpace(SelectedGender);
    }
}