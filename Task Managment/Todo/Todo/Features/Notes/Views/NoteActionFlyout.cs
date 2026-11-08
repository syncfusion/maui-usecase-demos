using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Popup;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public sealed class NoteActionFlyout : ContentView
{
    private readonly SfPopup _popup;
    private readonly SfPopup _destinationsPopup;
    private readonly ObservableCollection<NoteActionDestination> _destinations = new();
    private IReadOnlyList<Note> _visibleNotes = Array.Empty<Note>();
    private Note? _selectedNote;
    private VerticalStackLayout? _actionsLayout;
    private VerticalStackLayout? _destinationsLayout;
    private Border? _surface;
    private Border? _destinationsSurface;
    private Grid? _selectAllRow;
    private View? _selectAllSurface;
    private CheckBox? _selectAllCheckBox;
    private BoxView? _selectAllSeparator;
    private SfButton? _completionButton;
    private SfButton? _editButton;
    private SfButton? _moveButton;
    private SfButton? _restoreButton;
    private SfButton? _deleteButton;
    private BoxView? _deleteSeparator;
    private bool _closeAnimationRunning;
    private bool _allowPopupClose;
    private bool _isUpdatingSelectAll;
    private bool? _isCompactDeletedMenu;
    private bool _closingFlyoutsTogether;
    private Point _mainPopupPosition;
    private bool _hasMainPopupPosition;

    public event EventHandler<NoteActionRequestedEventArgs>? ActionRequested;

    public NoteActionFlyout()
    {
        _popup = new SfPopup
        {
            ShowHeader = false,
            ShowFooter = false,
            ShowOverlayAlways = false,
            OverlayMode = PopupOverlayMode.Transparent,
            StaysOpen = false,
            WidthRequest = 220,
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
            ContentTemplate = new DataTemplate(CreateMenuSurface)
        };

        _destinationsPopup = new SfPopup
        {
            ShowHeader = false,
            ShowFooter = false,
            ShowOverlayAlways = false,
            OverlayMode = PopupOverlayMode.Transparent,
            StaysOpen = false,
            WidthRequest = 220,
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
            }
        };
        _destinationsPopup.ContentTemplate = new DataTemplate(CreateDestinationsSurface);

        _popup.Closing += async (_, e) =>
        {
            if (_allowPopupClose)
            {
                _allowPopupClose = false;
                return;
            }

            if (_surface is null)
                return;

            var wasClosingFlyoutsTogether = _closingFlyoutsTogether;
            _closingFlyoutsTogether = true;
            _destinationsPopup.Dismiss();
            _closingFlyoutsTogether = wasClosingFlyoutsTogether;

            e.Cancel = true;
            if (_closeAnimationRunning)
                return;

            _closeAnimationRunning = true;
            await Task.WhenAll(
                _surface.FadeToAsync(0, 100, Easing.SinIn),
                _surface.ScaleToAsync(0.97, 100, Easing.SinIn));
            _closeAnimationRunning = false;
            _allowPopupClose = true;
            _popup.Dismiss();
        };

        _popup.PositionChanging += OnPopupPositionChanging;
        _destinationsPopup.PositionChanging += OnDestinationsPopupPositionChanging;
        _destinationsPopup.Closing += (_, _) =>
        {
            if (_closingFlyoutsTogether)
                return;

            _popup.StaysOpen = false;
            _closingFlyoutsTogether = true;
            _popup.Dismiss();
            _closingFlyoutsTogether = false;
        };
        _popup.Opened += OnPopupOpened;

        Content = new Grid { Children = { _popup, _destinationsPopup } };
    }

    public void ShowFor(View anchor, Note note, IEnumerable<NoteList> customLists, IEnumerable<Note> visibleNotes)
    {
        _selectedNote = note;
        _hasMainPopupPosition = false;
        _popup.StaysOpen = false;
        var wasClosingFlyoutsTogether = _closingFlyoutsTogether;
        _closingFlyoutsTogether = true;
        _destinationsPopup.Dismiss();
        _closingFlyoutsTogether = wasClosingFlyoutsTogether;
        _visibleNotes = visibleNotes.Where(visibleNote => visibleNote.DeletedAt is null).ToArray();
        _destinations.Clear();
        _destinations.Add(new("My Notes", NotesListViewModel.EncodeDropTarget(NotesListCategory.MyNotes)));
        _destinations.Add(new("Important", NotesListViewModel.EncodeDropTarget(NotesListCategory.Important)));
        _destinations.Add(new("Reminder", NotesListViewModel.EncodeDropTarget(NotesListCategory.Reminder)));
        _destinations.Add(new("Bin", NotesListViewModel.EncodeDropTarget(NotesListCategory.Bin)));
        foreach (var list in customLists) _destinations.Add(new(list.Name, NotesListViewModel.EncodeDropTarget(list.Id)));

        UpdateMenu();
        UpdateDestinations();
        UpdatePopupSize();
        _popup.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomRight);
    }

    private View CreateMenuSurface()
    {
        _actionsLayout = new VerticalStackLayout { Spacing = 5 , Margin = new Thickness(5, 0, 0, 0) };

        _selectAllRow = new Grid
        {
            HeightRequest = 40,
            ColumnDefinitions = new ColumnDefinitionCollection { new(32), new(GridLength.Star) },
            ColumnSpacing = 4
        };
        _selectAllCheckBox = new CheckBox
        {
            WidthRequest = 32,
            HeightRequest = 32,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        _selectAllCheckBox.SetAppThemeColor(CheckBox.ColorProperty, Color.FromArgb("#6750A4"), Color.FromArgb("#D0BCFF"));
        SemanticProperties.SetDescription(_selectAllCheckBox, "Select all visible notes");
        _selectAllCheckBox.CheckedChanged += OnSelectAllCheckedChanged;
        _selectAllRow.Children.Add(_selectAllCheckBox);
        var selectAllLabel = new Label
        {
            Text = "Select all",
            FontSize = 14,
            VerticalTextAlignment = TextAlignment.Center
        };
        selectAllLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#1C1B1F"), Color.FromArgb("#E6E1E5"));
        selectAllLabel.SetValue(Grid.ColumnProperty, 1);
        selectAllLabel.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                if (_selectAllCheckBox is not null)
                    _selectAllCheckBox.IsChecked = !_selectAllCheckBox.IsChecked;
            })
        });
        _selectAllRow.Children.Add(selectAllLabel);
        _selectAllSurface = PointerHoverSurface.Wrap(_selectAllRow, 8, Thickness.Zero);

        _selectAllSeparator = new BoxView { HeightRequest = 1, Margin = new Thickness(0) };
        _selectAllSeparator.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#ECE8F1"), Color.FromArgb("#494650"));

        _completionButton = CreateButton("Mark as completed", () => RaiseAction(NoteActionKind.ToggleCompletion), iconGlyph: "\ue876");
        _editButton = CreateButton("Edit note", () => RaiseAction(NoteActionKind.Edit), iconGlyph: "\ue150");
        _moveButton = CreateButton("Move notes to", ShowDestinations, iconGlyph: "\ue5c4");
        _restoreButton = CreateButton("Restore note", () => RaiseAction(NoteActionKind.Restore), iconGlyph: "\ue5d5");
        _deleteSeparator = new BoxView { HeightRequest = 1, Margin = new Thickness(0) };
        _deleteSeparator.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#ECE8F1"), Color.FromArgb("#FFFFFF"));
        _deleteButton = CreateButton("Delete task", () =>
        {
            if (_selectedNote?.DeletedAt is not null)
                RaiseAction(NoteActionKind.DeletePermanently);
            else
                RaiseAction(NoteActionKind.Delete);
        }, destructive: true, iconGlyph: "\ue92e", iconColor: "#B3261E");

        var stage = new Grid();
        stage.Margin = new Thickness(0,10);
        stage.Children.Add(_actionsLayout);

        _surface = new Border
        {
            WidthRequest = 220,
            Padding = 8,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(27) },
            Content = stage
        };
        _surface.SetAppTheme<Brush>(VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        _surface.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E3DFEA"), Color.FromArgb("#44414B"));
        _surface.Shadow = CreatePopupShadow();

        UpdateMenu();
        return _surface;
    }

    private View CreateDestinationsSurface()
    {
        _destinationsLayout = new VerticalStackLayout { Spacing = 0, Margin = new Thickness(5, 10) };
        _destinationsSurface = new Border
        {
            WidthRequest = 220,
            Padding = 8,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(27) },
            Content = _destinationsLayout
        };
        _destinationsSurface.SetAppTheme<Brush>(VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        _destinationsSurface.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E3DFEA"), Color.FromArgb("#44414B"));
        _destinationsSurface.Shadow = CreatePopupShadow();
        UpdateDestinations();
        return _destinationsSurface;
    }

    private static Shadow CreatePopupShadow() => new()
    {
        Brush = Color.FromArgb("#40000000"),
        Radius = 14,
        Offset = new Point(0, 6),
        Opacity = 1
    };

    private static SfButton CreateButton(string text, Action action, bool destructive = false, string? iconGlyph = null, string? iconColor = null)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = 40,
            Padding = 0,
            Background = Brush.Transparent,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            CornerRadius = 8,
            Command = new Command(action)
        };
        button.SetAppThemeColor(SfButton.TextColorProperty,
            destructive ? Color.FromArgb("#D32F2F") : Color.FromArgb("#1C1B1F"),
            destructive ? Color.FromArgb("#FFB4AB") : Color.FromArgb("#E6E1E5"));
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
                    destructive ? Color.FromArgb("#B3261E") : Color.FromArgb("#1C1B1F"),
                    destructive ? Color.FromArgb("#FFB4AB") : Color.FromArgb("#E6E1E5"));
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
        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        commonStates.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter
        {
            Property = VisualElement.BackgroundProperty,
            Value = Brush.Transparent
        });
        commonStates.States.Add(pointerOver);
        var pressed = new VisualState { Name = "Pressed" };
        pressed.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        commonStates.States.Add(pressed);
        VisualStateManager.SetVisualStateGroups(button, new VisualStateGroupList { commonStates });
        return button;
    }

    private void UpdateDestinations()
    {
        if (_destinationsLayout is null) return;
        _destinationsLayout.Children.Clear();
        _destinationsLayout.Children.Add(CreateButton("Move notes to", HideDestinations, iconGlyph: "\ue5c4", iconColor: "#49454F"));
        var separator = new BoxView { HeightRequest = 1, Margin = new Thickness(0) };
        separator.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#ECE8F1"), Color.FromArgb("#494650"));
        _destinationsLayout.Children.Add(separator);
        foreach (var destination in _destinations)
            _destinationsLayout.Children.Add(CreateButton(destination.Name, () => RaiseAction(NoteActionKind.Move, destination.Key)));

        if (_destinationsSurface is not null)
            _destinationsSurface.HeightRequest = GetDestinationsPopupHeight();
    }

    private void UpdateMenu()
    {
        if (_selectedNote is null || _selectAllRow is null || _selectAllCheckBox is null || _selectAllSeparator is null
            || _selectAllSurface is null || _completionButton is null || _editButton is null || _moveButton is null
            || _restoreButton is null || _deleteButton is null || _deleteSeparator is null || _actionsLayout is null) return;
        var isDeleted = _selectedNote.DeletedAt is not null;
        UpdateActionLayout(isDeleted);
        var canSelectAll = !isDeleted && _visibleNotes.Count > 0;
        _selectAllRow.IsVisible = canSelectAll;
        _selectAllSeparator.IsVisible = canSelectAll;
        UpdateSelectAllState();
        _completionButton.IsVisible = !isDeleted;
        _editButton.IsVisible = !isDeleted;
        _moveButton.IsVisible = !isDeleted;
        _restoreButton.IsVisible = isDeleted;
        _deleteSeparator.IsVisible = isDeleted;
        _deleteButton.Text = isDeleted ? "Delete permanently" : "Delete task";
    }

    private void UpdateActionLayout(bool isDeleted)
    {
        var useCompactLayout = isDeleted
            && (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Idiom == DeviceIdiom.Phone);
        if (_isCompactDeletedMenu == useCompactLayout || _actionsLayout is null)
            return;

        _isCompactDeletedMenu = useCompactLayout;
        _actionsLayout.Children.Clear();
        if (useCompactLayout)
        {
            _actionsLayout.Children.Add(_restoreButton!);
            _actionsLayout.Children.Add(_deleteSeparator!);
            _actionsLayout.Children.Add(_deleteButton!);
            return;
        }

        _actionsLayout.Children.Add(_selectAllSurface!);
        _actionsLayout.Children.Add(_selectAllSeparator!);
        _actionsLayout.Children.Add(_completionButton!);
        _actionsLayout.Children.Add(_editButton!);
        _actionsLayout.Children.Add(_moveButton!);
        _actionsLayout.Children.Add(_restoreButton!);
        _actionsLayout.Children.Add(_deleteSeparator!);
        _actionsLayout.Children.Add(_deleteButton!);
    }

    private void OnSelectAllCheckedChanged(object? sender, Microsoft.Maui.Controls.CheckedChangedEventArgs e)
    {
        if (_isUpdatingSelectAll || _visibleNotes.Count == 0 || _selectedNote is null)
            return;

        _popup.Dismiss();
        ActionRequested?.Invoke(this, new NoteActionRequestedEventArgs(
            _selectedNote,
            NoteActionKind.SetVisibleCompletion,
            null,
            e.Value,
            _visibleNotes.Select(note => note.Id).ToArray()));
    }

    private void UpdateSelectAllState()
    {
        if (_selectAllCheckBox is null)
            return;

        _isUpdatingSelectAll = true;
        _selectAllCheckBox.IsChecked = _visibleNotes.Count > 0 && _visibleNotes.All(note => note.IsCompleted);
        _isUpdatingSelectAll = false;
    }

    private void ShowDestinations()
    {
        if (_moveButton is null)
            return;

        UpdateDestinations();
        _destinationsPopup.HeightRequest = GetDestinationsPopupHeight();
        _popup.StaysOpen = true;
        _destinationsPopup.ShowRelativeToView(_moveButton, PopupRelativePosition.AlignToRightOf);
    }

    private double GetDestinationsPopupHeight()
        => ((_destinations.Count + 1) * 40) + 1 + 16 + 20;

    private void HideDestinations()
    {
        var wasClosingFlyoutsTogether = _closingFlyoutsTogether;
        _closingFlyoutsTogether = true;
        _destinationsPopup.Dismiss();
        _closingFlyoutsTogether = wasClosingFlyoutsTogether;
        _popup.StaysOpen = false;
    }

    private void UpdatePopupSize()
    {
        double contentHeight;
        var isDeleted = _selectedNote?.DeletedAt is not null;
        var useContentSizedPopup = isDeleted
            && (DeviceInfo.Current.Platform == DevicePlatform.Android || DeviceInfo.Current.Idiom == DeviceIdiom.Phone);
        if (isDeleted)
        {
            // Restore button, separator, and delete button.
            contentHeight = 40 + 1 + 40 + 10;
        }
        else
        {
            // Select-all row, separator, and the four note actions.
            contentHeight = 40 + 1 + (4 * 40) + (5 * 5);
        }

        _popup.AutoSizeMode = useContentSizedPopup ? PopupAutoSizeMode.Height : PopupAutoSizeMode.None;
        var heightRequest = useContentSizedPopup ? -1 : contentHeight + 16 + 20;
        _popup.HeightRequest = heightRequest;

        if (_surface is not null)
            _surface.HeightRequest = heightRequest;
    }

    private void RaiseAction(NoteActionKind action, string? targetKey = null)
    {
        if (_selectedNote is null) return;
        var args = new NoteActionRequestedEventArgs(_selectedNote, action, targetKey);
        _closingFlyoutsTogether = true;
        _destinationsPopup.Dismiss();
        _popup.StaysOpen = false;
        _popup.Dismiss();
        _closingFlyoutsTogether = false;
        ActionRequested?.Invoke(this, args);
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_surface is null) return;
        _surface.Opacity = 0;
        _surface.Scale = 0.97;
        await Task.WhenAll(_surface.FadeToAsync(1, 120, Easing.SinOut), _surface.ScaleToAsync(1, 120, Easing.SinOut));
    }

    private void OnPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
    {
        var window = Window;
        if (window is null || window.Width <= 0 || window.Height <= 0) return;
        const double edgePadding = 8;
        var maxX = Math.Max(edgePadding, window.Width - e.Bounds.Width - edgePadding);
        var maxY = Math.Max(edgePadding, window.Height - e.Bounds.Height - edgePadding);
        var position = new Point(Math.Clamp(e.MoveTo.X, edgePadding, maxX), Math.Clamp(e.MoveTo.Y, edgePadding, maxY));
        _mainPopupPosition = position;
        _hasMainPopupPosition = true;
        e.MoveTo = position;
        e.Handled = true;
    }

    private void OnDestinationsPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
    {
        var window = Window;
        if (window is null || window.Width <= 0 || window.Height <= 0)
            return;

        const double edgePadding = 8;
        var maxX = Math.Max(edgePadding, window.Width - e.Bounds.Width - edgePadding);
        var maxY = Math.Max(edgePadding, window.Height - e.Bounds.Height - edgePadding);
        var x = Math.Clamp(e.MoveTo.X, edgePadding, maxX);
        var y = Math.Clamp(e.MoveTo.Y, edgePadding, maxY);

        if (_hasMainPopupPosition)
        {
            var rightOfMain = _mainPopupPosition.X + _popup.WidthRequest + edgePadding;
            var leftOfMain = _mainPopupPosition.X - e.Bounds.Width - edgePadding;

            if (rightOfMain + e.Bounds.Width <= window.Width - edgePadding)
                x = rightOfMain;
            else if (leftOfMain >= edgePadding)
                x = leftOfMain;
            else
            {
                var belowMain = _mainPopupPosition.Y + _popup.HeightRequest + edgePadding;
                var aboveMain = _mainPopupPosition.Y - e.Bounds.Height - edgePadding;
                x = Math.Clamp(_mainPopupPosition.X, edgePadding, maxX);
                if (belowMain + e.Bounds.Height <= window.Height - edgePadding)
                    y = belowMain;
                else if (aboveMain >= edgePadding)
                    y = aboveMain;
            }
        }

        e.MoveTo = new Point(x, y);
        e.Handled = true;
    }
}

public enum NoteActionKind { ToggleCompletion, SetVisibleCompletion, Edit, Move, Restore, Delete, DeletePermanently }
public sealed record NoteActionDestination(string Name, string Key);
public sealed class NoteActionRequestedEventArgs(Note note, NoteActionKind action, string? targetKey, bool? isCompleted = null, IReadOnlyList<string>? noteIds = null) : EventArgs
{
    public Note Note { get; } = note;
    public NoteActionKind Action { get; } = action;
    public string? TargetKey { get; } = targetKey;
    public bool? IsCompleted { get; } = isCompleted;
    public IReadOnlyList<string>? NoteIds { get; } = noteIds;
}