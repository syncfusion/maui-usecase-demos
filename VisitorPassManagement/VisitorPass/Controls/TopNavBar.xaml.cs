namespace VisitorPass.Controls;

/// <summary>
/// VP-UI top navigation bar. Hosts a large title, optional subtitle,
/// </summary>
public partial class TopNavBar : ContentView
{
    /// <summary>Title text — large headline.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>Optional subtitle — small caption under the title.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(TopNavBar), defaultValue: string.Empty);

    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public static readonly BindableProperty ShowRegisterButtonProperty = BindableProperty.Create(
        nameof(ShowRegisterButton), typeof(bool), typeof(TopNavBar), defaultValue: true);

    public static readonly BindableProperty ShowTitleProperty =
    BindableProperty.Create(
        nameof(ShowTitle),
        typeof(bool),
        typeof(TopNavBar),
        true);

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


    /// <summary>When true the "Register Visitor" trigger is shown.</summary>
    public bool ShowRegisterButton
    {
        get => (bool)GetValue(ShowRegisterButtonProperty);
        set => SetValue(ShowRegisterButtonProperty, value);
    }

    public TopNavBar()
    {
        InitializeComponent();
    }

}
