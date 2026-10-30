using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Maps a boolean (important) to a star glyph: filled when true,
/// outline when false.
/// </summary>
public sealed class BoolToStarGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "\u2605" : "\u2606";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Maps important notes to the app's purple brand color and other notes
/// to the secondary text color.
/// </summary>
public sealed class BoolToStarColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b
            ? Color.FromArgb("#5C4EAE")
            : Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#C4C4CC")
                : Color.FromArgb("#757575");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}