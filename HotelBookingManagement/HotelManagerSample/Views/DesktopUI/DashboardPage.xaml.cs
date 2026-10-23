using HotelManagerSample;
using Syncfusion.Maui.DataPager;

namespace HotelManagerSample;

/// <summary>
/// Code-behind for the desktop Dashboard page. Owns the navigation
    /// hops that surface the Bookings / Rooms pages, the pager-reset
    /// hook, and the inline search clear-glyph behaviour. All styling
    /// state (revenue-range toggle, segment pills) and data work flow
    /// through the shared <see cref="DashboardViewModel"/> resolved
    /// from DI — there is no imperative styling left in this file.
    /// </summary>
public partial class DashboardPage : ContentPage
{
    /// <summary>Constructs the page, inflates the XAML, wires the
    /// shared view-model, and subscribes to the pager-reset event so
    /// a filter rebuild always restarts the SfDataPager at page 1.</summary>
    public DashboardPage()
    {
        InitializeComponent();

        // TitleBarController.Apply() (called from App.xaml.cs OnLaunched)
        // already flips ExtendsContentIntoTitleBar = true and paints the
        // chrome in #F6F8FD, so the original XAML padding stays — no
        // additional Windows-only inset is needed.

        // Resolve the SHARED view-model from the DI container so the
        // dashboard reads from the same IHotelStore instance the bookings
        // page writes to. Booking additions on the bookings page fire
        // IHotelStore.BookingsChanged which updates the dashboard's KPI
        // tiles, donut-shaped inventory and grid in one cascade.
        App.CaptureServices();
        var vm = App.Services!.GetRequiredService<DashboardViewModel>();
        BindingContext = vm;

        // Subscribe to VM filter changes so we can reset the SfDataPager back
        // to page 1 whenever the VisibleBookings list is rebuilt. Otherwise
        // a search can leave the user on an empty/out-of-range page.
        vm.PagerResetRequested += OnPagerResetRequested;
    }

    // Sidebar navigation is owned by HotelManagerSample.Controls.AppSidebar;
    // OnBookingsClicked / OnRoomsClicked have been removed.

    /// <summary>Routes the user to the Bookings page where they can open
    /// the Add-Booking popup. Replaces the prior Shell.GoToAsync call —
    /// the desktop surface now uses standard NavigationPage swap.</summary>
    /// <param name="sender">The originating "New Booking" button. Unused.</param>
    /// <param name="e">Event payload. Unused.</param>
    private async void OnNewBookingClicked(object? sender, EventArgs e)
    {
        var nav = Application.Current?.MainPage as NavigationPage;
        if (nav is null) return;

        try
        {
            await nav.PopToRootAsync(animated: false);
            await nav.PushAsync(new BookingsPage(), animated: false);
        }
        catch
        {
            // Swallow — the booking list is reachable from the sidebar too.
        }
    }

    /// <summary>
    /// Handles <see cref="SfDataPager.PageChanged"/> by refreshing the
    /// "Showing X–Y of N bookings" label so it stays in sync with the page
    /// that the viewport is currently showing.
    /// </summary>
    /// <param name="sender">The SfDataPager. Unused.</param>
    /// <param name="e">Page-changed payload from Syncfusion.</param>
    private void OnPagerPageChanged(object? sender, PageChangedEventArgs e)
    {
        if (BindingContext is not DashboardViewModel vm)
            return;

        vm.UpdatePaging(e.NewPageIndex, ReservationsPager.PageSize);
    }

    /// <summary>
    /// Reacts to <see cref="DashboardViewModel.PagerResetRequested"/> by
    /// moving the SfDataPager back to page 1 whenever filtering rebuilds
    /// the visible bookings list.
    /// </summary>
    /// <param name="sender">The VM raising the event. Unused.</param>
    /// <param name="e">Empty event payload. Unused.</param>
    private void OnPagerResetRequested(object? sender, EventArgs e)
    {
        // Guard against the trivial case where the pager is mid-loadout.
        if (ReservationsPager is null)
            return;

        // Sending Source=null/disconnect is fiddly; setting MoveToFirstPage
        // is the supported way to reset paged navigation.
        ReservationsPager.MoveToFirstPage();
    }

    /// <summary>
    /// Shows / hides the inline clear-search label and forwards the entered
    /// text to the VM (which does the actual filtering).
    /// </summary>
    /// <param name="sender">The <see cref="Entry"/> that fired the
    /// event. Held in case future direct IME hooks need a reference.</param>
    /// <param name="e">Standard <see cref="TextChangedEventArgs"/>
    /// containing the new value.</param>
    private void OnDashboardSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        UpdateDashboardClearSearchVisibility(e.NewTextValue);

        if (BindingContext is DashboardViewModel vm)
            vm.SearchText = e.NewTextValue ?? string.Empty;

        _ = entry; // entry reference held for future direct IME hooks
    }

    /// <summary>Toggles the trailing "×" clear label based on
    /// whether the search field currently holds any text.</summary>
    /// <param name="text">Current search-field text. When
    /// non-empty the glyph is shown.</param>
    private void UpdateDashboardClearSearchVisibility(string? text)
    {
        // Toggle the trailing "×" clear label based on whether the search
        // field currently holds any text. Equivalent to the deleted wrapper's
        // IsClearVisible property, but inlined for the new control.
        if (this.FindByName<Label>("DashboardSearchClear") is { } clear)
            clear.IsVisible = !string.IsNullOrEmpty(text);
    }

    /// <summary>
    /// Imperatively clears the search entry (and therefore the filter) when
    /// the user taps the small "×" chip overlaying it. Drives the bound
    /// <c>Text</c> directly so the TwoWay binding propagates the empty
    /// value back to the VM and the trailing glyph hides itself.
    /// </summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSearchTapped(object? sender, TappedEventArgs e)
    {
        if (DashboardSearchEntry is not null)
            DashboardSearchEntry.Text = string.Empty;
    }
}
