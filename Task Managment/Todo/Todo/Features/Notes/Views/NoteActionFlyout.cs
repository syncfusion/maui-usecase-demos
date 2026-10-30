using System.Collections.ObjectModel;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Popup;
using Microsoft.Maui.Controls.Shapes;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.Models;
using Todo.Features.Notes.ViewModels;

namespace Todo.Features.Notes.Views;

public sealed class NoteActionFlyout : ContentView
{
    private readonly SfPopup _popup;
    private readonly ObservableCollection<NoteActionDestination> _destinations = new();
    private Note? _selectedNote;
    private VerticalStackLayout? _actionsLayout;
    private VerticalStackLayout? _destinationsLayout;
    private Border? _surface;
    private SfButton? _completionButton;
    private SfButton? _restoreButton;
    private SfButton? _deleteButton;
    private BoxView? _deleteSeparator;
    private bool _closeAnimationRunning;
    private bool _allowPopupClose;

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
            HeightRequest = 212,
            AutoSizeMode = PopupAutoSizeMode.None,
            AnimationMode = PopupAnimationMode.None,
            AnimationDuration = 150,
            AnimationEasing = PopupAnimationEasing.SinOut,
            ContentTemplate = new DataTemplate(CreateMenuSurface)
        };
        _popup.Opened += OnPopupOpened;
        _popup.Closing += async (_, e) =>
        {
            if (_allowPopupClose)
            {
                _allowPopupClose = false;
                return;
            }

            if (_surface is null)
                return;

            e.Cancel = true;
            if (_closeAnimationRunning)
                return;

            _closeAnimationRunning = true;
            await Task.WhenAll(
                _surface.FadeToAsync(0, 100, Easing.SinIn),
                _surface.ScaleToAsync(0.96, 100, Easing.SinIn));

            _closeAnimationRunning = false;
            _allowPopupClose = true;
            _popup.Dismiss();
        };
        _popup.PositionChanging += OnPopupPositionChanging;

        Content = new Grid
        {
            Children = { _popup }
        };
    }

    public void ShowFor(View anchor, Note note, IEnumerable<NoteList> customLists)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(customLists);

        _selectedNote = note;
        _destinations.Clear();
        _destinations.Add(new("My Notes", NotesListViewModel.EncodeDropTarget(NotesListCategory.MyNotes)));
        _destinations.Add(new("Important", NotesListViewModel.EncodeDropTarget(NotesListCategory.Important)));
        _destinations.Add(new("Reminder", NotesListViewModel.EncodeDropTarget(NotesListCategory.Reminder)));
        _destinations.Add(new("Bin", NotesListViewModel.EncodeDropTarget(NotesListCategory.Bin)));
        foreach (var list in customLists)
            _destinations.Add(new(list.Name, NotesListViewModel.EncodeDropTarget(list.Id)));

        UpdateMenu();
        UpdateDestinations();
        ShowActions();
        _popup.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomRight);
    }

    private View CreateMenuSurface()
    {
        _actionsLayout = new VerticalStackLayout { Spacing = 0 };
        _destinationsLayout = new VerticalStackLayout { Spacing = 0, IsVisible = false };

        _completionButton = CreateButton(string.Empty, () => RaiseAction(NoteActionKind.ToggleCompletion));
        _actionsLayout.Children.Add(_completionButton);
        _actionsLayout.Children.Add(CreateButton("✎    Edit note", () => RaiseAction(NoteActionKind.Edit)));
        _actionsLayout.Children.Add(CreateButton("↪    Move notes to     ›", ShowDestinations));

        _restoreButton = CreateButton("↶    Restore note", () => RaiseAction(NoteActionKind.Restore));
        _actionsLayout.Children.Add(_restoreButton);
        _deleteSeparator = new BoxView { HeightRequest = 1, Margin = new Thickness(0, 4), Color = Color.FromArgb("#E6E2E2") };
        _actionsLayout.Children.Add(_deleteSeparator);
        _deleteButton = CreateButton(string.Empty, () =>
            RaiseAction(_selectedNote?.DeletedAt is null ? NoteActionKind.Delete : NoteActionKind.DeletePermanently),
            destructive: true);
        _actionsLayout.Children.Add(_deleteButton);

        var stage = new Grid();
        stage.Children.Add(_actionsLayout);
        stage.Children.Add(_destinationsLayout);

        _surface = new Border
        {
            WidthRequest = 220,
            HeightRequest = 212,
            Padding = new Thickness(1),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = new ScrollView
            {
                Content = stage,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            },
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.12f,
                Radius = 8,
                Offset = new Point(0, 2)
            }
        };
        _surface.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#29272F"));
        _surface.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#CAC4D0"), Color.FromArgb("#494650"));
        UpdateDestinations();
        UpdateMenu();

        return _surface;
    }

    private SfButton CreateButton(string text, Action action, bool destructive = false)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = 44,
            Padding = new Thickness(14, 0),
            Background = Colors.Transparent,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Fill,
            HorizontalTextAlignment = TextAlignment.Start,
            CornerRadius = 0,
            Command = new Command(action)
        };
        button.SetAppThemeColor(SfButton.TextColorProperty,
            destructive ? Color.FromArgb("#C62828") : Color.FromArgb("#29272F"),
            destructive ? Color.FromArgb("#FF6B6B") : Color.FromArgb("#F5F3F7"));
        SemanticProperties.SetDescription(button, text);
        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        commonStates.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter
        {
            Property = VisualElement.BackgroundColorProperty,
            Value = Color.FromArgb("#F1EEF2")
        });
        commonStates.States.Add(pointerOver);
        VisualStateManager.SetVisualStateGroups(button, new VisualStateGroupList { commonStates });
        return button;
    }

    private void UpdateDestinations()
    {
        if (_destinationsLayout is null)
            return;

        _destinationsLayout.Children.Clear();
        _destinationsLayout.Children.Add(CreateButton("‹    Move notes to", ShowActions));
        _destinationsLayout.Children.Add(new BoxView { HeightRequest = 1, Margin = new Thickness(0, 4), Color = Color.FromArgb("#E6E2E2") });
        foreach (var destination in _destinations)
        {
            _destinationsLayout.Children.Add(CreateButton(
                destination.Name,
                () => RaiseAction(NoteActionKind.Move, destination.Key)));
        }
    }

    private void UpdateMenu()
    {
        if (_selectedNote is null || _completionButton is null || _restoreButton is null || _deleteButton is null || _deleteSeparator is null)
            return;

        _completionButton.Text = _selectedNote.IsCompleted
            ? "✓    Mark as not completed"
            : "✓    Mark as completed";
        _restoreButton.IsVisible = _selectedNote.DeletedAt is not null;
        _deleteSeparator.IsVisible = _selectedNote.DeletedAt is not null;
        _deleteButton.Text = _selectedNote.DeletedAt is null
            ? "▱    Delete task"
            : "▱    Delete permanently";
    }

    private void ShowDestinations()
    {
        if (_actionsLayout is not null) _actionsLayout.IsVisible = false;
        if (_destinationsLayout is not null) _destinationsLayout.IsVisible = true;
    }

    private void ShowActions()
    {
        if (_actionsLayout is not null) _actionsLayout.IsVisible = true;
        if (_destinationsLayout is not null) _destinationsLayout.IsVisible = false;
    }

    private void RaiseAction(NoteActionKind action, string? targetKey = null)
    {
        if (_selectedNote is null)
            return;

        var args = new NoteActionRequestedEventArgs(_selectedNote, action, targetKey);
        _popup.Dismiss();
        ActionRequested?.Invoke(this, args);
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_surface is null)
            return;

        _surface.Opacity = 0;
        _surface.Scale = 0.96;
        await Task.WhenAll(
            _surface.FadeToAsync(1, 120, Easing.SinOut),
            _surface.ScaleToAsync(1, 120, Easing.SinOut));
    }

    private void OnPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
    {
        var window = Window;
        if (window is null || window.Width <= 0 || window.Height <= 0)
            return;

        const double edgePadding = 8;
        var maxX = Math.Max(edgePadding, window.Width - e.Bounds.Width - edgePadding);
        var maxY = Math.Max(edgePadding, window.Height - e.Bounds.Height - edgePadding);
        e.MoveTo = new Point(
            Math.Clamp(e.MoveTo.X, edgePadding, maxX),
            Math.Clamp(e.MoveTo.Y, edgePadding, maxY));
        e.Handled = true;
    }
}

public enum NoteActionKind
{
    ToggleCompletion,
    Edit,
    Move,
    Restore,
    Delete,
    DeletePermanently
}

public sealed record NoteActionDestination(string Name, string Key);

public sealed class NoteActionRequestedEventArgs(Note note, NoteActionKind action, string? targetKey) : EventArgs
{
    public Note Note { get; } = note;
    public NoteActionKind Action { get; } = action;
    public string? TargetKey { get; } = targetKey;
}
