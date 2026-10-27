using DiagramBuilder.Constants;
using System.Windows.Input;

namespace DiagramBuilder.Behaviors;

/// <summary>
/// Keyboard shortcut bridge for the designer page: maps Ctrl+C/V/X/Z/Y/A and Delete to
/// the bound <see cref="ShortcutCommand"/> with the command id.
/// Key events are not surfaced by the MAUI layer in this package version, so the
/// behavior hooks the platform view's key-down on handler attach (Windows primary;
/// other desktop TFMs follow the same pattern). Zero business logic —
/// gesture → command relaying only.
/// </summary>
public partial class DiagramKeyboardBehavior : Behavior<View>
{
    public static readonly BindableProperty ShortcutCommandProperty =
        BindableProperty.Create(nameof(ShortcutCommand), typeof(ICommand), typeof(DiagramKeyboardBehavior));

    /// <summary>Receives the command id string for each recognized gesture.</summary>
    public ICommand? ShortcutCommand
    {
        get => (ICommand?)GetValue(ShortcutCommandProperty);
        set => SetValue(ShortcutCommandProperty, value);
    }

    private View? _attached;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _attached = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
        if (bindable.Handler is not null)
        {
            AttachPlatformHook(bindable);
        }
    }

    protected override void OnDetachingFrom(View bindable)
    {
        bindable.HandlerChanged -= OnHandlerChanged;
        DetachPlatformHook();
        _attached = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        DetachPlatformHook();
        if (sender is View view && view.Handler is not null)
        {
            AttachPlatformHook(view);
        }
    }

    /// <summary>Gesture → command id; routed to the bound command. Pure relay.</summary>
    private void Dispatch(bool ctrl, string key)
    {
        string? id;
        if (ctrl)
        {
            id = key switch
            {
                "c" => AppConstants.CmdCopy,
                "v" => AppConstants.CmdPaste,
                "x" => AppConstants.CmdCut,
                "z" => AppConstants.CmdUndo,
                "y" => AppConstants.CmdRedo,
                "a" => AppConstants.CmdSelectAll,
                _ => null,
            };
        }
        else
        {
            id = key is "delete" or "back" ? AppConstants.CmdDelete : null;
        }

        if (id is null)
        {
            return;
        }

        if (ShortcutCommand?.CanExecute(id) == true)
        {
            ShortcutCommand.Execute(id);
        }
    }

    // Platform hooks are defined in platform-specific partial files; the default
    // (non-desktop) implementation is a no-op so every TFM compiles.
    partial void AttachPlatformHook(View view);

    partial void DetachPlatformHook();
}
