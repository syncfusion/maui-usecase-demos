namespace HotelManagerSample;

/// <summary>
/// Routes for the shared sidebar. Set on the page-level instance when hosting
/// the sidebar so it knows which entry to render in the active style.
/// </summary>
public enum NavigationTab
{
    /// <summary>Dashboard tab — root landing page.</summary>
    Dashboard,
    /// <summary>Bookings tab — reservation grid + add-booking popup.</summary>
    Bookings,
    /// <summary>Rooms tab — inventory board / table.</summary>
    Rooms,
    /// <summary>Guests tab — not implemented in the MVP; falls back to
    /// the dashboard route.</summary>
    Guests,
    /// <summary>Calendar tab — not implemented in the MVP; falls back
    /// to the dashboard route.</summary>
    Calendar,
    /// <summary>Reports tab — not implemented in the MVP; falls back to
    /// the dashboard route.</summary>
    Reports,
    /// <summary>Settings tab — not implemented in the MVP; falls back
    /// to the dashboard route.</summary>
    Settings
}

/// <summary>
/// Extension helpers that map a <see cref="NavigationTab"/> onto the
/// Shell route / display label / icon asset used by the sidebar.
/// </summary>
public static class NavigationTabExtensions
{
    /// <summary>Resolves the Shell route for the given tab. Tabs that
    /// have no dedicated route fall back to the dashboard route.</summary>
    /// <param name="tab">Tab to resolve a route for.</param>
    /// <returns>The route string for the tab.</returns>
    public static string ToRoute(this NavigationTab tab) => tab switch
    {
        NavigationTab.Dashboard => "//dashboard",
        NavigationTab.Bookings   => "//bookings",
        NavigationTab.Rooms      => "//rooms",
        NavigationTab.Guests     => "//dashboard",
        NavigationTab.Calendar   => "//dashboard",
        NavigationTab.Reports    => "//dashboard",
        NavigationTab.Settings   => "//dashboard",
        _ => "//dashboard"
    };

    /// <summary>Resolves the display label for the given tab.</summary>
    /// <param name="tab">Tab to resolve a label for.</param>
    /// <returns>The label string for the tab.</returns>
    public static string ToLabel(this NavigationTab tab) => tab switch
    {
        NavigationTab.Dashboard => "Dashboard",
        NavigationTab.Bookings   => "Bookings",
        NavigationTab.Rooms      => "Rooms",
        NavigationTab.Guests     => "Guests",
        NavigationTab.Calendar   => "Calendar",
        NavigationTab.Reports    => "Reports",
        NavigationTab.Settings   => "Settings",
        _ => string.Empty
    };

    /// <summary>Resolves the icon asset file name for the given tab.
    /// PNG icons cannot be recolored dynamically so the design ships a
    /// dedicated icon per tab.</summary>
    /// <param name="tab">Tab to resolve an icon for.</param>
    /// <returns>The asset file name (PNG) used as the icon.</returns>
    public static string ToIconSource(this NavigationTab tab) => tab switch
    {
        // Dashboard's default icon is the DARK variant (for light backgrounds);
        // dashboard.png is the white variant used on the dark active pill.
        NavigationTab.Dashboard => "dashboardblack.png",
        NavigationTab.Bookings   => "booking.png",
        NavigationTab.Rooms      => "rooms.png",
        NavigationTab.Guests     => "person.png",
        NavigationTab.Calendar   => "date.png",
        NavigationTab.Reports    => "container.png",
        NavigationTab.Settings   => "key.png",
        _ => string.Empty
    };
}
