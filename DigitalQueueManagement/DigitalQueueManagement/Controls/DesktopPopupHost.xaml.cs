using Microsoft.Maui.Controls.Shapes;

namespace DigitalQueueManagement.Controls;

public partial class DesktopPopupHost : ContentView
{
    private static readonly Color Slate200 = Color.FromArgb("#E2E8F0");

    public DesktopPopupHost()
    {
        InitializeComponent();
        Popup.Closed += (_, _) => IsVisible = false;
    }

    public bool IsOpen => Popup.IsOpen;

    public void Show(View content, double availableWidth)
    {
        IsVisible = true;
        var popupWidth = availableWidth > 0
            ? Math.Max(220, Math.Min(520, availableWidth - 32))
            : 520;
        var padding = popupWidth < 380 ? 14 : 22;
        Popup.ContentTemplate = new DataTemplate(() => new Border
        {
            WidthRequest = popupWidth,
            Padding = padding,
            BackgroundColor = Colors.White,
            Stroke = Slate200,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Content = content
        });
        Popup.IsOpen = true;
    }

    public void Hide()
    {
        Popup.IsOpen = false;
        IsVisible = false;
    }
}
