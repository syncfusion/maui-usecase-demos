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
    private bool _isMobileSheet;
    private bool _showingMobileThemeSheet;
    private bool _showingMobilePhotoTab;
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
            if (!_closingFlyoutsTogether)
                _isMobileSheet = false;
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
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            return MenuTopMargin + (MenuItemHeight * (CanEditSelectedList ? 4 : 2)) + 20;

        var height = MenuTopMargin
            + MenuItemHeight
            + SectionHeight
            + ThemePickerHeight
            + SectionHeight
            + ImageGalleryHeight
            + GetSurfaceBorderAllowance();

        if (CanEditSelectedList)
            height += MenuItemHeight + MenuItemHeight + 14;

        return height;
    }

    private void RebuildMenu()
    {
        if (_menuActions is null)
            return;

        _menuActions.Children.Clear();
        var isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
        var isDesktop = DeviceInfo.Current.Idiom == DeviceIdiom.Desktop;
        _menuActions.Margin = isPhone
            ? new Thickness(0, MenuTopMargin, 0, 0)
            : isDesktop
                ? new Thickness(0, MenuTopMargin, 0, 0)
                : new Thickness(10, MenuTopMargin, 10, 0);
        if (CanEditSelectedList)
        {   
            var renameButton = CreateMenuButton("Rename list", RenameList, iconGlyph: "\uf88c");
            if (!isPhone && !isDesktop)
                renameButton.Margin = new Thickness(10, 0, 10, 0);
            _menuActions.Children.Add(renameButton);
        }

        _sortButton = CreateMenuButton("Sort by", ShowSortMenu, iconGlyph: "\uea18", iconRotation: 90);
        if (isPhone)
        {
            _menuActions.Children.Add(_sortButton);
            _menuActions.Children.Add(CreateMenuButton(
                "Change Theme",
                ShowMobileThemeSheet,
                iconGlyph: "\ue40a"));
            _themePicker = null;
            _customImageGallery = null;
        }
        else
        {
            if (!isDesktop)
                _sortButton.Margin = new Thickness(10, 0, 0, 0);
            var sortRow = new Grid { HeightRequest = MenuItemHeight };
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
        }
        if (CanEditSelectedList)
        {
            var separator = new BoxView
            {
                HeightRequest = 1,
                Margin = new Thickness(12, 4),
                Color = Color.FromArgb("#E9E5EE")
            };
            separator.SetAppThemeColor(
                BoxView.ColorProperty,
                Color.FromArgb("#E9E5EE"),
                Color.FromArgb("#494650"));
            _menuActions.Children.Add(separator);
            var deleteButton = CreateMenuButton("Delete list", DeleteList, destructive: true, iconGlyph: "\ue92e", iconColor: "#B3261E");
            if (!isPhone)
                deleteButton.Margin = new Thickness(10, 0, 10, 0);
            _menuActions.Children.Add(deleteButton);
        }
        if (!isPhone)
        {
            RebuildThemePicker();
            RebuildCustomImageGallery();
        }
    }

    private View CreateSortSurface()
    {
        var actions = new VerticalStackLayout
        {
            Spacing = 0,
            Margin = DeviceInfo.Current.Idiom == DeviceIdiom.Phone
                ? new Thickness(0, MenuTopMargin, 0, 10)
                : DeviceInfo.Current.Idiom == DeviceIdiom.Desktop
                    ? new Thickness(0, MenuTopMargin, 0, 10)
                    : new Thickness(10, MenuTopMargin, 10, 10),
        };
        var currentSort = _selectedList?.SortOrder ?? NoteListSortOrder.Custom;
        actions.Children.Add(CreateMenuButton("Importance", () => ApplySort(NoteListSortOrder.Important), currentSort == NoteListSortOrder.Important, iconGlyph: "\ue83a"));
        actions.Children.Add(CreateMenuButton("Reminder", () => ApplySort(NoteListSortOrder.Reminder), currentSort == NoteListSortOrder.Reminder, iconGlyph: "\ue7f5"));
        actions.Children.Add(CreateMenuButton("Due date", () => ApplySort(NoteListSortOrder.DueDate), currentSort == NoteListSortOrder.DueDate, iconGlyph: "\ue24f"));
        actions.Children.Add(CreateMenuButton("Alphabetically", () => ApplySort(NoteListSortOrder.Alphabetical), currentSort == NoteListSortOrder.Alphabetical, iconGlyph: "\ue241"));
        var sortHeight = GetSortSurfaceHeight();
        _sortSurface = CreateSurface(180, sortHeight, actions);
        return _sortSurface;
    }

    private View CreateMobileSortSurface()
    {
        var currentSort = _selectedList?.SortOrder ?? NoteListSortOrder.Custom;
        var actions = new VerticalStackLayout { Spacing = 2 };
        actions.Children.Add(CreateMobileSheetHeader("Sort by"));
        actions.Children.Add(CreateMobileSortOption(
            "Important",
            "\ue83a",
            currentSort == NoteListSortOrder.Important,
            () => ApplySort(NoteListSortOrder.Important)));
        actions.Children.Add(CreateMobileSortOption(
            "Reminder",
            "\ue7f5",
            currentSort == NoteListSortOrder.Reminder,
            () => ApplySort(NoteListSortOrder.Reminder)));
        actions.Children.Add(CreateMobileSortOption(
            "Due date",
            "\ue24f",
            currentSort == NoteListSortOrder.DueDate,
            () => ApplySort(NoteListSortOrder.DueDate)));
        actions.Children.Add(CreateMobileSortOption(
            "Alphabetically",
            "\ue241",
            currentSort == NoteListSortOrder.Alphabetical,
            () => ApplySort(NoteListSortOrder.Alphabetical)));
        _sortSurface = CreateMobileSheetSurface(296, actions);
        return _sortSurface;
    }

    private View CreateMobileThemeSurface()
    {
        var content = new VerticalStackLayout { Spacing = 8 };
        content.Children.Add(CreateMobileSheetHeader("Change Theme"));
        var tabs = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new(GridLength.Star),
                new(GridLength.Star)
            },
            ColumnSpacing = 8
        };
        tabs.Children.Add(CreateMobileThemeTab("Color", showPhotoTab: false));
        var photoTab = CreateMobileThemeTab("Photo", showPhotoTab: true);
        photoTab.SetValue(Grid.ColumnProperty, 1);
        tabs.Children.Add(photoTab);
        content.Children.Add(tabs);

        if (!_showingMobilePhotoTab)
        {
            var colors = new HorizontalStackLayout { Spacing = 10 };
            foreach (var theme in NoteListThemePalette.Themes)
            {
                var isSelected = string.Equals(
                    _viewModel?.GetBackgroundColorHex(_selectedList, _selectedCategory),
                    theme.Hex,
                    StringComparison.OrdinalIgnoreCase);
                var color = Color.FromArgb(theme.Hex);
                var swatch = new Border
                {
                    WidthRequest = 38,
                    HeightRequest = 38,
                    Padding = 1,
                    BackgroundColor = color,
                    Stroke = isSelected ? Color.FromArgb("#5C4EAE") : Color.FromArgb("#CAC4D0"),
                    StrokeThickness = isSelected ? 3 : 1,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(19) },
                    AutomationId = $"mobile-theme-{theme.Name.ToLowerInvariant()}"
                };
                SemanticProperties.SetDescription(swatch, $"{theme.Name} theme");
                swatch.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => ApplyColorTheme(theme.Hex))
                });
                colors.Children.Add(swatch);
            }

            content.Children.Add(new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Content = colors,
                HeightRequest = 44,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            });
        }
        else
        {
            var photos = new HorizontalStackLayout { Spacing = 10 };
            var addPhotoLabel = new Label
            {
                Text = "+",
                FontSize = 24,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            addPhotoLabel.SetAppThemeColor(
                Label.TextColorProperty,
                Color.FromArgb("#514B56"),
                Color.FromArgb("#E6E1E5"));
            var addPhoto = new Border
            {
                WidthRequest = 38,
                HeightRequest = 38,
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#CAC4D0"),
                BackgroundColor = Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(19) },
                Content = addPhotoLabel
            };
            SemanticProperties.SetDescription(addPhoto, "Add a custom background photo");
            addPhoto.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await PickCustomBackgroundAsync())
            });
            photos.Children.Add(addPhoto);

            var selectedImage = _viewModel?.GetBackgroundImageSource(_selectedList, _selectedCategory);
            var imageSources = (_viewModel?.GetAvailableDefaultBackgroundImageSources(_selectedList, _selectedCategory) ?? [])
                .Concat(_viewModel?.GetCustomBackgroundImageSources(_selectedList, _selectedCategory) ?? [])
                .Distinct(StringComparer.Ordinal);
            foreach (var source in imageSources)
            {
                var isSelected = string.Equals(selectedImage, source, StringComparison.Ordinal);
                var imageGrid = new Grid();
                imageGrid.Children.Add(new Image
                {
                    Source = source,
                    WidthRequest = 36,
                    HeightRequest = 36,
                    Clip = new Microsoft.Maui.Controls.Shapes.EllipseGeometry
                    {
                        Center = new Point(18, 18),
                        RadiusX = 18,
                        RadiusY = 18
                    },
                    Aspect = Aspect.AspectFill,
                    InputTransparent = true
                });
                var removeButton = new Border
                {
                    WidthRequest = 17,
                    HeightRequest = 17,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    BackgroundColor = Color.FromArgb("#CC29272F"),
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(9) },
                    Content = new Label
                    {
                        Text = "×",
                        TextColor = Colors.White,
                        FontSize = 12,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    }
                };
                SemanticProperties.SetDescription(removeButton, "Remove background photo");
                removeButton.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => RemoveImageBackground(source))
                });
                imageGrid.Children.Add(removeButton);

                var photo = new Border
                {
                    WidthRequest = 38,
                    HeightRequest = 38,
                    Padding = 1,
                    Stroke = isSelected ? Color.FromArgb("#5C4EAE") : Color.FromArgb("#CAC4D0"),
                    StrokeThickness = isSelected ? 3 : 1,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(19) },
                    Content = imageGrid
                };
                SemanticProperties.SetDescription(photo, "Apply background photo");
                photo.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => ApplyImageBackground(source))
                });
                photos.Children.Add(photo);
            }

            content.Children.Add(new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Content = photos,
                HeightRequest = 44,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Never
            });
        }

        _sortSurface = CreateMobileSheetSurface(200, content);
        return _sortSurface;
    }

    private View CreateMobileThemeTab(string title, bool showPhotoTab)
    {
        var isSelected = _showingMobilePhotoTab == showPhotoTab;
        var button = new SfButton
        {
            Text = title,
            HeightRequest = 40,
            Padding = 0,
            CornerRadius = 20,
            FontSize = 14,
            Background = isSelected
                ? new SolidColorBrush(Color.FromArgb("#EADDFF"))
                : Brush.Transparent,
            Command = new Command(() =>
            {
                _showingMobilePhotoTab = showPhotoTab;
                _sortPopup.ContentTemplate = new DataTemplate(CreateMobileThemeSurface);
                _sortPopup.Refresh();
            })
        };
        button.SetAppThemeColor(
            SfButton.TextColorProperty,
            Color.FromArgb("#1C1B1F"),
            Color.FromArgb("#E6E1E5"));
        SemanticProperties.SetDescription(button, $"{title} theme options");
        return button;
    }

    private static View CreateMobileSheetHeader(string title)
    {
        var layout = new VerticalStackLayout { Spacing = 12, Margin = new Thickness(0, 0, 0, 4) };
        layout.Children.Add(new Border
        {
            WidthRequest = 32,
            HeightRequest = 4,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromArgb("#CAC4D0"),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(2) }
        });
        var titleLabel = new Label
        {
            Text = title,
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        };
        titleLabel.SetAppThemeColor(
            Label.TextColorProperty,
            Color.FromArgb("#1C1B1F"),
            Color.FromArgb("#E6E1E5"));
        layout.Children.Add(titleLabel);
        return layout;
    }

    private SfButton CreateMobileSortOption(string text, string iconGlyph, bool selected, Action action)
    {
        var button = CreateMenuButton(text, action, selected, iconGlyph: iconGlyph);
        button.HeightRequest = 48;
        button.FontSize = 16;
        button.CornerRadius = 12;
        button.Content = new DataTemplate(() =>
        {
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new(24),
                    new(GridLength.Star)
                },
                ColumnSpacing = 0,
                Padding = 0,
                InputTransparent = true
            };
            var icon = new Label
            {
                Text = iconGlyph,
                FontFamily = "MaterialIconRegular",
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Center
            };
            icon.SetAppThemeColor(
                Label.TextColorProperty,
                Color.FromArgb("#49454F"),
                Color.FromArgb("#E6E1E5"));
            row.Children.Add(icon);
            var label = new Label
            {
                FontSize = button.FontSize,
                VerticalTextAlignment = TextAlignment.Center
            };
            label.SetBinding(Label.TextProperty, new Binding(nameof(SfButton.Text), source: button));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(SfButton.TextColor), source: button));
            label.SetValue(Grid.ColumnProperty, 1);
            row.Children.Add(label);
            return row;
        });
        if (selected)
            button.Background = new SolidColorBrush(Color.FromArgb("#EADDFF"));
        return button;
    }

    private static Border CreateMobileSheetSurface(double height, View content)
    {
        var surface = new Border
        {
            WidthRequest = Application.Current?.Windows.FirstOrDefault()?.Width ?? 360,
            HeightRequest = height,
            Padding = new Thickness(20, 14, 20, 20),
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(24, 24, 0, 0)
            },
            Content = content
        };
        surface.SetAppTheme<Brush>(
            VisualElement.BackgroundProperty,
            Brush.White,
            new SolidColorBrush(Color.FromArgb("#29272F")));
        surface.Shadow = new Shadow
        {
            Brush = Color.FromArgb("#40000000"),
            Radius = 14,
            Offset = new Point(0, -4),
            Opacity = 1
        };
        return surface;
    }

    private static Border CreateSurface(double width, double height, View content)
    {
        var surface = new Border
        {
            WidthRequest = width,
            HeightRequest = height,
            Padding = DeviceInfo.Current.Idiom == DeviceIdiom.Desktop
                ? new Thickness(8)
                : new Thickness(1),
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

        var customImageSources = _viewModel?.GetCustomBackgroundImageSources(_selectedList, _selectedCategory) ?? [];
        foreach (var source in (_viewModel?.GetAvailableDefaultBackgroundImageSources(_selectedList, _selectedCategory) ?? [])
                     .Concat(customImageSources)
                     .Distinct(StringComparer.Ordinal))
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
            SemanticProperties.SetDescription(removeButton, "Remove background image");
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
                var isPhone = DeviceInfo.Current.Idiom == DeviceIdiom.Phone;
                var isDesktop = DeviceInfo.Current.Idiom == DeviceIdiom.Desktop;
                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new(isPhone ? 32 : isDesktop ? 24 : 22),
                        new(GridLength.Star)
                    },
                    ColumnSpacing = isDesktop ? 8 : 4,
                    Padding = isPhone
                        ? new Thickness(12, 0)
                        : isDesktop ? new Thickness(12, 0) : new Thickness(14, 0),
                    InputTransparent = true
                };
                var iconLabel = new Label
                {
                    Text = iconGlyph,
                    FontFamily = "MaterialIconRegular",
                    FontSize = 18,
                    Rotation = iconRotation,
                    Margin = isPhone
                        ? Thickness.Zero
                        : isDesktop ? Thickness.Zero : new Thickness(0, 0, 5, 0),
                    VerticalTextAlignment = TextAlignment.Center
                };
                if (isPhone)
                {
                    iconLabel.WidthRequest = 32;
                    iconLabel.HorizontalTextAlignment = TextAlignment.Center;
                }
                iconLabel.SetAppThemeColor(Label.TextColorProperty,
                    Color.FromArgb(iconColor ?? "#49454F"),
                    Color.FromArgb(iconColor ?? "#CAC4D0"));
                grid.Children.Add(iconLabel);
                var textLabel = new Label
                {
                    FontSize = button.FontSize,
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
            if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone)
            {
                ShowMobileBottomSheet(showTheme: false);
                return;
            }

            _isSortOpen = true;
            _popup.StaysOpen = true;
            var sortHeight = DeviceInfo.Current.Idiom == DeviceIdiom.Desktop
                ? GetSortSurfaceHeight()
                : MenuTopMargin + (MenuItemHeight * 4) + SurfaceBorderAllowance;
            _sortPopup.HeightRequest = sortHeight;
            _sortPopup.ContentTemplate = new DataTemplate(CreateSortSurface);
            _sortPopup.ShowRelativeToView(_sortButton, PopupRelativePosition.AlignToRightOf);
        }
    }

    private void ShowMobileThemeSheet()
    {
        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone && _viewModel is not null)
            ShowMobileBottomSheet(showTheme: true);
    }

    private void ShowMobileBottomSheet(bool showTheme)
    {
        var window = Window ?? Application.Current?.Windows.FirstOrDefault();
        if (window is null || window.Width <= 0 || window.Height <= 0)
            throw new InvalidOperationException("The mobile bottom sheet requires an active window with a measured size.");

        _showingMobileThemeSheet = showTheme;
        _isMobileSheet = true;
        _isSortOpen = false;
        _isOpen = false;
        _popup.StaysOpen = false;
        _allowPopupClose = true;
        _popup.Dismiss();

        var sheetHeight = showTheme ? 200 : 296;
        _sortPopup.WidthRequest = window.Width;
        _sortPopup.HeightRequest = sheetHeight;
        _sortPopup.AnimationMode = PopupAnimationMode.SlideOnBottom;
        _sortPopup.AnimationDuration = 240;
        _sortPopup.ShowOverlayAlways = true;
        _sortPopup.OverlayMode = PopupOverlayMode.Transparent;
        _sortPopup.PopupStyle = new PopupStyle
        {
            OverlayColor = Color.FromArgb("#66000000"),
            MessageBackground = Colors.Transparent,
            HasShadow = false,
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            CornerRadius = 0
        };
        _sortPopup.BackgroundColor = Colors.Transparent;
        Func<View> contentFactory = showTheme ? CreateMobileThemeSurface : CreateMobileSortSurface;
        if (showTheme)
            _showingMobilePhotoTab = false;
        _sortPopup.ContentTemplate = new DataTemplate(contentFactory);
        _sortPopup.Show(0, Math.Max(0, window.Height - sheetHeight));
    }

    private static double GetSurfaceBorderAllowance()
        => DeviceInfo.Current.Idiom == DeviceIdiom.Desktop ? 18 : SurfaceBorderAllowance;

    private static double GetSortSurfaceHeight()
        => DeviceInfo.Current.Idiom == DeviceIdiom.Desktop
            ? (MenuItemHeight * 4) + (MenuTopMargin * 2) + 18
            : MenuTopMargin + (MenuItemHeight * 4) + SurfaceBorderAllowance + 20;

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
        if (_isMobileSheet && _showingMobileThemeSheet)
        {
            _sortPopup.ContentTemplate = new DataTemplate(CreateMobileThemeSurface);
            _sortPopup.Refresh();
        }
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
        _isMobileSheet = false;
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
        if (_isMobileSheet)
            return;
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
        var window = Window ?? Application.Current?.Windows.FirstOrDefault();
        if (_isMobileSheet && window is { Width: > 0, Height: > 0 })
        {
            e.MoveTo = new Point(0, Math.Max(0, window.Height - _sortPopup.HeightRequest));
            e.Handled = true;
            return;
        }

        ClampPopupPosition(e);

        window = Window;
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
