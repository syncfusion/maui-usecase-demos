using System.Windows.Input;

namespace DiagramBuilder.Controls;

/// <summary>
/// Reusable panel header (title + optional collapse chevron + optional trailing content).
/// Bindable: <see cref="Title"/> text, <see cref="Collapsed"/> state,
/// <see cref="ShowChevron"/> to hide the chevron (used by property panel),
/// <see cref="ToggleCommand"/> executed on chevron tap. Used by the stencil and
/// property panels (see ui-spec.md §4 and §6).
/// </summary>
public partial class PanelHeaderControl : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PanelHeaderControl), string.Empty,
            propertyChanged: OnTitleChanged);

    public static readonly BindableProperty CollapsedProperty =
        BindableProperty.Create(nameof(Collapsed), typeof(bool), typeof(PanelHeaderControl), false,
            propertyChanged: OnCollapsedChanged);

    public static readonly BindableProperty ShowChevronProperty =
        BindableProperty.Create(nameof(ShowChevron), typeof(bool), typeof(PanelHeaderControl), true,
            propertyChanged: OnShowChevronChanged);

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(PanelHeaderControl));

    public static readonly BindableProperty TrailingProperty =
        BindableProperty.Create(nameof(Trailing), typeof(View), typeof(PanelHeaderControl),
            propertyChanged: OnTrailingChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool Collapsed
    {
        get => (bool)GetValue(CollapsedProperty);
        set => SetValue(CollapsedProperty, value);
    }

    public bool ShowChevron
    {
        get => (bool)GetValue(ShowChevronProperty);
        set => SetValue(ShowChevronProperty, value);
    }

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public View? Trailing
    {
        get => (View?)GetValue(TrailingProperty);
        set => SetValue(TrailingProperty, value);
    }

    public PanelHeaderControl()
    {
        InitializeComponent();
        // Material Icons chevron (expand-more / expand-less).
        Chevron.FontFamily = "MaterialIcons";
        Chevron.Text = Constants.MaterialIcons.DropdownCaret;
        Chevron.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(() => ToggleCommand?.Execute(null)),
        });
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PanelHeaderControl header && newValue is string title)
        {
            header.TitleLabel.Text = title;
        }
    }

    private static void OnCollapsedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PanelHeaderControl header && newValue is bool collapsed)
        {
            // Expanded shows the expand-more caret; collapsed shows the
            // expand-less caret (Material Icons).
            header.Chevron.Text = collapsed
                ? Constants.MaterialIcons.CollapseCaret
                : Constants.MaterialIcons.DropdownCaret;
        }
    }

    private static void OnShowChevronChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PanelHeaderControl header && newValue is bool show)
        {
            header.Chevron.IsVisible = show;
        }
    }

    private static void OnTrailingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PanelHeaderControl header)
        {
            header.TrailingHost.Content = newValue as View;
        }
    }
}