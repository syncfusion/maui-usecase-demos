namespace DiagramBuilder.Controls;

internal static class PopupXamlRootHelper
{
    public static void AttachIfAvailable(object popup, VisualElement? anchor)
    {
        var platformView = anchor?.Handler?.PlatformView;
        if (platformView is null)
        {
            return;
        }

        var xamlRoot = platformView.GetType().GetProperty("XamlRoot")?.GetValue(platformView);
        if (xamlRoot is null)
        {
            return;
        }

        popup.GetType().GetProperty("XamlRoot")?.SetValue(popup, xamlRoot);
    }
}
