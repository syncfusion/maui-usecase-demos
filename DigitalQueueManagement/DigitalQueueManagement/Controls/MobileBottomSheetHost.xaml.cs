using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Popup;

namespace DigitalQueueManagement.Controls;

public partial class MobileBottomSheetHost : ContentView
{
    private static readonly Color Slate200 = Color.FromArgb("#E2E8F0");

    public MobileBottomSheetHost()
    {
        InitializeComponent();
        Popup.Closed += (_, _) => IsVisible = false;
    }

    public bool IsOpen => Popup.IsOpen;

    public void Show(View content, View relativeView, double width, double availableHeight)
    {
        IsVisible = true;
        var sheetLayout = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
        };
        sheetLayout.Add(new Border
        {
            WidthRequest = 42,
            HeightRequest = 4,
            Margin = new Thickness(0, 0, 0, 12),
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromArgb("#D6D9E5"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(3) }
        }, 0, 0);
        sheetLayout.Add(new ScrollView
        {
            Content = content,
            VerticalScrollBarVisibility = ScrollBarVisibility.Never,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
        }, 0, 1);

        Popup.ContentTemplate = new DataTemplate(() => new Border
        {
            WidthRequest = width,
            Padding = new Thickness(20, 12, 20, 20),
            BackgroundColor = Colors.White,
            Stroke = Slate200,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24) },
            MaximumHeightRequest = availableHeight > 0 ? availableHeight : -1,
            VerticalOptions = LayoutOptions.End,
            Content = sheetLayout
        });
        Popup.ShowRelativeToView(relativeView, PopupRelativePosition.AlignBottom);
    }

    public void Hide()
    {
        Popup.IsOpen = false;
        IsVisible = false;
    }
}
