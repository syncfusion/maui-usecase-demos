using VisitorPass.Models;
using VisitorPass.ViewModels;

namespace VisitorPass.Views;

/// <summary>
/// VP-071..074 — Reusable ContentView hosting the registration form
/// layout. See VP-070..074 comments for the full architecture; this
/// code-behind only forwards two VM lifecycle events so any
/// container that hosts this view directly (SfPopup on
/// desktop/tablet, phone overlay on phone) still gets the dismiss
/// and navigate-to-details cues:
///
///   - RegisterVisitorViewModel.CancelRequested -> DismissRequested
///   - RegisterVisitorViewModel.Submitted       -> PassGenerated
///
/// The view rebinds the lifecycle events each time its BindingContext
/// changes so the form can be reused with fresh VMs (per
/// MauiProgram.cs: AddTransient for the registration VM).
/// </summary>
public partial class RegisterVisitorView : ContentView
{
    private RegisterVisitorViewModel? _attachedVm;

    /// <summary>Raised when the user taps "Cancel".</summary>
    public event EventHandler? DismissRequested;

    /// <summary>Raised after a successful Generate Pass.</summary>
    public event EventHandler<Visitor>? PassGenerated;

    public RegisterVisitorView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        // Detach from the old VM and reattach to the new one so we
        // keep the same dispatcher pattern as a one-shot handler.
        if (_attachedVm is not null)
        {
            _attachedVm.CancelRequested -= OnVmCancelRequested;
            _attachedVm.Submitted -= OnVmSubmitted;
        }
        _attachedVm = BindingContext as RegisterVisitorViewModel;
        if (_attachedVm is not null)
        {
            _attachedVm.CancelRequested += OnVmCancelRequested;
            _attachedVm.Submitted += OnVmSubmitted;
        }
    }

    private void OnVmCancelRequested(object? sender, EventArgs e)
        => DismissRequested?.Invoke(this, EventArgs.Empty);

    private void OnVmSubmitted(object? sender, Visitor visitor)
        => PassGenerated?.Invoke(this, visitor);

    private void datePickerButton_Clicked(object sender, EventArgs e)
    {
        datePicker.IsOpen = true;
    }

    private void timePickerButton_Clicked(object sender, EventArgs e)
    {
        timePicker.IsOpen = true;
    }

    private void timePicker_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.Picker.TimePickerSelectionChangedEventArgs e)
    {
        if (BindingContext is RegisterVisitorViewModel vm &&
        e.NewValue is TimeSpan time)
        {
            vm.VisitTime = time;
        }

        timePickerButton.Text = e.NewValue.ToString();
    }

    private void datePicker_SelectionChanged(object sender, Syncfusion.Maui.Toolkit.Picker.DatePickerSelectionChangedEventArgs e)
    {
        if (BindingContext is RegisterVisitorViewModel vm &&
        e.NewValue is DateTime date)
        {
            vm.VisitDate = date;
        }
        datePickerButton.Text = e.NewValue.ToString();
    }
}
