#if WINDOWS
using Microsoft.Maui.Handlers;

namespace DiagramBuilder.Behaviors;

/// <summary>
/// Windows implementation of the keyboard hook: subscribes to the WinUI root
/// content's <see cref="Microsoft.UI.Xaml.UIElement.KeyDown"/> events.
/// </summary>
public partial class DiagramKeyboardBehavior
{
    private Microsoft.UI.Xaml.Controls.Panel? _rootPanel;

    partial void AttachPlatformHook(View view)
    {
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Panel panel)
        {
            _rootPanel = panel;
            panel.KeyDown += OnPlatformKeyDown;
        }
    }

    partial void DetachPlatformHook()
    {
        if (_rootPanel is not null)
        {
            _rootPanel.KeyDown -= OnPlatformKeyDown;
            _rootPanel = null;
        }
    }

    private void OnPlatformKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Confirmed via compiler: MAUI on Windows binds WinUI 3, where
        // KeyRoutedEventArgs.KeyStatus is Microsoft.UI.Input.CorePhysicalKeyStatus,
        // whose property is IsControlPressed (UWP's CoreVirtualKeyStatus uses
        // IsControlKeyDown — hence the earlier failures).
        var ctrl = e.KeyStatus.IsControlPressed;
        var key = e.Key.ToString().ToLowerInvariant();
        Dispatch(ctrl, key);
    }
}
#endif
