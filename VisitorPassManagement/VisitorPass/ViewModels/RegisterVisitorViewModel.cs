using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VisitorPass.Models;
using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// VP-070 — ViewModel for the registration surface (Phase 7).
///
/// A 3-step wizard that produces a new <see cref="Visitor"/> on
/// submit. Step 1 collects identity (visitor name, host, purpose,
/// contact number, vehicle). Step 2 collects scheduling (date,
/// time, duration). Step 3 is the success / confirmation surface
/// (the Digital Pass appears there once <see cref="SubmitCommand"/>
/// has finished).
///
/// Validation is centralised in <see cref="ValidateStep1"/> and
/// <see cref="ValidateStep2"/> so the XAML's "Next" buttons can
/// bind via [RelayCommand(CanExecute = …)] without imperative
/// checks in code-behind. Validation errors populate
/// <see cref="ValidationErrors"/> so the page can show an inline
/// error banner via the standard <c>StringNotEmptyToBoolConverter</c>.
/// </summary>
public partial class RegisterVisitorViewModel : BaseViewModel
{
    /// <summary>Number of steps in the wizard.</summary>
    public const int TotalSteps = 3;

    private readonly IVisitorStore _store;
    private readonly PassCodeGenerator _passCodes;

    /// <summary>
    /// 1-based current step. Bound to the <c>SfStepProgressBar</c>'s
    /// <c>SelectedIndex</c> via <see cref="StepIndex"/> (zero-based)
    /// and the action buttons' <c>IsVisible</c> toggles.
    /// </summary>
    [ObservableProperty]
    private int _currentStep = 1;

    /// <summary>Zero-based step index for Syncfusion step progress bars.</summary>
    public int StepIndex => CurrentStep - 1;

    /// <summary>Display label for the current step (used in the header).</summary>
    public string StepHeading => CurrentStep switch
    {
        1 => "Step 1 of 3 — Visitor Information",
        2 => "Step 2 of 3 — Schedule",
        3 => "Step 3 of 3 — Pass Generated",
        _ => string.Empty,
    };

    partial void OnCurrentStepChanged(int value) =>
        OnPropertyChanged(nameof(StepIndex));

    // ----- Step 1 fields -----
    [ObservableProperty] private string _visitorName = string.Empty;
    [ObservableProperty] private string _host = string.Empty;
    [ObservableProperty] private string _purpose = string.Empty;
    [ObservableProperty] private string _contactNumber = string.Empty;
    [ObservableProperty] private string? _vehicleNumber;

    // ----- Step 2 fields -----
    [ObservableProperty] private DateTime _visitDate = DateTime.Today;
    [ObservableProperty] private TimeSpan _visitTime = new(9, 0, 0);
    [ObservableProperty] private int _expectedDurationMinutes = 60;

    [ObservableProperty]
    private bool hasVisitorNameError;

    [ObservableProperty]
    private bool hasContactNumberError;

    [ObservableProperty]
    private bool hasHostError;

    [ObservableProperty]
    private bool hasPurposeError;

    [ObservableProperty]
    private bool hasVisitDateError;

    [ObservableProperty]
    private bool hasDurationError;

    public string VisitDateDisplay =>
    VisitDate.ToString("dd MMM yyyy");

    public string VisitTimeDisplay =>
        VisitTime.ToString(@"hh\:mm");
    /// <summary>
    /// Available duration presets (15 / 30 / 60 / 120 / 240 minutes)
    /// for the <c>SfComboBox</c> in the schedule step.
    /// </summary>
    public IReadOnlyList<int> DurationPresets { get; } = new[] { 15, 30, 60, 120, 240 };

    /// <summary>
    /// Available host names from the predefined list (rendered as
    /// <c>SfComboBox</c> choices).
    /// </summary>
    public IReadOnlyList<string> HostOptions { get; } = new[]
    {
        "Anandh Krishnan",
        "Priya Sundaram",
        "Vikram Iyer",
        "Lakshmi Narayanan",
        "Arjun Venkatesh",
        "Reception Desk",
    };

    private void ResetValidation()
    {
        HasVisitorNameError = false;
        HasContactNumberError = false;
        HasHostError = false;
        HasPurposeError = false;
        HasVisitDateError = false;
        HasDurationError = false;
    }

    /// <summary>
    /// Available visit purposes (rendered as <c>SfComboBox</c>
    /// choices). Strings here must equal the value of
    /// <c>VisitPurposeExtensions.Display</c>.
    /// </summary>
    public IReadOnlyList<string> PurposeOptions { get; } = VisitPurposeExtensions.All
        .Select(p => p.Display())
        .ToArray();

    /// <summary>
    /// Inverted-true when <see cref="ValidationErrors"/> is empty.
    /// Drives the inline error banner's <c>IsVisible</c>.
    /// </summary>
    public bool IsValid => ValidationErrors.Count == 0;

    /// <summary>
    /// Mutable list of human-readable validation error messages.
    /// Bound to the page's inline error banner; cleared and rebuilt
    /// on every <c>Validate…</c> pass.
    /// </summary>
    public ObservableCollection<string> ValidationErrors { get; } = new();

    /// <summary>
    /// The newly-created <see cref="Visitor"/> after <see cref="SubmitCommand"/>
    /// succeeds. The page reads this to navigate to the Digital Pass
    /// screen with the new visitor's PassId.
    /// </summary>
    public Visitor? CreatedVisitor { get; private set; }

    /// <summary>Raised after <see cref="SubmitCommand"/> produces a visitor.</summary>
    public event EventHandler<Visitor>? Submitted;

    public RegisterVisitorViewModel(IVisitorStore store, PassCodeGenerator passCodes)
    {
        _store = store;
        _passCodes = passCodes;
        ValidationErrors.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsValid));
    }

    /// <summary>
    /// "Next" button on Step 1. Validates the required identity
    /// fields and advances to Step 2.
    /// </summary>
    [RelayCommand]
    private void NextFromStep1()
    {
        if (!ValidateStep1()) return;
        ValidationErrors.Clear();
        CurrentStep = 2;
    }

    /// <summary>
    /// "Back" button on Step 2. Returns to Step 1 without validating.
    /// </summary>
    [RelayCommand]
    private void BackFromStep2()
    {
        CurrentStep = 1;
    }

    /// <summary>
    /// "Generate Pass" button on Step 2. Validates scheduling,
    /// constructs the <see cref="Visitor"/>, appends to the store,
    /// and advances to Step 3.
    /// </summary>
    [RelayCommand]
    private void Submit()
    {
        if (!ValidateStep1() || !ValidateStep2())
        {
            return;
        }

        ValidationErrors.Clear();

        var visitorCount = _store.Visitors.Count;

        var newId = _passCodes.Next(VisitDate, visitorCount);
        var visitor = new Visitor
        {
            PassId = newId,
            VisitorName = VisitorName.Trim(),
            Host = Host.Trim(),
            Purpose = Purpose.Trim(),
            ContactNumber = ContactNumber.Trim(),
            VehicleNumber = string.IsNullOrWhiteSpace(VehicleNumber) ? null : VehicleNumber!.Trim(),
            VisitDate = VisitDate.Date,
            VisitTime = VisitTime,
            ExpectedDurationMinutes = ExpectedDurationMinutes,
            Status = VisitorStatus.Expected,
            PassCode = newId,
        };

        _store.AddVisitor(visitor);
        CreatedVisitor = visitor;
        CurrentStep = 3;
        Submitted?.Invoke(this, visitor);
    }

    /// <summary>
    /// "Cancel" button. The page code-behind listens for this and
    /// dismisses the registration surface (popup / modal sheet).
    /// </summary>
    public event EventHandler? CancelRequested;

    [RelayCommand]
    private void RequestCancel() => CancelRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Validates Step 1 identity fields. Returns true when every
    /// required field is non-empty; otherwise populates
    /// <see cref="ValidationErrors"/> and returns false.
    /// </summary>
    private bool ValidateStep1()
    {
        ValidationErrors.Clear();
        ResetValidation();

        if (string.IsNullOrWhiteSpace(VisitorName))
        {
            HasVisitorNameError = true;
            ValidationErrors.Add("Visitor name is required.");
        }

        if (string.IsNullOrWhiteSpace(Host))
        {
            HasHostError = true;
            ValidationErrors.Add("Host is required.");
        }

        if (string.IsNullOrWhiteSpace(Purpose))
        {
            HasPurposeError = true;
            ValidationErrors.Add("Purpose is required.");
        }

        var digitsOnlyContact =
            new string((ContactNumber ?? string.Empty)
            .Where(char.IsDigit)
            .ToArray());

        if (digitsOnlyContact.Length != 10)
        {
            HasContactNumberError = true;
            ValidationErrors.Add("Contact number must contain exactly 10 digits.");
        }

        return ValidationErrors.Count == 0;
    }

    /// <summary>
    /// Validates Step 2 scheduling fields. The visit date must be
    /// today or in the future; the duration must be one of the
    /// preset values.
    /// </summary>
    private bool ValidateStep2()
    {
        ValidationErrors.Clear();

        HasVisitDateError = false;
        HasDurationError = false;

        if (VisitDate.Date < DateTime.Today)
        {
            HasVisitDateError = true;
            ValidationErrors.Add("Visit date cannot be in the past.");
        }

        if (!DurationPresets.Contains(ExpectedDurationMinutes))
        {
            HasDurationError = true;
            ValidationErrors.Add("Duration must be one of the available presets.");
        }

        return ValidationErrors.Count == 0;
    }
}
