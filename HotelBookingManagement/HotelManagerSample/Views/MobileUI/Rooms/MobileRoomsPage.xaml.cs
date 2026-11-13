using HotelManagerSample;
using ChipSelectionChangedEventArgs = Syncfusion.Maui.Core.Chips.SelectionChangedEventArgs;

namespace HotelManagerSample.Rooms;

/// <summary>
/// Code-behind for the mobile Rooms page. Pure navigation glue — the
/// heavy lifting is owned by <see cref="MobileRoomsViewModel"/>.
/// </summary>
public partial class MobileRoomsPage : ContentView
{
    private readonly MobileRoomsViewModel _vm;

    /// <summary>Resolves the shared view-model from DI and binds it as
    /// the <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileRoomsPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileRoomsViewModel>();
        BindingContext = _vm;
    }

    // ============== Floor chip selection ==============

    /// <summary>Forwards the chosen room filter chip's label into the
    /// VM's <see cref="MobileRoomsViewModel.ActiveFloorFilter"/>
    /// setter. The chip group runs in Choice mode so selecting a
    /// different chip fires exactly one invocation.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnFloorChipSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        var chip = e?.AddedItem as RoomFilterChip;
        _vm.ActiveFloorFilter = chip?.Label ?? MobileRoomsViewModel.FilterAll;
    }

    // ============== Status chip selection ==============

    /// <summary>Forwards the chosen status chip's label into the VM's
    /// <see cref="MobileRoomsViewModel.ActiveStatusFilter"/> setter.</summary>
    /// <param name="sender">The SfChipGroup. Unused.</param>
    /// <param name="e">Chip-selection-changed payload from Syncfusion.</param>
    private void OnStatusChipSelectionChanged(object sender, ChipSelectionChangedEventArgs e)
    {
        var chip = e?.AddedItem as RoomFilterChip;
        _vm.ActiveStatusFilter = chip?.Label ?? MobileRoomsViewModel.FilterAll;
    }

    /// <summary>Clears the inline search entry on tap of the trailing
    /// "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
        => _vm.SearchText = string.Empty;
}
