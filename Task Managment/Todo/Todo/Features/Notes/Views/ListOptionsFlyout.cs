using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Popup;
using Todo.Common;
using Todo.Features.CustomLists.Models;
using Todo.Features.Notes.ViewModels;

#if WINDOWS
using Windows.System;
#endif

namespace Todo.Features.Notes.Views;

public sealed class ListOptionsFlyout : ContentView
{
    private readonly SfPopup _popup;
    private readonly SfPopup _sortPopup;
    private Border? _surface;
    private Border? _sortSurface;
    private SfButton? _sortButton;
    private ScrollView? _themePicker;
    private HorizontalStackLayout? _customImageGallery;
    private VerticalStackLayout? _menuActions;
    private NoteList? _selectedList;
    private NotesListCategory _selectedCategory;
    private NotesListViewModel? _viewModel;
    private bool _closeAnimationRunning;
    private bool _allowPopupClose;
    private bool _isOpen;
    private bool _isSortOpen;
    private bool _closingFlyoutsTogether;
    private bool CanEditSelectedList => _selectedList is { IsSystem: false };

#if WINDOWS
    private Microsoft.UI.Xaml.FrameworkElement? _nativeRoot;
#endif

    public ListOptionsFlyout()
    {
        _popup = CreatePopup(250, 330, CreateMainSurface);
        _sortPopup = CreatePopup(190, 220, CreateSortSurface);
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

            _closingFlyoutsTogether = true;
            _sortPopup.Dismiss();
            _closingFlyoutsTogether = false;
            _closeAnimationRunning = true;
            await Task.WhenAll(
                _surface.FadeToAsync(0, 100, Easing.SinIn),
                _surface.ScaleToAsync(0.96, 100, Easing.SinIn));
            _closeAnimationRunning = false;
            _isOpen = false;
            _allowPopupClose = true;
            _popup.Dismiss();
        };
        _popup.PositionChanging += OnPopupPositionChanging;
        _sortPopup.Opened += OnSortPopupOpened;
        _sortPopup.Closing += (_, _) =>
        {
            _isSortOpen = false;
            if (!_closingFlyoutsTogether && _isOpen)
            {
                _popup.StaysOpen = false;
                _popup.Dismiss();
            }
        };
        _sortPopup.PositionChanging += OnSortPopupPositionChanging;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        Content = new Grid
        {
            Children = { _popup, _sortPopup }
        };
    }

    public void ShowFor(View anchor, NoteList list, NotesListViewModel viewModel)
        => ShowFor(anchor, list, NotesListCategory.MyNotes, viewModel);

    public void ShowFor(View anchor, NotesListCategory category, NotesListViewModel viewModel)
        => ShowFor(anchor, null, category, viewModel);

    private void ShowFor(View anchor, NoteList? list, NotesListCategory category, NotesListViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(anchor);
        ArgumentNullException.ThrowIfNull(viewModel);

        _selectedList = list;
        _selectedCategory = category;
        _viewModel = viewModel;
        RebuildMenu();
        _isOpen = true;
        _popup.HeightRequest = CanEditSelectedList ? 330 : 250;
        if (_surface is not null)
            _surface.HeightRequest = _popup.HeightRequest;
        _popup.ShowRelativeToView(anchor, PopupRelativePosition.AlignBottomRight);
    }

    private static SfPopup CreatePopup(double width, double height, Func<View> contentFactory)
        => new()
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
            ContentTemplate = new DataTemplate(contentFactory)
        };

    private View CreateMainSurface()
    {
        _menuActions = new VerticalStackLayout { Spacing = 0 };
        RebuildMenu();
        _surface = CreateSurface(250, CanEditSelectedList ? 330 : 250, _menuActions);
        return _surface;
    }

    private void RebuildMenu()
    {
        if (_menuActions is null)
            return;

        _menuActions.Children.Clear();
        if (CanEditSelectedList)
            _menuActions.Children.Add(CreateMenuButton("Rename list", RenameList));

        _sortButton = CreateMenuButton("Sort by", ShowSortMenu);
        var sortRow = new Grid { HeightRequest = 42 };
        sortRow.Children.Add(_sortButton);
        sortRow.Children.Add(new Label
        {
            Text = "›",
            FontSize = 18,
            Margin = new Thickness(0, 0, 16, 0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true
        });
        _menuActions.Children.Add(sortRow);
        _menuActions.Children.Add(CreateSectionLabel("Theme colors"));
        _themePicker = CreateThemePicker();
        _menuActions.Children.Add(_themePicker);
        _menuActions.Children.Add(CreateSectionLabel("Customised"));
        _customImageGallery = new HorizontalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(12, 0, 12, 8)
        };
        _menuActions.Children.Add(new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = _customImageGallery,
            HeightRequest = 66,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
        });
        if (CanEditSelectedList)
        {
            _menuActions.Children.Add(new BoxView
            {
                HeightRequest = 1,
                Margin = new Thickness(12, 5),
                Color = Color.FromArgb("#E6E2E8")
            });
            _menuActions.Children.Add(CreateMenuButton("Delete list", DeleteList, destructive: true));
        }
        RebuildThemePicker();
        RebuildCustomImageGallery();
    }

    private View CreateSortSurface()
    {
        var actions = new VerticalStackLayout { Spacing = 0 };
        actions.Children.Add(CreateMenuButton("Importance", () => ApplySort(NoteListSortOrder.Important)));
        actions.Children.Add(CreateMenuButton("Reminder", () => ApplySort(NoteListSortOrder.Reminder)));
        actions.Children.Add(CreateMenuButton("Due date", () => ApplySort(NoteListSortOrder.DueDate)));
        actions.Children.Add(CreateMenuButton("Alphabetically", () => ApplySort(NoteListSortOrder.Alphabetical)));
        _sortSurface = CreateSurface(190, 220, actions);
        return _sortSurface;
    }

    private static Border CreateSurface(double width, double height, View content)
    {
        var surface = new Border
        {
            WidthRequest = width,
            HeightRequest = height,
            Padding = new Thickness(1),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) },
            Content = new ScrollView
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            },
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.14f,
                Radius = 9,
                Offset = new Point(0, 2)
            }
        };
        surface.SetAppThemeColor(BackgroundColorProperty, Colors.White, Color.FromArgb("#29272F"));
        surface.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#CAC4D0"), Color.FromArgb("#494650"));
        return surface;
    }

    private static Label CreateSectionLabel(string text)
        => new()
        {
            Text = text,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#6C6872"),
            Margin = new Thickness(14, 8, 12, 3)
        };

    private ScrollView CreateThemePicker()
    {
        var colors = new HorizontalStackLayout { Spacing = 8, Padding = new Thickness(12, 0, 12, 7) };
        PopulateThemePicker(colors);
        return new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = colors,
            HeightRequest = 42,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never
        };
    }

    private void RebuildThemePicker()
    {
        if (_themePicker is null)
            return;

        if (_themePicker.Content is HorizontalStackLayout colors)
            PopulateThemePicker(colors);
    }

    private void PopulateThemePicker(HorizontalStackLayout colors)
    {
        colors.Children.Clear();
        var selectedColor = _viewModel?.GetBackgroundColorHex(_selectedList, _selectedCategory);
        foreach (var theme in NoteListThemePalette.Themes)
        {
            var color = Color.FromArgb(theme.Hex);
            var isSelected = string.Equals(selectedColor, theme.Hex, StringComparison.OrdinalIgnoreCase);
            var swatch = new Border
            {
                WidthRequest = 27,
                HeightRequest = 27,
                Padding = 0,
                BackgroundColor = color,
                Stroke = isSelected ? Color.FromArgb("#5C4EAE") : Color.FromArgb("#CAC4D0"),
                StrokeThickness = isSelected ? 2 : 1,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6) },
                AutomationId = $"theme-{theme.Name.ToLowerInvariant()}"
            };
            SemanticProperties.SetDescription(swatch, $"{theme.Name} theme");
            swatch.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => ApplyColorTheme(theme.Hex))
            });
            colors.Children.Add(swatch);
        }
    }

    private void RebuildCustomImageGallery()
    {
        if (_customImageGallery is null)
            return;

        _customImageGallery.Children.Clear();
        var addButton = new Border
        {
            WidthRequest = 30,
            HeightRequest = 30,
            Margin = new Thickness(0, 4, 0, 0),
            StrokeThickness = 0,
            BackgroundColor = Colors.Transparent,
            Content = new Label
            {
                Text = "+",
                FontSize = 24,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#514B56")
            }
        };
        SemanticProperties.SetDescription(addButton, "Add a custom background image");
        addButton.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () => await PickCustomBackgroundAsync())
        });
        _customImageGallery.Children.Add(addButton);

        foreach (var source in _viewModel?.GetCustomBackgroundImageSources(_selectedList, _selectedCategory) ?? [])
        {
            var isSelected = string.Equals(
                _viewModel?.GetBackgroundImageSource(_selectedList, _selectedCategory),
                source,
                StringComparison.Ordinal);
            var thumbnailGrid = new Grid();
            thumbnailGrid.Children.Add(new Image
            {
                Source = source,
                Aspect = Aspect.AspectFill,
                InputTransparent = true
            });

            var removeButton = new Border
            {
                WidthRequest = 18,
                HeightRequest = 18,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, -3, -3, 0),
                StrokeThickness = 0,
                BackgroundColor = Color.FromArgb("#CC29272F"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(9) },
                Content = new Label
                {
                    Text = "×",
                    TextColor = Colors.White,
                    FontSize = 13,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                }
            };
            SemanticProperties.SetDescription(removeButton, "Remove custom background image");
            removeButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => RemoveImageBackground(source))
            });
            thumbnailGrid.Children.Add(removeButton);

            var thumbnail = new Border
            {
                WidthRequest = 46,
                HeightRequest = 42,
                Padding = 1,
                StrokeThickness = isSelected ? 2 : 1,
                Stroke = isSelected ? Color.FromArgb("#5C4EAE") : Color.FromArgb("#CAC4D0"),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6) },
                Content = thumbnailGrid
            };
            SemanticProperties.SetDescription(thumbnail, "Apply custom background image");
            thumbnail.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => ApplyImageBackground(source))
            });
            _customImageGallery.Children.Add(thumbnail);
        }
    }

    private static SfButton CreateMenuButton(string text, Action action, bool destructive = false)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = 42,
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

    private void RenameList()
    {
        if (!CanEditSelectedList || _viewModel is null || _selectedList is null)
            return;
        CloseFlyouts();
        _viewModel.OpenRenameListDialogCommand.Execute(_selectedList);
    }

    private void DeleteList()
    {
        if (!CanEditSelectedList || _viewModel is null || _selectedList is null)
            return;
        CloseFlyouts();
        _viewModel.ConfirmDeleteListCommand.Execute(_selectedList);
    }

    private void ShowSortMenu()
    {
        if (!_isSortOpen && _sortButton is not null && _viewModel is not null)
        {
            _isSortOpen = true;
            _popup.StaysOpen = true;
            _sortPopup.ShowRelativeToView(_sortButton, PopupRelativePosition.AlignToRightOf);
        }
    }

    private void ApplySort(NoteListSortOrder sortOrder)
    {
        if (_viewModel is null)
            return;
        _viewModel.ApplyListSort(_selectedList, _selectedCategory, sortOrder);
        CloseFlyouts();
    }

    private void ApplyColorTheme(string colorHex)
    {
        if (_viewModel is null)
            return;
        _viewModel.SetListBackgroundColor(_selectedList, _selectedCategory, colorHex);
        CloseFlyouts();
    }

    private void ApplyImageBackground(string source)
    {
        if (_viewModel is null)
            return;
        _viewModel.SetListBackgroundImage(_selectedList, _selectedCategory, source);
        CloseFlyouts();
    }

    private void RemoveImageBackground(string source)
    {
        if (_viewModel is null)
            return;

        _viewModel.RemoveCustomBackgroundImage(_selectedList, _selectedCategory, source);
        RebuildCustomImageGallery();
    }

    private async Task PickCustomBackgroundAsync()
    {
        if (_viewModel is null)
            return;

        var list = _selectedList;
        var category = _selectedCategory;
        CloseFlyouts();

        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync();
            var photo = photos.FirstOrDefault();
            if (photo is null)
                return;

            var listId = list?.Id ?? category.ToString();
            var extension = Path.GetExtension(Path.GetFileName(photo.FileName));
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".jpg";
            var destination = Path.Combine(
                FileSystem.AppDataDirectory,
                $"list-{listId}-{Guid.NewGuid():N}{extension}");
            await using var source = await photo.OpenReadAsync();
            await using var target = File.Create(destination);
            await source.CopyToAsync(target);

            _viewModel.AddCustomBackgroundImage(list, category, destination);
        }
        catch (Exception ex)
        {
            if (Window?.Page is Page page)
                await page.DisplayAlertAsync("Background image unavailable", ex.Message, "OK");
            else
                throw;
        }
    }

    private void CloseFlyouts()
    {
        _closingFlyoutsTogether = true;
        _popup.StaysOpen = false;
        _sortPopup.Dismiss();
        _popup.Dismiss();
        _closingFlyoutsTogether = false;
    }

    private async void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_surface is null)
            return;
        _isOpen = true;
        _surface.Opacity = 0;
        _surface.Scale = 0.96;
        await Task.WhenAll(
            _surface.FadeToAsync(1, 120, Easing.SinOut),
            _surface.ScaleToAsync(1, 120, Easing.SinOut));
    }

    private async void OnSortPopupOpened(object? sender, EventArgs e)
    {
        if (_sortSurface is null)
            return;
        _isSortOpen = true;
        _sortSurface.Opacity = 0;
        _sortSurface.Scale = 0.96;
        await Task.WhenAll(
            _sortSurface.FadeToAsync(1, 120, Easing.SinOut),
            _sortSurface.ScaleToAsync(1, 120, Easing.SinOut));
    }

    private void OnPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
        => ClampPopupPosition(e);

    private void OnSortPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
        => ClampPopupPosition(e);

    private void ClampPopupPosition(PopupPositionChangingEventArgs e)
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

    private void OnLoaded(object? sender, EventArgs e)
    {
#if WINDOWS
        if (Window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window nativeWindow
            && nativeWindow.Content is Microsoft.UI.Xaml.FrameworkElement root)
        {
            _nativeRoot = root;
            _nativeRoot.KeyDown += OnNativeRootKeyDown;
        }
#endif
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
#if WINDOWS
        if (_nativeRoot is not null)
        {
            _nativeRoot.KeyDown -= OnNativeRootKeyDown;
            _nativeRoot = null;
        }
#endif
    }

#if WINDOWS
    private void OnNativeRootKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Escape || !_isOpen)
            return;
        CloseFlyouts();
        e.Handled = true;
    }
#endif
}
