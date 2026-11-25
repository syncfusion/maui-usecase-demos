using System.Globalization;

namespace SprintManagementBoard;

/// <summary>
/// Maps a task priority name to a badge color.
/// </summary>
public class PriorityToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value?.ToString() ?? string.Empty).ToLowerInvariant() switch
        {
            "critical" => Color.FromArgb("#EF4444"),
            "high" => Color.FromArgb("#F97316"),
            "low" => Color.FromArgb("#6B7280"),
            _ => Color.FromArgb("#3B82F6"),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
