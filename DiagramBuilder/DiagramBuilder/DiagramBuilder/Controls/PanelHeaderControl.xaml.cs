using System.Windows.Input;

namespace DiagramBuilder.Controls;

/// <summary>
/// Reusable panel header (title + collapse chevron + optional trailing content).
/// Bindable: <see cref="Title"/> text, <see cref="Collapsed"/> state,
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

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(PanelHeaderControl));

    public static readonly BindableProperty TrailingContentProperty =
        BindableProperty.Create(nameof(TrailingContent), typeof(View), typeof(PanelHeaderControl),
            propertyChanged: OnTrailingContentChanged);

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

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public new View? TrailingContent
    {
        get => (View?)GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    public PanelHeaderControl()
    {
        InitializeComponent();
        Chevron.Text = "\u203A"; // › chevron
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
            header.Chevron.Text = collapsed ? "\u2039" : "\u203A"; // ‹ / ›
        }
    }

    private static void OnTrailingContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PanelHeaderControl header)
        {
            header.TrailingContent.Content = newValue as View;
        }
    }
}