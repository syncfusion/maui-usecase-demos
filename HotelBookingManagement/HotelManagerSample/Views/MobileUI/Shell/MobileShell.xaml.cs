using HotelManagerSample.Bookings;

namespace HotelManagerSample;


/// <summary>
/// Hosts the four mobile pages inside a Syncfusion SfTabView. The brief
/// is explicit that there must be exactly three tabs (Dashboard,
/// Bookings, Rooms) — no fourth tab, no overflow menu, no More Tabs
/// grouping.
/// </summary>
public partial class MobileShell : ContentPage
{
    private readonly MobileShellViewModel _vm;

    /// <summary>Resolves the shell VM from DI, binds it and hooks
    /// the NewBookingRequested event so the FAB tap pushes the
    /// wizard modal on top of the SfTabView.</summary>
    public MobileShell()
    {
        InitializeComponent();

        // Resolve the shell view-model from the same DI container the
        // rest of the app uses. The shell view-model is the only piece
        // that owns cross-tab navigation state (selected tab + the
        // "is the new-booking wizard alive" flag).
        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileShellViewModel>();
        BindingContext = _vm;

        // Wire the FAB / new-booking request into a real modal push.
        _vm.NewBookingRequested += OnNewBookingRequested;
    }

    /// <summary>
    /// Programmatic navigation helper used by the pages when they need
    /// to switch tabs (e.g. the New-Booking wizard finishes and pops back
    /// to the Bookings list).
    /// </summary>
    /// <param name="index">Destination tab index. Clamped to the
    /// supported range before assignment.</param>
    public void SwitchToTab(int index)
    {
        // Three tabs: 0=Dashboard, 1=Bookings, 2=Rooms.
        _vm.SelectedTabIndex = Math.Clamp(index, 0, 2);
    }

    /// <summary>Raised whenever the user invokes the FAB. Pushes the
    /// New Booking wizard via <see cref="MobileShellViewModel.NewBookingRequested"/>.
    /// The wizard page is resolved from DI and used as a Shell modal.</summary>
    /// <param name="sender">The VM raising the event. Unused.</param>
    /// <param name="e">Empty event payload. Unused.</param>
    private async void OnNewBookingRequested(object? sender, EventArgs e)
    {
        if (_vm.IsNewBookingOpen)
            return;

        _vm.IsNewBookingOpen = true;
        try
        {
            var page = App.Services!.GetRequiredService<MobileNewBookingPage>();
            await Navigation.PushModalAsync(page);
        }
        finally
        {
            _vm.IsNewBookingOpen = false;
        }
    }
}
