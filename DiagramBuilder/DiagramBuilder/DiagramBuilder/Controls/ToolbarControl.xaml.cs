using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;
using Syncfusion.Maui.Buttons;
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
        BindableProperty.Create(nameof(HasSingleSelection), typeof(bool), typeof(ToolbarControl), false,
            propertyChanged: OnSelectionGateChanged);

    public static readonly BindableProperty HasAnySelectionProperty =
        BindableProperty.Create(nameof(HasAnySelection), typeof(bool), typeof(ToolbarControl), false,
            propertyChanged: OnSelectionGateChanged);

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

    public bool HasAnySelection
    {
        get => (bool)GetValue(HasAnySelectionProperty);
        set => SetValue(HasAnySelectionProperty, value);
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

    /// <summary>Selection gates rebuild the toolbar (shape tools appear/disappear).</summary>
    private static void OnSelectionGateChanged(BindableObject bindable, object oldValue, object newValue)
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
        CenterGroupsBar.Children.Clear();
        RightGroupsBar.Children.Clear();
        _toggleButtons.Clear();

        if (ItemsSource is null)
            return;

        var items = ((IEnumerable)ItemsSource).Cast<DiagramToolbarItem>().ToList();

        // Left side: Groups 1-4 (undo/redo, pan/select, shapes, connectors)
        BuildLeftToolbar(items);

        // Center: Group 5 (Fill/Stroke/Delete) — only with a shape selection,
        // mirroring the property panel's behaviour.
        BuildCenterToolbar(items);

        // Right side: zoom dropdown + properties toggle
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

    }

    /// <summary>
    /// Center group: Fill / Stroke colour swatches, z-order, lock, and delete.
    /// Only rendered when a selection exists (with colour swatches still limited
    /// to a single node selection). This keeps the active shape actions centered
    /// while placing the ordering and lock controls next to the colour picker.
    /// </summary>
    private void BuildCenterToolbar(List<DiagramToolbarItem> allItems)
    {
        var byGroup = allItems.GroupBy(i => i.Group).OrderBy(g => g.Key).ToList();
        if (!byGroup.Any(g => g.Key is 5 or 6))
        {
            return;
        }

        var group5Items = byGroup.FirstOrDefault(g => g.Key == 5)?.ToList() ?? [];
        var group6Items = byGroup.FirstOrDefault(g => g.Key == 6)?.ToList() ?? [];

        if (HasSingleSelection)
        {
            foreach (var item in group5Items.Where(i => i.Id is "tool.stroke.color" or "tool.fill.color"))
            {
                if (item.Id == "tool.stroke.color")
                {
                    CenterGroupsBar.Children.Add(BuildStrokeColorSwatch());
                }
                else
                {
                    CenterGroupsBar.Children.Add(BuildFillColorSwatch());
                }
            }
        }

        if (HasAnySelection)
        {
            if (group6Items.Any(i => i.Id == AppConstants.CmdBringToFront || i.Id == AppConstants.CmdSendToBack))
            {
                CenterGroupsBar.Children.Add(BuildArrangeDropdown());
            }

            foreach (var item in group6Items.Where(i => i.Id == AppConstants.ToolLock))
            {
                CenterGroupsBar.Children.Add(BuildIconButton(item));
            }

            foreach (var item in group5Items.Where(i => i.Id == AppConstants.CmdDelete))
            {
                CenterGroupsBar.Children.Add(BuildIconButton(item));
            }
        }
    }

    private void BuildRightToolbar(List<DiagramToolbarItem> allItems)
    {
        // Group 7: Zoom dropdown
        RightGroupsBar.Children.Add(BuildZoomDropdown());

        // Group 8: Properties button (Material icon)
        var btn = new Button
        {
            Text = Constants.MaterialIcons.TextFor(AppConstants.CmdShowProperties),
            FontFamily = "MaterialIcons",
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_properties",
        };
        btn.Clicked += (_, _) => ShowPropertiesCommand?.Execute(null);
        RightGroupsBar.Children.Add(btn);
    }

    private HorizontalStackLayout BuildGroup(IGrouping<int, DiagramToolbarItem> group)
    {
        var layout = new HorizontalStackLayout { Spacing = 4 };
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
            // Material icon glyph (raw Unicode fallback only if the id is unknown).
            Text = Constants.MaterialIcons.TextFor(item.Id) ?? item.Id,
            FontFamily = "MaterialIcons",
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
        var selectedId = SelectedElementId;
        var colorValue = ReadSelectedColor("Fill", selectedId) ?? _fillColorKey;
        _fillColorKey = colorValue;
        var picker = new ColorPickerButton
        {
            ColorValue = colorValue,
            Margin = new Thickness(7, 0),
            AutomationId = "toolbar_fill_color",
        };

        picker.ColorChanged += (_, value) =>
        {
            _fillColorKey = value;
            if (!string.IsNullOrEmpty(selectedId) && _propertyService is not null)
            {
                _propertyService.Apply(selectedId, new PropertyModel
                {
                    Key = "FillColor",
                    Value = value,
                    Group = PropertyGroup.Fill,
                });
            }
        };

        return picker;
    }

    private View BuildStrokeColorSwatch()
    {
        var selectedId = SelectedElementId;
        var colorValue = ReadSelectedColor("Border", selectedId) ?? _strokeColorKey;
        _strokeColorKey = colorValue;
        var picker = new ColorPickerButton
        {
            ColorValue = colorValue,
            Margin = new Thickness(7, 0),
            AutomationId = "toolbar_stroke_color",
        };

        picker.ColorChanged += (_, value) =>
        {
            _strokeColorKey = value;
            if (!string.IsNullOrEmpty(selectedId) && _propertyService is not null)
            {
                _propertyService.Apply(selectedId, new PropertyModel
                {
                    Key = "BorderColor",
                    Value = value,
                    Group = PropertyGroup.Border,
                });
            }
        };

        return picker;
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

    private View BuildShapesFlyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = "tool.shape.rectangle", TextKey = AppStrings.ToolbarRectangle, IconKey = "tool.shape" },
            new() { Id = "tool.shape.ellipse", TextKey = AppStrings.ToolbarEllipse, IconKey = "tool.shape" },
            new() { Id = "tool.shape.polygon", TextKey = AppStrings.ToolbarPolygon, IconKey = "tool.shape" },
        };
        return BuildFlyoutDropdown("tool.shape", AppStrings.ToolbarBasicShapes, subItems);
    }

    private View BuildConnectorsFlyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = AppConstants.ToolConnectorStraight, TextKey = AppStrings.ToolbarStraightConnector, IconKey = "tool.connector" },
            new() { Id = AppConstants.ToolConnectorOrthogonal, TextKey = AppStrings.ToolbarOrthogonalConnector, IconKey = "tool.connector" },
            new() { Id = AppConstants.ToolConnectorBezier, TextKey = AppStrings.ToolbarBezierConnector, IconKey = "tool.connector" },
        };
        return BuildFlyoutDropdown("tool.connector", AppStrings.ToolbarConnectors, subItems);
    }

    private View BuildArrangeDropdown()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = AppConstants.CmdBringToFront, TextKey = "Bring to Front", IconKey = AppConstants.CmdBringToFront },
            new() { Id = AppConstants.CmdSendToBack, TextKey = "Send to Back", IconKey = AppConstants.CmdSendToBack },
            new() { Id = AppConstants.CmdBringForward, TextKey = "Bring Forward", IconKey = AppConstants.CmdBringForward },
            new() { Id = AppConstants.CmdSendBackward, TextKey = "Send Backward", IconKey = AppConstants.CmdSendBackward },
        };
        return BuildFlyoutDropdown("arrange", "Arrange", subItems);
    }

    private View BuildFlyoutDropdown(string id, string headerKey, List<MenuCommandItem> items)
    {
        var popup = new SfPopup
        {
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
            // Size the popup to its content instead of the default fixed width.
            AutoSizeMode = PopupAutoSizeMode.Both,
            AutoSizeTarget = PopupAutoSizeTarget.Content,
            // Visible border around the dropdown surface.
            PopupStyle = new PopupStyle
            {
                Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
                StrokeThickness = 1,
                PopupBackground = Services.ResourceLookup.ResourceColor("CanvasBackground"),
                CornerRadius = new CornerRadius(4),
            },
        };
        popup.ContentTemplate = new DataTemplate(() => BuildFlyoutContent(items, popup));

        // The icon and the caret are two separate labels in a horizontal stack
        // rather than one concatenated string: the shared icon-button style is
        // intentionally compact, so a single "icon + caret" string would clip.
        // The stack is sized to its own content, so both glyphs stay visible.
        var button = new SfButton
        {
            Style = (Style)Application.Current!.Resources["ToolbarDropdownButtonStyle"],
            AutomationId = "toolbar_" + id.Replace('.', '_'),
            // SfButton.Content is a DataTemplate (same pattern as the menu rows).
            Content = new DataTemplate(() => new HorizontalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = Constants.MaterialIcons.TextFor(id),
                        FontFamily = "MaterialIcons",
                        FontSize = 14,
                        TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                        VerticalTextAlignment = TextAlignment.Center,
                    },
                    new Label
                    {
                        Text = Constants.MaterialIcons.DropdownCaret,
                        FontFamily = "MaterialIcons",
                        FontSize = 14,
                        TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                        VerticalTextAlignment = TextAlignment.Center,
                    },
                },
            }),
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
                    if (popup.IsOpen)
                    {
                        popup.IsOpen = false;
                    }
                }),
                Content = new DataTemplate(() => new HorizontalStackLayout
                {
                    Spacing = 8,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = string.IsNullOrEmpty(item.IconKey) ? string.Empty : Constants.MaterialIcons.TextFor(item.IconKey),
                            FontFamily = string.IsNullOrEmpty(item.IconKey) ? string.Empty : "MaterialIcons",
                            FontSize = 14,
                            TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                            VerticalTextAlignment = TextAlignment.Center,
                            IsVisible = !string.IsNullOrEmpty(item.IconKey),
                            WidthRequest = 18,
                        },
                        new Label
                        {
                            Text = item.TextKey,
                            FontSize = 12,
                            TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                            VerticalTextAlignment = TextAlignment.Center,
                        },
                    },
                }),
            };
            stack.Children.Add(row);
        }

        // The popup root owns the surface: PopupStyle supplies the background,
        // the border and the corner radius, so the content only contributes
        // the rows (a second border here would double up).
        return stack;
    }

    private View BuildZoomDropdown()
    {
        var popup = new SfPopup
        {
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
            // Size the popup to its content instead of the default fixed width.
            AutoSizeMode = PopupAutoSizeMode.Both,
            AutoSizeTarget = PopupAutoSizeTarget.Content,
            // Visible border around the dropdown surface.
            PopupStyle = new PopupStyle
            {
                Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
                StrokeThickness = 1,
                PopupBackground = Services.ResourceLookup.ResourceColor("CanvasBackground"),
                CornerRadius = new CornerRadius(4),
            },
        };
        popup.ContentTemplate = new DataTemplate(() => BuildZoomContent(popup));

        var button = new SfButton
        {
            Style = (Style)Application.Current!.Resources["ToolbarDropdownButtonStyle"],
            AutomationId = "toolbar_zoom",
            // SfButton.Content is a DataTemplate (same pattern as the menu rows).
            Content = new DataTemplate(() => new HorizontalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = Math.Round(CurrentZoom * 100).ToString("0") + "%",
                        FontSize = 14,
                        TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                        VerticalTextAlignment = TextAlignment.Center,
                    },
                    new Label
                    {
                        Text = Constants.MaterialIcons.DropdownCaret,
                        FontFamily = "MaterialIcons",
                        FontSize = 14,
                        TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                        VerticalTextAlignment = TextAlignment.Center,
                    },
                },
            }),
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

                var isSelected = Math.Abs(CurrentZoom - zl.Value) < 0.0001;
                var row = new SfButton
                {
                    Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
                    Padding = new Thickness(8, 6),
                    HeightRequest = 32,
                    Command = new Command(() =>
                    {
                        SetZoomCommand?.Execute(zl);
                        if (popup.IsOpen)
                        {
                            popup.IsOpen = false;
                        }
                    }),
                    Content = new DataTemplate(() => new HorizontalStackLayout
                    {
                        Spacing = 8,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label
                            {
                                Text = isSelected ? Constants.MaterialIcons.CheckGlyph : string.Empty,
                                FontFamily = "MaterialIcons",
                                FontSize = 14,
                                TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                                VerticalTextAlignment = TextAlignment.Center,
                                WidthRequest = 18,
                            },
                            new Label
                            {
                                Text = zl.LabelKey,
                                FontSize = 12,
                                TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                                VerticalTextAlignment = TextAlignment.Center,
                            },
                        },
                    }),
                };
                stack.Children.Add(row);
            }
        }

        // The popup root owns the surface: PopupStyle supplies the background,
        // the border and the corner radius, so the content only contributes
        // the rows (a second border here would double up).
        return stack;
    }

}
