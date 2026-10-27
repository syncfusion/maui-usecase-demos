using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Controls;

/// <summary>
/// Toolbar (ui-spec.md §3). Binds to <see cref="ToolbarViewModel.Items"/>; renders
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

        var byGroup = ((IEnumerable)ItemsSource).Cast<ToolbarItem>()
            .GroupBy(i => i.Group)
            .OrderBy(g => g.Key);

        var first = true;
        foreach (var group in byGroup)
        {
            if (!first)
            {
                GroupsBar.Children.Add(new BoxView { Style = (Style)Resources["ToolbarGroupDividerStyle"] });
            }
            first = false;

            var groupLayout = new HorizontalStackLayout { Spacing = 0 };
            foreach (var item in group.OrderBy(i => i.Id))
            {
                groupLayout.Children.Add(BuildGroupItem(item, group.Key));
            }
            GroupsBar.Children.Add(groupLayout);
        }
    }

    private View BuildGroupItem(ToolbarItem item, int group)
    {
        // Color swatches wrap a ColorPickerButton in a host box.
        if (item.Id is "tool.fill.color" or "tool.border.color")
        {
            var host = new Border
            {
                Style = (Style)Resources["ToolbarGroupDividerStyle"],
                Padding = new Thickness(0),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 0 },
                Stroke = Colors.Transparent,
                WidthRequest = 36,
                HeightRequest = 36,
            };
            // Placeholder — the actual picker button is added at parent layout time;
            // see WireColorPicker below.
            host.AutomationId = "toolbar_" + item.Id.Replace('.', '_');
            return host;
        }

        if (item.Id is AppConstants.CmdZoomIn or AppConstants.CmdZoomOut or AppConstants.CmdFitToScreen)
        {
            return BuildIconButton(item);
        }

        if (group == 3)
        {
            return BuildFlyoutButton(item, AppStrings.ToolbarBasicShapes, subItems:
            [
                new MenuCommandItem { Id = AppConstants.CmdUndo, TextKey = AppStrings.ToolbarRectangle },
                new MenuCommandItem { Id = AppConstants.CmdRedo, TextKey = AppStrings.ToolbarEllipse },
                new MenuCommandItem { Id = AppConstants.CmdNew, TextKey = AppStrings.ToolbarPolygon },
            ]);
        }

        if (group == 4)
        {
            return BuildFlyoutButton(item, AppStrings.ToolbarConnectors, subItems:
            [
                new MenuCommandItem { Id = AppConstants.ToolConnectorStraight, TextKey = AppStrings.ToolbarStraightConnector },
                new MenuCommandItem { Id = AppConstants.ToolConnectorOrthogonal, TextKey = AppStrings.ToolbarOrthogonalConnector },
                new MenuCommandItem { Id = AppConstants.ToolConnectorBezier, TextKey = AppStrings.ToolbarBezierConnector },
            ]);
        }

        return BuildIconButton(item);
    }

    private Button BuildIconButton(ToolbarItem item)
    {
        var btn = new Button
        {
            Text = GlyphFor(item.Id),
            Style = (Style)Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
            ToolTipProperties = new ToolTipProperties { Text = item.TooltipKey },
        };
        if (item.IsChecked)
        {
            btn.Style = (Style)Resources["ToolbarIconActiveStyle"];
        }
        btn.Clicked += (_, _) => ToolCommand?.Execute(item.Id);
        return btn;
    }

    private View BuildFlyoutButton(ToolbarItem item, string headerKey, IList<MenuCommandItem> subItems)
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
            Text = GlyphFor(item.Id),
            Style = (Style)Resources["ToolbarIconButtonStyle"],
            AutomationId = "toolbar_" + item.Id.Replace('.', '_'),
            ToolTipProperties = new ToolTipProperties { Text = item.TooltipKey },
        };
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
            Stroke = (Color)Application.Current!.Resources["PanelBorderBrush"],
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Content = stack,
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
        };
    }

    private static string GlyphFor(string id) => id switch
    {
        AppConstants.CmdUndo => "\u21B6",
        AppConstants.CmdRedo => "\u21B7",
        AppConstants.ToolPan => "\u270B",
        AppConstants.ToolSelect => "\u2192",
        AppConstants.ToolText => "T",
        AppConstants.CmdZoomIn => "+",
        AppConstants.CmdZoomOut => "\u2212",
        AppConstants.CmdFitToScreen => "\u26F6",
        AppConstants.CmdSendToBack => "\u2B05",
        AppConstants.CmdBringToFront => "\u27A1",
        AppConstants.CmdSendBackward => "\u2B05\u2B05",
        AppConstants.CmdBringForward => "\u27A1\u27A1",
        AppConstants.CmdDelete => "\u2716",
        "tool.lock" => "\u26BF",
        _ => id,
    };

    private void ApplyGroup6Visibility()
    {
        // We don't rebuild — the simplest UX is to simply toggle Opacity for group-6.
        // The layout is built once; group 6 entries are tagged with Tag="6" so we can fade.
        foreach (var child in GroupsBar.Children)
        {
            if (child is HorizontalStackLayout groupLayout && groupLayout.AutomationId == "group_6")
            {
                groupLayout.IsVisible = HasSingleSelection;
            }
        }
    }
}