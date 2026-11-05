using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Popup;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public sealed class CustomListOptionsFlyout : ContentView
{
    private const double MenuItemHeight = 42;
    private readonly SfPopup _popup;
    private NoteList? _selectedList;
    private NotesListViewModel? _viewModel;

    public CustomListOptionsFlyout()
    {
        _popup = CreatePopup();
        Content = _popup;
    }

    public void ShowFor(View anchor, NoteList list, NotesListViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(viewModel);

        if (list.IsSystem)
            return;

        _selectedList = list;
        _viewModel = viewModel;
        _popup.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomRight);
    }

    private SfPopup CreatePopup()
        => new()
        {
            ShowHeader = false,
            ShowFooter = false,
            ShowOverlayAlways = false,
            OverlayMode = PopupOverlayMode.Transparent,
            StaysOpen = false,
            WidthRequest = 190,
            HeightRequest = MenuItemHeight * 2 + 24,
            AutoSizeMode = PopupAutoSizeMode.None,
            AnimationMode = PopupAnimationMode.None,
            PopupStyle = new PopupStyle
            {
                OverlayColor = Colors.Transparent,
                MessageBackground = Colors.Transparent,
                HasShadow = true,
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                CornerRadius = 27
            },
            ContentTemplate = new DataTemplate(CreateSurface)
        };

    private View CreateSurface()
    {
        var actions = new VerticalStackLayout
        {
            Spacing = 0,
            Margin = new Thickness(10, 10)
        };
        actions.Children.Add(CreateActionButton(
            "Rename list",
            "\uf88c",
            destructive: false,
            RenameList));
        actions.Children.Add(CreateActionButton(
            "Delete list",
            "\ue92e",
            destructive: true,
            DeleteList));

        var surface = new Border
        {
            WidthRequest = 190,
            HeightRequest = MenuItemHeight * 2 + 20,
            Padding = new Thickness(1),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(27) },
            Content = actions
        };
        surface.SetAppTheme<Brush>(
            VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        surface.SetAppThemeColor(
            Border.StrokeProperty,
            Color.FromArgb("#E3DFEA"),
            Color.FromArgb("#44414B"));
        surface.Shadow = new Shadow
        {
            Brush = Color.FromArgb("#40000000"),
            Radius = 14,
            Offset = new Point(0, 6),
            Opacity = 1
        };
        return surface;
    }

    private SfButton CreateActionButton(string text, string iconGlyph, bool destructive, Action action)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = MenuItemHeight,
            Padding = new Thickness(10, 0),
            Background = Brush.Transparent,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Fill,
            CornerRadius = 8,
            Command = new Command(action)
        };
        button.SetAppThemeColor(
            SfButton.TextColorProperty,
            destructive ? Color.FromArgb("#D32F2F") : Color.FromArgb("#1C1B1F"),
            destructive ? Color.FromArgb("#FFB4AB") : Color.FromArgb("#E6E1E5"));

        button.Content = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection { new(22), new(GridLength.Star) },
                ColumnSpacing = 4
            };
            var icon = new Label
            {
                Text = iconGlyph,
                FontFamily = "MaterialIconRegular",
                FontSize = 18,
                VerticalTextAlignment = TextAlignment.Center
            };
            icon.SetAppThemeColor(
                Label.TextColorProperty,
                destructive ? Color.FromArgb("#B3261E") : Color.FromArgb("#49454F"),
                destructive ? Color.FromArgb("#FFB4AB") : Color.FromArgb("#CAC4D0"));
            grid.Children.Add(icon);

            var label = new Label
            {
                Text = text,
                FontSize = 14,
                VerticalTextAlignment = TextAlignment.Center
            };
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(SfButton.TextColor), source: button));
            label.SetValue(Grid.ColumnProperty, 1);
            grid.Children.Add(label);
            return grid;
        });

        SemanticProperties.SetDescription(button, text);
        var states = new VisualStateGroup { Name = "CommonStates" };
        states.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        states.States.Add(pointerOver);
        var pressed = new VisualState { Name = "Pressed" };
        pressed.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        states.States.Add(pressed);
        VisualStateManager.SetVisualStateGroups(button, new VisualStateGroupList { states });
        return button;
    }

    private void RenameList()
    {
        if (_selectedList is not { IsSystem: false } list || _viewModel is not { } viewModel)
            return;

        _popup.Dismiss();
        viewModel.OpenRenameListDialogCommand.Execute(list);
    }

    private void DeleteList()
    {
        if (_selectedList is not { IsSystem: false } list || _viewModel is not { } viewModel)
            return;

        _popup.Dismiss();
        viewModel.ConfirmDeleteListCommand.Execute(list);
    }
}
