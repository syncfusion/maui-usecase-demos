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
/// Item taps execute the bound <see cref="MenuCommand"/>; checkable items show a check glyph
/// and reflect the VM check-state delegate (<see cref="IsChecked"/>). Rows get hover +
/// pressed feedback via explicit pointer handlers (Grid has no automatic Pressed state).
/// Submenu items render as indented section headers with their children inlined.
/// Zero business logic — only binds and dispatches by id.
/// </summary>
public partial class MenuBarControl : ContentView
{
    // Menu chrome colors. Kept local (this MAUI build exposes Color.FromArgb(string) only): the
    // chrome is distinct from the app's HoverBackground which is for surfaces on light panels.
    private static readonly Color ChromeBackground = Color.FromArgb("#0D6EFD");
    private static readonly Color TextSecondary = Color.FromArgb("#5F6368");
    private static readonly Color CheckBlue = Color.FromArgb("#0D6EFD");

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MenuBarControl),
            propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty MenuCommandProperty =
        BindableProperty.Create(nameof(MenuCommand), typeof(ICommand), typeof(MenuBarControl));

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(Func<string, bool>), typeof(MenuBarControl));

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
        var popup = new Syncfusion.Maui.Popup.SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildFlyoutContent(root)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
            OverlayMode = Syncfusion.Maui.Popup.PopupOverlayMode.Transparent,
            // Let the panel size to the content instead of SfPopup's default 313px width —
            // this keeps File/Edit/View/Arrange/Window at a narrow desktop-flyout width.
            AutoSizeMode = Syncfusion.Maui.Popup.PopupAutoSizeMode.Width,
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
        popup.Opened += (_, _) => button.Style = activeStyle;
        popup.Closed += (_, _) => button.Style = normalStyle;

        button.Clicked += (_, _) =>
        {
            popup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        };
        return button;
    }

    private View BuildFlyoutContent(MenuCommandItem root)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(0, 4, 0, 4) };
        AppendRows(stack, root.Children, indent: 0, isTopLevel: true);
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
        };
        return border;
    }

    private void AppendRows(VerticalStackLayout stack, IList<MenuCommandItem> items, int indent, bool isTopLevel)
    {
        foreach (var child in items)
        {
            // Submenu parents render as a section header with their children inlined below.
            if (child.Children.Count > 0)
            {
                stack.Children.Add(BuildSubmenuHeaderRow(child, indent));
                AppendRows(stack, child.Children.ToList(), indent + 1, isTopLevel: false);
            }
            else
            {
                stack.Children.Add(BuildLeafRow(child, indent));
            }
        }
    }

    private View BuildLeafRow(MenuCommandItem item, int indent)
    {
        if (item.IsSeparator)
        {
            return new BoxView
            {
                HeightRequest = 1,
                BackgroundColor = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
                Margin = new Thickness(8, 4),
            };
        }

        var row = new Grid
        {
            Style = (Style)Application.Current!.Resources["MenuFlyoutRowStyle"],
            // No padding here — the full-width Button beneath (see below) must span the whole
            // row so the hover/pressed highlight is edge-to-edge. Text indentation is applied
            // to the labels directly.
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = GridLength.Auto },   // check mark
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // label
                new ColumnDefinition { Width = GridLength.Auto },   // shortcut
            }),
        };

        // Interaction + visual-state host. A Button is the only element whose visual-state
        // manager reliably fires the pressed/touch overlay on Android in this MAUI build, so it
        // fills the row and supplies the hover/pressed/selected background (MenuRowHitAreaStyle).
        // The text is empty and it is transparent at rest; the VSM states paint the fill.
        if (item.IsEnabled)
        {
            var hitArea = new Button
            {
                Style = (Style)Application.Current!.Resources["MenuRowHitAreaStyle"],
                Command = new Command(() => MenuCommand?.Execute(item.Id)),
            };
            Grid.SetColumnSpan(hitArea, 3);
            row.Children.Add(hitArea);
        }

        var indentPx = 12 + (indent * 16);
        var checkGlyph = new Label
        {
            Text = (item.IsCheckable && IsCheckedSafe(item.Id)) ? "\u2713" : string.Empty,
            FontSize = 13,
            TextColor = CheckBlue,
            WidthRequest = 18,
            Margin = new Thickness(indentPx, 0, 0, 0),
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,   // let the touch fall through to the Button beneath
        };
        var label = new Label
        {
            Text = item.TextKey,
            FontSize = 13,
            TextColor = item.IsEnabled ? Services.ResourceLookup.ResourceColor("TextPrimary") : TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true,
        };
        var shortcut = new Label
        {
            Text = item.GestureKey,
            FontSize = 11,
            TextColor = TextSecondary,
            Margin = new Thickness(24, 0, 12, 0),
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
            InputTransparent = true,
        };

        row.Children.Add(checkGlyph);
        row.Children.Add(label);
        row.Children.Add(shortcut);
        Grid.SetColumn(checkGlyph, 0);
        Grid.SetColumn(label, 1);
        Grid.SetColumn(shortcut, 2);
        row.Opacity = item.IsEnabled ? 1 : 0.5;
        return row;
    }

    private View BuildSubmenuHeaderRow(MenuCommandItem item, int indent)
    {
        var grid = new Grid
        {
            Style = (Style)Application.Current!.Resources["MenuFlyoutHeaderStyle"],
            Padding = new Thickness(8 + (indent * 16), 0, 8, 0),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            }),
        };
        var label = new Label
        {
            Text = item.TextKey,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            CharacterSpacing = 0.5,
            TextColor = TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var chevron = new Label
        {
            Text = "\u25B8", // ▸
            FontSize = 11,
            TextColor = TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        grid.Children.Add(label);
        grid.Children.Add(chevron);
        Grid.SetColumn(label, 0);
        Grid.SetColumn(chevron, 1);
        return grid;
    }

    private bool IsCheckedSafe(string id) => IsChecked?.Invoke(id) ?? false;
}
