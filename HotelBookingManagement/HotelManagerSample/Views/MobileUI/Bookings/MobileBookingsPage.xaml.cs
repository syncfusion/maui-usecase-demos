using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;

namespace HotelManagerSample.Bookings;

/// <summary>
/// Code-behind for the mobile bookings page.  Reuses the master
/// <see cref="MobileBookingsViewModel"/>, which itself already listens
/// to the shared <see cref="IHotelStore"/> so reservation changes are
/// reflected without ceremony.
/// </summary>
public partial class MobileBookingsPage : ContentView
{
    private readonly MobileBookingsViewModel _vm;

    /// <summary>Resolves the shared view-model from DI and binds it as
    /// the <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileBookingsPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileBookingsViewModel>();
        BindingContext = _vm;
    }

    // ============== Filter chip selection ==============

    /// <summary>SfChipGroup fires SelectionChanged with the chosen
    /// BookingFilterChip instance. We forward the chip's label into the
    /// VM's ActiveFilter setter, which kicks off ApplyFilters.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnChipGroupSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        // AddedItem is the selected chip. With ChoiceMode=Single there is
        // always exactly one. If the AddedItem is null the user
        // deselected everything — fall back to "All".
        var chip = e?.AddedItem as BookingFilterChip;
        _vm.ActiveFilter = chip?.Label ?? MobileBookingsViewModel.FilterAll;
    }

    /// <summary>Clears the inline search entry on tap of the trailing
    /// "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
        => _vm.SearchText = string.Empty;
}
