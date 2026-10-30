using Syncfusion.Maui.Popup;

namespace VisitorPass.Controls;

/// <summary>
/// VP-080 — visual-tree helpers shared by every page that hosts a
/// <see cref="TopNavBar"/>. Walks the ContentPage's layout tree to
/// find the single TopNavBar so pages can subscribe to its events
/// without naming it from XAML.
/// </summary>
public static class TopNavBarExtensions
{
    /// <summary>
    /// Returns the first <see cref="TopNavBar"/> in <paramref name="page"/>'s
    /// visual tree, or <c>null</c> if the page doesn't host one.
    /// </summary>
    public static TopNavBar? FindTopNavBar(this ContentPage page)
    {
        if (page.Content is IView root)
            return FindInLayout<TopNavBar>(root);
        return default;
    }

    /// <summary>
    /// Returns the first <see cref="SideNavigation"/> in
    /// <paramref name="page"/>'s visual tree, or <c>null</c> if the
    /// page doesn't host one (e.g. on phone idioms where the
    /// sidebar is collapsed via OnIdiom).
    /// </summary>
    public static SideNavigation? FindSideNavigation(this ContentPage page)
    {
        if (page.Content is IView root)
            return FindInLayout<SideNavigation>(root);
        return default;
    }

    /// <summary>
    /// Returns the first <see cref="SfPopup"/> in
    /// <paramref name="page"/>'s visual tree, or <c>null</c> if
    /// the page doesn't host one.
    /// </summary>
    public static SfPopup? FindRegisterPopup(this ContentPage page)
    {
        if (page.Content is IView root)
            return FindInLayout<SfPopup>(root);
        return default;
    }

    private static T? FindInLayout<T>(IView view) where T : class, IView
    {
        if (view is T match)
            return match;

        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                var found = FindInLayout<T>(child);
                if (found is not null)
                    return found;
            }
        }

        if (view is IContentView contentView && contentView.Content is IView nested)
        {
            var found = FindInLayout<T>(nested);
            if (found is not null)
                return found;
        }

        return default;
    }
}
