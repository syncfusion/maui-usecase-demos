using HotelManagerSample;
using MobileShellHost = HotelManagerSample.MobileShell;

namespace HotelManagerSample.Dashboard;

/// <summary>
/// Code-behind for the mobile dashboard. Reuses the
/// <see cref="MobileDashboardViewModel"/>, which itself listens to the
/// shared <see cref="IHotelStore"/>. Tap handlers forward to the shell
/// (a plain ContentPage hosting SfTabView) so the Bookings wizard opens
/// on top of the tab view. No reference to Shell.Current at runtime.
/// </summary>
public partial class MobileDashboardPage : ContentView
{
    private readonly MobileDashboardViewModel _vm;

    /// <summary>Resolves the shared view-model from DI and binds it as
    /// the <see cref="BindableObject.BindingContext"/>.</summary>
    public MobileDashboardPage()
    {
        InitializeComponent();

        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<MobileDashboardViewModel>();
        BindingContext = _vm;
    }

    // ============== Item taps ==============

    /// <summary>Recent-booking card tap. Currently routes the user to
    /// the Bookings tab — deep-linking to a detail page is not part
    /// of the brief MVP.</summary>
    /// <param name="sender">The tapped card. Unused.</param>
    /// <param name="e">Tapped-event payload. <see cref="TappedEventArgs.Parameter"/>
    /// carries the booking payload.</param>
    private void OnRecentBookingTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Booking booking)
            return;

        // Switch to the Bookings tab and let the new tab's lifetime take
        // over; deep-linking to the details page from the dashboard is not
        // part of the brief MVP, and the dashboard list is informational
        // anyway. Future iterations can route to MobileBookingDetailsPage.
        SwitchShellTab(MobileShellViewModel.BookingsTabIndex);
    }

    /// <summary>"View all" jumps the user to the Bookings tab. The
    /// booking list there drives the same shared IHotelStore, so any
    /// new entry committed on the bookings tab brings the dashboard in
    /// sync via the OnStoreBookingsChanged cascade.</summary>
    /// <param name="sender">The "View all" button. Unused.</param>
    /// <param name="e">Event payload. Unused.</param>
    private void OnViewAllBookingsClicked(object? sender, EventArgs e)
    {
        SwitchShellTab(MobileShellViewModel.BookingsTabIndex);
    }

    /// <summary>Walks up the visual tree looking for the
    /// <see cref="MobileShellHost"/> host so the dashboard can drive
    /// navigation from a child page.</summary>
    /// <returns>The discovered shell host, or <c>null</c> when the
    /// page has not been parented under one yet.</returns>
    private MobileShellHost? ResolveShell()
    {
        Element? walker = this.Parent;
        while (walker is not null)
        {
            if (walker is MobileShellHost shell)
                return shell;
            walker = walker.Parent;
        }
        return null;
    }

    /// <summary>Switches the bottom tab without navigating.</summary>
    /// <param name="index">Destination tab index. Clamped by the
    /// shell to the supported range.</param>
    private void SwitchShellTab(int index)
    {
        var shell = ResolveShell();
        if (shell is not null)
            shell.SwitchToTab(index);
    }

    /// <summary>Triggers the Floating Action Button's NewBookingCommand
    /// via the shell VM so the wizard opens on top of the tab view.</summary>
    /// <param name="sender">The FAB. Unused.</param>
    /// <param name="e">Event payload. Unused.</param>
    private void OnNewBookingTapped(object sender, EventArgs e)
    {
        var vm = App.Services!.GetRequiredService<MobileShellViewModel>();
        vm.NewBookingCommand.Execute(null);
    }
}
