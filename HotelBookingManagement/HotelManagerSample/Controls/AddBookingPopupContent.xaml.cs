using HotelManagerSample;
using Syncfusion.Maui.Calendar;

namespace HotelManagerSample.Controls;

/// <summary>
/// Modal content for the Add-Booking popup. Hosts all the entries,
/// calendar and combo-box fields and forwards Commit / Cancel back to the
/// host page through two events. The popup itself lives on
/// <c>BookingsPage.xaml</c>. The view-model owns all validation,
/// booking-assembly and shared-store mutation; this code-behind only
/// translates taps into VM calls and rebroadcasts the result.
/// </summary>
public partial class AddBookingPopupContent : ContentView
{
    /// <summary>Fired when the user has submitted a fully-validated
    /// reservation; the host page finalises the booking using the supplied
    /// <see cref="AddBookingFormResult"/> payload.</summary>
    public event EventHandler<AddBookingFormResult>? Committed;

    /// <summary>Fired when the user dismissed the popup without saving.</summary>
    public event EventHandler? Cancelled;

    private readonly AddBookingViewModel _viewModel;

    /// <summary>Parameterless constructor used by XAML inflation. Resolves
    /// the shared view-model from <see cref="App.Resolve{T}"/>.</summary>
    public AddBookingPopupContent() : this(App.Resolve<AddBookingViewModel>())
    {
    }

    /// <summary>Constructor used by the host page when it wants to
    /// inject a custom <paramref name="viewModel"/> instance (typically
    /// in tests).</summary>
    /// <param name="viewModel">The view-model to bind to.</param>
    public AddBookingPopupContent(AddBookingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Project rooms for the auto-selected first floor BEFORE the
        // SfChipGroup has had a chance to raise its SelectionChanged
        // event. Doing this synchronously in the ctor means the
        // AvailableRooms dropdown is populated on the very first paint
        // of the popup — without needing the user to re-tap the
        // already-highlighted first chip. SfChipGroup with ChipType=Choice
        // also raises SelectionChanged on the initial render, which we
        // handle idempotently in OnSfChipGroupSelectionChanged.
        _viewModel.SetSelectedFloor(_viewModel.SelectedFloor);
    }

    /// <summary>Raises the <see cref="Cancelled"/> event so the host
    /// page can close the popup without committing.</summary>
    /// <param name="sender">The close glyph. Unused.</param>
    /// <param name="e">Event payload. Unused.</param>
    private void OnCloseTapped(object? sender, EventArgs e)
        => Cancelled?.Invoke(this, EventArgs.Empty);

    // ───── Steppers ─────
    /// <summary>Increments the adult count via the view-model.</summary>
    /// <param name="sender">The plus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnAdultPlusTapped(object? sender, TappedEventArgs e)   => _viewModel.IncrementAdults();
    /// <summary>Decrements the adult count via the view-model.</summary>
    /// <param name="sender">The minus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnAdultMinusTapped(object? sender, TappedEventArgs e)  => _viewModel.DecrementAdults();
    /// <summary>Increments the children count via the view-model.</summary>
    /// <param name="sender">The plus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnChildPlusTapped(object? sender, TappedEventArgs e)   => _viewModel.IncrementChildren();
    /// <summary>Decrements the children count via the view-model.</summary>
    /// <param name="sender">The minus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnChildMinusTapped(object? sender, TappedEventArgs e)  => _viewModel.DecrementChildren();

    // ───── Inline calendar toggle ─────
    /// <summary>Toggles the inline calendar dropdown open/closed.</summary>
    /// <param name="sender">The chevron button. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnCalendarToggleTapped(object? sender, TappedEventArgs e)
        => _viewModel.ToggleCalendar();

    // ───── Phone entry — strip non-digits & cap to 10 chars ─────
    /// <summary>Strips non-digit characters from the phone entry, caps
    /// the value at 10 digits, and pushes the cleaned value into the
    /// view-model. Mirrors the cleanup back into the Entry when needed
    /// so a paste containing "+1 (555) 123-4567" lands as "5551234567".
    /// The actual rule lives in
    /// <see cref="PhoneSanitiser"/> so the mobile New-Booking wizard
    /// never drifts out of sync.</summary>
    /// <param name="sender">The <see cref="Entry"/> that fired the
    /// event. Used to write the sanitised text back into the control.</param>
    /// <param name="e">Standard <see cref="TextChangedEventArgs"/>
    /// payload containing the new value.</param>
    private void OnPhoneValueChanged(object? sender, TextChangedEventArgs e)
    {
        var raw        = e.NewTextValue ?? string.Empty;
        var digitsOnly = PhoneSanitiser.Clean(raw);

        // Push the cleaned value into the VM so CanSubmit / TryValidate
        // observe a sanitised string.
        if (!string.Equals(_viewModel.Phone, digitsOnly, StringComparison.Ordinal))
            _viewModel.Phone = digitsOnly;

        // If user typed/pasted something we cleaned up, mirror that
        // cleanup back into the entry. Both string.Equals guards
        // protect against an infinite TextChanged loop.
        if (!string.Equals(raw, digitsOnly, StringComparison.Ordinal)
            && sender is Entry entry
            && !string.Equals(entry.Text, digitsOnly, StringComparison.Ordinal))
        {
            entry.Text = digitsOnly;
        }
    }

    // ───── Commit ─────
    private void OnAddBookingTapped(object? sender, EventArgs e)
    {
        var result = _viewModel.TryCreateBooking();
        if (result is null)
            return; // error message is already projected into ErrorMessage
        Committed?.Invoke(this, result);
    }

    private void Calendar_SelectionChanged(object sender, CalendarSelectionChangedEventArgs e)
    {

        if (BindingContext is AddBookingViewModel vm &&
            vm.SelectedDateRange != null &&
            vm.SelectedDateRange.StartDate.HasValue &&
            vm.SelectedDateRange.EndDate.HasValue)
        {          
             vm.IsCalendarOpen = false;
        }
    }

    private void SfChipGroup_SelectionChanged(object sender, Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs e)
    {
        // Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs exposes
        // single-value AddedItem + RemovedItem properties (not the
        // plural AddedItems seen in MAUI SelectionChangedEventArgs /
        // WinUI NotifyCollectionChangedEventArgs). With ChipType=Choice
        // + ChoiceMode=Single the group only ever emits one AddedItem
        // element at a time, and the same handler also fires once on
        // the initial render for the auto-selected first chip —
        // SetSelectedFloor handles that idempotently.
        if (e is null || e.AddedItem is null)
        {
            // User deselected everything; clear the selected floor so
            // the warning strip appears.
            _viewModel.SetSelectedFloor(string.Empty);
            return;
        }

        if (e.AddedItem is not FloorOption option)
            return;

        _viewModel.SetSelectedFloor(option.FloorNumber);
    }
}