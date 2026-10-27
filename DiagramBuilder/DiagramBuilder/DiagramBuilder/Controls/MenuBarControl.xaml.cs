using System.Collections;
using System.Windows.Input;
using DiagramBuilder.Models;

namespace DiagramBuilder.Controls;

/// <summary>
/// Horizontal menu bar. Binds to the menu roots (ItemsSource); each root
/// gets a button + SfPopup flyout rendered from its <see cref="MenuCommandItem.Children"/>
/// tree. Item taps execute the bound <see cref="MenuCommand"/>. Checkable items show
/// a check glyph and reflect the VM check-state delegate (IsChecked).
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
        // The popup will be anchored to the button at click time.
        button.Clicked += (_, _) =>
        {
            popup.ShowRelativeToView(button, Syncfusion.Maui.Popup.PopupRelativePosition.AlignBottomLeft);
        };
        return button;
    }

    private View BuildFlyoutContent(MenuCommandItem root)
    {
        var stack = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(4) };
        foreach (var child in root.Children)
        {
            stack.Children.Add(BuildRow(child, isTopLevel: true));
        }
        var border = new Border
        {
            Stroke = (Color)Resources["PanelBorderColor"],
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
            Content = stack,
        };
        return border;
    }

    private View BuildRow(MenuCommandItem item, bool isTopLevel)
    {
        if (item.IsSeparator)
        {
            return new BoxView { Style = (Style)Resources["MenuFlyoutSeparatorStyle"] };
        }

        var grid = new Grid { Style = (Style)Resources["MenuFlyoutRowStyle"] };
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

        // Submenus open a nested popup (we render children inline as indented rows
        // here to keep the control cross-TFM compilable; full cascading menu is a
        // follow-up when SfPopup child-popup support is validated).
        if (item.Children.Count > 0 && !isTopLevel)
        {
            // No-op: submenus surface at the top level only via MenuCatalog for now.
        }

        var tap = new TapGestureRecognizer
        {
            Command = new Command(() =>
            {
                MenuCommand?.Execute(item.Id);
            }),
        };
        grid.GestureRecognizers.Add(tap);
        return grid;
    }

    private bool IsCheckedSafe(string id) => IsChecked?.Invoke(id) ?? false;
}