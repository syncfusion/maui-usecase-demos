using Microsoft.Maui.Controls;

namespace SmartMovieBooking.Behaviors;

public class HoverAnimationBehavior : Behavior<View>
{
    private View? _host;
    private PointerGestureRecognizer? _pointerRecognizer;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _host = bindable;

        _pointerRecognizer = new PointerGestureRecognizer();
        _pointerRecognizer.PointerEntered += OnPointerEntered;
        _pointerRecognizer.PointerExited += OnPointerExited;
        bindable.GestureRecognizers.Add(_pointerRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (_pointerRecognizer is not null)
        {
            _pointerRecognizer.PointerEntered -= OnPointerEntered;
            _pointerRecognizer.PointerExited -= OnPointerExited;
            bindable.GestureRecognizers.Remove(_pointerRecognizer);
            _pointerRecognizer = null;
        }

        _host = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_host is null) return;

        try
        {
            await _host.ScaleToAsync(1.06, 90, Easing.CubicOut);
        }
        catch
        {
        }
    }

    private async void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_host is null) return;

        try
        {
            await _host.ScaleToAsync(1.0, 120, Easing.CubicIn);
        }
        catch
        {
        }
    }
}
