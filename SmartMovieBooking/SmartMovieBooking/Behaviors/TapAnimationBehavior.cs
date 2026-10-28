using Microsoft.Maui.Controls;

namespace SmartMovieBooking.Behaviors;

public class TapAnimationBehavior : Behavior<View>
{
    private View? _host;
    private TapGestureRecognizer? _tapRecognizer;
    private PointerGestureRecognizer? _pointerRecognizer;

    protected override void OnAttachedTo(View bindable)
    {
        base.OnAttachedTo(bindable);
        _host = bindable;

        _tapRecognizer = new TapGestureRecognizer();
        _tapRecognizer.Tapped += OnTapped;
        bindable.GestureRecognizers.Add(_tapRecognizer);

        _pointerRecognizer = new PointerGestureRecognizer();
        _pointerRecognizer.PointerEntered += OnPointerEntered;
        _pointerRecognizer.PointerExited += OnPointerExited;
        _pointerRecognizer.PointerPressed += OnPointerPressed;
        _pointerRecognizer.PointerReleased += OnPointerReleased;
        bindable.GestureRecognizers.Add(_pointerRecognizer);
    }

    protected override void OnDetachingFrom(View bindable)
    {
        if (_tapRecognizer is not null)
        {
            _tapRecognizer.Tapped -= OnTapped;
            bindable.GestureRecognizers.Remove(_tapRecognizer);
            _tapRecognizer = null;
        }

        if (_pointerRecognizer is not null)
        {
            _pointerRecognizer.PointerEntered -= OnPointerEntered;
            _pointerRecognizer.PointerExited -= OnPointerExited;
            _pointerRecognizer.PointerPressed -= OnPointerPressed;
            _pointerRecognizer.PointerReleased -= OnPointerReleased;
            bindable.GestureRecognizers.Remove(_pointerRecognizer);
            _pointerRecognizer = null;
        }

        _host = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnTapped(object? sender, TappedEventArgs e)
    {
        if (_host is null) return;
        await TapAnimationHelper.AnimateTapEffectAsync(_host);
    }

    private async void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_host is null) return;
        try
        {
            await _host.ScaleToAsync(1.02, 90, Easing.CubicOut);
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

    private async void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        if (_host is null) return;
        try
        {
            await _host.ScaleToAsync(0.98, 50, Easing.CubicOut);
        }
        catch
        {
        }
    }

    private async void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        if (_host is null) return;
        try
        {
            await _host.ScaleToAsync(1.0, 70, Easing.CubicIn);
        }
        catch
        {
        }
    }
}