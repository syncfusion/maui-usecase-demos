using System.Threading.Tasks;
using HotelManagerSample;

namespace HotelManagerSample.Controls;

/// <summary>
/// Sidebar navigation component shared by every desktop page. Exposes
/// two bindable properties — the currently active tab and an optional
/// badge label for the Bookings row — and routes taps through the
/// surrounding <see cref="NavigationPage"/>.
/// </summary>
public partial class AppSidebar : ContentView
{
    /// <summary>Bindable backing for <see cref="ActiveTab"/>.</summary>
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(NavigationTab), typeof(AppSidebar), NavigationTab.Dashboard);

    /// <summary>The currently-highlighted sidebar entry. Bound from
    /// the page so the active styling follows the visible surface.</summary>
    public NavigationTab ActiveTab
    {
        get => (NavigationTab)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    /// <summary>Bindable backing for <see cref="BookingsBadgeText"/>.</summary>
    public static readonly BindableProperty BookingsBadgeTextProperty = BindableProperty.Create(
        nameof(BookingsBadgeText), typeof(string), typeof(AppSidebar), string.Empty);

    /// <summary>Small label rendered on the Bookings row, e.g. the
    /// count of active reservations. Empty by default.</summary>
    public string BookingsBadgeText
    {
        get => (string)GetValue(BookingsBadgeTextProperty);
        set => SetValue(BookingsBadgeTextProperty, value);
    }

    /// <summary>Builds the sidebar and inflates the XAML tree.</summary>
    public AppSidebar()
    {
        InitializeComponent();
    }

    // Tap handlers - swap the root page via standard MAUI NavigationPage
    // navigation. We PopToRootAsync before Pushing so repeated taps don't
    // build up an ever-growing back-stack on top of the NavigationPage.

    /// <summary>Sidebar "Dashboard" tap handler. Navigates to the
    /// dashboard page through the surrounding <see cref="NavigationPage"/>.</summary>
    /// <param name="sender">The tapped row. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private async void OnDashboardTapped(object? sender, TappedEventArgs e) => await NavigateToAsync(NavigationTab.Dashboard);
    /// <summary>Sidebar "Bookings" tap handler. Navigates to the
    /// bookings page through the surrounding <see cref="NavigationPage"/>.</summary>
    /// <param name="sender">The tapped row. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private async void OnBookingsTapped(object? sender, TappedEventArgs e)  => await NavigateToAsync(NavigationTab.Bookings);
    /// <summary>Sidebar "Rooms" tap handler. Navigates to the rooms
    /// page through the surrounding <see cref="NavigationPage"/>.</summary>
    /// <param name="sender">The tapped row. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private async void OnRoomsTapped(object? sender, TappedEventArgs e)     => await NavigateToAsync(NavigationTab.Rooms);

    /// <summary>Replaces the top-of-stack page with a fresh instance for the
    /// picked tab. Falls back gracefully if the host isn't a
    /// <see cref="NavigationPage"/> (defensive — happens during the very
    /// first render frame before Maui hands the page back to its
    /// <c>Window</c>).</summary>
    /// <param name="tab">The destination tab the user just tapped.</param>
    private static async Task NavigateToAsync(NavigationTab tab)
    {
        try
        {
            Page? target = tab switch
            {
                NavigationTab.Dashboard => new DashboardPage(),
                NavigationTab.Bookings  => new BookingsPage(),
                NavigationTab.Rooms     => new RoomsPage(),
                _                       => null,
            };
            if (target is null) return;

            var nav = Application.Current?.MainPage as NavigationPage;
            if (nav is null) return;

            // If the user is already on this tab, do nothing — pressing
            // a tab you're already on shouldn't reset state.
            if (nav.CurrentPage?.GetType() == target.GetType())
                return;

            // Pop everything but the root, then push the new tab page.
            await nav.PopToRootAsync(animated: false);
            await nav.PushAsync(target, animated: false);
        }
        catch
        {
            // Swallow — nav failures during startup shouldn't crash the app.
        }
    }
}
