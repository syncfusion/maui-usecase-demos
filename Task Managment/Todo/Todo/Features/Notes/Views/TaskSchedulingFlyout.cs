using Syncfusion.Maui.Buttons;
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
    private string _selectedMonth = DateTime.Today.ToString("MMMM");
    private int _selectedDay = DateTime.Today.Day;
    private int _selectedYear = DateTime.Today.Year;

    public TaskSchedulingFlyout()
    {
        _datePopup = CreatePopup(360, 344, () => CreateDateSurface(false));
        _repeatPopup = CreatePopup(238, 312, CreateRepeatSurface);
        _customPopup = CreatePopup(330, 214, CreateCustomSurface);
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
        ContentTemplate = new DataTemplate(factory)
    };

    public void ShowDueDate(View anchor, NotesListViewModel viewModel) => ShowDate(anchor, viewModel, false);
    public void ShowReminder(View anchor, NotesListViewModel viewModel) => ShowDate(anchor, viewModel, true);

    private void ShowDate(View anchor, NotesListViewModel viewModel, bool isReminder)
    {
        _viewModel = viewModel;
        var value = isReminder ? viewModel.DraftReminderDate ?? DateTime.Today.AddHours(12) : viewModel.DraftDueDate ?? DateTime.Today.AddHours(12);
        _selectedMonth = value.ToString("MMMM");
        _selectedDay = value.Day;
        _selectedYear = value.Year;
        _datePopup.ContentTemplate = new DataTemplate(() => CreateDateSurface(isReminder));
        _datePopup.ShowRelativeToView(anchor, PopupRelativePosition.AlignTopRight);
    }

    public void ShowRepeat(View anchor, NotesListViewModel viewModel)
    {
        _viewModel = viewModel;
        _repeatAnchor = anchor;
        _repeatPopup.ShowRelativeToView(anchor, PopupRelativePosition.AlignTopRight);
    }

    private View CreateDateSurface(bool isReminder)
    {
        var months = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        var days = Enumerable.Range(1, 31).Select(v => v.ToString()).ToList();
        var years = Enumerable.Range(DateTime.Today.Year - 10, 21).Select(v => v.ToString()).ToList();

        var monthView = CreateWheel(months.ToList(), _selectedMonth, 148, v => _selectedMonth = v);
        var dayView = CreateWheel(days, _selectedDay.ToString(), 72, v => _selectedDay = int.Parse(v));
        var yearView = CreateWheel(years, _selectedYear.ToString(), 140, v => _selectedYear = int.Parse(v));

        var wheels = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(148), new(72), new(140) },
            HeightRequest = 238,
            BackgroundColor = Colors.White
        };
        wheels.Children.Add(monthView); Grid.SetColumn(monthView, 0);
        wheels.Children.Add(dayView); Grid.SetColumn(dayView, 1);
        wheels.Children.Add(yearView); Grid.SetColumn(yearView, 2);

        var monthIndex = Array.FindIndex(months, v => v == _selectedMonth);
        var headerDate = monthIndex >= 0
            ? $"{months[monthIndex]} {_selectedDay}, {_selectedYear}"
            : DateTime.Today.ToString("MMMM d, yyyy");
        var existing = isReminder ? _viewModel?.DraftReminderDate : _viewModel?.DraftDueDate;
        var header = new Grid
        {
            HeightRequest = 56,
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) },
            Padding = new Thickness(20, 0)
        };
        header.Children.Add(new Label
        {
            Text = headerDate,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#29272F"),
            VerticalOptions = LayoutOptions.Center
        });
        var time = new Label
        {
            Text = (existing ?? DateTime.Today.AddHours(12)).ToString("hh:mm tt"),
            FontSize = 13,
            TextColor = Color.FromArgb("#5C4EAE"),
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(time, 1);
        header.Children.Add(time);

        var divider = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E9E5EE") };
        var cancel = CreateTextButton("Cancel", () => _datePopup.Dismiss());
        var save = CreateTextButton("Save", () =>
        {
            var selectedMonthIndex = Array.FindIndex(months, v => v == _selectedMonth) + 1;
            try
            {
                var source = (isReminder ? _viewModel?.DraftReminderDate : _viewModel?.DraftDueDate) ?? DateTime.Today.AddHours(12);
                var value = new DateTime(_selectedYear, selectedMonthIndex, _selectedDay, source.Hour, source.Minute, 0);
                if (isReminder) _viewModel?.SetDraftReminderDate(value);
                else _viewModel?.SetDraftDueDate(value);
            }
            catch { }
            _datePopup.Dismiss();
        });
        var buttons = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) },
            ColumnSpacing = 4,
            HeightRequest = 50,
            Padding = new Thickness(16, 4)
        };
        buttons.Children.Add(cancel); Grid.SetColumn(cancel, 1);
        buttons.Children.Add(save); Grid.SetColumn(save, 2);

        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(56), new(1), new(238), new(50) } };
        content.Children.Add(header);
        content.Children.Add(divider); Grid.SetRow(divider, 1);
        content.Children.Add(wheels); Grid.SetRow(wheels, 2);
        content.Children.Add(buttons); Grid.SetRow(buttons, 3);
        return CreateBorder(360, 344, content);
    }

    private static CollectionView CreateWheel(List<string> items, string selected, double width, Action<string> onSelected)
    {
        var view = new CollectionView
        {
            ItemsSource = items,
            SelectionMode = SelectionMode.Single,
            SelectedItem = selected,
            WidthRequest = width,
            HeightRequest = 238,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 0 },
            BackgroundColor = Colors.White,
            ItemTemplate = new DataTemplate(() =>
            {
                var itemLabel = new Label { FontSize = 13, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                itemLabel.SetBinding(Label.TextProperty, ".");
                var border = new Border
                {
                    HeightRequest = 34,
                    Margin = new Thickness(6, 0),
                    StrokeThickness = 0,
                    BackgroundColor = Colors.Transparent,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                    Content = itemLabel
                };
                var states = new VisualStateGroup { Name = "CommonStates" };
                states.States.Add(new VisualState { Name = "Normal" });
                var selectedState = new VisualState { Name = "Selected" };
                selectedState.Setters.Add(new Setter { Property = Border.BackgroundColorProperty, Value = Color.FromArgb("#F0ECFA") });
                selectedState.Setters.Add(new Setter { Property = Border.StrokeProperty, Value = Color.FromArgb("#C9C0EC") });
                selectedState.Setters.Add(new Setter { Property = Border.StrokeThicknessProperty, Value = 1 });
                states.States.Add(selectedState);
                VisualStateManager.SetVisualStateGroups(border, new VisualStateGroupList { states });
                return border;
            })
        };
        view.SelectionChanged += (_, e) => { if (e.CurrentSelection.FirstOrDefault() is string value) onSelected(value); };
        view.Loaded += (_, _) => view.ScrollTo(selected, null, ScrollToPosition.Center, false);
        return view;
    }

    private View CreateRepeatSurface()
    {
        var content = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(8) };
        content.Children.Add(new Label
        {
            Text = "Repeat",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#29272F"),
            HeightRequest = 44,
            Padding = new Thickness(12, 0),
            VerticalTextAlignment = TextAlignment.Center
        });

        foreach (var option in new[] { NoteRepeatFrequency.Daily, NoteRepeatFrequency.Weekdays, NoteRepeatFrequency.Weekly, NoteRepeatFrequency.Monthly, NoteRepeatFrequency.Yearly })
        {
            var isSelected = _viewModel?.DraftRepeatFrequency == option;
            content.Children.Add(CreateMenuButton(option.ToString(), isSelected,
                () => { _viewModel?.SetDraftRepeatFrequency(option); _repeatPopup.Dismiss(); }));
        }

        content.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#E9E5EE"), Margin = new Thickness(4, 4) });
        content.Children.Add(CreateMenuButton("Custom", _viewModel?.DraftRepeatFrequency == NoteRepeatFrequency.Custom,
            () => { _repeatPopup.Dismiss(); if (_repeatAnchor is not null) _customPopup.ShowRelativeToView(_repeatAnchor, PopupRelativePosition.AlignTopRight); }));
        return CreateBorder(238, 312, content);
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

        var frequency = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(68), new(56), new(GridLength.Star) },
            ColumnSpacing = 8,
            Padding = new Thickness(20, 4, 20, 4)
        };
        frequency.Children.Add(new Label { Text = "Every", FontSize = 13, VerticalTextAlignment = TextAlignment.Center });
        var count = new Border
        {
            WidthRequest = 56,
            HeightRequest = 36,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E1DDED"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#F7F5FA"),
            Content = new Label { Text = "11", FontSize = 14, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        }; Grid.SetColumn(count, 1); frequency.Children.Add(count);
        var interval = new Border
        {
            HeightRequest = 36,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#E1DDED"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Color.FromArgb("#F7F5FA"),
            Content = new Picker { ItemsSource = new[] { "Days", "Weeks", "Months", "Years" }, SelectedIndex = 1, FontSize = 13, Margin = new Thickness(8, 0) }
        }; Grid.SetColumn(interval, 2); frequency.Children.Add(interval);

        var days = new HorizontalStackLayout { Spacing = 7, HorizontalOptions = LayoutOptions.Center, Padding = new Thickness(0, 10, 0, 10) };
        for (var index = 0; index < dayNames.Length; index++)
        {
            var button = CreateDayButton(dayNames[index], selected[index]);
            var captured = index;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                selected[captured] = !selected[captured];
                button.Stroke = selected[captured] ? Color.FromArgb("#7265D6") : Color.FromArgb("#C9C5D4");
                button.BackgroundColor = selected[captured] ? Color.FromArgb("#7265D6") : Colors.Transparent;
                if (button.Content is Label label)
                    label.TextColor = selected[captured] ? Colors.White : Color.FromArgb("#5D5966");
            };
            button.GestureRecognizers.Add(tap);
            days.Children.Add(button);
        }

        var buttons = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto), new(GridLength.Auto) }, ColumnSpacing = 4, Padding = new Thickness(16, 4), HeightRequest = 50 };
        var cancel = CreateTextButton("Cancel", () => _customPopup.Dismiss());
        var save = CreateTextButton("Save", () => { _viewModel?.SetDraftRepeatFrequency(NoteRepeatFrequency.Custom); _customPopup.Dismiss(); });
        buttons.Children.Add(cancel); Grid.SetColumn(cancel, 1); buttons.Children.Add(save); Grid.SetColumn(save, 2);

        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(44), new(52), new(68), new(50) } };
        content.Children.Add(title);
        content.Children.Add(frequency); Grid.SetRow(frequency, 1);
        content.Children.Add(days); Grid.SetRow(days, 2);
        content.Children.Add(buttons); Grid.SetRow(buttons, 3);
        return CreateBorder(330, 214, content);
    }

    private static Border CreateDayButton(string text, bool selected) => new()
    {
        WidthRequest = 34,
        HeightRequest = 34,
        StrokeThickness = 1,
        Stroke = selected ? Color.FromArgb("#7265D6") : Color.FromArgb("#C9C5D4"),
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 17 },
        BackgroundColor = selected ? Color.FromArgb("#7265D6") : Colors.Transparent,
        Content = new Label
        {
            Text = text,
            FontSize = 12,
            TextColor = selected ? Colors.White : Color.FromArgb("#5D5966"),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        }
    };

    private static SfButton CreateMenuButton(string text, bool selected, Action action) => new()
    {
        Text = selected ? $"{text}        ✓" : text,
        HeightRequest = 40,
        HorizontalOptions = LayoutOptions.Fill,
        HorizontalTextAlignment = TextAlignment.Start,
        BackgroundColor = selected ? Color.FromArgb("#F0ECFA") : Colors.Transparent,
        TextColor = selected ? Color.FromArgb("#5C4EAE") : Color.FromArgb("#29272F"),
        FontSize = 14,
        CornerRadius = 8,
        Margin = new Thickness(0, 1),
        Padding = new Thickness(12, 0),
        Command = new Command(action)
    };

    private static SfButton CreateTextButton(string text, Action action) => new()
    {
        Text = text,
        MinimumWidthRequest = 72,
        HeightRequest = 36,
        BackgroundColor = Colors.Transparent,
        TextColor = Color.FromArgb("#6F66CC"),
        FontSize = 13,
        Command = new Command(action)
    };

    private static Border CreateBorder(double width, double height, View content) => new()
    {
        WidthRequest = width,
        HeightRequest = height,
        StrokeThickness = 1,
        Stroke = Color.FromArgb("#D8D4DE"),
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
        BackgroundColor = Colors.White,
        Shadow = new Shadow { Brush = Colors.Black, Opacity = 0.18f, Radius = 12, Offset = new Point(0, 4) },
        Content = content
    };

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
