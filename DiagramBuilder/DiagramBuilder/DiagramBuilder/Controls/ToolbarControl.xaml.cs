using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;
using DiagramToolbarItem = DiagramBuilder.Models.ToolbarItem;

namespace DiagramBuilder.Controls;

/// <summary>
/// Simplified toolbar matching expected design (ui-spec.md §3).
/// Layout: [Undo|Redo] [Pan|Select] [Shapes▼] [Connector▼] [Text] [Fill] [Stroke] [Arrange▼] [Zoom%▼] [Properties]
/// </summary>
public partial class ToolbarControl : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ToolbarControl),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ToolCommandProperty =
        BindableProperty.Create(nameof(ToolCommand), typeof(ICommand), typeof(ToolbarControl));

    public static readonly BindableProperty HasSingleSelectionProperty =
        BindableProperty.Create(nameof(HasSingleSelection), typeof(bool), typeof(ToolbarControl), false);

    public static readonly BindableProperty CurrentZoomProperty =
        BindableProperty.Create(nameof(CurrentZoom), typeof(double), typeof(ToolbarControl), 1.0d,
            propertyChanged: OnCurrentZoomChanged);

    public static readonly BindableProperty ZoomLevelsProperty =
        BindableProperty.Create(nameof(ZoomLevels), typeof(IEnumerable), typeof(ToolbarControl));

    public static readonly BindableProperty SetZoomCommandProperty =
        BindableProperty.Create(nameof(SetZoomCommand), typeof(ICommand), typeof(ToolbarControl));

    public static readonly BindableProperty ShowPropertiesCommandProperty =
        BindableProperty.Create(nameof(ShowPropertiesCommand), typeof(ICommand), typeof(ToolbarControl));

    public static readonly BindableProperty ActiveToolIdProperty =
        BindableProperty.Create(nameof(ActiveToolId), typeof(string), typeof(ToolbarControl),
            string.Empty, propertyChanged: OnActiveToolIdChanged);

    public static readonly BindableProperty SelectedElementIdProperty =
        BindableProperty.Create(nameof(SelectedElementId), typeof(string), typeof(ToolbarControl));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand? ToolCommand
    {
        get => (ICommand?)GetValue(ToolCommandProperty);
        set => SetValue(ToolCommandProperty, value);
    }

    public bool HasSingleSelection
    {
        get => (bool)GetValue(HasSingleSelectionProperty);
        set => SetValue(HasSingleSelectionProperty, value);
    }

    public double CurrentZoom
    {
        get => (double)GetValue(CurrentZoomProperty);
        set => SetValue(CurrentZoomProperty, value);
    }

    public IEnumerable? ZoomLevels
    {
        get => (IEnumerable?)GetValue(ZoomLevelsProperty);
        set => SetValue(ZoomLevelsProperty, value);
    }

    public ICommand? SetZoomCommand
    {
        get => (ICommand?)GetValue(SetZoomCommandProperty);
        set => SetValue(SetZoomCommandProperty, value);
    }

    public ICommand? ShowPropertiesCommand
    {
        get => (ICommand?)GetValue(ShowPropertiesCommandProperty);
        set => SetValue(ShowPropertiesCommandProperty, value);
    }

    public string ActiveToolId
    {
        get => (string)GetValue(ActiveToolIdProperty);
        set => SetValue(ActiveToolIdProperty, value);
    }

    public string? SelectedElementId
    {
        get => (string?)GetValue(SelectedElementIdProperty);
        set => SetValue(SelectedElementIdProperty, value);
    }

    private readonly Dictionary<string, Button> _toggleButtons = new();
    private string _fillColorKey = "AccentFlow";
    private string _strokeColorKey = "TextPrimary";
    private IPropertyService? _propertyService;

    public ToolbarControl()
    {
        InitializeComponent();
        _propertyService = MauiProgram.ServiceProvider?.GetService<IPropertyService>();
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarControl tb)
        {
            tb.Rebuild();
        }
    }

    private static void OnActiveToolIdChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarControl tb)
        {
            tb.ApplyActiveToolHighlight();
        }
    }

    private static void OnCurrentZoomChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarControl tb)
        {
            tb.Rebuild();
        }
    }

    private void ApplyActiveToolHighlight()
    {
        var active = (Style)Application.Current!.Resources["ToolbarIconActiveStyle"];
        var normal = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"];
        foreach (var (id, button) in _toggleButtons)
        {
            button.Style = string.Equals(id, ActiveToolId, StringComparison.Ordinal) ? active : normal;
        }
    }

    private void Rebuild()
    {
        LeftGroupsBar.Children.Clear();
        RightGroupsBar.Children.Clear();
        _toggleButtons.Clear();

        if (ItemsSource is null)
            return;

        var items = ((IEnumerable)ItemsSource).Cast<DiagramToolbarItem>().ToList();

        // Left side: Groups 1-6 (main toolbar items)
        BuildLeftToolbar(items);

        // Right side: Groups 7-8 (zoom + properties)
        BuildRightToolbar(items);

        ApplyActiveToolHighlight();
    }

    private void BuildLeftToolbar(List<DiagramToolbarItem> allItems)
    {
        var byGroup = allItems.GroupBy(i => i.Group).OrderBy(g => g.Key).ToList();

        // Group 1: Undo/Redo
        if (byGroup.Any(g => g.Key == 1))
        {
            LeftGroupsBar.Children.Add(BuildGroup(byGroup.First(g => g.Key == 1)));
            LeftGroupsBar.Children.Add(BuildDivider());
        }

        // Group 2: Pan/Select
        if (byGroup.Any(g => g.Key == 2))
        {
            LeftGroupsBar.Children.Add(BuildGroup(byGroup.First(g => g.Key == 2)));
            LeftGroupsBar.Children.Add(BuildDivider());
        }

        // Group 3: Shapes (flyout)
        if (byGroup.Any(g => g.Key == 3))
        {
            LeftGroupsBar.Children.Add(BuildShapesFlyout());
            LeftGroupsBar.Children.Add(BuildDivider());
        }

        // Group 4: Connectors (flyout)
        if (byGroup.Any(g => g.Key == 4))
        {
            LeftGroupsBar.Children.Add(BuildConnectorsFlyout());
            LeftGroupsBar.Children.Add(BuildDivider());
        }

        // Group 5: Text + Colors
        if (byGroup.Any(g => g.Key == 5))
        {
            var group5Items = byGroup.First(g => g.Key == 5).ToList();
            foreach (var item in group5Items.OrderBy(x => x.Id))
            {
                if (item.Id == AppConstants.ToolText)
                {
                    LeftGroupsBar.Children.Add(BuildIconButton(item));
                }
                else if (item.Id == "tool.stroke.color")
                {
                    LeftGroupsBar.Children.Add(BuildStrokeColorSwatch());
                }
                else if (item.Id == "tool.fill.color")
                {
                    LeftGroupsBar.Children.Add(BuildFillColorSwatch());
                }
            }
            LeftGroupsBar.Children.Add(BuildDivider());
        }

        // Group 6: Arrange dropdown (selection-only)
        if (HasSingleSelection && byGroup.Any(g => g.Key == 6))
        {
            LeftGroupsBar.Children.Add(BuildArrangeDropdown());
            LeftGroupsBar.Children.Add(BuildDivider());
        }
    }

    private void BuildRightToolbar(List<DiagramToolbarItem> allItems)
    {
        // Group 7: Zoom dropdown
        RightGroupsBar.Children.Add(BuildZoomDropdown());

        // Group 8: Properties button
        var btn = new Button
        {
            Text = "☰",
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_properties",
        };
        btn.Clicked += (_, _) => ShowPropertiesCommand?.Execute(null);
        RightGroupsBar.Children.Add(btn);
    }

    private HorizontalStackLayout BuildGroup(IGrouping<int, DiagramToolbarItem> group)
    {
        var layout = new HorizontalStackLayout { Spacing = 0 };
        foreach (var item in group.OrderBy(x => x.Id))
        {
            layout.Children.Add(BuildIconButton(item));
        }
        return layout;
    }

    private View BuildDivider()
    {
        return new BoxView
        {
            Style = (Style)Resources["ToolbarGroupDividerStyle"]
        };
    }

    private Button BuildIconButton(DiagramToolbarItem item)
    {
        var btn = new Button
        {
            Text = GlyphFor(item.Id),
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
        };

        if (item.IsToggle)
        {
            _toggleButtons[item.Id] = btn;
        }

        btn.Clicked += (_, _) => ToolCommand?.Execute(item.Id);
        return btn;
    }

    private View BuildFillColorSwatch()
    {
        var swatch = new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Services.ResourceLookup.Color(_fillColorKey, Colors.White),
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 8),
            AutomationId = "toolbar_fill_color",
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ShowFillColorPicker(swatch);
        swatch.GestureRecognizers.Add(tap);
        return swatch;
    }

    private View BuildStrokeColorSwatch()
    {
        var swatch = new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 2 },
            WidthRequest = 20,
            HeightRequest = 20,
            BackgroundColor = Services.ResourceLookup.Color(_strokeColorKey, Colors.White),
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 8),
            AutomationId = "toolbar_stroke_color",
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => ShowStrokeColorPicker(swatch);
        swatch.GestureRecognizers.Add(tap);
        return swatch;
    }

    private void ShowFillColorPicker(Border swatch)
    {
        var selectedId = SelectedElementId;
        var picker = new SfColorPicker
        {
            ColorMode = ColorPickerMode.Spectrum,
            ShowAlphaSlider = true,
            ShowInputArea = true,
        };

        // Seed the picker (and the swatch) from the selected shape's current fill
        // so the dialog reflects reality rather than the toolbar's last pick.
        var currentFill = ReadSelectedColor("Fill", selectedId);
        if (currentFill is not null)
        {
            _fillColorKey = currentFill;
            swatch.BackgroundColor = Services.ResourceLookup.Color(currentFill, Colors.White);
            SeedPicker(picker, currentFill);
        }

        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => picker),
            ShowHeader = false,
            ShowFooter = true,
            ShowCloseButton = false,
            AppearanceMode = PopupButtonAppearanceMode.TwoButton,
            AcceptButtonText = "Apply",
            DeclineButtonText = "Cancel",
            AcceptCommand = new Command(() =>
            {
                var newColor = picker.SelectedColor ?? Colors.White;
                _fillColorKey = FindNearestKey(newColor);
                swatch.BackgroundColor = Services.ResourceLookup.Color(_fillColorKey, Colors.White);

                // Apply the color to the element that was selected when the picker
                // was opened; popup focus changes should not invalidate the target.
                if (!string.IsNullOrEmpty(selectedId) && _propertyService is not null)
                {
                    var property = new PropertyModel
                    {
                        Key = "FillColor",
                        Value = _fillColorKey,
                        Group = PropertyGroup.Fill,
                    };
                    _propertyService.Apply(selectedId, property);
                }

                popup!.IsOpen = false;
            }),
            DeclineCommand = new Command(() => popup!.IsOpen = false),
        };

        PopupXamlRootHelper.AttachIfAvailable(popup, swatch);
        popup.ShowRelativeToView(swatch, PopupRelativePosition.AlignBottomLeft);
    }

    private void ShowStrokeColorPicker(Border swatch)
    {
        var selectedId = SelectedElementId;
        var picker = new SfColorPicker
        {
            ColorMode = ColorPickerMode.Spectrum,
            ShowAlphaSlider = true,
            ShowInputArea = true,
        };

        var currentStroke = ReadSelectedColor("Border", selectedId);
        if (currentStroke is not null)
        {
            _strokeColorKey = currentStroke;
            swatch.BackgroundColor = Services.ResourceLookup.Color(currentStroke, Colors.White);
            SeedPicker(picker, currentStroke);
        }

        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => picker),
            ShowHeader = false,
            ShowFooter = true,
            ShowCloseButton = false,
            AppearanceMode = PopupButtonAppearanceMode.TwoButton,
            AcceptButtonText = "Apply",
            DeclineButtonText = "Cancel",
            AcceptCommand = new Command(() =>
            {
                var newColor = picker.SelectedColor ?? Colors.White;
                _strokeColorKey = FindNearestKey(newColor);
                swatch.BackgroundColor = Services.ResourceLookup.Color(_strokeColorKey, Colors.White);

                // Apply the color to the element that was selected when the picker
                // was opened; popup focus changes should not invalidate the target.
                if (!string.IsNullOrEmpty(selectedId) && _propertyService is not null)
                {
                    var property = new PropertyModel
                    {
                        Key = "BorderColor",
                        Value = _strokeColorKey,
                        Group = PropertyGroup.Border,
                    };
                    _propertyService.Apply(selectedId, property);
                }

                popup!.IsOpen = false;
            }),
            DeclineCommand = new Command(() => popup!.IsOpen = false),
        };

        PopupXamlRootHelper.AttachIfAvailable(popup, swatch);
        popup.ShowRelativeToView(swatch, PopupRelativePosition.AlignBottomLeft);
    }

    /// <summary>
    /// Reads the selected element's current fill/stroke colour key so the toolbar
    /// swatches mirror the shape (empty when nothing is selected or untracked).
    /// </summary>
    private string? ReadSelectedColor(string panelKey, string? selectedId = null)
    {
        selectedId ??= SelectedElementId;
        if (string.IsNullOrEmpty(selectedId) || _propertyService is null)
        {
            return null;
        }

        return _propertyService.Read(selectedId)
            .FirstOrDefault(p => p.Key == panelKey)?.Value as string;
    }

    /// <summary>Applies a colour key to an SfColorPicker via its writable colour property.</summary>
    private static void SeedPicker(SfColorPicker picker, string colorKey)
    {
        var color = Services.ResourceLookup.Color(colorKey, Colors.White);
        var property = picker.GetType().GetProperty("SelectedColor");
        if (property?.CanWrite == true && property.PropertyType == typeof(Color))
        {
            property.SetValue(picker, color);
        }
    }

    private View BuildShapesFlyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = "tool.shape.rectangle", TextKey = AppStrings.ToolbarRectangle },
            new() { Id = "tool.shape.ellipse", TextKey = AppStrings.ToolbarEllipse },
            new() { Id = "tool.shape.polygon", TextKey = AppStrings.ToolbarPolygon },
        };
        return BuildFlyoutDropdown("tool.shape", AppStrings.ToolbarBasicShapes, subItems);
    }

    private View BuildConnectorsFlyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = AppConstants.ToolConnectorStraight, TextKey = AppStrings.ToolbarStraightConnector },
            new() { Id = AppConstants.ToolConnectorOrthogonal, TextKey = AppStrings.ToolbarOrthogonalConnector },
            new() { Id = AppConstants.ToolConnectorBezier, TextKey = AppStrings.ToolbarBezierConnector },
        };
        return BuildFlyoutDropdown("tool.connector", AppStrings.ToolbarConnectors, subItems);
    }

    private View BuildArrangeDropdown()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = AppConstants.CmdSendToBack, TextKey = AppStrings.ToolbarArrangeObject },
            new() { Id = AppConstants.CmdBringToFront, TextKey = AppStrings.ToolbarArrangeObject },
            new() { Id = AppConstants.CmdSendBackward, TextKey = AppStrings.ToolbarArrangeObject },
            new() { Id = AppConstants.CmdBringForward, TextKey = AppStrings.ToolbarArrangeObject },
        };
        return BuildFlyoutDropdown("arrange", "Arrange", subItems);
    }

    private View BuildFlyoutDropdown(string id, string headerKey, List<MenuCommandItem> items)
    {
        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildFlyoutContent(items, popup!)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
        };

        var button = new Button
        {
            Text = GlyphFor(id) + " ▼",
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + id.Replace('.', '_'),
        };

        button.Clicked += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, button);
            popup.ShowRelativeToView(button, PopupRelativePosition.AlignBottomLeft);
        };

        return button;
    }

    private View BuildFlyoutContent(List<MenuCommandItem> items, SfPopup popup)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };

        foreach (var item in items)
        {
            var row = new SfButton
            {
                Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
                Padding = new Thickness(8, 6),
                HeightRequest = 32,
                Command = new Command(() =>
                {
                    ToolCommand?.Execute(item.Id);
                    popup.IsOpen = false;
                }),
                Content = new DataTemplate(() => new Label
                {
                    Text = item.TextKey,
                    FontSize = 12,
                    TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(8, 0),
                }),
            };
            stack.Children.Add(row);
        }

        return new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = stack,
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
        };
    }

    private View BuildZoomDropdown()
    {
        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildZoomContent(popup!)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
        };

        var button = new Button
        {
            Text = Math.Round(CurrentZoom * 100).ToString("0") + "% ▼",
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_zoom",
        };

        button.Clicked += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, button);
            popup.ShowRelativeToView(button, PopupRelativePosition.AlignBottomLeft);
        };

        return button;
    }

    private View BuildZoomContent(SfPopup popup)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };

        if (ZoomLevels is not null)
        {
            foreach (var level in ZoomLevels)
            {
                if (level is not ZoomLevel zl)
                    continue;

                var row = new SfButton
                {
                    Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
                    Padding = new Thickness(8, 6),
                    HeightRequest = 32,
                    Command = new Command(() =>
                    {
                        SetZoomCommand?.Execute(zl);
                        popup.IsOpen = false;
                    }),
                    Content = new DataTemplate(() => new Label
                    {
                        Text = zl.LabelKey,
                        FontSize = 12,
                        TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                        VerticalTextAlignment = TextAlignment.Center,
                        Margin = new Thickness(8, 0),
                    }),
                };
                stack.Children.Add(row);
            }
        }

        return new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = stack,
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
        };
    }

    private static string GlyphFor(string id) => id switch
    {
        AppConstants.CmdUndo => "↶",
        AppConstants.CmdRedo => "↷",
        AppConstants.ToolPan => "✋",
        AppConstants.ToolSelect => "↗",
        "tool.shape" => "◯",
        "tool.connector" => "→",
        AppConstants.ToolText => "T",
        "arrange" => "≡",
        _ => "□",
    };

    private static string FindNearestKey(Color color)
    {
        var palette = Services.ResourceLookup.PaletteSnapshot();
        var (bestKey, bestDistance) = (string.Empty, double.MaxValue);
        foreach (var (key, candidate) in palette)
        {
            var d = Math.Abs(color.Red - candidate.Red)
                  + Math.Abs(color.Green - candidate.Green)
                  + Math.Abs(color.Blue - candidate.Blue);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestKey = key;
            }
        }
        return bestKey;
    }
}
