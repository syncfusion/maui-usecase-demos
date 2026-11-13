using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;
using Syncfusion.Maui.Core.Chips;
using HotelManagerSample.Controls;

namespace HotelManagerSample.Bookings;

/// <summary>
/// Code-behind for the mobile New Booking wizard. Wires the field
/// handlers (steppers, phone sanitisation, chip selection), then on Create
/// it forwards to <see cref="MobileNewBookingViewModel.CreateBooking"/>
/// which commits the reservation through the shared
/// <see cref="HotelManagerSample.IHotelStore"/>. The Modal-pushed
/// page is popped on success.
/// </summary>
public partial class MobileNewBookingPage : ContentPage
{
    private readonly MobileNewBookingViewModel _vm;

    /// <summary>Resolves the wizard VM from DI and binds it as the
    /// page's <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileNewBookingPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileNewBookingViewModel>();
        BindingContext = _vm;
    }

    // ============== Header close ==============

    /// <summary>Pops the wizard modal back to the underlying
    /// SfTabView host. Goes through <see cref="Navigation"/> instead
    /// of <c>Shell.GoToAsync</c> because some MAUI shell hosts leave
    /// <c>Shell.Current</c> null on this configuration.</summary>
    /// <param name="sender">The close glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private async void OnCloseTapped(object? sender, TappedEventArgs e)
    {
        // Pop the wizard modal. We go straight through the page's
        // Navigation property — Shell.GoToAsync would dereference
        // Shell.Current which is null when the host is the SfTabView
        // ContentPage (some MAUI shells don't expose GoToAsync in this
        // configuration, leading to a NullReferenceException at runtime).
        await Navigation.PopModalAsync();
    }

    // ============== Steppers ==============

    /// <summary>Increments the wizard's adult count by one.</summary>
    /// <param name="sender">The plus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnAdultPlusTapped(object? sender, TappedEventArgs e)  => _vm.Adults++;
    /// <summary>Decrements the wizard's adult count by one.</summary>
    /// <param name="sender">The minus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnAdultMinusTapped(object? sender, TappedEventArgs e) => _vm.Adults--;
    /// <summary>Increments the wizard's child count by one.</summary>
    /// <param name="sender">The plus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnChildPlusTapped(object? sender, TappedEventArgs e)  => _vm.Children++;
    /// <summary>Decrements the wizard's child count by one.</summary>
    /// <param name="sender">The minus glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnChildMinusTapped(object? sender, TappedEventArgs e) => _vm.Children--;

    // ============== Phone sanitisation ==============
    // Phone must be exactly 10 digits. We strip everything that isn't a
    // digit and clamp the length so the field can never hold a country-code
    // prefix, a dash, or a stray 11th character. Mirrors the cleaned value
    // back into the Entry so a paste containing "+1 (555) 123-4567" lands
    // in the field as "5551234567". The actual rule lives in
    // HotelManagerSample.Controls.PhoneSanitiser so the desktop Add-Booking
    // popup never drifts out of sync.

    /// <summary>Strips non-digit characters from the phone entry and
    /// clamps its length to 10 so the VM's validator never sees an
    /// over-long value.</summary>
    /// <param name="sender">The <see cref="Entry"/> that fired the
    /// event. Used to write the sanitised text back into the control.</param>
    /// <param name="e">Standard <see cref="TextChangedEventArgs"/>
    /// payload containing the new value.</param>
    private void OnPhoneTextChanged(object? sender, TextChangedEventArgs e)
    {
        var raw = e.NewTextValue ?? string.Empty;
        var digitsOnly = PhoneSanitiser.Clean(raw);

        if (!string.Equals(raw, digitsOnly, StringComparison.Ordinal)
            && sender is Entry entry
            && !string.Equals(entry.Text, digitsOnly, StringComparison.Ordinal))
        {
            entry.Text = digitsOnly;
        }
    }

    // ============== Floor chip selection ==============

    /// <summary>Forwards the floor chip selection onto the VM.
    /// Empty selection clears the VM's choice so the wizard falls back
    /// to the "select a floor" placeholder.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnFloorChipSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        if (e?.AddedItem is MobileFloorOption option)
            _vm.SelectedFloor = option.FloorNumber;
        else
            _vm.SelectedFloor = string.Empty;
    }

    // ============== Submit ==============

    /// <summary>Handles the Create Booking tap. Returns early when
    /// validation failed (the VM has already pushed the offending
    /// message into <see cref="MobileNewBookingViewModel.ErrorMessage"/>).
    /// On success pops the modal back to the SfTabView host.</summary>
    /// <param name="sender">The Create Booking button. Unused.</param>
    /// <param name="e">Event payload. Unused.</param>
    private async void OnCreateBookingClicked(object? sender, EventArgs e)
    {
        if (!_vm.CreateBooking())
        {
            // TryValidate already pushed the message into ErrorMessage;
            // nothing else to do. The user remains on the page so they
            // can correct the failures.
            return;
        }

        // Pop the wizard modal back to the underlying SfTabView host.
        // PopModalAsync returns false when nothing is on the stack
        // (e.g. on hot-reload race) — swallow that case instead of
        // throwing, which previously crashed on a Shell-less host.
        try
        {
            await Navigation.PopModalAsync();
        }
        catch
        {
            // Ignore — no-op pop is preferable to a hard crash.
        }
    }
}
