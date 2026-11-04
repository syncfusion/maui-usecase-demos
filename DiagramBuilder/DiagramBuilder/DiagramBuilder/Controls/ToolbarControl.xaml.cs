using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramToolbarItem = DiagramBuilder.Models.ToolbarItem;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;

namespace DiagramBuilder.Controls;

/// <summary>
/// Toolbar (ui-spec.md §3). Binds to the toolbar items (ItemsSource); renders
/// grouped actions, flyouts for shapes/connectors, a zoom dropdown, and hides
/// group 6 unless <see cref="HasSingleSelection"/>.
/// Code-behind dispatches tool ids via <see cref="ToolCommand"/>.
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
            propertyChanged: OnHasSingleSelectionChanged);

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

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string ActiveToolId
    {
        get => (string)GetValue(ActiveToolIdProperty);
        set => SetValue(ActiveToolIdProperty, value);
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

    // Toggle buttons that carry the active-tool highlight (Pan / Selection).
    private readonly Dictionary<string, Button> _toggleButtons = new();

    public ToolbarControl()
    {
        InitializeComponent();
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

    private static void OnHasSingleSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarControl tb)
        {
            tb.ApplyGroup6Visibility();
        }
    }

    private void Rebuild()
    {
        LeftGroupsBar.Children.Clear();
        RightGroupsBar.Children.Clear();
        _toggleButtons.Clear();
        if (ItemsSource is null)
        {
            return;
        }

        var byGroup = ((IEnumerable)ItemsSource).Cast<DiagramToolbarItem>()
            .GroupBy(i => i.Group)
            .OrderBy(g => g.Key)
            .ToList();

        var firstLeft = true;
        foreach (var group in byGroup.Where(g => g.Key <= 6))
        {
            if (!firstLeft)
            {
                LeftGroupsBar.Children.Add(new BoxView { Style = (Style)Resources["ToolbarGroupDividerStyle"] });
            }
            firstLeft = false;
            LeftGroupsBar.Children.Add(BuildGroupLayout(group));
        }

        if (byGroup.Any(g => g.Key <= 6) && (byGroup.Any(g => g.Key == 7) || byGroup.Any(g => g.Key == 8)))
        {
            RightGroupsBar.Children.Add(new BoxView { Style = (Style)Resources["ToolbarGroupDividerStyle"] });
        }

        if (byGroup.Any(g => g.Key == 7))
        {
            RightGroupsBar.Children.Add(BuildZoomDropdown());
        }

        if (byGroup.Any(g => g.Key == 8))
        {
            RightGroupsBar.Children.Add(BuildPropertiesButton());
        }

        ApplyGroup6Visibility();
        ApplyActiveToolHighlight();
    }

    private View BuildGroupLayout(IGrouping<int, DiagramToolbarItem> group)
    {
        var groupLayout = new HorizontalStackLayout
        {
            Spacing = 0,
            AutomationId = "group_" + group.Key,
        };

        if (group.Key == 3)
        {
            groupLayout.Children.Add(BuildGroup3Flyout());
        }
        else if (group.Key == 4)
        {
            groupLayout.Children.Add(BuildGroup4Flyout());
        }
        else
        {
            foreach (var item in group.OrderBy(i => i.Id))
            {
                groupLayout.Children.Add(BuildGroupItem(item));
            }
        }

        return groupLayout;
    }

    private void ApplyGroup6Visibility()
    {
        foreach (var child in LeftGroupsBar.Children)
        {
            if (child is HorizontalStackLayout groupLayout && groupLayout.AutomationId == "group_6")
            {
                groupLayout.IsVisible = HasSingleSelection;
            }
        }
    }

    private View BuildGroupItem(DiagramToolbarItem item)
    {
        if (item.Id is "tool.fill.color" or "tool.border.color")
        {
            return BuildColorSwatchButton(item);
        }

        return BuildIconButton(item);
    }

    private View BuildGroup3Flyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = "tool.shape.rectangle", TextKey = AppStrings.ToolbarRectangle },
            new() { Id = "tool.shape.ellipse", TextKey = AppStrings.ToolbarEllipse },
            new() { Id = "tool.shape.polygon", TextKey = AppStrings.ToolbarPolygon },
        };
        return BuildFlyoutButton("tool.shape", AppStrings.ToolbarBasicShapes, subItems);
    }

    private View BuildGroup4Flyout()
    {
        var subItems = new List<MenuCommandItem>
        {
            new() { Id = AppConstants.ToolConnectorStraight, TextKey = AppStrings.ToolbarStraightConnector },
            new() { Id = AppConstants.ToolConnectorOrthogonal, TextKey = AppStrings.ToolbarOrthogonalConnector },
            new() { Id = AppConstants.ToolConnectorBezier, TextKey = AppStrings.ToolbarBezierConnector },
        };
        return BuildFlyoutButton("tool.connector", AppStrings.ToolbarConnectors, subItems);
    }

    private View BuildColorSwatchButton(DiagramToolbarItem item)
    {
        var fillColor = Services.ResourceLookup.ResourceColor(item.Id == "tool.fill.color" ? "AccentFlow" : "TextPrimary");
        var swatch = new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            WidthRequest = 24,
            HeightRequest = 24,
            BackgroundColor = fillColor,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0),
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
        };

        string pendingColorKey = item.Id == "tool.fill.color" ? "AccentFlow" : "TextPrimary";
        var picker = new SfColorPicker
        {
            ColorMode = ColorPickerMode.Spectrum,
            ShowAlphaSlider = true,
            ShowInputArea = true,
        };
        picker.ColorChanged += (_, args) =>
        {
            pendingColorKey = FindNearestKey(args.NewColor ?? Colors.White);
        };

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
                ApplyToSwatch(swatch, pendingColorKey);
                ToolCommand?.Execute(item.Id);
            }),
            DeclineCommand = new Command(() => { }),
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, swatch);
            popup.ShowRelativeToView(swatch, PopupRelativePosition.AlignBottomLeft);
        };
        swatch.GestureRecognizers.Add(tap);
        return swatch;
    }

    private static void ApplyToSwatch(Border swatch, string key)
    {
        swatch.BackgroundColor = Services.ResourceLookup.Color(key, Colors.White);
    }

    private Button BuildIconButton(DiagramToolbarItem item)
    {
        var btn = new Button
        {
            Text = GlyphFor(item.Id),
            Style = item.Id == AppConstants.CmdShowProperties
                ? (Style)Application.Current!.Resources["ToolbarPrimaryActionStyle"]
                : (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
        };
        AutomationProperties.SetHelpText(btn, item.TooltipKey);

        if (item.IsToggle && item.Id != AppConstants.CmdShowProperties)
        {
            _toggleButtons[item.Id] = btn;
            btn.Style = string.Equals(item.Id, ActiveToolId, StringComparison.Ordinal)
                ? (Style)Application.Current!.Resources["ToolbarIconActiveStyle"]
                : (Style)Application.Current!.Resources["ToolbarIconButtonStyle"];
        }

        if (item.Id == AppConstants.CmdShowProperties)
        {
            btn.Clicked += (_, _) => ShowPropertiesCommand?.Execute(null);
        }
        else
        {
            btn.Clicked += (_, _) => ToolCommand?.Execute(item.Id);
        }

        return btn;
    }

    private View BuildZoomDropdown()
    {
        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildZoomPopupContent(popup!)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
        };

        var button = BuildDropdownButton(CurrentZoomLabel(), null, "toolbar_zoom_dropdown");
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, button);
            popup.ShowRelativeToView(button, PopupRelativePosition.AlignBottomLeft);
        };
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private View BuildPropertiesButton()
    {
        var btn = new Button
        {
            Text = GlyphFor(AppConstants.CmdShowProperties),
            Style = (Style)Application.Current!.Resources["ToolbarPrimaryActionStyle"],
            AutomationId = "toolbar_" + AppConstants.CmdShowProperties.Replace('.', '_'),
        };
        AutomationProperties.SetHelpText(btn, AppConstants.CmdShowProperties);
        btn.Clicked += (_, _) => ShowPropertiesCommand?.Execute(null);
        return btn;
    }

    private View BuildFlyoutButton(string id, string headerKey, IList<MenuCommandItem> subItems)
    {
        SfPopup? popup = null;
        popup = new SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildFlyoutContent(subItems, popup!)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
        };

        var button = BuildDropdownButton(null, GlyphFor(id), "toolbar_" + id.Replace('.', '_'));
        AutomationProperties.SetHelpText(button, headerKey);
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, button);
            popup.ShowRelativeToView(button, PopupRelativePosition.AlignBottomLeft);
        };
        button.GestureRecognizers.Add(tap);
        return button;
    }

    private Border BuildDropdownButton(string? text, string? icon, string automationId)
    {
        var content = new HorizontalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        };

        if (!string.IsNullOrEmpty(icon))
        {
            content.Children.Add(new Label
            {
                Text = icon,
                FontSize = 13,
                TextColor = Application.Current!.Resources.TryGetValue("TextPrimary", out var iconColor) && iconColor is Color c1 ? c1 : Colors.Black,
                VerticalTextAlignment = TextAlignment.Center,
            });
        }

        if (!string.IsNullOrEmpty(text))
        {
            content.Children.Add(new Label
            {
                Text = text,
                FontSize = 13,
                TextColor = Application.Current!.Resources.TryGetValue("TextPrimary", out var textColor) && textColor is Color c2 ? c2 : Colors.Black,
                VerticalTextAlignment = TextAlignment.Center,
            });
        }

        content.Children.Add(new Label
        {
            Text = "⌄",
            FontSize = 12,
            TextColor = Application.Current!.Resources.TryGetValue("TextSecondary", out var chevronColor) && chevronColor is Color c3 ? c3 : Colors.Black,
            VerticalTextAlignment = TextAlignment.Center,
        });

        return new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
            HeightRequest = 32,
            MinimumWidthRequest = string.IsNullOrEmpty(text) ? 44 : 76,
            Padding = new Thickness(10, 0),
            Margin = new Thickness(4, 0),
            AutomationId = automationId,
            Content = content,
            VerticalOptions = LayoutOptions.Center,
        };
    }

    private View BuildFlyoutContent(IList<MenuCommandItem> subItems, SfPopup popup)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };

        foreach (var child in subItems)
        {
            stack.Children.Add(BuildPopupRow(child.TextKey, GlyphFor(child.Id), () =>
            {
                ToolCommand?.Execute(child.Id);
                popup.IsOpen = false;
            }));
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

    private View BuildZoomPopupContent(SfPopup popup)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };

        if (ZoomLevels is not null)
        {
            foreach (var level in ZoomLevels)
            {
                if (level is not ZoomLevel zl)
                {
                    continue;
                }

                stack.Children.Add(BuildPopupRow(zl.LabelKey, null, () =>
                {
                    SetZoomCommand?.Execute(zl);
                    popup.IsOpen = false;
                }));
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

    private View BuildPopupRow(string label, string? icon, Action onTap)
    {
        var row = new SfButton
        {
            Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
            Padding = new Thickness(8, 6),
            HeightRequest = 32,
            Command = new Command(onTap),
            Content = new DataTemplate(() => BuildPopupRowContent(label, icon)),
        };
        return row;
    }

    private View BuildPopupRowContent(string label, string? icon)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center,
        };

        if (!string.IsNullOrEmpty(icon))
        {
            row.Add(new Label
            {
                Text = icon,
                FontSize = 13,
                TextColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
                VerticalTextAlignment = TextAlignment.Center,
            });
        }

        var textLabel = new Label
        {
            Text = label,
            VerticalTextAlignment = TextAlignment.Center,
            FontSize = 13,
            TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
        };
        row.Add(textLabel);
        Grid.SetColumn(textLabel, 1);

        return row;
    }

    private string CurrentZoomLabel()
    {
        var percent = Math.Round(CurrentZoom * 100.0);
        return percent.ToString("0") + "%";
    }

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

    private static string GlyphFor(string id) => id switch
    {
        AppConstants.CmdUndo => "\u21B6",
        AppConstants.CmdRedo => "\u21B7",
        AppConstants.ToolPan => "\u270B",
        AppConstants.ToolSelect => "\u2197",
        "tool.shape" => "\u25EF",
        "tool.shape.rectangle" => "\u25AD",
        "tool.shape.ellipse" => "\u25EF",
        "tool.shape.polygon" => "\u25B3",
        "tool.connector" => "\u279C",
        AppConstants.ToolConnectorStraight => "\u2500",
        AppConstants.ToolConnectorOrthogonal => "\u21AA",
        AppConstants.ToolConnectorBezier => "\u223C",
        AppConstants.ToolText => "T",
        "tool.fill.color" => "\u2B22",
        "tool.border.color" => "\u25A1",
        AppConstants.CmdSendToBack => "\u2B05",
        AppConstants.CmdBringToFront => "\u27A1",
        AppConstants.CmdSendBackward => "\u2B05\u2B05",
        AppConstants.CmdBringForward => "\u27A1\u27A1",
        "tool.lock" => "\u26BF",
        AppConstants.CmdDelete => "\u2716",
        AppConstants.CmdShowProperties => "\u25A3",
        _ => "\u25A1",
    };
}
