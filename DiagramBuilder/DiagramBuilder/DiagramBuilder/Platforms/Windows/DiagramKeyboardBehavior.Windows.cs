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
        var ctrl = Microsoft.UI.Input.KeyboardInput.GetModifierState() is not null
            ? Microsoft.UI.Input.KeyboardInput.GetModifierState().HasFlag(
                global::Windows.System.VirtualKeyModifiers.Control)
            : false;

        var key = e.Key.ToString().ToLowerInvariant();
        Dispatch(ctrl, key);
    }
}
#endif
