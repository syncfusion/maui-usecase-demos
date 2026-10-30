using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using Microsoft.Maui.Controls.Shapes;

namespace DiagramBuilder.Controls;

/// <summary>
/// Toolbar (ui-spec.md §3). Binds to the toolbar items (ItemsSource); renders
/// 8 groups with separators, flyouts for shapes/connectors, zoom chips, color
/// swatches (group 6), and hides group 6 unless <see cref="HasSingleSelection"/>.
/// Code-behind dispatches tool ids via <see cref="ToolCommand"/>; the flyouts
/// re-use <see cref="MenuBarControl"/>'s row builder.
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
        BindableProperty.Create(nameof(CurrentZoom), typeof(double), typeof(ToolbarControl), 1.0d);

    public static readonly BindableProperty ZoomLevelsProperty =
        BindableProperty.Create(nameof(ZoomLevels), typeof(IEnumerable), typeof(ToolbarControl));

    public static readonly BindableProperty SetZoomCommandProperty =
        BindableProperty.Create(nameof(SetZoomCommand), typeof(ICommand), typeof(ToolbarControl));

    public static readonly BindableProperty ShowPropertiesCommandProperty =
        BindableProperty.Create(nameof(ShowPropertiesCommand), typeof(ICommand), typeof(ToolbarControl));

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

    private static void OnHasSingleSelectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToolbarControl tb)
        {
            tb.ApplyGroup6Visibility();
        }
    }

    private void Rebuild()
    {
        GroupsBar.Children.Clear();
        if (ItemsSource is null)
        {
            return;
        }

        var byGroup = ((IEnumerable)ItemsSource).Cast<Models.ToolbarItem>()
            .GroupBy(i => i.Group)
            .OrderBy(g => g.Key)
            .ToList();

        var first = true;
        foreach (var group in byGroup)
        {
            if (!first)
            {
                GroupsBar.Children.Add(new BoxView { Style = (Style)Resources["ToolbarGroupDividerStyle"] });
            }
            first = false;

            var groupLayout = new HorizontalStackLayout
            {
                Spacing = 0,
                AutomationId = "group_" + group.Key,
            };

            // Group 3 = single "Basic Shapes" flyout (instead of three buttons).
            if (group.Key == 3)
            {
                groupLayout.Children.Add(BuildGroup3Flyout());
            }
            // Group 4 = single "Connectors" flyout.
            else if (group.Key == 4)
            {
                groupLayout.Children.Add(BuildGroup4Flyout());
            }
            // Group 7 = zoom in / zoom out / fit + chip row appended after.
            else if (group.Key == 7)
            {
                foreach (var item in group.OrderBy(i => i.Id))
                {
                    groupLayout.Children.Add(BuildIconButton(item));
                }

                // Append the zoom chips inside the same group.
                groupLayout.Children.Add(BuildZoomChipRow());
            }
            else
            {
                foreach (var item in group.OrderBy(i => i.Id))
                {
                    groupLayout.Children.Add(BuildGroupItem(item, group.Key));
                }
            }

            GroupsBar.Children.Add(groupLayout);
        }
    }

    private void ApplyGroup6Visibility()
    {
        foreach (var child in GroupsBar.Children)
        {
            if (child is HorizontalStackLayout groupLayout && groupLayout.AutomationId == "group_6")
            {
                groupLayout.IsVisible = HasSingleSelection;
            }
        }
    }

    private View BuildGroupItem(Models.ToolbarItem item, int group)
    {
        // Color swatches: render as a tiny colored Border with a tap-to-open popup.
        if (item.Id is "tool.fill.color" or "tool.border.color")
        {
            return BuildColorSwatchButton(item);
        }

        return BuildIconButton(item);
    }

    private View BuildGroup3Flyout()
    {
        // Group 3 = "Basic Shapes" flyout: Rectangle / Ellipse / Polygon.
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

    private View BuildColorSwatchButton(Models.ToolbarItem item)
    {
        var fillColor = (Color)Application.Current!.Resources[item.Id == "tool.fill.color" ? "AccentFlow" : "TextPrimary"];
        var swatch = new Border
        {
            Stroke = (Color)Application.Current!.Resources["PanelBorderColor"],
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            WidthRequest = 24,
            HeightRequest = 24,
            BackgroundColor = fillColor,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(4, 0),
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
        };

        var picker = new Syncfusion.Maui.Inputs.SfColorPicker
        {
            ColorMode = Syncfusion.Maui.Inputs.ColorPickerMode.Spectrum,
            ShowAlphaSlider = false,
        };
        picker.ColorChanged += (_, _) => ToolCommand?.Execute(item.Id);

        var popup = new Syncfusion.Maui.Popup.SfPopup
        {
            ContentTemplate = new DataTemplate(() => picker),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => popup.ShowRelativeToView(swatch, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        swatch.GestureRecognizers.Add(tap);
        return swatch;
    }

    private Button BuildIconButton(Models.ToolbarItem item)
    {
        var btn = new Button
        {
            Text = GlyphFor(item.Id),
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
        };
        AutomationProperties.SetHelpText(btn, item.TooltipKey);
        if (item.IsChecked)
        {
            btn.Style = (Style)Application.Current!.Resources["ToolbarIconActiveStyle"];
        }

        // Wire to either ToolCommand or the dedicated properties toggle.
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

    private View BuildZoomChipRow()
    {
        var stack = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(8, 0, 0, 0) };
        if (ZoomLevels is null)
        {
            return stack;
        }

        foreach (var level in ZoomLevels)
        {
            if (level is not Models.ZoomLevel zl)
            {
                continue;
            }

            var chip = new Button
            {
                Text = zl.LabelKey,
                Style = (Style)Application.Current!.Resources["ZoomChipStyle"],
                AutomationId = "toolbar_zoom_" + zl.Value.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture).Replace('.', '_'),
            };
            AutomationProperties.SetHelpText(chip, zl.LabelKey);
            chip.Clicked += (_, _) => SetZoomCommand?.Execute(zl);
            stack.Children.Add(chip);
        }

        return stack;
    }

    private View BuildFlyoutButton(string id, string headerKey, IList<MenuCommandItem> subItems)
    {
        var popup = new Syncfusion.Maui.Popup.SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildFlyoutContent(headerKey, subItems)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
        };
        var btn = new Button
        {
            Text = GlyphFor(id),
            Style = (Style)Application.Current!.Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + id.Replace('.', '_'),
        };
        AutomationProperties.SetHelpText(btn, headerKey);
        btn.Clicked += (_, _) => popup.ShowRelativeToView(btn, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        return btn;
    }

    private View BuildFlyoutContent(string headerKey, IList<MenuCommandItem> subItems)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };
        stack.Children.Add(new Label
        {
            Text = headerKey,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["TextSecondary"],
            Margin = new Thickness(8, 4),
        });
        foreach (var child in subItems)
        {
            var row = new Grid
            {
                Padding = new Thickness(8, 6),
                HeightRequest = 32,
                ColumnDefinitions = new ColumnDefinitionCollection(new[]
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                }),
            };
            row.Children.Add(new Label
            {
                Text = child.TextKey,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 13,
            });
            var tap = new TapGestureRecognizer
            {
                Command = new Command(() => ToolCommand?.Execute(child.Id)),
            };
            row.GestureRecognizers.Add(tap);
            stack.Children.Add(row);
        }

        return new Border
        {
            Stroke = (Color)Application.Current!.Resources["PanelBorderColor"],
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Content = stack,
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
        };
    }

    private static string GlyphFor(string id) => id switch
    {
        // Group 1 — undo/redo
        AppConstants.CmdUndo => "\u21B6",          // ↶
        AppConstants.CmdRedo => "\u21B7",          // ↷

        // Group 2 — pan/selection
        AppConstants.ToolPan => "\u270B",          // ✋
        AppConstants.ToolSelect => "\u2197",       // ↗ (selection arrow)

        // Group 3 — basic shapes flyout
        "tool.shape" => "\u25EF",                  // ◯ (open circle as shapes glyph)

        // Group 4 — connectors flyout
        "tool.connector" => "\u279C",             // ➜ (arrow as connector glyph)

        // Group 5 — text tool
        AppConstants.ToolText => "T",

        // Group 6 — selection-only actions
        "tool.fill.color" => "\u2B22",             // ⬢ (filled hex)
        "tool.border.color" => "\u25A1",           // □ (bordered square)
        AppConstants.CmdSendToBack => "\u2B05",    // ⬅
        AppConstants.CmdBringToFront => "\u27A1",  // ➡
        AppConstants.CmdSendBackward => "\u2B05\u2B05",
        AppConstants.CmdBringForward => "\u27A1\u27A1",
        "tool.lock" => "\u26BF",                   // ⚿
        AppConstants.CmdDelete => "\u2716",        // ✖

        // Group 7 — zoom
        AppConstants.CmdZoomIn => "+",
        AppConstants.CmdZoomOut => "\u2212",       // −
        AppConstants.CmdFitToScreen => "\u26F6",   // ⛶

        // Group 8 — toggle property panel
        AppConstants.CmdShowProperties => "\u25A3", // ▣

        _ => "\u25A1",                            // □ fallback
    };
}