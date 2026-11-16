namespace Todo.Features.Notes.Views;

internal static class KeyModifierState
{
    public static (bool Ctrl, bool Shift) Get()
    {
#if WINDOWS
        static bool Down(Windows.System.VirtualKey key)
            => Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key)
                .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        return (Down(Windows.System.VirtualKey.Control), Down(Windows.System.VirtualKey.Shift));
#else
        return (false, false);
#endif
    }
}
