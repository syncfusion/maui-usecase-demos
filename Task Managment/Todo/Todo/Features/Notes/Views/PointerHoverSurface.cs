using Microsoft.Maui.Controls.Shapes;

namespace Todo.Features.Notes.Views;

internal static class PointerHoverSurface
{
    public static Border Wrap(View content, double cornerRadius, Thickness padding)
    {
        var border = new Border
        {
            Padding = padding,
            BackgroundColor = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(cornerRadius) },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Content = content
        };
        Attach(border);
        return border;
    }

    public static void Attach(Border border, Func<Color>? restoreBackground = null)
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetHovered(border);
        pointer.PointerExited += (_, _) => Reset(border, restoreBackground?.Invoke() ?? Colors.Transparent);
        border.GestureRecognizers.Add(pointer);
    }

    public static void SetHovered(Border border)
    {
        border.BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#14CAC4D0")
            : Color.FromArgb("#1449454F");
    }

    public static void Reset(Border border, Color? backgroundColor = null)
    {
        border.BackgroundColor = backgroundColor ?? Colors.Transparent;
    }
}
