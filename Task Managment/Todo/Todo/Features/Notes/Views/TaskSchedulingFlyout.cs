using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Picker;
using Syncfusion.Maui.Popup;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public sealed class TaskSchedulingFlyout : ContentView
{
    private readonly SfPopup _datePopup;
    private readonly SfPopup _repeatPopup;
    private readonly SfPopup _customPopup;
    private NotesListViewModel? _viewModel;
    private View? _repeatAnchor;
    private DateTime _selectedDateTime = DateTime.Now;
    private bool _isReminder;

    public TaskSchedulingFlyout()
    {
        _datePopup = CreatePopup(240, 350, CreateDateSurface);
        _repeatPopup = CreatePopup(210, 332, CreateRepeatSurface);
        _customPopup = CreatePopup(300, 224, CreateCustomSurface);
        foreach (var popup in new[] { _datePopup, _repeatPopup, _customPopup })
            popup.PositionChanging += ClampPosition;
        Content = new Grid { Children = { _datePopup, _repeatPopup, _customPopup } };
    }

    private static SfPopup CreatePopup(double width, double height, Func<View> factory) => new()
    {
        ShowHeader = false,
        ShowFooter = false,
        ShowOverlayAlways = false,
        OverlayMode = PopupOverlayMode.Transparent,
        StaysOpen = false,
        WidthRequest = width,
        HeightRequest = height,
        AutoSizeMode = PopupAutoSizeMode.None,
        AnimationMode = PopupAnimationMode.None,
        BackgroundColor = Colors.Transparent,
        PopupStyle = new PopupStyle
        {
            OverlayColor = Colors.Transparent,
            MessageBackground = Colors.Transparent,
            HasShadow = true,
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            CornerRadius = 27
        },
        ContentTemplate = new DataTemplate(factory)
    };

    public void ShowDueDate(View anchor, NotesListViewModel viewModel) => ShowDate(anchor, viewModel, false);
    public void ShowReminder(View anchor, NotesListViewModel viewModel) => ShowDate(anchor, viewModel, true);

    private void ShowDate(View anchor, NotesListViewModel viewModel, bool isReminder)
    {
        _viewModel = viewModel;
        _isReminder = isReminder;
        _selectedDateTime = isReminder ? viewModel.DraftReminderDate ?? DateTime.Now : viewModel.DraftDueDate ?? DateTime.Now;
        _datePopup.ContentTemplate = new DataTemplate(CreateDateSurface);
        _datePopup.ShowRelativeToView(anchor, PopupRelativePosition.AlignTopRight);
    }

    public void ShowRepeat(View anchor, NotesListViewModel viewModel)
    {
        _viewModel = viewModel;
        _repeatAnchor = anchor;
        _repeatPopup.ShowRelativeToView(anchor, PopupRelativePosition.AlignTopRight);
    }

    private View CreateDateSurface()
    {
        var picker = new SfDateTimePicker
        {
            SelectedDate = _selectedDateTime,
            HeightRequest = 250,
            Background = Brush.Transparent,
            DateFormat = PickerDateFormat.MMM_dd_yyyy
        };

        var cancelButton = CreateTextActionButton("Cancel", () => _datePopup.Dismiss());
        var saveButton = CreateActionButton("Save", () =>
        {
            if (_viewModel is null)
                return;

            var value = picker.SelectedDate ?? _selectedDateTime;
            if (_isReminder)
                _viewModel.SetDraftReminderDate(value);
            else
                _viewModel.SetDraftDueDate(value);

            _datePopup.Dismiss();
        });

        var content = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new(GridLength.Auto),
                new(GridLength.Auto),
                new(250),
                new(GridLength.Auto),
                new(GridLength.Auto)
            },
            Padding = new Thickness(0,10,0,10),
            RowSpacing = 0
        };

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Margin = new Thickness(10)
        };

        content.Children.Add(header);
        var divider = new BoxView
        {
            HeightRequest = 1,
            Margin = new Thickness(0, 4),
            HorizontalOptions = LayoutOptions.Fill
        };
        divider.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#E9E5EE"), Color.FromArgb("#494650"));
        content.Children.Add(divider);
        Grid.SetRow(divider, 1);
        content.Children.Add(picker);
        Grid.SetRow(picker, 2);

        var pickerDivider = new BoxView
        {
            HeightRequest = 1,
            Margin = new Thickness(0, 4),
            HorizontalOptions = LayoutOptions.Fill
        };
        pickerDivider.SetAppThemeColor(
            BoxView.ColorProperty,
            Color.FromArgb("#E9E5EE"),
            Color.FromArgb("#494650"));
        content.Children.Add(pickerDivider);
        Grid.SetRow(pickerDivider, 3);

        var buttons = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 8,
            HeightRequest = 44, 
            Margin = new Thickness(0, 15, 10, 0)
        };
        buttons.Children.Add(cancelButton);
        buttons.Children.Add(saveButton);
        Grid.SetColumn(cancelButton, 1);
        Grid.SetColumn(saveButton, 2);
        content.Children.Add(buttons);
        Grid.SetRow(buttons, 4);

        return CreateBorder(240, 350, content);
    }

    private View CreateRepeatSurface()
    {
        var content = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(8) };
        var repeatTitle = new Label
        {
            Text = "Repeat",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#29272F"),
            HeightRequest = 44,
            Padding = new Thickness(12, 0),
            VerticalTextAlignment = TextAlignment.Center
        };
        repeatTitle.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));
        content.Children.Add(repeatTitle);

        foreach (var option in new[] { NoteRepeatFrequency.Daily, NoteRepeatFrequency.Weekdays, NoteRepeatFrequency.Weekly, NoteRepeatFrequency.Monthly, NoteRepeatFrequency.Yearly })
        {
            var isSelected = _viewModel?.DraftRepeatFrequency == option;
            content.Children.Add(CreateMenuButton(option.ToString(), isSelected,
                () => { _viewModel?.SetDraftRepeatFrequency(option); _repeatPopup.Dismiss(); }));
        }

        var repeatDivider = new BoxView { HeightRequest = 1, Margin = new Thickness(0, 4) };
        repeatDivider.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#E9E5EE"), Color.FromArgb("#494650"));
        content.Children.Add(repeatDivider);
        var customButton = CreateMenuButton("Custom", _viewModel?.DraftRepeatFrequency == NoteRepeatFrequency.Custom,
            () => { _repeatPopup.Dismiss(); if (_repeatAnchor is not null) _customPopup.ShowRelativeToView(_repeatAnchor, PopupRelativePosition.AlignTopRight); },
            iconGlyph: "\ue24f");
        customButton.Margin = new Thickness(10, 4, 0, 0);
        content.Children.Add(customButton);
        return CreateBorder(210, 332, content);
    }

    private View CreateCustomSurface()
    {
        var selected = new[] { true, true, false, false, false, false, false };
        var dayNames = new[] { "S", "M", "T", "W", "T", "F", "S" };

        var title = new Label
        {
            Text = "Custom repeat",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#29272F"),
            Padding = new Thickness(20, 12, 20, 4),
            VerticalOptions = LayoutOptions.Center
        };
        title.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));

        var frequency = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(68), new(56), new(GridLength.Star) },
            ColumnSpacing = 8,
            Padding = new Thickness(20, 4, 20, 4)
        };
        var everyLabel = new Label { Text = "Every", FontSize = 13, VerticalTextAlignment = TextAlignment.Center };
        everyLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));
        frequency.Children.Add(everyLabel);
        var count = new Border
        {
            WidthRequest = 56,
            HeightRequest = 36,
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Content = new Label { Text = "11", FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        };
        count.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E1DDED"), Color.FromArgb("#494650"));
        count.Background = Brush.Transparent;
        if (count.Content is Label countLabel)
            countLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));
        Grid.SetColumn(count, 1);
        frequency.Children.Add(count);
        var interval = new Border
        {
            HeightRequest = 36,
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Content = new Picker { ItemsSource = new[] { "Days", "Weeks", "Months", "Years" }, SelectedIndex = 1, FontSize = 13, Margin = new Thickness(0, 0) }
        };
        interval.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E1DDED"), Color.FromArgb("#494650"));
        interval.Background = Brush.Transparent;
        if (interval.Content is Picker pickerControl)
            pickerControl.SetAppThemeColor(Picker.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));
        Grid.SetColumn(interval, 2);
        frequency.Children.Add(interval);

        var days = new HorizontalStackLayout { Spacing = 7, HorizontalOptions = LayoutOptions.Center, Padding = new Thickness(0, 10, 0, 10) };
        for (var index = 0; index < dayNames.Length; index++)
        {
            var button = CreateDayButton(dayNames[index], selected[index]);
            var captured = index;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                selected[captured] = !selected[captured];
                button.Stroke = selected[captured] ? Color.FromArgb("#7633DA") : Color.FromArgb("#C9C5D4");
                button.Background = Brush.Transparent;
                if (button.Content is Label label)
                    label.SetAppThemeColor(Label.TextColorProperty,
                        selected[captured] ? Color.FromArgb("#7633DA") : Color.FromArgb("#1C1B1F"),
                        selected[captured] ? Color.FromArgb("#D0BCFF") : Color.FromArgb("#E6E1E5"));
            };
            button.GestureRecognizers.Add(tap);
            days.Children.Add(button);
        }

        var buttons = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) }, ColumnSpacing = 8, Padding = new Thickness(16, 4), HeightRequest = 50 };
        var cancel = CreateTextActionButton("Cancel", () => _customPopup.Dismiss());
        var save = CreateActionButton("Save", () => { _viewModel?.SetDraftRepeatFrequency(NoteRepeatFrequency.Custom); _customPopup.Dismiss(); });
        buttons.Children.Add(cancel); Grid.SetColumn(cancel, 1); buttons.Children.Add(save); Grid.SetColumn(save, 2);

        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(44), new(52), new(68), new(50) } };
        content.Children.Add(title);
        content.Children.Add(frequency); Grid.SetRow(frequency, 1);
        content.Children.Add(days); Grid.SetRow(days, 2);
        content.Children.Add(buttons); Grid.SetRow(buttons, 3);
        return CreateBorder(300, 224, content);
    }

    private static Border CreateDayButton(string text, bool selected)
    {
        var label = new Label
        {
            Text = text,
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        label.SetAppThemeColor(Label.TextColorProperty,
            selected ? Color.FromArgb("#7633DA") : Color.FromArgb("#1C1B1F"),
            selected ? Color.FromArgb("#D0BCFF") : Color.FromArgb("#E6E1E5"));

        return new Border
        {
            WidthRequest = 34,
            HeightRequest = 34,
            StrokeThickness = 1,
            Stroke = selected ? Color.FromArgb("#7633DA") : Color.FromArgb("#C9C5D4"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 17 },
            Background = Brush.Transparent,
            Content = label
        };
    }

    private static SfButton CreateMenuButton(string text, bool selected, Action action, string? iconGlyph = null)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = 40,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            Background = Brush.Transparent,
            FontSize = 14,
            CornerRadius = 8,
            Margin = new Thickness(0, 1),
            Padding = 0,
            Command = new Command(action)
        };
        ApplyButtonStates(button, selected);
        button.SetAppThemeColor(SfButton.TextColorProperty,
            selected ? Color.FromArgb("#7633DA") : Color.FromArgb("#1C1B1F"),
            selected ? Color.FromArgb("#D0BCFF") : Color.FromArgb("#E6E1E5"));
        button.Content = new DataTemplate(() =>
        {
            var hasIcon = !string.IsNullOrEmpty(iconGlyph);
            var grid = new Grid
            {
                ColumnDefinitions = hasIcon
                    ? new ColumnDefinitionCollection { new(22), new(GridLength.Star) }
                    : new ColumnDefinitionCollection { new(GridLength.Star) },
                ColumnSpacing = 4,
                Padding = new Thickness(12, 0),
                InputTransparent = true
            };
            if (hasIcon)
            {
                var iconLabel = new Label
                {
                    Text = iconGlyph,
                    FontFamily = "MaterialIconRegular",
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalTextAlignment = TextAlignment.Center
                };
                iconLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb("#49454F"),
                    Color.FromArgb("#CAC4D0"));
                grid.Children.Add(iconLabel);
            }
            var textLabel = new Label
            {
                FontSize = 14,
                VerticalTextAlignment = TextAlignment.Center
            };
            textLabel.SetBinding(Label.TextProperty,
                new Binding(nameof(SfButton.Text), source: button));
            textLabel.SetBinding(Label.TextColorProperty,
                new Binding(nameof(SfButton.TextColor), source: button));
            grid.Children.Add(textLabel);
            if (hasIcon)
                textLabel.SetValue(Grid.ColumnProperty, 1);
            return grid;
        });
        return button;
    }

    private static SfButton CreateActionButton(string text, Action action)
    {
        var button = new SfButton
        {
            Text = text,
            MinimumWidthRequest = 72,
            HeightRequest = 36,
            Background = Brush.Transparent,
            TextColor = Color.FromArgb("#6750A4"),
            FontSize = 13,
            CornerRadius = 8,
            Command = new Command(action)
        };
        return button;
    }

    private static SfButton CreateTextActionButton(string text, Action action)
    {
        var button = new SfButton
        {
            Text = text,
            MinimumWidthRequest = 72,
            HeightRequest = 36,
            Background = Brush.Transparent,
            FontSize = 13,
            CornerRadius = 8,
            Padding = new Thickness(12, 8),
            TextColor = Color.FromArgb("#6750A4"),
            Command = new Command(action)
        };
        return button;
    }

    private static SfButton CreateIconButton(string glyph, Action action)
    {
        var button = new SfButton
        {
            Text = glyph,
            FontFamily = "MaterialIconRegular",
            WidthRequest = 40,
            HeightRequest = 40,
            Padding = 0,
            Background = Brush.Transparent,
            FontSize = 18,
            Command = new Command(action)
        };
        button.SetAppThemeColor(SfButton.TextColorProperty,
            Color.FromArgb("#1C1B1F"),
            Color.FromArgb("#E6E1E5"));
        ApplyIconButtonStates(button);
        return button;
    }

    private static void ApplyIconButtonStates(SfButton button)
    {
        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        commonStates.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        pointerOver.Setters.Add(new Setter { Property = SfButton.TextColorProperty, Value = Color.FromArgb("#7633DA") });
        commonStates.States.Add(pointerOver);
        var pressed = new VisualState { Name = "Pressed" };
        pressed.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        pressed.Setters.Add(new Setter { Property = SfButton.TextColorProperty, Value = Color.FromArgb("#7633DA") });
        commonStates.States.Add(pressed);
        VisualStateManager.SetVisualStateGroups(button, new VisualStateGroupList { commonStates });
    }

    private static void ApplyButtonStates(SfButton button, bool selected)
    {
        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        commonStates.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        pointerOver.Setters.Add(new Setter { Property = SfButton.TextColorProperty, Value = Color.FromArgb("#7633DA") });
        commonStates.States.Add(pointerOver);
        var pressed = new VisualState { Name = "Pressed" };
        pressed.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        pressed.Setters.Add(new Setter { Property = SfButton.TextColorProperty, Value = Color.FromArgb("#7633DA") });
        commonStates.States.Add(pressed);
        VisualStateManager.SetVisualStateGroups(button, new VisualStateGroupList { commonStates });
    }

    private static Border CreateBorder(double width, double height, View content)
    {
        var border = new Border
        {
            WidthRequest = width,
            HeightRequest = height,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Padding = 0,
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 27 },
            Content = content
        };
        border.SetAppTheme<Brush>(VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        return border;
    }

    private void ClampPosition(object? sender, PopupPositionChangingEventArgs e)
    {
        if (Window is null || Window.Width <= 0 || Window.Height <= 0) return;
        const double padding = 8;
        var maxX = Math.Max(padding, Window.Width - e.Bounds.Width - padding);
        var maxY = Math.Max(padding, Window.Height - e.Bounds.Height - padding);
        e.MoveTo = new Point(Math.Clamp(e.MoveTo.X, padding, maxX), Math.Clamp(e.MoveTo.Y, padding, maxY));
        e.Handled = true;
    }
}