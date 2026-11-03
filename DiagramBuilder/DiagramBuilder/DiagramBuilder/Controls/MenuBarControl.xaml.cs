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

        var popup = new Syncfusion.Maui.Popup.SfPopup
        {
            ContentTemplate = new DataTemplate(() => BuildFlyoutContent(root)),
            ShowHeader = false,
            ShowFooter = false,
            ShowCloseButton = false,
            StaysOpen = false,
            OverlayMode = Syncfusion.Maui.Popup.PopupOverlayMode.Transparent,
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
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4, 6, 4, 6) };
        AppendRows(stack, root.Children, indent: 0, isTopLevel: true);
        var border = new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
            Shadow = new Shadow { Radius = 8, Offset = new Microsoft.Maui.Graphics.Point(0, 4), Opacity = 0.18f },
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

        var grid = new Grid
        {
            Style = (Style)Application.Current!.Resources["MenuFlyoutRowStyle"],
            Padding = new Thickness(8 + (indent * 16), 0, 8, 0),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = GridLength.Auto },   // icon
                new ColumnDefinition { Width = GridLength.Auto },   // check
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, // label
                new ColumnDefinition { Width = GridLength.Auto },   // shortcut
            }),
        };

        var icon = new Label
        {
            Text = IconGlyphFor(item.Id),
            FontSize = 13,
            TextColor = TextSecondary,
            WidthRequest = 16,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        var checkGlyph = new Label
        {
            Text = (item.IsCheckable && IsCheckedSafe(item.Id)) ? "\u2713" : string.Empty,
            FontSize = 13,
            TextColor = CheckBlue,
            WidthRequest = 16,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };
        var label = new Label
        {
            Text = item.TextKey,
            FontSize = 13,
            TextColor = item.IsEnabled ? Services.ResourceLookup.ResourceColor("TextPrimary") : TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var shortcut = new Label
        {
            Text = item.GestureKey,
            FontSize = 11,
            TextColor = TextSecondary,
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        grid.Children.Add(icon);
        grid.Children.Add(checkGlyph);
        grid.Children.Add(label);
        grid.Children.Add(shortcut);
        Grid.SetColumn(icon, 0);
        Grid.SetColumn(checkGlyph, 1);
        Grid.SetColumn(label, 2);
        Grid.SetColumn(shortcut, 3);
        grid.Opacity = item.IsEnabled ? 1 : 0.5;

        if (item.IsEnabled)
        {
            // This MAUI build has no Pointer/Press events on a Grid, so leaf rows are plain
            // tap targets. The root menu buttons + toolbar buttons carry the full visual-state
            // feedback (hover/pressed/selected) via their Button VSM.
            grid.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => MenuCommand?.Execute(item.Id)),
            });
        }
        return grid;
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

    /// <summary>
    /// Monochrome icon glyph per command id for the flyout rows. Drawn from the same OpenSans
    /// glyph set the toolbar already renders (proven on Android), so the menu stays monochrome
    /// and visually consistent with the rest of the app rather than relying on color emoji.
    /// </summary>
    private static string IconGlyphFor(string id) => id switch
    {
        AppConstants.CmdNew => "\u25A2",         // ▢  new document
        AppConstants.CmdOpen => "\u2191",        // ↑  open
        AppConstants.CmdSave => "\u25A3",        // ▣  save
        AppConstants.CmdSaveAs => "\u25A3",      // ▣  save as
        AppConstants.CmdExport => "\u2197",      // ↗  export
        AppConstants.CmdPrint => "\u2318",       // ⌘  print (monochrome stand-in)

        AppConstants.CmdUndo => "\u21B6",        // ↶
        AppConstants.CmdRedo => "\u21B7",        // ↷
        AppConstants.CmdCut => "\u2702",         // ✂
        AppConstants.CmdCopy => "\u2460",        // ① (two stacked, see paste)
        AppConstants.CmdPaste => "\u25A4",       // ▤  clipboard
        AppConstants.CmdDelete => "\u2296",      // ⊖  remove
        AppConstants.CmdDuplicate => "\u2461",   // ②
        AppConstants.CmdSelectAll => "\u2713",   // ✓

        AppConstants.CmdZoomIn => "\u2295",      // ⊕
        AppConstants.CmdZoomOut => "\u2296",     // ⊖
        AppConstants.CmdFitToScreen => "\u26F6", // ⛶
        AppConstants.CmdShowGrid => "\u2317",    // ⌗
        AppConstants.CmdSnapToGrid => "\u2317",  // ⌗

        AppConstants.CmdSendToBack => "\u21E8",  // ⇈
        AppConstants.CmdBringToFront => "\u21E8",// ⇈
        AppConstants.CmdSendBackward => "\u21E7",// ⇇
        AppConstants.CmdBringForward => "\u21E9",// ⇉
        AppConstants.CmdAlignLeft => "\u2190",   // ←
        AppConstants.CmdAlignRight => "\u2192",  // →
        AppConstants.CmdAlignCenter => "\u2194", // ↔
        AppConstants.CmdAlignTop => "\u2191",    // ↑
        AppConstants.CmdAlignBottom => "\u2193", // ↓
        AppConstants.CmdAlignMiddle => "\u2195", // ↕
        AppConstants.CmdGroup => "\u25A6",       // ▦
        AppConstants.CmdUngroup => "\u25A6",     // ▦

        AppConstants.ToolPan => "\u26F6",        // ⛶ pan
        AppConstants.ToolSelect => "\u25A2",     // ▢ select
        AppConstants.ToolConnectorStraight => "\u2014",  // —
        AppConstants.ToolConnectorOrthogonal => "\u21B4", // ↴
        AppConstants.ToolConnectorBezier => "\u223C",    // ∼
        AppConstants.ToolText => "\u2713",       // ✓

        _ => string.Empty,
    };
}
