#if WINDOWS
using Windows.System;

namespace DiagramBuilder.Behaviors;

/// <summary>
/// Windows implementation of the keyboard hook. Verified against the
/// WindowsAppSDK 1.6 / Windows SDK 19041 reference metadata:
/// <c>KeyRoutedEventArgs.Key</c> is a <c>Windows.System.VirtualKey</c>, and
/// <c>KeyStatus</c> is a <c>Windows.UI.Core.CorePhysicalKeyStatus</c> that exposes
/// NO modifier state (only RepeatCount, ScanCode, IsExtendedKey, IsMenuKeyDown,
/// WasKeyDown, IsKeyReleased). The Control key is therefore tracked explicitly
/// via KeyDown/KeyUp, and key auto-repeat is suppressed.
/// </summary>
public partial class DiagramKeyboardBehavior
{
    private Microsoft.UI.Xaml.UIElement? _platformView;
    private bool _controlPressed;
    private VirtualKey? _lastPressedKey;

    partial void AttachPlatformHook(View view)
    {
        if (view.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            _platformView = element;
            _platformView.KeyDown += OnPlatformKeyDown;
            _platformView.KeyUp += OnPlatformKeyUp;
        }
    }

    partial void DetachPlatformHook()
    {
        if (_platformView is not null)
        {
            _platformView.KeyDown -= OnPlatformKeyDown;
            _platformView.KeyUp -= OnPlatformKeyUp;
            _platformView = null;
        }
        _controlPressed = false;
        _lastPressedKey = null;
    }

    private void OnPlatformKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.LeftControl or VirtualKey.RightControl)
        {
            _controlPressed = true;
            return;
        }

        if (e.Key == _lastPressedKey)
        {
            return; // ignore key auto-repeat
        }
        _lastPressedKey = e.Key;

        var key = e.Key.ToString().ToLowerInvariant();
        Dispatch(_controlPressed, key);
    }

    private void OnPlatformKeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.LeftControl or VirtualKey.RightControl)
        {
            _controlPressed = false;
        }
        if (e.Key == _lastPressedKey)
        {
            _lastPressedKey = null;
        }
    }
}
#endif
