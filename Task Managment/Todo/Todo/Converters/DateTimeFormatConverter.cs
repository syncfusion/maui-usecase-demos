using System.Globalization;

namespace Todo.Converters;

/// <summary>
/// Formats a nullable <see cref="DateTime"/> using a culture-aware
/// pattern. Returns an empty string for <c>null</c>.
///
/// <para>
/// Output format is selected via the <c>parameter</c> on the binding:
/// </para>
/// <list type="bullet">
///   <item><c>date</c> (default) – <c>dd_MMM_yyyy</c> (e.g. "18_Feb_2027")</item>
///   <item><c>time</c> – <c>HH:mm</c> (e.g. "10:00")</item>
///   <item><c>datetime</c> – <c>dd_MMM_yyyy, HH:mm</c> (e.g. "18_Feb_2027, 10:00")</item>
///   <item><c>display</c> – <c>ddd dd MMM, HH:mm</c> (e.g. "Wed 18 Feb, 10:00")</item>
/// </list>
/// </summary>
public sealed class DateTimeFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt)
            return string.Empty;

        var mode = (parameter as string ?? "date").ToLowerInvariant();
        return mode switch
        {
            "time"     => dt.ToString("HH:mm", culture),
            "datetime" => dt.ToString("dd_MMM_yyyy, HH:mm", culture),
            "display"  => dt.ToString("ddd dd MMM, HH:mm", CultureInfo.InvariantCulture),
            _          => dt.ToString("dd_MMM_yyyy", culture)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
