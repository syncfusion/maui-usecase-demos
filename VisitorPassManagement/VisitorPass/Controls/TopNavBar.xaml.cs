using VisitorPass.Services;

namespace VisitorPass.Controls;

/// <summary>
/// Top navigation bar. Hosts a large title, optional subtitle,
/// today's date, a quick search entry, a theme toggle, and an
/// operator avatar tile.
///
/// The theme toggle reads / writes <see cref="ThemeService"/> via DI;
/// the glyph flips between <see cref="IconKeys.LightMode"/> and
/// <see cref="IconKeys.DarkMode"/> so the user always sees the
/// action that the next tap will perform.
/// </summary>
public partial class TopNavBar : ContentView
{
    private ThemeService? _theme;

    /// <summary>Title text — large headline.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>Optional subtitle — small caption under the title.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public static readonly BindableProperty ShowRegisterButtonProperty = BindableProperty.Create(
        nameof(ShowRegisterButton), typeof(bool), typeof(TopNavBar), defaultValue: true);

    public static readonly BindableProperty ShowTitleProperty = BindableProperty.Create(
        nameof(ShowTitle),
        typeof(bool),
        typeof(TopNavBar),
        true);

    public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(
        nameof(SearchText),
        typeof(string),
        typeof(TopNavBar),
        defaultValue: string.Empty,
        defaultBindingMode: BindingMode.TwoWay);

    public bool ShowTitle
    {
        get => (bool)GetValue(ShowTitleProperty);
        set => SetValue(ShowTitleProperty, value);
    }

    public DateTime CurrentDate { get; set; } = DateTime.Now.Date;

    /// <summary>Title text — large headline.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional subtitle — small caption under the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Two-way bound search text the TopNavBar surfaces for
    /// the page's quick-search affordance.</summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public bool ShowRegisterButton
    {
        get => (bool)GetValue(ShowRegisterButtonProperty);
        set => SetValue(ShowRegisterButtonProperty, value);
    }

    public TopNavBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (_theme is null)
        {
            // Resolve ThemeService on demand. We don't take it via
            // constructor injection because the TopNavBar is
            // instantiated by XAML inflation, not DI.
            var services = Handler?.MauiContext?.Services;
            _theme = services?.GetService(typeof(ThemeService)) as ThemeService;
            if (_theme is not null)
            {
                _theme.PropertyChanged += OnThemePropertyChanged;
                UpdateThemeGlyph(_theme.IsDarkMode);
            }
        }
    }

    private void OnThemePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_theme is not null && e.PropertyName == nameof(ThemeService.IsDarkMode))
            UpdateThemeGlyph(_theme.IsDarkMode);
    }

    private void UpdateThemeGlyph(bool isDarkMode)
    {
        if (ThemeGlyph is null) return;
        ThemeGlyph.Text = isDarkMode ? IconKeys.LightMode : IconKeys.DarkMode;
    }

    private void OnThemeToggleTapped(object? sender, TappedEventArgs e)
    {
        _theme?.Toggle();
    }
}
