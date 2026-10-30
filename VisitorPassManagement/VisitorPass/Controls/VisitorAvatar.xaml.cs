using Syncfusion.Maui.Core;

namespace VisitorPass.Controls;

/// <summary>
/// VP-021 — reusable <c>SfAvatarView</c> wrapper that renders a 1-2
/// character monogram on a coloured background.
///
/// Drop into any page with:
///
///     &lt;controls:VisitorAvatar Initials="{Binding Initials}"
///                             BackgroundColor="{Binding AvatarColor}"
///                             AvatarSize="Large" /&gt;
///
/// Defaults match the Digital Pass identity card (large, white
/// initials on a brand-coloured tile).
///
/// Note on the underlying Syncfusion API: SfAvatarView exposes the
/// monogram text through the AvatarName property (NOT a property
/// named Initials, which would conflict with the underlying
/// VisualElement.Initials of a different shape).
/// ContentType must be set to AvatarContentType.Initials so the
/// control renders the text rather than an image fallback.
/// </summary>
public partial class VisitorAvatar : ContentView
{
    /// <summary>Monogram (1-2 characters). Empty string falls back to <c>"?"</c>.</summary>
    public static readonly BindableProperty InitialsProperty = BindableProperty.Create(
        nameof(Initials),
        typeof(string),
        typeof(VisitorAvatar),
        defaultValue: "?",
        propertyChanged: OnInitialsChanged);

    /// <summary>Background colour (hex string or <see cref="Color"/>).</summary>
    public static readonly new BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor),
        typeof(Color),
        typeof(VisitorAvatar),
        defaultValueCreator: static _ => Color.FromArgb("#5146E5"),
        propertyChanged: OnBackgroundColorChanged);

    /// <summary>Syncfusion <c>AvatarSize</c> enum (Small / Large / ExtraLarge).</summary>
    public static readonly BindableProperty AvatarSizeProperty = BindableProperty.Create(
        nameof(AvatarSize),
        typeof(AvatarSize),
        typeof(VisitorAvatar),
        defaultValue: AvatarSize.Large,
        propertyChanged: OnAvatarSizeChanged);

    /// <summary>Monogram (1-2 characters). Empty string falls back to <c>"?"</c>.</summary>
    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    /// <summary>Background colour (hex string or <see cref="Color"/>).</summary>
    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    /// <summary>Syncfusion <c>AvatarSize</c> enum (Small / Large / ExtraLarge).</summary>
    public AvatarSize AvatarSize
    {
        get => (AvatarSize)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public VisitorAvatar()
    {
        InitializeComponent();
        // Sync initial state so the first paint matches the property values.
        Avatar.BackgroundColor = this.BackgroundColor;
        Avatar.AvatarSize = this.AvatarSize;
        Avatar.AvatarName = string.IsNullOrWhiteSpace(this.Initials) ? "?" : this.Initials;
    }

    private static void OnInitialsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisitorAvatar self)
            self.Avatar.AvatarName = newValue as string ?? "?";
    }

    private static void OnBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisitorAvatar self && newValue is Color c)
            self.Avatar.BackgroundColor = c;
    }

    private static void OnAvatarSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is VisitorAvatar self && newValue is AvatarSize s)
            self.Avatar.AvatarSize = s;
    }
}
