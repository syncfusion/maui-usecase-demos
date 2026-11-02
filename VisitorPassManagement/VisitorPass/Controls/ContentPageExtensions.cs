using Syncfusion.Maui.Popup;

namespace VisitorPass.Controls;

/// <summary>
/// Visual-tree helpers shared by every page. Pages walk the
/// <c>ContentPage</c>'s layout tree to find a single child control
/// without naming it from XAML — keeps the page's XAML focused on
/// layout instead of element wiring.
/// </summary>
public static class ContentPageExtensions
{
    /// <summary>
    /// Returns the first <see cref="SideNavigation"/> in
    /// <paramref name="page"/>'s visual tree, or <c>null</c> if the
    /// page doesn't host one (e.g. on phone idioms where the
    /// sidebar is collapsed via <c>OnIdiom</c>).
    /// </summary>
    public static SideNavigation? FindSideNavigation(this ContentPage page)
        => FindInLayout<SideNavigation>(page.Content as IView);

    /// <summary>
    /// Returns the first <see cref="SfPopup"/> in
    /// <paramref name="page"/>'s visual tree, or <c>null</c> if the
    /// page doesn't host one.
    /// </summary>
    public static SfPopup? FindRegisterPopup(this ContentPage page)
        => FindInLayout<SfPopup>(page.Content as IView);

    private static T? FindInLayout<T>(IView? view) where T : class, IView
    {
        if (view is null) return default;
        if (view is T match) return match;

        if (view is Layout layout)
        {
            foreach (var child in layout.Children)
            {
                var found = FindInLayout<T>(child);
                if (found is not null) return found;
            }
        }

        if (view is IContentView contentView && contentView.Content is IView nested)
        {
            var found = FindInLayout<T>(nested);
            if (found is not null) return found;
        }

        return default;
    }
}
