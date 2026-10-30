using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Models;

namespace DiagramBuilder.Controls;

/// <summary>
/// Horizontal menu bar. Binds to the menu roots (ItemsSource); each root
/// gets a button + SfPopup flyout rendered from its <see cref="MenuCommandItem.Children"/>
/// tree. Item taps execute the bound <see cref="MenuCommand"/>. Checkable items show
/// a check glyph and reflect the VM check-state delegate (IsChecked).
/// Submenu items are rendered inline as indented rows under their parent header.
/// Zero business logic — only binds and dispatches by id.
/// </summary>
public partial class MenuBarControl : ContentView
{
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
            Style = (Style)Resources["MenuRootButtonStyle"],
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
        button.Clicked += (_, _) =>
        {
            popup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        };
        return button;
    }

    private View BuildFlyoutContent(MenuCommandItem root)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };
        AppendRows(stack, root.Children, indent: 0, isTopLevel: true);
        var border = new Border
        {
            Stroke = (Color)Application.Current!.Resources["PanelBorderColor"],
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
            Content = stack,
        };
        return border;
    }

    private void AppendRows(VerticalStackLayout stack, IList<MenuCommandItem> items, int indent, bool isTopLevel)
    {
        foreach (var child in items)
        {
            // Hide submenu parents at top level — their children are rendered inline.
            if (child.Children.Count > 0)
            {
                // Render submenu header (non-clickable, just a label).
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
            return new BoxView { Style = (Style)Resources["MenuFlyoutSeparatorStyle"] };
        }

        var grid = new Grid
        {
            Style = (Style)Resources["MenuFlyoutRowStyle"],
            Padding = new Thickness(8 + (indent * 16), 0, 8, 0),
        };
        var checkGlyph = new Label
        {
            Text = (item.IsCheckable && IsCheckedSafe(item.Id)) ? "\u2713" : string.Empty,
            Style = (Style)Resources["MenuFlyoutCheckGlyphStyle"],
        };
        var label = new Label
        {
            Text = item.TextKey,
            Style = (Style)Resources["MenuFlyoutLabelStyle"],
        };
        var shortcut = new Label
        {
            Text = item.GestureKey,
            Style = (Style)Resources["MenuFlyoutShortcutStyle"],
        };
        grid.Children.Add(checkGlyph);
        grid.Children.Add(label);
        grid.Children.Add(shortcut);
        Grid.SetColumn(checkGlyph, 0);
        Grid.SetColumn(label, 1);
        Grid.SetColumn(shortcut, 2);

        var tap = new TapGestureRecognizer
        {
            Command = new Command(() => MenuCommand?.Execute(item.Id)),
        };
        grid.GestureRecognizers.Add(tap);
        return grid;
    }

    private View BuildSubmenuHeaderRow(MenuCommandItem item, int indent)
    {
        var grid = new Grid
        {
            Padding = new Thickness(8 + (indent * 16), 4, 8, 4),
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
            TextColor = (Color)Application.Current!.Resources["TextSecondary"],
            VerticalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var chevron = new Label
        {
            Text = "\u25B8", // ▸
            FontSize = 11,
            TextColor = (Color)Application.Current!.Resources["TextSecondary"],
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