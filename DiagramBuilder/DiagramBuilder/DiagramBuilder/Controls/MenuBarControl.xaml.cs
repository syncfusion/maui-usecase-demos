using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;

namespace DiagramBuilder.Controls;

/// <summary>
/// Horizontal menu bar on the blue chrome. Each root (File / Edit / View / Arrange / Window)
/// is a white-on-blue pill that opens an SfPopup flyout built from its
/// <see cref="MenuCommandItem.Children"/> tree. Tapping an open root shows the "Selected"
/// state (white pill, blue label); the state clears when the flyout closes.
/// Each leaf row is a Syncfusion SfButton — its native handler drives Normal / Pressed /
/// Checked visual states from Android touch events (the only element in this MAUI build
/// whose touch overlay works reliably). Item taps execute the bound
/// <see cref="MenuCommand"/>; checkable items show a check glyph and a persistent Checked
/// highlight via the IsChecked delegate.
/// Submenu items render as indented section headers with their children inlined.
/// Zero business logic — only binds and dispatches by id.
/// </summary>
public partial class MenuBarControl : ContentView
{
    // Menu chrome colors. Kept local (this MAUI build exposes Color.FromArgb(string) only): the
    // chrome is distinct from the app's HoverBackground which is for surfaces on light panels.
    private static readonly Color ChromeBackground = Color.FromArgb("#0D6EFD");
    private static readonly Color TextSecondary = Color.FromArgb("#5F6368");
    private static readonly Color DisabledText = Color.FromArgb("#B0B4B9");
    private static readonly Color CheckBlue = Color.FromArgb("#0D6EFD");

    private static readonly System.Collections.Generic.Dictionary<string, double> FlyoutMinWidths = new()
    {
        ["menu.file"] = 240,
        ["menu.edit"] = 250,
        ["menu.view"] = 220,
        ["menu.arrange"] = 288,
        ["menu.window"] = 190,
    };

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MenuBarControl),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty MenuCommandProperty =
        BindableProperty.Create(nameof(MenuCommand), typeof(ICommand), typeof(MenuBarControl));

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(Func<string, bool>), typeof(MenuBarControl));

    public static readonly BindableProperty IsItemEnabledProperty =
        BindableProperty.Create(nameof(IsItemEnabled), typeof(Func<string, bool>), typeof(MenuBarControl));

    /// <summary>Selection-aware enabled state per command id (evaluated each time a flyout opens).</summary>
    public Func<string, bool>? IsItemEnabled
    {
        get => (Func<string, bool>?)GetValue(IsItemEnabledProperty);
        set => SetValue(IsItemEnabledProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ICommand? MenuCommand
    {
        get => (ICommand?)GetValue(MenuCommandProperty);
        set => SetValue(MenuCommandProperty, value);
    }

    /// <summary>Pure delegate handed in by the VM so the view never owns check state.</summary>
    public Func<string, bool>? IsChecked
    {
        get => (Func<string, bool>?)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public MenuBarControl()
    {
        InitializeComponent();
        RootsBar.BackgroundColor = ChromeBackground;
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MenuBarControl bar)
        {
            bar.Rebuild();
        }
    }

    private void Rebuild()
    {
        RootsBar.Children.Clear();
        if (ItemsSource is null)
        {
            return;
        }

        foreach (var item in ItemsSource)
        {
            if (item is MenuCommandItem root)
            {
                RootsBar.Children.Add(BuildRootButton(root));
            }
        }
    }

    private View BuildRootButton(MenuCommandItem root)
    {
        var button = new Button
        {
            Text = root.TextKey,
            Style = (Style)Application.Current!.Resources["MenuRootStyle"],
            AutomationId = "menu_root_" + root.Id,
        };

        // PopupStyle: SfPopup's default CornerRadius is 28, which wrapped the flyout in a
        // large circular-cornered panel. The design uses ~2px sharp corners on a white panel,
        // so override it here (the inner Border below is the visible surface).
        Action? refill = null;
        var popup = new Syncfusion.Maui.Popup.SfPopup
        {
            ContentTemplate = new DataTemplate(() =>
            {
                var content = BuildFlyoutContent(root, out var fill);
                refill = fill;
                return content;
            }),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
            OverlayMode = Syncfusion.Maui.Popup.PopupOverlayMode.Transparent,
            // Both dimensions hug the inner Border — replaces SfPopup's default 313px width.
            AutoSizeMode = Syncfusion.Maui.Popup.PopupAutoSizeMode.Both,
            AutoSizeTarget = Syncfusion.Maui.Popup.PopupAutoSizeTarget.Content,
            PopupStyle = new Syncfusion.Maui.Popup.PopupStyle
            {
                CornerRadius = new CornerRadius(2),
                PopupBackground = new SolidColorBrush(Colors.Transparent),
                HasShadow = false,
            },
        };

        // Active-root highlight: swap to the white-pill style while the flyout is open.
        // (.NET 9 Button has no programmatic GoToElementState, so the state is a style swap.)
        var normalStyle = (Style)Application.Current!.Resources["MenuRootStyle"];
        var activeStyle = (Style)Application.Current!.Resources["MenuRootActiveStyle"];
        popup.Opened += (_, _) =>
        {
            button.Style = activeStyle;
            // Selection may have changed since the flyout was built: re-evaluate enabled states.
            refill?.Invoke();
        };
        popup.Closed += (_, _) => button.Style = normalStyle;

        button.Clicked += (_, _) =>
        {
            PopupXamlRootHelper.AttachIfAvailable(popup, button);
            popup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        };
        return button;
    }

    // Per-menu vertical padding for the content stack — used to make short menus (Window)
    // sit tighter against the panel border without changing row height or row padding.
    private static readonly System.Collections.Generic.Dictionary<string, Thickness> _menuPaddingByRootId =
        new()
        {
            // Compact (Window has only 3 items, so reduce the top/bottom padding so the
            // flyout height shrinks — without touching the 32px row height or its padding).
            ["window"] = new Thickness(0, 2, 0, 2),
            // Default: a few px breathing room top/bottom for menus with many items.
            ["arrange"] = new Thickness(0, 3, 0, 3),
            ["edit"] = new Thickness(0, 4, 0, 4),
            ["file"] = new Thickness(0, 4, 0, 4),
            ["view"] = new Thickness(0, 4, 0, 4),
        };

    private View BuildFlyoutContent(MenuCommandItem root, out Action refill)
    {
        // Use a per-menu padding so the Window menu is shorter overall; row height + row
        // padding (MenuFlyoutRowStyle / SfButton HeightRequest) are unchanged for every menu.
        var pad = _menuPaddingByRootId.TryGetValue(root.Id, out var p)
            ? p
            : new Thickness(0, 4, 0, 4);
        var stack = new VerticalStackLayout { Spacing = 0, Padding = pad };

        // Expanding / collapsing a section only re-fills this stack. Rebuilding the whole
        // bar (the old behaviour) recreated the root button the popup is anchored to and
        // dismissed the flyout, so sections appeared not to toggle.
        void Fill()
        {
            stack.Children.Clear();
            AppendRows(stack, root.Children, indent: 0, Fill);
        }

        Fill();
        refill = Fill;

        // The SfPopup panel is transparent (see BuildRootButton), so this Border is the actual
        // white flyout surface. ~2px corners + a thin border + soft shadow, per the design.
        var border = new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 2 },
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
            Shadow = new Shadow { Radius = 6, Offset = new Microsoft.Maui.Graphics.Point(0, 3), Opacity = 0.16f },
            Content = stack,
            HorizontalOptions = LayoutOptions.Start,
            MinimumWidthRequest = GetFlyoutMinWidth(root.Id),
        };
        return border;
    }

    private void AppendRows(VerticalStackLayout stack, IList<MenuCommandItem> items, int indent, Action refill)
    {
        foreach (var child in items)
        {
            if (child.Children.Count > 0)
            {
                stack.Children.Add(BuildSubmenuHeaderRow(child, indent, refill));
                if (child.IsExpanded)
                {
                    AppendRows(stack, child.Children.ToList(), indent + 1, refill);
                }
            }
            else
            {
                stack.Children.Add(BuildLeafRow(child, indent));
            }
        }
    }

    private bool IsItemEnabledSafe(MenuCommandItem item) =>
        item.IsEnabled && (IsItemEnabled?.Invoke(item.Id) ?? true);

    private View BuildLeafRow(MenuCommandItem item, int indent)
    {
        if (item.IsSeparator)
        {
            return new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
                Margin = new Thickness(12, 4),
            };
        }

        // The menu row is a single Syncfusion SfButton. Its native handler drives Normal /
        // Pressed / Checked visual states from Android touch events, so the touch overlay
        // is reliable (MAUI Button's VSM press is unreliable on this MAUI build). The row's
        // check + icon + label + shortcut are hosted inside SfButton.Content as a DataTemplate —
        // the factory builds a NEW grid per expansion, as MAUI templates require.
        var enabled = IsItemEnabledSafe(item);
        var row = new Syncfusion.Maui.Buttons.SfButton
        {
            Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
            Content = new DataTemplate(() => BuildRowLayout(item, indent, enabled)),
            IsCheckable = item.IsCheckable,
            IsChecked = item.IsCheckable && IsCheckedSafe(item.Id),
            IsEnabled = enabled,
            CommandParameter = item.Id,
            Command = new Command<string?>(id => MenuCommand?.Execute(id)),
        };
        return row;
    }

    /// <summary>Check mark + icon + label + shortcut layout hosted inside the row's SfButton.Content.</summary>
    private Grid BuildRowLayout(MenuCommandItem item, int indent, bool enabled)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(24) },                       // check slot
                new ColumnDefinition { Width = new GridLength(18) },                       // icon slot
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },    // label
                new ColumnDefinition { Width = new GridLength(64) },                      // shortcut
            }),
            Margin = new Thickness(indent * 16, 0, 0, 0),
        };

        var checkGlyph = new Label
        {
            // Material check icon for checked menu items.
            Text = (item.IsCheckable && IsCheckedSafe(item.Id))
                ? Constants.MaterialIcons.CheckGlyph
                : string.Empty,
            FontFamily = "MaterialIcons",
            FontSize = 13,
            TextColor = CheckBlue,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        var icon = new Label
        {
            Text = MenuIconFor(item),
            FontFamily = "MaterialIcons",
            FontSize = 14,
            TextColor = enabled ? TextSecondary : DisabledText,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        var label = new Label
        {
            Text = item.TextKey,
            FontSize = 13,
            TextColor = enabled ? Services.ResourceLookup.ResourceColor("TextPrimary") : DisabledText,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var shortcut = new Label
        {
            Text = item.GestureKey,
            FontSize = 12,
            TextColor = TextSecondary,
            Margin = new Thickness(0, 0, 14, 0),
            HorizontalTextAlignment = TextAlignment.End,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        grid.Children.Add(checkGlyph);
        grid.Children.Add(icon);
        grid.Children.Add(label);
        grid.Children.Add(shortcut);
        Grid.SetColumn(checkGlyph, 0);
        Grid.SetColumn(icon, 1);
        Grid.SetColumn(label, 2);
        Grid.SetColumn(shortcut, 3);
        return grid;
    }

    private View BuildSubmenuHeaderRow(MenuCommandItem item, int indent, Action refill)
    {
        return new Syncfusion.Maui.Buttons.SfButton
        {
            Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
            // A DataTemplate must build a NEW layout per call (a captured instance can only
            // have one parent), so the header is created inside the factory.
            Content = new DataTemplate(() => BuildSubmenuHeaderLayout(item, indent)),
            Command = new Command(() =>
            {
                item.IsExpanded = !item.IsExpanded;
                // Defer: the tapped button is removed from the stack while refilling.
                Dispatcher.Dispatch(refill);
            }),
        };
    }

    private Grid BuildSubmenuHeaderLayout(MenuCommandItem item, int indent)
    {
        var grid = new Grid
        {
            Padding = new Thickness(8 + (indent * 16), 0, 8, 0),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(18) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            }),
            ColumnSpacing = 8,
        };
        var icon = new Label
        {
            Text = MenuIconFor(item),
            FontFamily = "MaterialIcons",
            FontSize = 14,
            TextColor = TextSecondary,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        var label = new Label
        {
            Text = item.TextKey,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var chevron = new Label
        {
            // expand-more when open, expand-less when collapsed (design: chevron-down while open).
            Text = item.IsExpanded
                ? Constants.MaterialIcons.DropdownCaret
                : Constants.MaterialIcons.CollapseCaret,
            FontFamily = "MaterialIcons",
            FontSize = 16,
            TextColor = TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        grid.Children.Add(icon);
        grid.Children.Add(label);
        grid.Children.Add(chevron);
        Grid.SetColumn(icon, 0);
        Grid.SetColumn(label, 1);
        Grid.SetColumn(chevron, 2);
        return grid;
    }

    private static double GetFlyoutMinWidth(string rootId) => FlyoutMinWidths.TryGetValue(rootId, out var width)
        ? width
        : 220;

    private static string MenuIconFor(MenuCommandItem item) =>
        // Material Icons font (see Constants.MaterialIcons). Empty for ids with
        // no icon so the label sits alone.
        Constants.MaterialIcons.TextFor(item.Id) ?? string.Empty;

    private bool IsCheckedSafe(string id) => IsChecked?.Invoke(id) ?? false;
}
