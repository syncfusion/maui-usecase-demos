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
    private const double MenuItemHeight = 42;
    private const double SectionHeight = 25;
    private const double ThemePickerHeight = 42;
    private const double ImageGalleryHeight = 66;
    private const double MenuTopMargin = 10;
    private const double SurfaceBorderAllowance = 4;

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
    private Point _mainPopupPosition;
    private bool _hasMainPopupPosition;
    private bool CanEditSelectedList => _selectedList is { IsSystem: false };

#if WINDOWS
    private Microsoft.UI.Xaml.FrameworkElement? _nativeRoot;
#endif

    public ListOptionsFlyout()
    {
        _popup = CreatePopup(220, 220, CreateMainSurface);
        _sortPopup = CreatePopup(180, 178, CreateSortSurface);
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
        _hasMainPopupPosition = false;
        RebuildMenu();
        _isOpen = true;
        var popupHeight = GetMainMenuHeight();
        _popup.HeightRequest = popupHeight;
        if (_surface is not null)
        {
            _surface.HeightRequest = _popup.HeightRequest;
            RebuildMenu();
        }
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
            PopupStyle = new PopupStyle
            {
                OverlayColor = Colors.Transparent,
                MessageBackground = Colors.Transparent,
                HasShadow = true,
                Stroke = Colors.Transparent,
                StrokeThickness = 0,
                CornerRadius = 27
            },
            ContentTemplate = new DataTemplate(contentFactory)
        };

    private View CreateMainSurface()
    {
        _menuActions = new VerticalStackLayout
        {
            Spacing = 0,
            Margin = new Thickness(0, MenuTopMargin, 0, 0)
        };
        RebuildMenu();
        _surface = CreateSurface(220, GetMainMenuHeight(), _menuActions);
        return _surface;
    }

    private double GetMainMenuHeight()
    {
        var height = MenuTopMargin
            + MenuItemHeight
            + SectionHeight
            + ThemePickerHeight
            + SectionHeight
            + ImageGalleryHeight
            + SurfaceBorderAllowance;

        if (CanEditSelectedList)
            height += MenuItemHeight + MenuItemHeight + 12;

        return height;
    }

    private void RebuildMenu()
    {
        if (_menuActions is null)
            return;

        _menuActions.Children.Clear();
        _menuActions.Margin = new Thickness(10, MenuTopMargin, 10, 0);
        if (CanEditSelectedList)
        {   
            var renameButton = CreateMenuButton("Rename list", RenameList, iconGlyph: "\uf88c");
            renameButton.Margin = new Thickness(10, 0, 10, 0);
            _menuActions.Children.Add(renameButton);
        }

        _sortButton = CreateMenuButton("Sort by", ShowSortMenu, iconGlyph: "\uea18", iconRotation: 90);
        _sortButton.Margin = new Thickness(10, 0, 0, 0); 
        var sortRow = new Grid { HeightRequest = 42 };
        sortRow.Children.Add(_sortButton);
        sortRow.Children.Add(new Label
        {
            Text = "›",
            FontSize = 18,
            Margin = new Thickness(0, 0, 16, 0),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Center,
            TextColor = Color.FromArgb("#1C1B1F"),
            InputTransparent = true
        });
        var sortArrow = (Label)sortRow.Children[1];
        sortArrow.SetAppThemeColor(Label.TextColorProperty,
            Color.FromArgb("#1C1B1F"),
            Color.FromArgb("#E6E1E5"));
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
                Margin = new Thickness(12, 3),
                Color = Color.FromArgb("#E6E2E8")
            });
            var deleteButton = CreateMenuButton("Delete list", DeleteList, destructive: true, iconGlyph: "\ue92e", iconColor: "#B3261E");
            deleteButton.Margin = new Thickness(10, 0, 10, 0);
            _menuActions.Children.Add(deleteButton);
        }
        RebuildThemePicker();
        RebuildCustomImageGallery();
    }

    private View CreateSortSurface()
    {
        var actions = new VerticalStackLayout
        {
            Spacing = 0,
            Margin = new Thickness(10, MenuTopMargin, 10, 10),
        };
        var currentSort = _selectedList?.SortOrder ?? NoteListSortOrder.Custom;
        actions.Children.Add(CreateMenuButton("Importance", () => ApplySort(NoteListSortOrder.Important), currentSort == NoteListSortOrder.Important, iconGlyph: "\ue83a"));
        actions.Children.Add(CreateMenuButton("Reminder", () => ApplySort(NoteListSortOrder.Reminder), currentSort == NoteListSortOrder.Reminder, iconGlyph: "\ue7f5"));
        actions.Children.Add(CreateMenuButton("Due date", () => ApplySort(NoteListSortOrder.DueDate), currentSort == NoteListSortOrder.DueDate, iconGlyph: "\ue24f"));
        actions.Children.Add(CreateMenuButton("Alphabetically", () => ApplySort(NoteListSortOrder.Alphabetical), currentSort == NoteListSortOrder.Alphabetical, iconGlyph: "\ue241"));
        var sortHeight = MenuTopMargin + (MenuItemHeight * 4) + SurfaceBorderAllowance + 20;
        _sortSurface = CreateSurface(180, sortHeight, actions);
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
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(27) },
            Content = new ScrollView
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            },
        };
        surface.SetAppTheme<Brush>(VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        surface.SetAppThemeColor(Border.StrokeProperty, Color.FromArgb("#E3DFEA"), Color.FromArgb("#44414B"));
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
            var swatchSurface = PointerHoverSurface.Wrap(swatch, 8, new Thickness(3));
            SemanticProperties.SetDescription(swatchSurface, $"{theme.Name} theme");
            swatchSurface.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => ApplyColorTheme(theme.Hex))
            });
            colors.Children.Add(swatchSurface);
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

    private static SfButton CreateMenuButton(string text, Action action, bool selected = false, bool destructive = false, string? iconGlyph = null, double iconRotation = 0, string? iconColor = null)
    {
        var button = new SfButton
        {
            Text = text,
            HeightRequest = 42,
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
        if (!string.IsNullOrEmpty(iconGlyph))
        {
            button.Content = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection { new(22), new(GridLength.Star) },
                    ColumnSpacing = 4,
                    Padding = new Thickness(14, 0),
                    InputTransparent = true
                };
                var iconLabel = new Label
                {
                    Text = iconGlyph,
                    FontFamily = "MaterialIconRegular",
                    FontSize = 18,
                    Rotation = iconRotation,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalTextAlignment = TextAlignment.Center
                };
                iconLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb(iconColor ?? "#49454F"),
                    Color.FromArgb(iconColor ?? "#CAC4D0"));
                grid.Children.Add(iconLabel);
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
                textLabel.SetValue(Grid.ColumnProperty, 1);
                return grid;
            });
        }
        SemanticProperties.SetDescription(button, text);
        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        commonStates.States.Add(new VisualState { Name = "Normal" });
        var pointerOver = new VisualState { Name = "PointerOver" };
        pointerOver.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        commonStates.States.Add(pointerOver);
        var pressed = new VisualState { Name = "Pressed" };
        pressed.Setters.Add(new Setter { Property = VisualElement.BackgroundProperty, Value = Brush.Transparent });
        commonStates.States.Add(pressed);
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
            var sortHeight = MenuTopMargin + (MenuItemHeight * 4) + SurfaceBorderAllowance;
            _sortPopup.HeightRequest = sortHeight;
            _sortPopup.ContentTemplate = new DataTemplate(CreateSortSurface);
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
    {
        ClampPopupPosition(e);
        _mainPopupPosition = e.MoveTo;
        _hasMainPopupPosition = true;
    }

    private void OnSortPopupPositionChanging(object? sender, PopupPositionChangingEventArgs e)
    {
        ClampPopupPosition(e);

        var window = Window;
        if (!_hasMainPopupPosition || window is null || window.Width <= 0 || window.Height <= 0)
            return;

        const double edgePadding = 8;
        const double popupGap = 8;
        var maxY = Math.Max(edgePadding, window.Height - e.Bounds.Height - edgePadding);
        var mainRight = _mainPopupPosition.X + _popup.WidthRequest;
        var mainBottom = _mainPopupPosition.Y + _popup.HeightRequest;
        var sortRight = e.MoveTo.X + e.Bounds.Width;
        var sortBottom = e.MoveTo.Y + e.Bounds.Height;
        var overlapsMain = e.MoveTo.X < mainRight
            && sortRight > _mainPopupPosition.X
            && e.MoveTo.Y < mainBottom
            && sortBottom > _mainPopupPosition.Y;

        if (!overlapsMain)
            return;

        var rightX = mainRight + popupGap;
        if (rightX + e.Bounds.Width <= window.Width - edgePadding)
        {
            e.MoveTo = new Point(rightX, Math.Clamp(e.MoveTo.Y, edgePadding, maxY));
        }
        else
        {
            var leftX = _mainPopupPosition.X - e.Bounds.Width - popupGap;
            if (leftX >= edgePadding)
            {
                e.MoveTo = new Point(leftX, Math.Clamp(e.MoveTo.Y, edgePadding, maxY));
            }
            else
            {
                var belowY = mainBottom + popupGap;
                var aboveY = _mainPopupPosition.Y - e.Bounds.Height - popupGap;
                var y = belowY + e.Bounds.Height <= window.Height - edgePadding
                    ? belowY
                    : Math.Max(edgePadding, aboveY);
                var maxX = Math.Max(edgePadding, window.Width - e.Bounds.Width - edgePadding);
                e.MoveTo = new Point(Math.Clamp(e.MoveTo.X, edgePadding, maxX), y);
            }
        }

        e.Handled = true;
    }

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
